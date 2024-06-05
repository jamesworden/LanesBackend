﻿using System.Diagnostics;

namespace LanesBackend.Models
{
  public class Game
  {
    public PlayerOrNone WonBy = PlayerOrNone.None;

    public bool IsHostPlayersTurn = true;

    public string HostConnectionId { get; set; }

    public string GuestConnectionId { get; set; }

    public string GameCode { get; set; }

    public Lane[] Lanes = new Lane[5];

    public Player HostPlayer { get; set; }

    public Player GuestPlayer { get; set; }

    public int? RedJokerLaneIndex { get; set; }

    public int? BlackJokerLaneIndex { get; set; }

    public DateTime GameCreatedTimestampUTC { get; set; }

    public List<MoveMade> MovesMade = new();

    public DurationOption DurationOption { get; set; }

    public DateTime? GameEndedTimestampUTC { get; set; }

    public List<List<CandidateMove>> CandidateMoves { get; set; } = new();

    public bool HasEnded = false;

    public List<ChatMessage> ChatMessages { get; set; } = new();

    public List<ChatMessageView> ChatMessageViews { get; set; } = new();

    public string? HostName { get; set; }

    public string? GuestName { get; set; }

    public DateTime? HostPlayerDisconnectedTimestampUTC = null;

    public DateTime? GuestPlayerDisconnectedTimestampUTC = null;

    public Timer? DisconnectTimer = null;

    public Stopwatch? HostTimer = null;

    public Stopwatch? GuestTimer = null;

    public int DurationInSeconds;

    public Game(
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
      HostConnectionId = hostConnectionId;
      GuestConnectionId = guestConnectionId;
      GameCode = gameCode;
      HostPlayer = hostPlayer;
      GuestPlayer = guestPlayer;
      Lanes = lanes;
      GameCreatedTimestampUTC = gameCreatedTimestampUTC;
      DurationOption = durationOption;
      GameEndedTimestampUTC = gameEndedTimestampUTC;
      HostName = hostName;
      GuestName = guestName;
      DurationInSeconds = durationInSeconds;
    }
  }
}
