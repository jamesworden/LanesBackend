using LanesBackend.Exceptions;
using LanesBackend.Interfaces;
using LanesBackend.Models;
using LanesBackend.Results;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Results;

namespace LanesBackend.Hubs;

public class GameHub(IGameService gameService, IGameBroadcaster gameBroadcaster) : Hub
{
  private readonly IGameService GameService = gameService;

  private readonly IGameBroadcaster GameBroadcaster = gameBroadcaster;

  public async Task CreatePendingGame(string? stringifiedOptions)
  {
    string hostConnectionId = Context.ConnectionId;

    try
    {
      var options = stringifiedOptions is null
        ? null
        : JsonConvert.DeserializeObject<PendingGameOptions>(stringifiedOptions);

      var (pendingGame, results) = GameService.CreatePendingGame(hostConnectionId, options);

      if (results.Contains(CreatePendingGameResults.InvalidName))
      {
        await Clients.Client(hostConnectionId).SendAsync(MessageType.InvalidName);
        return;
      }

      if (pendingGame is null)
      {
        return;
      }

      var pendingGameView = new PendingGameView(
        pendingGame.GameCode,
        pendingGame.DurationOption,
        pendingGame.HostName
      );

      var serializedPendingGameView = JsonConvert.SerializeObject(
        pendingGame,
        new StringEnumConverter()
      );

      await Clients
        .Client(hostConnectionId)
        .SendAsync(MessageType.CreatedPendingGame, serializedPendingGameView);
    }
    catch (Exception) { }
  }

  public async Task JoinGame(string gameCode, string? stringifiedOptions)
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var options = stringifiedOptions is null
        ? null
        : JsonConvert.DeserializeObject<JoinPendingGameOptions>(stringifiedOptions);

      var (game, results) = GameService.JoinGame(connectionId, gameCode, options);

      if (results.Contains(JoinGameResults.InvalidName))
      {
        await Clients.Client(connectionId).SendAsync(MessageType.InvalidName);
      }

      if (results.Contains(JoinGameResults.InvalidGameCode))
      {
        await Clients.Client(connectionId).SendAsync(MessageType.InvalidGameCode);
      }

      if (game is null)
      {
        return;
      }

