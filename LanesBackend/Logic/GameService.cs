﻿using System.Diagnostics;
using LanesBackend.Caching;
using LanesBackend.Exceptions;
using LanesBackend.Hubs;
using LanesBackend.Interfaces;
using LanesBackend.Models;
using LanesBackend.Results;
using LanesBackend.Util;
using Microsoft.AspNetCore.SignalR;
using Results;

namespace LanesBackend.Logic;

public class GameService(
  IGameCache gameCache,
  IPendingGameCache pendingGameCache,
  IGameCodeService gameCodeService,
  IHubContext<GameHub> gameHubContext
) : IGameService
{
  private readonly IGameCache GameCache = gameCache;

  private readonly IHubContext<GameHub> GameHubContext = gameHubContext;

  private readonly IPendingGameCache PendingGameCache = pendingGameCache;

  private readonly IGameCodeService GameCodeService = gameCodeService;

  public Game CreateGame(
    string hostConnectionId,
    string guestConnectionId,
    string gameCode,
    DurationOption durationOption,
    bool playerIsHost,
    string? hostName,
    string? guestName
  )
  {
    var playerDecks = new Deck().Shuffle().Split();

    var hostDeck = playerDecks.Item1;
    var guestDeck = playerDecks.Item2;

    var hostHandCards = hostDeck.DrawCards(5);
    var guestHandCards = guestDeck.DrawCards(5);

    var hostHand = new Hand(hostHandCards);
    var guestHand = new Hand(guestHandCards);

    var hostPlayer = new Player(hostDeck, hostHand);
    var guestPlayer = new Player(guestDeck, guestHand);

    var lanes = GameUtil.CreateEmptyLanes();

    var gameCreatedTimestampUTC = DateTime.UtcNow;

    var durationInSeconds = GameUtil.GetMinutes(durationOption) * 60;

    Game game =
      new(
        hostConnectionId,
        guestConnectionId,
        gameCode,
        hostPlayer,
        guestPlayer,
        lanes,
        gameCreatedTimestampUTC,
        durationOption,
        durationInSeconds,
        null,
        hostName,
        guestName
      );

    var candidateMoves = game.GetCandidateMoves(true, true);
    game.CandidateMoves.Add(candidateMoves);

    GameCache.AddGame(game);

    game.HostTimer = new Stopwatch();
    game.GuestTimer = new Stopwatch();
    game.HostTimer.Start();
    game.EndGameTimer = GetEndGameTimer(game);

    return game;
  }

  public (Game, IEnumerable<MoveMadeResults>) MakeMove(
    string connectionId,
    Move move,
    List<Card>? rearrangedCardsInHand
  )
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      throw new GameNotExistsException();
    }

    var lastCandidateMoves = game.CandidateMoves.LastOrDefault();
    var moveIsOneOfLastCandidates =
      lastCandidateMoves?.Any(candidateMove => GameUtil.MovesMatch(candidateMove.Move, move))
      ?? false;
    if (lastCandidateMoves is not null && !moveIsOneOfLastCandidates)
    {
      throw new InvalidMoveException();
    }

    var playerIsHost = game.HostConnectionId == connectionId;
    var placedMultipleCards = move.PlaceCardAttempts.Count > 1;
    var cardMovements = GameUtil.PlaceCardsAndApplyGameRules(
      game,
      move.PlaceCardAttempts,
      playerIsHost
    );

    if (rearrangedCardsInHand is not null)
    {
      RearrangeHand(connectionId, rearrangedCardsInHand);
    }

    var drawnCardMovements = placedMultipleCards
      ? GameUtil.DrawCardsFromDeck(game, playerIsHost, 1)
      : GameUtil.DrawCardsUntil(game, playerIsHost, 5);
    cardMovements.AddRange(drawnCardMovements);

    var playedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
    var moveMade = new MoveMade(playedBy, move, DateTime.UtcNow, cardMovements);
    game.MovesMade.Add(moveMade);

    var opponentCandidateMoves = game.GetCandidateMoves(
      !game.IsHostPlayersTurn,
      !game.IsHostPlayersTurn
    );
    var anyValidOpponentCandidateMoves = opponentCandidateMoves.Any(move => move.IsValid);
    var playerCandidateMoves = game.GetCandidateMoves(
      game.IsHostPlayersTurn,
      game.IsHostPlayersTurn
    );
    var anyValidPlayerCandidateMoves = playerCandidateMoves.Any(move => move.IsValid);
    var moveMadeResults = new List<MoveMadeResults>();

    if ((!placedMultipleCards && anyValidOpponentCandidateMoves) || !anyValidPlayerCandidateMoves)
    {
      game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
      game.CandidateMoves.Add(opponentCandidateMoves);
    }
    else
    {
      game.CandidateMoves.Add(playerCandidateMoves);
    }

    if (!anyValidOpponentCandidateMoves)
    {
      moveMadeResults.Add(
        game.IsHostPlayersTurn
          ? MoveMadeResults.GuestTurnSkippedNoMoves
          : MoveMadeResults.HostTurnSkippedNoMoves
      );
    }

    SwitchTimeClocks(game);

    if (!anyValidPlayerCandidateMoves && !anyValidOpponentCandidateMoves)
    {
      EndGame(game);
    }

    return (game, moveMadeResults);
  }

  public Game PassMove(string connectionId)
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      throw new GameNotExistsException();
    }

    var playerIsHost = game.HostConnectionId == connectionId;
    var hostAndHostTurn = playerIsHost && game.IsHostPlayersTurn;
    var guestAndGuestTurn = !playerIsHost && !game.IsHostPlayersTurn;
    var isPlayersTurn = hostAndHostTurn || guestAndGuestTurn;
    if (!isPlayersTurn)
    {
      throw new NotPlayersTurnException();
    }

    var cardMovements = GameUtil.DrawCardsUntil(game, playerIsHost, 5);
    var move = new Move([]);
    var playedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
    var timeStampUTC = DateTime.UtcNow;
    game.MovesMade.Add(new MoveMade(playedBy, move, timeStampUTC, cardMovements, true));
    game.IsHostPlayersTurn = !game.IsHostPlayersTurn;

    if (GameUtil.HasThreeBackToBackPasses(game))
    {
      game.HasEnded = true;
      game.GameEndedTimestampUTC = timeStampUTC;
      GameCache.RemoveGameByConnectionId(connectionId);
    }
    else
    {
      var candidateMoves = game.GetCandidateMoves(game.IsHostPlayersTurn, game.IsHostPlayersTurn);
      game.CandidateMoves.Add(candidateMoves);
    }

    SwitchTimeClocks(game);

    return game;
  }

  /// <summary>
  /// Starts the clock of the player whose turn it is.
  /// Pauses the clock of the player whose turn it is not.
  /// Ensures that the game's `EndGameTimer` is configured to match the player of whose turn it is.
  /// </summary>
  private void SwitchTimeClocks(Game game)
  {
    var activeTimer = game.IsHostPlayersTurn ? game.HostTimer : game.GuestTimer;
    var inactiveTimer = game.IsHostPlayersTurn ? game.GuestTimer : game.HostTimer;

    inactiveTimer?.Stop();
    activeTimer?.Start();

    game.EndGameTimer?.Dispose();
    game.EndGameTimer = GetEndGameTimer(game);
  }

  private Timer GetEndGameTimer(Game game)
  {
    var secondsRemaining = game.IsHostPlayersTurn
      ? game.DurationInSeconds - (game.HostTimer?.Elapsed.Seconds ?? 0)
      : game.DurationInSeconds - (game.GuestTimer?.Elapsed.Seconds ?? 0);

    return new Timer(
      OnTimerRanOut,
      game,
      TimeSpan.FromSeconds(secondsRemaining),
      Timeout.InfiniteTimeSpan
    );
  }

  public async void OnTimerRanOut(object? state)
  {
    if (state is null)
    {
      return;
    }

    var game = (Game)state;
    var hostLost = game.IsHostPlayersTurn;
    game.WonBy = hostLost ? PlayerOrNone.Guest : PlayerOrNone.Host;

    EndGame(game);

    var winningConnectionId = hostLost ? game.GuestConnectionId : game.HostConnectionId;
    var losingConnectionId = hostLost ? game.HostConnectionId : game.GuestConnectionId;

    await GameHubContext
      .Clients.Client(losingConnectionId)
      .SendAsync(MessageType.GameOver, "You ran out of time. You lose!");
    await GameHubContext
      .Clients.Client(winningConnectionId)
      .SendAsync(MessageType.GameOver, "Opponent ran out of time. You win!");
  }

  public (Hand?, IEnumerable<RearrangeHandResults>) RearrangeHand(
    string connectionId,
    List<Card> cards
  )
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      return (null, [RearrangeHandResults.GameDoesNotExist]);
    }

    var playerIsHost = game.HostConnectionId == connectionId;
    var existingHand = playerIsHost ? game.HostPlayer.Hand : game.GuestPlayer.Hand;
    var existingCards = existingHand.Cards;
    bool containsDifferentCards = GameUtil.ContainsDifferentCards(existingCards, cards);
    if (containsDifferentCards)
    {
      return (existingHand, [RearrangeHandResults.InvalidCards]);
    }

    existingHand.Cards = cards;
    return (existingHand, []);
  }

  public Game? FindGame(string connectionId)
  {
    return GameCache.FindGameByConnectionId(connectionId);
  }

  public Game AcceptDrawOffer(string connectionId)
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      throw new GameNotExistsException();
    }

    // [Security Hardening]: Actually verify that the opponent offererd a draw to begin with.

    return EndGame(game);
  }

  public Game ResignGame(string connectionId)
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      throw new GameNotExistsException();
    }

    var playerIsHost = game.HostConnectionId == connectionId;
    game.WonBy = playerIsHost ? PlayerOrNone.Guest : PlayerOrNone.Host;

    return EndGame(game);
  }

  public Game EndGame(string connectionId)
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      throw new GameNotExistsException();
    }

    return EndGame(game);
  }

  private Game EndGame(Game game)
  {
    game.End();

    if (GameCache.RemoveGameByConnectionId(game.HostConnectionId) is null)
    {
      GameCache.RemoveGameByConnectionId(game.GuestConnectionId);
    }
    return game;
  }

  public Game UpdateGame(TestingGameData testingGameData, string gameCode)
  {
    var game = GameCache.FindGameByGameCode(gameCode);
    if (game is null)
    {
      throw new GameNotExistsException();
    }
    return game.Update(testingGameData);
  }

  public Game? MarkPlayerAsDisconnected(string connectionId)
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      return null;
    }

    var hostPlayerIsDisconnected = connectionId == game.HostConnectionId;
    if (hostPlayerIsDisconnected)
    {
      game.HostPlayerDisconnectedTimestampUTC = DateTime.UtcNow;
    }
    else
    {
      game.GuestPlayerDisconnectedTimestampUTC = DateTime.UtcNow;
    }

    var bothPlayersDisconnected =
      game.HostPlayerDisconnectedTimestampUTC is not null
      && game.GuestPlayerDisconnectedTimestampUTC is not null;
    if (bothPlayersDisconnected)
    {
      game.End();
      GameCache.RemoveGameByConnectionId(connectionId);

      return game;
    }

    game.DisconnectTimer = new Timer(
      OnDisconnectionTimeout,
      game,
      TimeSpan.FromSeconds(30),
      Timeout.InfiniteTimeSpan
    );

    return game;
  }

  /// <summary>
  /// At some point, this connection to the client should be moved outside of this service file.
  /// </summary>
  private async void OnDisconnectionTimeout(object? state)
  {
    if (state is null)
    {
      return;
    }

    var game = (Game)state;
    var hostLost = game.HostPlayerDisconnectedTimestampUTC is not null;
    game.WonBy = hostLost ? PlayerOrNone.Guest : PlayerOrNone.Host;
    game.HasEnded = true;
    game.GameEndedTimestampUTC = DateTime.UtcNow;
    var remainingConnectionId = hostLost ? game.GuestConnectionId : game.HostConnectionId;

    GameCache.RemoveGameByConnectionId(remainingConnectionId);

    await GameHubContext
      .Clients.Client(remainingConnectionId)
      .SendAsync(MessageType.GameOver, "Opponent left the game.");
  }

  public (Game, ChatMessageView) AddChatMessageToGame(string connectionId, string rawMessage)
  {
    var game = GameCache.FindGameByConnectionId(connectionId);
    if (game is null)
    {
      throw new GameNotExistsException();
    }

    var sensoredMessage = ChatUtil.ReplaceBadWordsWithAsterisks(rawMessage);
    var sentBy = connectionId == game.HostConnectionId ? PlayerOrNone.Host : PlayerOrNone.Guest;
    var sentAt = DateTime.UtcNow;
    var chatMessage = new ChatMessage(rawMessage, sensoredMessage, sentAt, sentBy);
    var chatMessageView = new ChatMessageView(sensoredMessage, sentBy, sentAt);
    game.ChatMessages.Add(chatMessage);
    game.ChatMessageViews.Add(chatMessageView);

    return (game, chatMessageView);
  }

  private Game? JoinExistingGame(string gameCode, string connectionId)
  {
    var game = GameCache.FindGameByGameCode(gameCode);
    if (game is null)
    {
      return null;
    }

    if (game.HostPlayerDisconnectedTimestampUTC is not null)
    {
      game.HostConnectionId = connectionId;
      game.HostPlayerDisconnectedTimestampUTC = null;
      game.DisconnectTimer?.Dispose();
      game.DisconnectTimer = null;
    }
    else if (game.GuestPlayerDisconnectedTimestampUTC is not null)
    {
      game.GuestConnectionId = connectionId;
      game.GuestPlayerDisconnectedTimestampUTC = null;
      game.DisconnectTimer?.Dispose();
      game.DisconnectTimer = null;
    }
    else
    {
      return null;
    }

    return game;
  }

  public (PendingGame?, IEnumerable<CreatePendingGameResults>) CreatePendingGame(
    string hostConnectionId,
    PendingGameOptions? pendingGameOptions
  )
  {
    if (pendingGameOptions?.HostName is not null && pendingGameOptions.HostName.Trim() == "")
    {
      pendingGameOptions.HostName = null;
    }

    if (pendingGameOptions?.HostName is not null)
    {
      var sensoredName = ChatUtil.ReplaceBadWordsWithAsterisks(pendingGameOptions.HostName);
      if (sensoredName != pendingGameOptions.HostName)
      {
        return (null, [CreatePendingGameResults.InvalidName]);
      }
    }

    string gameCode = GameCodeService.GenerateUniqueGameCode();
    var pendingGame = new PendingGame(gameCode, hostConnectionId, pendingGameOptions);
    PendingGameCache.AddPendingGame(pendingGame);
    return (pendingGame, []);
  }

  private Game? JoinPendingGame(
    string gameCode,
    string guestConnectionId,
    JoinPendingGameOptions? joinPendingGameOptions
  )
  {
    var upperCaseGameCode = gameCode.ToUpper();
    var pendingGame = PendingGameCache.GetPendingGameByGameCode(upperCaseGameCode);
    if (pendingGame is null)
    {
      return null;
    }

    var playerIsHost = false;

    var game = CreateGame(
      pendingGame.HostConnectionId,
      guestConnectionId,
      gameCode,
      pendingGame.DurationOption,
      playerIsHost,
      pendingGame.HostName,
      joinPendingGameOptions?.GuestName
    );

    PendingGameCache.RemovePendingGame(gameCode);

    return game;
  }

  public PendingGame? RemovePendingGame(string connectionId)
  {
    var pendingGame = PendingGameCache.GetPendingGameByConnectionId(connectionId);
    if (pendingGame is not null)
    {
      PendingGameCache.RemovePendingGame(pendingGame.GameCode);
      return pendingGame;
    }
    return pendingGame;
  }

  public (Game?, IEnumerable<JoinGameResults>) JoinGame(
    string connectionId,
    string gameCode,
    JoinPendingGameOptions? options
  )
  {
    var existingGame = JoinExistingGame(gameCode, connectionId);
    if (existingGame is not null)
    {
      var isHost = existingGame.HostConnectionId == connectionId;
      var result = isHost ? JoinGameResults.HostReconnected : JoinGameResults.GuestReconnected;
      return (existingGame, [result]);
    }

    if (options?.GuestName is not null && options?.GuestName.Trim() == "")
    {
      options.GuestName = null;
    }

    if (options?.GuestName is not null)
    {
      var sensoredName = ChatUtil.ReplaceBadWordsWithAsterisks(options.GuestName);
      if (sensoredName != options.GuestName)
      {
        return (null, [JoinGameResults.InvalidName]);
      }
    }

    var game = JoinPendingGame(gameCode, connectionId, options);
    if (game is null)
    {
      return (game, [JoinGameResults.InvalidGameCode]);
    }

    return (game, [JoinGameResults.GameStarted]);
  }
}
