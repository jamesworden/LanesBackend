﻿namespace LanesBackend.Models
{
  public class PlayerGameView
  {
    public int NumCardsInOpponentsHand { get; set; }

    public int NumCardsInOpponentsDeck { get; set; }

    public int NumCardsInPlayersDeck { get; set; }

    public Hand Hand { get; set; }

    public Lane[] Lanes { get; set; }

    public bool IsHost { get; set; }

    public bool IsHostPlayersTurn { get; set; }

    public int? RedJokerLaneIndex { get; set; }

    public int? BlackJokerLaneIndex { get; set; }

    public DateTime GameCreatedTimestampUTC { get; set; }

    public List<MoveMade> MovesMade { get; set; }

    public DurationOption DurationOption { get; set; }

    public DateTime? GameEndedTimestampUTC { get; set; }

    public string GameCode { get; set; }

    public List<CandidateMove>? CandidateMoves { get; set; }

    public bool HasEnded { get; set; }

    public List<ChatMessageView> ChatMessages { get; set; }

    public string? HostName { get; set; }

    public string? GuestName { get; set; }

    public int HostSecondsRemaining;

    public int GuestSecondsRemaining;

    public PlayerGameView(
      int numCardsInOpponentsDeck,
      int numCardsInOpponentsHand,
      int numCardsInPlayersDeck,
      Hand hand,
      Lane[] lanes,
      bool isHost,
      bool isHostPlayersTurn,
      int? redJokerLaneIndex,
      int? blackJokerLaneIndex,
      DateTime gameCreatedTimestampUTC,
      List<MoveMade> movesMade,
      DurationOption durationOption,
      DateTime? gameEndedTimestampUTC,
      string gameCode,
      List<CandidateMove>? candidateMoves,
      bool hasEnded,
      List<ChatMessageView> chatMessages,
      int hostSecondsRemaining,
      int guestSecondsRemaining,
      string? hostName,
      string? guestName
    )
    {
      NumCardsInOpponentsDeck = numCardsInOpponentsDeck;
      NumCardsInOpponentsHand = numCardsInOpponentsHand;
      NumCardsInPlayersDeck = numCardsInPlayersDeck;
      Hand = hand;
      Lanes = lanes;
      IsHost = isHost;
      IsHostPlayersTurn = isHostPlayersTurn;
      RedJokerLaneIndex = redJokerLaneIndex;
      BlackJokerLaneIndex = blackJokerLaneIndex;
      GameCreatedTimestampUTC = gameCreatedTimestampUTC;
      MovesMade = movesMade;
      DurationOption = durationOption;
      GameEndedTimestampUTC = gameEndedTimestampUTC;
      GameCode = gameCode;
      CandidateMoves = candidateMoves;
      HasEnded = hasEnded;
      ChatMessages = chatMessages;
      HostName = hostName;
      GuestName = guestName;
      HostSecondsRemaining = hostSecondsRemaining;
      GuestSecondsRemaining = guestSecondsRemaining;
    }
  }
}