      if (results.Contains(JoinGameResults.GameStarted))
      {
        await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameStarted);
      }

      var isHost = game.HostConnectionId == connectionId;

      if (results.Contains(JoinGameResults.HostReconnected))
      {
        await GameBroadcaster.BroadcastHostGameView(game, MessageType.Reconnected);
        await Clients.Client(game.GuestConnectionId).SendAsync(MessageType.OpponentReconnected);
      }

      if (results.Contains(JoinGameResults.GuestReconnected))
      {
        await GameBroadcaster.BroadcastGuestGameView(game, MessageType.Reconnected);
        await Clients.Client(game.HostConnectionId).SendAsync(MessageType.OpponentReconnected);
      }
    }
    catch (Exception) { }
  }

  public void RearrangeHand(string stringifiedCards)
  {
    var connectionId = Context.ConnectionId;
    var cards = JsonConvert.DeserializeObject<List<Card>>(stringifiedCards);

    if (cards is null)
    {
      return;
    }

    try
    {
      GameService.RearrangeHand(connectionId, cards);
    }
    catch (Exception) { }
  }

  public async Task MakeMove(string stringifiedMove, string? stringifiedRearrangedCardsInHand)
  {
    var move = JsonConvert.DeserializeObject<Move>(stringifiedMove);
    var rearrangedCardsInHand = stringifiedRearrangedCardsInHand is null
      ? null
      : JsonConvert.DeserializeObject<List<Card>>(stringifiedRearrangedCardsInHand);

    if (move is null)
    {
      return;
    }

    var connectionId = Context.ConnectionId;

    try
    {
      var (game, results) = GameService.MakeMove(connectionId, move, rearrangedCardsInHand);

      if (game is null)
      {
        return;
      }

      if (results.Contains(MoveMadeResults.InvalidMove))
      {
        return;
      }

      if (!game.HasEnded)
      {
        if (results.Contains(MoveMadeResults.HostTurnSkippedNoMoves))
        {
          await Clients.Client(game.HostConnectionId).SendAsync(MessageType.TurnSkippedNoMoves);
        }
        else if (results.Contains(MoveMadeResults.GuestTurnSkippedNoMoves))
        {
          await Clients.Client(game.GuestConnectionId).SendAsync(MessageType.TurnSkippedNoMoves);
        }

        await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameUpdated);
        return;
      }

      if (game.WonBy == PlayerOrNone.None)
      {
        await GameBroadcaster.BroadcastPlayerGameViews(
          game,
          MessageType.GameOver,
          "It's a draw. No player has moves!"
        );
        return;
      }

      await GameBroadcaster.BroadcastHostGameView(
        game,
        MessageType.GameOver,
        game.WonBy == PlayerOrNone.Host ? "You win!" : "You lose!"
      );
      await GameBroadcaster.BroadcastGuestGameView(
        game,
        MessageType.GameOver,
        game.WonBy == PlayerOrNone.Guest ? "You win!" : "You lose!"
      );
    }
    catch (Exception) { }
  }

  public async Task PassMove()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var (game, results) = GameService.PassMove(connectionId);

      if (results.Contains(PassMoveResults.NotPlayersTurn))
      {
        return;
      }

      if (game is null)
      {
        return;
      }

      if (results.Contains(PassMoveResults.GameHasEnded))
      {
        await GameBroadcaster.BroadcastPlayerGameViews(
          game,
          MessageType.GameOver,
          "It's a draw by repetition!"
        );
        return;
      }

      await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.PassedMove);
    }
    catch (Exception) { }
  }

  public async Task OfferDraw()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var (game, results) = GameService.OfferDraw(connectionId);

      if (results.Contains(OfferDrawResults.AlreadyOfferedDraw))
      {
        return;
      }

      if (game is null)
      {
        return;
      }

      var playerIsHost = game.HostConnectionId == connectionId;
      var opponentConnectionId = playerIsHost ? game.GuestConnectionId : game.HostConnectionId;

      await Clients.Client(opponentConnectionId).SendAsync(MessageType.DrawOffered);
    }
    catch { }
  }

  public async Task AcceptDrawOffer()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var (game, results) = GameService.AcceptDrawOffer(connectionId);
      if (results.Contains(AcceptDrawOfferResults.NoOfferExists))
      {
        return;
      }

      if (game is null)
      {
        return;
      }

      await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameOver, "It's a draw.");
    }
    catch (Exception) { }
  }

  public async Task ResignGame()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var game = GameService.ResignGame(connectionId);

      if (game is null)
      {
        return;
      }

      var playerIsHost = game.HostConnectionId == connectionId;

      await GameBroadcaster.BroadcastHostGameView(
        game,
        MessageType.GameOver,
        playerIsHost ? "Game resigned." : "Opponent resigned."
      );
      await GameBroadcaster.BroadcastGuestGameView(
        game,
        MessageType.GameOver,
        playerIsHost ? "Game resigned." : "Opponent resigned."
      );
    }
    catch (Exception) { }
  }

  public async Task SendChatMessage(string rawMessage)
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var (game, results) = GameService.SendChatMessage(connectionId, rawMessage);

      if (game is null)
      {
        return;
      }

      if (results.Contains(SendChatMessageResults.MessageHasNoContent))
      {
        return;
      }

      await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.NewChatMessage);
    }
    catch (Exception) { }
  }

  public void DeletePendingGame()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      GameService.RemovePendingGame(connectionId);
    }
    catch (Exception) { }
  }

  public override async Task OnDisconnectedAsync(Exception? _)
  {
    var connectionId = Context.ConnectionId;

    var pendingGame = GameService.RemovePendingGame(connectionId);
    if (pendingGame is not null)
    {
      return;
    }

    try
    {
      var game = GameService.MarkPlayerAsDisconnected(connectionId);
      if (game is null)
      {
        return;
      }

      var hostDisconnected = connectionId == game.HostConnectionId;
      var opponentConnectionId = hostDisconnected ? game.GuestConnectionId : game.HostConnectionId;
      await Clients.Client(opponentConnectionId).SendAsync(MessageType.OpponentDisconnected);
    }
    catch (Exception) { }
  }
}
