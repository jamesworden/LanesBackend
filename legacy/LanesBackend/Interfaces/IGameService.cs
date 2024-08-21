using LanesBackend.Models;
using LanesBackend.Results;
using Results;

namespace LanesBackend.Interfaces;

public interface IGameService
{
  public Game CreateGame(
    string hostConnectionId,
    string guestConnectionId,
    string gameCode,
    DurationOption durationOption,
    bool playerIsHost,
    string? HostName,
    string? GuestName
  );

  public (Game?, IEnumerable<MoveMadeResults>) MakeMove(
    string connectionId,
    Move move,
    List<Card>? rearrangedCardsInHand
  );

  public (Game?, IEnumerable<PassMoveResults>) PassMove(string connectionId);

  public (Hand?, IEnumerable<RearrangeHandResults>) RearrangeHand(
    string connectionId,
    List<Card> cards
  );

  public (Game?, IEnumerable<AcceptDrawOfferResults>) AcceptDrawOffer(string connectionId);

  public Game? ResignGame(string connectionId);

  public Game? EndGame(string connectionId);

  public Game? UpdateGame(TestingGameData testingGameData, string gameCode);

  public (Game?, IEnumerable<SendChatMessageResults>) SendChatMessage(
    string connectionId,
    string rawMessage
  );

  public Game? MarkPlayerAsDisconnected(string connectionId);

  public (PendingGame?, IEnumerable<CreatePendingGameResults>) CreatePendingGame(
    string hostConnectionId,
    PendingGameOptions? options
  );

  public (Game?, IEnumerable<JoinGameResults>) JoinGame(
    string connectionId,
    string gameCode,
    JoinPendingGameOptions? options
  );

  public (Game?, IEnumerable<OfferDrawResults>) OfferDraw(string connectionId);

  public PendingGame? RemovePendingGame(string connectionId);
}
