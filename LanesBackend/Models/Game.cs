using System.Diagnostics;
using LanesBackend.Util;

namespace LanesBackend.Models;

public class Game(
  string hostConnectionId,
  string guestConnectionId,
  string gameCode,
  Player hostPlayer,
  Player guestPlayer,
  Lane[] lanes,
  DateTime gameCreatedTimestampUTC,
  DurationOption durationOption,
  int durationInSeconds,
  DateTime? gameEndedTimestampUTC,
  string? hostName,
  string? guestName
)
{
  public PlayerOrNone WonBy = PlayerOrNone.None;

  public bool IsHostPlayersTurn = true;

  public string HostConnectionId { get; set; } = hostConnectionId;

  public string GuestConnectionId { get; set; } = guestConnectionId;

  public string GameCode { get; set; } = gameCode;

  public Lane[] Lanes = lanes;

  public Player HostPlayer { get; set; } = hostPlayer;

  public Player GuestPlayer { get; set; } = guestPlayer;

  public int? RedJokerLaneIndex { get; set; }

  public int? BlackJokerLaneIndex { get; set; }

  public DateTime GameCreatedTimestampUTC { get; set; } = gameCreatedTimestampUTC;

  public List<MoveMade> MovesMade = new();

  public DurationOption DurationOption { get; set; } = durationOption;

  public DateTime? GameEndedTimestampUTC { get; set; } = gameEndedTimestampUTC;

  public List<List<CandidateMove>> CandidateMoves { get; set; } = new();

  public bool HasEnded = false;

  public List<ChatMessage> ChatMessages { get; set; } = new();

  public List<ChatMessageView> ChatMessageViews { get; set; } = new();

  public string? HostName { get; set; } = hostName;

  public string? GuestName { get; set; } = guestName;

  public DateTime? HostPlayerDisconnectedTimestampUTC = null;

  public DateTime? GuestPlayerDisconnectedTimestampUTC = null;

  public Timer? DisconnectTimer = null;

  public Stopwatch? HostTimer = null;

  public Stopwatch? GuestTimer = null;

  public Timer? EndGameTimer = null;

  public int DurationInSeconds = durationInSeconds;

  public bool DrawOfferFromHost = false;

  public bool DrawOfferFromGuest = false;

  public List<CandidateMove> GetCandidateMoves(bool asHostPlayer, bool isHostPlayersTurn)
  {
    var player = asHostPlayer ? HostPlayer : GuestPlayer;
    var candidateMoves = new List<CandidateMove>();

    foreach (var card in player.Hand.Cards)
    {
      var cardCandidateMoves = GetCandidateMoves(card, asHostPlayer, isHostPlayersTurn);
      candidateMoves.AddRange(cardCandidateMoves);
    }

    return candidateMoves;
  }

  private List<CandidateMove> GetCandidateMoves(
    Card card,
    bool asHostPlayer,
    bool isHostPlayersTurn
  )
  {
    var candidateMoves = new List<CandidateMove>();

    for (var rowIndex = 0; rowIndex < 7; rowIndex++)
    {
      for (var laneIndex = 0; laneIndex < 5; laneIndex++)
      {
        var placeCardAttempt = new PlaceCardAttempt(card, laneIndex, rowIndex);
        var placeCardAttempts = new List<PlaceCardAttempt> { placeCardAttempt };
        var move = new Move(placeCardAttempts);
        var candidateMove = GetCandidateMove(move, asHostPlayer, isHostPlayersTurn);
        candidateMoves.Add(candidateMove);

        if (GameUtil.IsDefensive(placeCardAttempt, IsHostPlayersTurn))
        {
          var player = asHostPlayer ? HostPlayer : GuestPlayer;
          var cardsInHand = player.Hand.Cards;
          var placeMultipleCandidateMoves = GetPlaceMultipleCandidateMoves(
            placeCardAttempt,
            cardsInHand,
            asHostPlayer,
            isHostPlayersTurn
          );
          candidateMoves.AddRange(placeMultipleCandidateMoves);
        }
      }
    }

    return candidateMoves;
  }

  private CandidateMove GetCandidateMove(Move move, bool asHostPlayer, bool isHostPlayersTurn)
  {
    var invalidReason = GameUtil.GetReasonIfMoveInvalid(
      move,
      this,
      asHostPlayer,
      isHostPlayersTurn
    );
    var isValid = invalidReason is null;
    return new CandidateMove(move, isValid, invalidReason);
  }

  private List<CandidateMove> GetPlaceMultipleCandidateMoves(
    PlaceCardAttempt initialPlaceCardAttempt,
    List<Card> cardsInHand,
    bool asHostPlayer,
    bool isHostPlayersTurn
  )
  {
    var candidateMoves = new List<CandidateMove>();

    var candidateCardsInHand = cardsInHand
      .Where(cardInHand =>
        GameUtil.KindMatches(initialPlaceCardAttempt.Card, cardInHand)
        && !GameUtil.SuitMatches(initialPlaceCardAttempt.Card, cardInHand)
      )
      .ToList();

    List<List<Card>> candidateCardPermutationSubsets = PermutationsUtil.GetSubsetsPermutations(
      candidateCardsInHand
    );

    foreach (var candidateCards in candidateCardPermutationSubsets)
    {
      var totalCandidatePlaceCardAttempts = new List<PlaceCardAttempt> { initialPlaceCardAttempt };
      var candidatePlaceCardAttempts = new List<PlaceCardAttempt>();

      for (var i = 0; i < candidateCards.Count; i++)
      {
        var rowIndex = asHostPlayer
          ? initialPlaceCardAttempt.TargetRowIndex + 1 + i
          : initialPlaceCardAttempt.TargetRowIndex - 1 - i;

        // When placing multiple cards beyond the middle of the lane, the target row index of 3
        // is skipped. For example, the host might play one move from row indexes 1, 2, and 4,
        // while the guest might play one move from row indexes 5, 4, 2.
        if (asHostPlayer && rowIndex >= 3)
        {
          rowIndex++;
        }
        else if (!asHostPlayer && rowIndex <= 3)
        {
          rowIndex--;
        }

        var card = candidateCards[i];
        if (card is not null)
        {
          candidatePlaceCardAttempts.Add(
            new PlaceCardAttempt(card, initialPlaceCardAttempt.TargetLaneIndex, rowIndex)
          );
        }
      }

      totalCandidatePlaceCardAttempts.AddRange(candidatePlaceCardAttempts);
      var move = new Move(totalCandidatePlaceCardAttempts);
      var candidateMove = GetCandidateMove(move, asHostPlayer, isHostPlayersTurn);
      candidateMoves.Add(candidateMove);
    }

    return candidateMoves;
  }

  public Game Update(TestingGameData testingGameData)
  {
    Lanes = testingGameData.Lanes;
    HostPlayer.Hand = testingGameData.HostHand;
    GuestPlayer.Hand = testingGameData.GuestHand;
    HostPlayer.Deck = testingGameData.HostDeck;
    GuestPlayer.Deck = testingGameData.GuestDeck;
    RedJokerLaneIndex = testingGameData.RedJokerLaneIndex;
    BlackJokerLaneIndex = testingGameData.BlackJokerLaneIndex;
    IsHostPlayersTurn = testingGameData.IsHostPlayersTurn;

    CandidateMoves.Add(GetCandidateMoves(IsHostPlayersTurn, IsHostPlayersTurn));

    return this;
  }

  public void End()
  {
    HasEnded = true;
    GameEndedTimestampUTC = DateTime.UtcNow;

    DisconnectTimer?.Dispose();
    DisconnectTimer = null;

    HostTimer?.Stop();
    HostTimer = null;

    GuestTimer?.Stop();
    GuestTimer = null;

    EndGameTimer?.Dispose();
    EndGameTimer = null;
  }

  public bool MoveIsLatestCandidate(Move move)
  {
    var lastCandidateMoves = CandidateMoves.LastOrDefault();

    var moveIsOneOfLastCandidates =
      lastCandidateMoves?.Any(candidateMove => GameUtil.MovesMatch(candidateMove.Move, move))
      ?? false;

    return lastCandidateMoves is not null && !moveIsOneOfLastCandidates;
  }

  /// <summary>
  /// Returns whether a draw was previously offered or not.
  /// </summary>
  public bool OfferDrawIfNotOfferedYet(string connectionId)
  {
    var isHost = connectionId == HostConnectionId;
    var alreadyOfferedDraw = (isHost && DrawOfferFromHost) || (!isHost && DrawOfferFromGuest);

    if (alreadyOfferedDraw)
    {
      return true;
    }

    if (isHost)
    {
      DrawOfferFromHost = true;
    }
    else
    {
      DrawOfferFromGuest = true;
    }

    return false;
  }
}
