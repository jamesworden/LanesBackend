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
      var (hand, results) = GameService.RearrangeHand(connectionId, cards);

      if (results.Contains(RearrangeHandResults.InvalidCards))
      {
        return;
      }

      if (results.Contains(RearrangeHandResults.GameDoesNotExist))
      {
        return;
      }

      if (hand is null)
      {
        return;
      }
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
      var (game, moveMadeResults) = GameService.MakeMove(connectionId, move, rearrangedCardsInHand);
      await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameUpdated);

      if (!game.HasEnded)
      {
        if (moveMadeResults.Contains(MoveMadeResults.HostTurnSkippedNoMoves))
        {
          await Clients.Client(game.HostConnectionId).SendAsync(MessageType.TurnSkippedNoMoves);
        }
        else if (moveMadeResults.Contains(MoveMadeResults.GuestTurnSkippedNoMoves))
        {
          await Clients.Client(game.GuestConnectionId).SendAsync(MessageType.TurnSkippedNoMoves);
        }

        return;
      }

      if (game.WonBy == PlayerOrNone.None)
      {
        await Clients
          .Client(game.HostConnectionId)
          .SendAsync(MessageType.GameOver, "It's a draw. No player has moves!");
        await Clients
          .Client(game.GuestConnectionId)
          .SendAsync(MessageType.GameOver, "It's a draw. No player has moves!");
        return;
      }

      var winnerConnId =
        game.WonBy == PlayerOrNone.Host ? game.HostConnectionId : game.GuestConnectionId;
      var loserConnId =
        game.WonBy == PlayerOrNone.Host ? game.GuestConnectionId : game.HostConnectionId;

      await Clients.Client(winnerConnId).SendAsync(MessageType.GameOver, "You win!");
      await Clients.Client(loserConnId).SendAsync(MessageType.GameOver, "You lose!");
    }
    catch (GameNotExistsException) { }
    catch (InvalidMoveException) { }
    catch { }
  }

  public async Task PassMove()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var (game, results) = GameService.PassMove(connectionId);

      if (results.Contains(PassMoveResults.GameDoesNotExist))
      {
        return;
      }

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

      if (results.Contains(OfferDrawResults.GameDoesNotExist))
      {
        return;
      }

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
      var (game, results) = GameService.ResignGame(connectionId);

      if (results.Contains(ResignGameResults.GameDoesNotExist))
      {
        return;
      }

      if (game is null)
      {
        return;
      }

      var playerIsHost = game.HostConnectionId == connectionId;
      var winnerConnectionId = playerIsHost ? game.GuestConnectionId : game.HostConnectionId;
      var loserConnectionId = playerIsHost ? game.HostConnectionId : game.GuestConnectionId;

      await Clients
        .Client(winnerConnectionId)
        .SendAsync(MessageType.GameOver, "Opponent resigned.");
      await Clients.Client(loserConnectionId).SendAsync(MessageType.GameOver, "Game resigned.");
    }
    catch (Exception) { }
  }

  public async Task CheckHostForEmptyTimer()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var game = GameService.EndGame(connectionId);
      await Clients
        .Client(game.HostConnectionId)
        .SendAsync(MessageType.GameOver, "Your timer ran out. You lose.");
      await Clients
        .Client(game.GuestConnectionId)
        .SendAsync(MessageType.GameOver, "Your opponent's timer ran out. You win!");
    }
    catch (GameNotExistsException) { }
    catch (Exception) { }
  }

  public async Task CheckGuestForEmptyTimer()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var game = GameService.EndGame(connectionId);
      await Clients
        .Client(game.GuestConnectionId)
        .SendAsync(MessageType.GameOver, "Your timer ran out. You lose.");
      await Clients
        .Client(game.HostConnectionId)
        .SendAsync(MessageType.GameOver, "Your opponent's timer ran out. You win!");
    }
    catch (GameNotExistsException) { }
    catch (Exception) { }
  }

  public async Task SendChatMessage(string rawMessage)
  {
    var connectionId = Context.ConnectionId;

    try
    {
      if (rawMessage.Trim().Length == 0)
      {
        return;
      }

      var (game, chatMessage) = GameService.AddChatMessageToGame(connectionId, rawMessage);
      var playerIsHost = game.HostConnectionId == connectionId;
      var playerId = playerIsHost ? game.HostConnectionId : game.GuestConnectionId;
      var opponentId = playerIsHost ? game.GuestConnectionId : game.HostConnectionId;

      await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.NewChatMessage);
    }
    catch (GameNotExistsException) { }
    catch (Exception) { }
  }

  public void DeletePendingGame()
  {
    var connectionId = Context.ConnectionId;

    try
    {
      var game = GameService.RemovePendingGame(connectionId);
    }
    catch (GameNotExistsException) { }
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
      if (game is not null)
      {
        var hostDisconnected = connectionId == game.HostConnectionId;
        var opponentConnectionId = hostDisconnected
          ? game.GuestConnectionId
          : game.HostConnectionId;
        await Clients.Client(opponentConnectionId).SendAsync(MessageType.OpponentDisconnected);
      }
    }
    catch (Exception) { }
  }
}
