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

  public (Game, IEnumerable<MoveMadeResults>) MakeMove(
    string connectionId,
    Move move,
    List<Card>? rearrangedCardsInHand
  );

  public Game PassMove(string connectionId);

  public Hand RearrangeHand(string connectionId, List<Card> cards);

  public Game? FindGame(string connectionId);

  public Game AcceptDrawOffer(string connectionId);

  public Game ResignGame(string connectionId);

  public Game EndGame(string connectionId);

  public Game UpdateGame(TestingGameData testingGameData, string gameCode);

  public (Game, ChatMessageView) AddChatMessageToGame(string connectionId, string rawMessage);

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

  public PendingGame? RemovePendingGame(string connectionId);
}
