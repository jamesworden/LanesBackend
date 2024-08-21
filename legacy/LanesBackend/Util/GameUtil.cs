using LanesBackend.Models;

namespace LanesBackend.Util
{
  public static class GameUtil
  {
    private static readonly int NUMBER_OF_LANES = 5;

    private static readonly int NUMBER_OF_ROWS = 7;

    public static string? GetReasonIfMoveInvalid(
      Move move,
      Game game,
      bool playerIsHost,
      bool isHostPlayersTurn
    )
    {
      if (!((playerIsHost && isHostPlayersTurn) || (!playerIsHost && !isHostPlayersTurn)))
      {
        return "It's not your turn!";
      }
      if (move.PlaceCardAttempts.Count == 0)
      {
        return "You need to place a card!";
      }
      if (move.PlaceCardAttempts.Count > 4)
      {
        return "You placed too many cards!";
      }
      if (move.PlaceCardAttempts.Select(attempt => attempt.TargetLaneIndex).Distinct().Count() > 1)
      {
        return "You can't place cards on different lanes!";
      }
      if (
        move.PlaceCardAttempts.Select(attempt => attempt.TargetRowIndex).Distinct().Count()
        < move.PlaceCardAttempts.Count
      )
      {
        return "You can't place cards on the same position!";
      }
      if (TargetLaneHasBeenWon(game, move))
      {
        return "This lane was won already!";
      }
      if (move.PlaceCardAttempts.Any(placeCardAttempt => placeCardAttempt.TargetRowIndex == 3))
      {
        return "You can't place a card in the middle!";
      }
      if (!ContainsConsecutivePlaceCardAttempts(move.PlaceCardAttempts))
      {
        return "You can't place cards that are separate from one another!";
      }
      if (move.PlaceCardAttempts.Select(attempt => attempt.Card.Kind).Distinct().Count() > 1)
      {
        return "Placing multiple cards must be of the same kind!";
      }
      if (TriedToCaptureDistantRow(game, move, playerIsHost))
      {
        return "You can't capture this position yet!";
      }
      if (TriedToCaptureGreaterCard(game, move, playerIsHost))
      {
        return "You can't capture a greater card!";
      }
      if (
        StartedMovePlayerSide(game, move, playerIsHost)
        && PlayerHasAdvantage(game, move, playerIsHost)
      )
      {
        return "You must attack this lane!";
      }
      if (
        StartedMoveOpponentSide(game, move, playerIsHost)
        && OpponentHasAdvantage(game, move, playerIsHost)
      )
      {
        return "You must defend this lane!";
      }
      if (StartedMoveOpponentSide(game, move, playerIsHost) && LaneHasNoAdvantage(game, move))
      {
        return "You aren't ready to attack here yet.";
      }
      if (!SuitOrKindMatchesLastCardPlayed(game, move, playerIsHost))
      {
        return "This card can't be placed here.";
      }
      if (TriedToReinforceGreaterCard(game, move, playerIsHost))
      {
        return "You can't reinforce a greater card!";
      }
      return null;
    }

    public static bool IsOffensive(PlaceCardAttempt placeCardAttempt, bool playerIsHost)
    {
      return (placeCardAttempt.TargetRowIndex > 3 && playerIsHost)
        || (placeCardAttempt.TargetRowIndex < 3 && !playerIsHost);
    }

    public static bool IsToMiddle(PlaceCardAttempt placeCardAttempt)
    {
      return placeCardAttempt.TargetRowIndex == 3;
    }

    public static bool IsDefensive(PlaceCardAttempt placeCardAttempt, bool playerIsHost)
    {
      return (placeCardAttempt.TargetRowIndex < 3 && playerIsHost)
        || (placeCardAttempt.TargetRowIndex > 3 && !playerIsHost);
    }

    public static bool IsPlayersTurn(Game game, bool playerIsHost)
    {
      return game.IsHostPlayersTurn && playerIsHost || !game.IsHostPlayersTurn && !playerIsHost;
    }

    public static bool SuitMatches(Card card1, Card card2)
    {
      return card1.Suit == card2.Suit;
    }

    public static bool KindMatches(Card card1, Card card2)
    {
      return card1.Kind == card2.Kind;
    }

    public static bool SuitAndKindMatches(Card card1, Card card2)
    {
      return SuitMatches(card1, card2) && KindMatches(card1, card2);
    }

    public static bool ContainsConsecutivePlaceCardAttempts(
      List<PlaceCardAttempt> placeCardAttempts
    )
    {
      var targetRowIndexes = placeCardAttempts
        .Select(placeCardAttempt => placeCardAttempt.TargetRowIndex)
        .ToList();
      targetRowIndexes.Sort();

      for (int i = 0; i < targetRowIndexes.Count - 1; i++)
      {
        var upperIndex = targetRowIndexes[i + 1];
        var lowerIndex = targetRowIndexes[i];
        var indexesSurroundMiddle = upperIndex > 3 && lowerIndex < 3;
        var rowIndexesSeperate = upperIndex - lowerIndex != 1;
        if (rowIndexesSeperate && !indexesSurroundMiddle)
        {
          return false;
        }
      }

      return true;
    }

    public static bool TriedToCaptureDistantRow(Game game, Move move, bool playerIsHost)
    {
      var firstPlaceCardAttempt = GetInitialPlaceCardAttempt(move, playerIsHost);
      if (firstPlaceCardAttempt is null)
      {
        return false;
      }

      if (playerIsHost)
      {
        var startIndex = StartedMovePlayerSide(game, move, playerIsHost) ? 0 : 4;
        return !CapturedAllPreviousRows(game, firstPlaceCardAttempt, startIndex, playerIsHost);
      }

      var endIndex = StartedMoveOpponentSide(game, move, playerIsHost) ? 2 : 6;
      return !CapturedAllFollowingRows(game, firstPlaceCardAttempt, endIndex, playerIsHost);
    }

    public static bool StartedMovePlayerSide(Game game, Move move, bool playerIsHost)
    {
      var firstPlaceCardAttempt = move.PlaceCardAttempts.FirstOrDefault();
      if (firstPlaceCardAttempt is null)
      {
        return false;
      }

      return playerIsHost
        ? firstPlaceCardAttempt.TargetRowIndex < 3
        : firstPlaceCardAttempt.TargetRowIndex > 3;
    }

    public static bool StartedMoveOpponentSide(Game game, Move move, bool playerIsHost)
    {
      var firstPlaceCardAttempt = move.PlaceCardAttempts.FirstOrDefault();
      if (firstPlaceCardAttempt is null)
      {
        return false;
      }

      return playerIsHost
        ? firstPlaceCardAttempt.TargetRowIndex > 3
        : firstPlaceCardAttempt.TargetRowIndex < 3;
    }

    /// <summary>
    /// Return true if there were no previous rows to capture
    /// </summary>
    public static bool CapturedAllPreviousRows(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      int startIndex,
      bool playerIsHost
    )
    {
      var targetLaneIndex = placeCardAttempt.TargetLaneIndex;
      var targetRowIndex = placeCardAttempt.TargetRowIndex;
      var lane = game.Lanes[targetLaneIndex];

      if (placeCardAttempt.TargetRowIndex == startIndex)
      {
        return true;
      }

      for (int i = startIndex; i < targetRowIndex; i++)
      {
        var previousRow = lane.Rows[i];
        var previousRowNotOccupied = previousRow.Count == 0;

        if (previousRowNotOccupied)
        {
          return false;
        }

        var topCard = previousRow[previousRow.Count - 1];
        var topCardPlayedByPlayer = playerIsHost
          ? topCard.PlayedBy == PlayerOrNone.Host
          : topCard.PlayedBy == PlayerOrNone.Guest;

        if (!topCardPlayedByPlayer)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Return true if there were no following rows to capture
    /// </summary>
    public static bool CapturedAllFollowingRows(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      int endIndex,
      bool playerIsHost
    )
    {
      var targetLaneIndex = placeCardAttempt.TargetLaneIndex;
      var targetRowIndex = placeCardAttempt.TargetRowIndex;
      var lane = game.Lanes[targetLaneIndex];

      if (placeCardAttempt.TargetRowIndex == endIndex)
      {
        return true;
      }

      for (int i = endIndex; i > targetRowIndex; i--)
      {
        var followingRow = lane.Rows[i];
        var followingRowNotOccupied = followingRow.Count == 0;

        if (followingRowNotOccupied)
        {
          return false;
        }

        var topCard = followingRow[followingRow.Count - 1];
        var topCardPlayedByPlayer = playerIsHost
          ? topCard.PlayedBy == PlayerOrNone.Host
          : topCard.PlayedBy == PlayerOrNone.Guest;

        if (!topCardPlayedByPlayer)
        {
          return false;
        }
      }

      return true;
    }

    public static bool TargetLaneHasBeenWon(Game game, Move move)
    {
      foreach (var placeCardAttempt in move.PlaceCardAttempts)
      {
        var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
        if (lane.WonBy != PlayerOrNone.None)
        {
          return true;
        }
      }

      return false;
    }

    public static bool TriedToCaptureGreaterCard(Game game, Move move, bool playerIsHost)
    {
      var firstPlaceCardAttempt = GetInitialPlaceCardAttempt(move, playerIsHost);
      var card = firstPlaceCardAttempt.Card;
      var targetLaneIndex = firstPlaceCardAttempt.TargetLaneIndex;
      var targetRowIndex = firstPlaceCardAttempt.TargetRowIndex;
      var targetRow = game.Lanes[targetLaneIndex].Rows[targetRowIndex];

      if (targetRow.Count <= 0)
      {
        return false;
      }

      var targetCard = targetRow[targetRow.Count - 1];
      var suitsMatch = targetCard.Suit == card.Suit;
      var targetCardIsGreater = !CardTrumpsCard(card, targetCard);
      var playerPlayedCard =
        targetCard.PlayedBy == (playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest);

      return suitsMatch && targetCardIsGreater && !playerPlayedCard;
    }

    public static PlaceCardAttempt GetInitialPlaceCardAttempt(Move move, bool playerIsHost)
    {
      PlaceCardAttempt initialPlaceCardAttempt = move.PlaceCardAttempts[0];

      foreach (var placeCardAttempt in move.PlaceCardAttempts)
      {
        var isMoreInitial = playerIsHost
          ? placeCardAttempt.TargetRowIndex < initialPlaceCardAttempt.TargetRowIndex
          : placeCardAttempt.TargetRowIndex > initialPlaceCardAttempt.TargetRowIndex;

        if (isMoreInitial)
        {
          initialPlaceCardAttempt = placeCardAttempt;
        }
      }

      return initialPlaceCardAttempt;
    }

    public static bool CardTrumpsCard(Card offender, Card defender)
    {
      return SuitMatches(offender, defender)
        ? offender.Kind > defender.Kind
        : KindMatches(offender, defender);
    }

    public static bool PlayerHasAdvantage(Game game, Move move, bool playerIsHost)
    {
      var targetLaneIndex = move.PlaceCardAttempts[0].TargetLaneIndex;
      return game.Lanes[targetLaneIndex].LaneAdvantage
        == (playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest);
    }

    public static bool OpponentHasAdvantage(Game game, Move move, bool playerIsHost)
    {
      var targetLaneIndex = move.PlaceCardAttempts[0].TargetLaneIndex;
      return game.Lanes[targetLaneIndex].LaneAdvantage
        == (playerIsHost ? PlayerOrNone.Guest : PlayerOrNone.Host);
    }

    public static bool LaneHasNoAdvantage(Game game, Move move)
    {
      return game.Lanes[move.PlaceCardAttempts[0].TargetLaneIndex].LaneAdvantage
        == PlayerOrNone.None;
    }

    /// <summary>
    /// Returns true if the target lane has no last card played.
    /// </summary>
    public static bool SuitOrKindMatchesLastCardPlayed(Game game, Move move, bool playerIsHost)
    {
      var firstAttempt = GetInitialPlaceCardAttempt(move, playerIsHost);
      var lastCardPlayedInLane = game.Lanes[firstAttempt.TargetLaneIndex].LastCardPlayed;

      if (lastCardPlayedInLane is null)
      {
        return true;
      }

      return SuitMatches(firstAttempt.Card, lastCardPlayedInLane)
        || KindMatches(firstAttempt.Card, lastCardPlayedInLane);
    }

    static bool TriedToReinforceGreaterCard(Game game, Move move, bool playerIsHost)
    {
      var firstAttempt = GetInitialPlaceCardAttempt(move, playerIsHost);
      var targetRow = game.Lanes[firstAttempt.TargetLaneIndex].Rows[firstAttempt.TargetRowIndex];
      var targetCard = targetRow.LastOrDefault();

      if (targetCard is null)
      {
        return false;
      }

      var playerPlayedTargetCard = playerIsHost
        ? targetCard.PlayedBy == PlayerOrNone.Host
        : targetCard.PlayedBy == PlayerOrNone.Guest;

      return playerPlayedTargetCard
        && SuitMatches(targetCard, firstAttempt.Card)
        && CardTrumpsCard(targetCard, firstAttempt.Card);
    }

    public static bool HasThreeBackToBackPasses(Game game)
    {
      if (game.MovesMade.Count < 6)
      {
        return false;
      }

      for (var i = game.MovesMade.Count - 1; i >= game.MovesMade.Count - 6; i--)
      {
        var moveMade = game.MovesMade[i];
        if (!moveMade.PassedMove)
        {
          return false;
        }
      }

      return true;
    }

    public static bool MovesMatch(Move move1, Move move2)
    {
      if (move1.PlaceCardAttempts.Count != move2.PlaceCardAttempts.Count)
      {
        return false;
      }

      for (var i = 0; i < move1.PlaceCardAttempts.Count; i++)
      {
        var attempt1 = move1.PlaceCardAttempts[i];
        var attempt2 = move2.PlaceCardAttempts[i];

        if (attempt1.TargetLaneIndex != attempt2.TargetLaneIndex)
        {
          return false;
        }
        else if (attempt1.TargetRowIndex != attempt2.TargetRowIndex)
        {
          return false;
        }
        else if (!SuitAndKindMatches(attempt1.Card, attempt2.Card))
        {
          return false;
        }
      }

      return true;
    }

    public static string GetCardMovementNotation(PlaceCardAttempt placeCardAttempt)
    {
      var kindLetter = GetKindNotationLetter(placeCardAttempt.Card.Kind);
      var suitLetter = GetSuitNotationLetter(placeCardAttempt.Card.Suit);
      var laneLetter = GetLaneNotationLetter(placeCardAttempt.TargetLaneIndex);
      return $"{kindLetter}{suitLetter}{laneLetter}{placeCardAttempt.TargetRowIndex + 1}";
    }

    public static string GetKindNotationLetter(Kind kind)
    {
      return kind switch
      {
        Kind.Ace => "A",
        Kind.Two => "2",
        Kind.Three => "3",
        Kind.Four => "4",
        Kind.Five => "5",
        Kind.Six => "6",
        Kind.Seven => "7",
        Kind.Eight => "8",
        Kind.Nine => "9",
        Kind.Ten => "T",
        Kind.Jack => "J",
        Kind.Queen => "Q",
        Kind.King => "K",
        _ => "",
      };
    }

    public static string GetSuitNotationLetter(Suit suit)
    {
      return suit switch
      {
        Suit.Clubs => "♣",
        Suit.Diamonds => "♦",
        Suit.Hearts => "♥",
        Suit.Spades => "♠",
        _ => "",
      };
    }

    public static string GetLaneNotationLetter(int laneIndex)
    {
      return laneIndex switch
      {
        0 => "a",
        1 => "b",
        2 => "c",
        3 => "d",
        4 => "e",
        _ => "",
      };
    }

    public static List<List<CardMovement>> PlaceCardsAndApplyGameRules(
      Game game,
      List<PlaceCardAttempt> placeCardAttempts,
      bool playerIsHost
    )
    {
      return placeCardAttempts
        .SelectMany(placeCardAttempt =>
          PlaceCardAndApplyGameRules(game, placeCardAttempt, playerIsHost)
        )
        .ToList();
    }

    public static bool ContainsDifferentCards(List<Card> list1, List<Card> list2)
    {
      if (list1.Count != list2.Count)
      {
        return true;
      }

      for (var i = 0; i < list1.Count; i++)
      {
        var card1 = list1[i];
        var hasCard = false;

        for (var j = 0; j < list2.Count; j++)
        {
          var card2 = list2[j];

          var kindMatches = card1.Kind == card2.Kind;
          var suitMatches = card1.Suit == card2.Suit;

          if (kindMatches && suitMatches)
          {
            hasCard = true;
            break;
          }
        }

        if (!hasCard)
        {
          return true;
        }
      }
      return false;
    }

    public static List<List<CardMovement>> DrawCardsFromDeck(
      Game game,
      bool playerIsHost,
      int numCardsToDraw
    )
    {
      var cardMovements = new List<List<CardMovement>>();
      var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;

      for (int i = 0; i < numCardsToDraw; i++)
      {
        var cardFromDeck = player.Deck.DrawCard();

        if (cardFromDeck is null)
        {
          return cardMovements;
        }

        var index = player.Hand.Cards.Count;

        var from = new CardStore() { HostDeck = playerIsHost, GuestDeck = !playerIsHost };

        var to = new CardStore()
        {
          HostHandCardIndex = playerIsHost ? index : null,
          GuestHandCardIndex = playerIsHost ? null : index
        };

        var cardMovement = new CardMovement(from, to, cardFromDeck);
        var cardMovementList = new List<CardMovement>() { cardMovement };
        cardMovements.Add(cardMovementList);

        player.Hand.AddCard(cardFromDeck);
      }

      return cardMovements;
    }

    public static List<List<CardMovement>> DrawCardsUntil(
      Game game,
      bool playerIsHost,
      int maxNumCards
    )
    {
      var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
      var numCardsInPlayersHand = player.Hand.Cards.Count;
      var numCardsNeeded = maxNumCards - numCardsInPlayersHand;

      return numCardsNeeded > 0
        ? DrawCardsFromDeck(game, playerIsHost, numCardsNeeded)
        : new List<List<CardMovement>>();
    }

    public static List<List<CardMovement>> PlaceCardAndApplyGameRules(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var initialCardMovements = new List<CardMovement>
      {
        PlaceCard(game, placeCardAttempt, playerIsHost)
      };
      var cardMovements = new List<List<CardMovement>> { initialCardMovements };

      var aceRuleCardMovements = TriggerAceRuleIfAppropriate(game, placeCardAttempt, playerIsHost);
      if (aceRuleCardMovements.Any())
      {
        cardMovements.Add(aceRuleCardMovements);
        return cardMovements;
      }

      var capturedMiddleCardMovements = CaptureMiddleIfAppropriate(
        game,
        placeCardAttempt,
        playerIsHost
      );
      if (capturedMiddleCardMovements.Any())
      {
        cardMovements.AddRange(capturedMiddleCardMovements);
        return cardMovements;
      }

      var laneWonCardMovements = WinLaneAndOrGameIfAppropriate(
        game,
        placeCardAttempt,
        playerIsHost
      );
      if (laneWonCardMovements.Any())
      {
        cardMovements.Add(laneWonCardMovements);
      }

      return cardMovements;
    }

    public static CardMovement PlaceCard(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
      var currentPlayedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
      var targetRow = lane.Rows[placeCardAttempt.TargetRowIndex];
      var topCard = targetRow.LastOrDefault();
      var cardReinforced = topCard is not null && topCard.PlayedBy == currentPlayedBy;
      var mostOffensiveCard = GetMostOffensiveCard(lane, playerIsHost);
      var isCardMostOffensive =
        mostOffensiveCard is not null
        && topCard is not null
        && SuitAndKindMatches(mostOffensiveCard, topCard);

      lane.LastCardPlayed =
        !isCardMostOffensive && cardReinforced ? mostOffensiveCard : placeCardAttempt.Card;

      placeCardAttempt.Card.PlayedBy = currentPlayedBy;
      targetRow.Add(placeCardAttempt.Card);

      var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
      var indexInHand = RemoveCardWithMatchingKindAndSuit(player.Hand.Cards, placeCardAttempt.Card);

      if (indexInHand is null)
      {
        throw new Exception("Attempted to place a card that a player did not have.");
      }

      var from = new CardStore
      {
        HostHandCardIndex = playerIsHost ? indexInHand : null,
        GuestHandCardIndex = playerIsHost ? null : indexInHand
      };

      var to = new CardStore
      {
        CardPosition = new CardPosition(
          placeCardAttempt.TargetLaneIndex,
          placeCardAttempt.TargetRowIndex
        )
      };

      var notation = GetCardMovementNotation(placeCardAttempt);

      return new CardMovement(from, to, placeCardAttempt.Card, notation);
    }

    public static List<List<CardMovement>> CaptureMiddleIfAppropriate(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var cardIsLastOnPlayerSide = playerIsHost
        ? placeCardAttempt.TargetRowIndex == 2
        : placeCardAttempt.TargetRowIndex == 4;

      if (!cardIsLastOnPlayerSide)
      {
        return new List<List<CardMovement>>();
      }

      var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];

      var noAdvantage = lane.LaneAdvantage == PlayerOrNone.None;
      if (noAdvantage)
      {
        return new List<List<CardMovement>>
        {
          CaptureNoAdvantageLane(lane, placeCardAttempt, playerIsHost)
        };
      }

      var opponentAdvantage = playerIsHost
        ? lane.LaneAdvantage == PlayerOrNone.Guest
        : lane.LaneAdvantage == PlayerOrNone.Host;
      if (opponentAdvantage)
      {
        return CaptureOpponentAdvantageLane(game, placeCardAttempt, playerIsHost);
      }

      return new List<List<CardMovement>>();
    }

    public static List<CardMovement> CaptureNoAdvantageLane(
      Lane lane,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var advantagePlayer = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
      var laneCardsAndRowIndexes = GrabAllCardsFromLane(lane)
        .OrderBy(cardAndRowIndex => cardAndRowIndex.Item1.PlayedBy == advantagePlayer)
        .ThenBy(cardAndRowIndex => playerIsHost ? cardAndRowIndex.Item2 : -cardAndRowIndex.Item2)
        .ToList();

      var laneCards = laneCardsAndRowIndexes.Select(cardAndRowIndex => cardAndRowIndex.Item1);
      var middleRow = lane.Rows[3];
      middleRow.AddRange(laneCards);
      lane.LaneAdvantage = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;

      return GetCardMovementsGoingToTheMiddle(laneCardsAndRowIndexes, placeCardAttempt);
    }

    public static List<CardMovement> GetCardMovementsGoingToTheMiddle(
      List<(Card, int)> cardsAndRowIndexes,
      PlaceCardAttempt placeCardAttempt
    )
    {
      var cardMovements = new List<CardMovement>();

      foreach (var (card, rowIndex) in cardsAndRowIndexes)
      {
        var to = new CardStore
        {
          CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, 3)
        };

        var from = new CardStore
        {
          CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, rowIndex)
        };

        cardMovements.Add(new CardMovement(from, to, card));
      }

      return cardMovements;
    }

    public static List<List<CardMovement>> CaptureOpponentAdvantageLane(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
      var topCardsWithRowIndexes = GrabTopCardsOfFirstThreeRows(lane, playerIsHost);
      var topCards = topCardsWithRowIndexes
        .Select(cardsWithRowIndexes => cardsWithRowIndexes.Item1)
        .ToList();
      var remainingCardsInLaneWithRowIndexes = GrabAllCardsFromLane(lane);
      var remainingCardsInLane = remainingCardsInLaneWithRowIndexes.Select(x => x.Item1).ToList();

      var middleRow = lane.Rows[3];
      middleRow.AddRange(topCards);

      var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
      player.Deck.Cards.AddRange(remainingCardsInLane);
      player.Deck.Shuffle();
      lane.LaneAdvantage = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;

      var cardMovements = GetCardMovementsGoingToTheMiddle(
        topCardsWithRowIndexes,
        placeCardAttempt
      );
      cardMovements.AddRange(
        GetCapturedCardMovementsGoingToTheDeck(
          remainingCardsInLaneWithRowIndexes,
          placeCardAttempt,
          playerIsHost
        )
      );

      return new List<List<CardMovement>> { cardMovements };
    }

    public static List<CardMovement> GetCapturedCardMovementsGoingToTheDeck(
      List<(Card, int)> cardsWithRowIndexes,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var cardMovements = new List<CardMovement>();

      foreach (var (card, rowIndex) in cardsWithRowIndexes)
      {
        var from = new CardStore()
        {
          CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, rowIndex)
        };

        var to = new CardStore() { HostDeck = playerIsHost, GuestDeck = !playerIsHost };

        cardMovements.Add(new CardMovement(from, to, card));
      }

      return cardMovements;
    }

    /// <returns>Cards alongside their row indexes.</returns>
    public static List<(Card, int)> GrabTopCardsOfFirstThreeRows(Lane lane, bool playerIsHost)
    {
      List<(Card, int)> topCardsOfFirstThreeRows = new();

      int startRow = playerIsHost ? 0 : 6;
      int endRow = playerIsHost ? 3 : 4;
      int step = playerIsHost ? 1 : -1;

      for (int i = startRow; playerIsHost ? i < endRow : i >= endRow; i += step)
      {
        var row = lane.Rows[i];

        if (row.Count > 0)
        {
          var card = row.Last();
          row.RemoveAt(row.Count - 1);

          topCardsOfFirstThreeRows.Add((card, i));
        }
      }

      return topCardsOfFirstThreeRows;
    }

    public static List<CardMovement> TriggerAceRuleIfAppropriate(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var playerPlayedAnAce = placeCardAttempt.Card.Kind == Kind.Ace;
      if (!playerPlayedAnAce)
      {
        return new List<CardMovement>();
      }

      var laneIndex = placeCardAttempt.TargetLaneIndex;
      var lane = game.Lanes[laneIndex];
      var playerAceIsFacingOpponentAce = IsPlayerAceFacingOpponentAce(lane, playerIsHost);

      if (!playerAceIsFacingOpponentAce)
      {
        return new List<CardMovement>();
      }

      lane.LastCardPlayed = null;
      lane.LaneAdvantage = PlayerOrNone.None;
      var destroyedCardsAndRowIndexes = GrabAllCardsFromLane(lane);

      return GetCardMovementsFromDestroyedCards(destroyedCardsAndRowIndexes, laneIndex);
    }

    public static List<CardMovement> GetCardMovementsFromDestroyedCards(
      List<(Card, int)> destroyedCardsAndRowIndexes,
      int laneIndex
    )
    {
      var cardMovements = new List<CardMovement>();

      foreach (var (destroyedCard, rowIndex) in destroyedCardsAndRowIndexes)
      {
        var from = new CardStore { CardPosition = new CardPosition(laneIndex, rowIndex) };

        var to = new CardStore { Destroyed = true };

        cardMovements.Add(new CardMovement(from, to, destroyedCard));
      }

      return cardMovements;
    }

    public static List<CardMovement> WinLaneAndOrGameIfAppropriate(
      Game game,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var placeCardInLastRow = playerIsHost
        ? placeCardAttempt.TargetRowIndex == 6
        : placeCardAttempt.TargetRowIndex == 0;

      if (!placeCardInLastRow)
      {
        return new List<CardMovement>();
      }

      game.Lanes[placeCardAttempt.TargetLaneIndex].WonBy = playerIsHost
        ? PlayerOrNone.Host
        : PlayerOrNone.Guest;

      var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
      var allCardsInLaneWithRowIndexes = GrabAllCardsFromLane(lane);
      var allCardsInLane = allCardsInLaneWithRowIndexes
        .Select(cardWithRowIndex => cardWithRowIndex.Item1)
        .ToList();

      var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
      player.Deck.Cards.AddRange(allCardsInLane);
      player.Deck.Shuffle();

      if (game.RedJokerLaneIndex is null)
      {
        game.RedJokerLaneIndex = placeCardAttempt.TargetLaneIndex;
      }
      else
      {
        game.BlackJokerLaneIndex = placeCardAttempt.TargetLaneIndex;
      }

      WinGameIfAppropriate(game);

      return GetCardMovementsFromWonCards(
        allCardsInLaneWithRowIndexes,
        placeCardAttempt,
        playerIsHost
      );
    }

    public static List<CardMovement> GetCardMovementsFromWonCards(
      List<(Card, int)> cardsWithRowIndexes,
      PlaceCardAttempt placeCardAttempt,
      bool playerIsHost
    )
    {
      var cardMovements = new List<CardMovement>();

      foreach (var (card, rowIndex) in cardsWithRowIndexes)
      {
        var from = new CardStore()
        {
          CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, rowIndex)
        };

        var to = new CardStore() { HostDeck = playerIsHost, GuestDeck = !playerIsHost };

        cardMovements.Add(new CardMovement(from, to, card));
      }

      return cardMovements;
    }

    public static bool WinGameIfAppropriate(Game game)
    {
      var lanesWonByHost = game.Lanes.Where(lane => lane.WonBy == PlayerOrNone.Host);
      var hostWon = lanesWonByHost.Count() == 2;
      if (hostWon)
      {
        game.WonBy = PlayerOrNone.Host;
        game.HasEnded = true;
        return true;
      }

      var lanesWonByGuest = game.Lanes.Where(lane => lane.WonBy == PlayerOrNone.Guest);
      var guestWon = lanesWonByGuest.Count() == 2;
      if (guestWon)
      {
        game.WonBy = PlayerOrNone.Guest;
        game.HasEnded = true;
        return true;
      }

      return false;
    }

    public static bool IsPlayerAceFacingOpponentAce(Lane lane, bool playerIsHost)
    {
      var playersMostOffensiveCard = GetMostOffensiveCard(lane, playerIsHost);
      var opponentsMostOffensiveCard = GetMostOffensiveCard(lane, !playerIsHost);
      var cardsAreAces =
        playersMostOffensiveCard is not null
        && playersMostOffensiveCard.Kind == Kind.Ace
        && opponentsMostOffensiveCard is not null
        && opponentsMostOffensiveCard.Kind == Kind.Ace;

      return cardsAreAces || TopTwoCardsInLaneAreOpposingAces(lane);
    }

    public static bool TopTwoCardsInLaneAreOpposingAces(Lane lane)
    {
      foreach (var row in lane.Rows)
      {
        if (row.Count < 2)
        {
          continue;
        }

        var topCard = row[row.Count - 1];
        var secondTopCard = row[row.Count - 2];
        if (topCard is null || secondTopCard is null)
        {
          continue;
        }

        if (topCard.Kind == Kind.Ace && secondTopCard.Kind == Kind.Ace)
        {
          return true;
        }
      }

      return false;
    }

    public static Card? GetMostOffensiveCard(Lane lane, bool forHostPlayer)
    {
      var mostToLeastOffensive = forHostPlayer ? lane.Rows.Reverse() : lane.Rows;

      foreach (var row in mostToLeastOffensive)
      {
        if (row is null)
        {
          continue;
        }

        var topCard = row.LastOrDefault();
        if (topCard is null)
        {
          continue;
        }

        if (topCard.PlayedBy == (forHostPlayer ? PlayerOrNone.Host : PlayerOrNone.Guest))
        {
          return topCard;
        }
      }

      return null;
    }

    public static int GetMinutes(DurationOption durationOption)
    {
      return durationOption switch
      {
        DurationOption.FiveMinutes => 5,
        DurationOption.ThreeMinutes => 3,
        DurationOption.OneMinute => 1,
        _ => 0,
      };
    }

    public static int? RemoveCardWithMatchingKindAndSuit(List<Card> cardList, Card card)
    {
      for (int i = 0; i < cardList.Count; i++)
      {
        var cardFromList = cardList[i];
        bool sameSuit = cardFromList.Suit.Equals(card.Suit);
        bool sameKind = cardFromList.Kind.Equals(card.Kind);

        if (sameSuit && sameKind)
        {
          cardList.RemoveAt(i);
          return i;
        }
      }

      return null;
    }

    public static Lane[] CreateEmptyLanes()
    {
      Lane[] lanes = new Lane[NUMBER_OF_LANES];

      for (int i = 0; i < lanes.Length; i++)
      {
        lanes[i] = CreateEmptyLane();
      }

      return lanes;
    }

    /// <returns>Cards alongside their row indexes ordered from the bottom to the top of the row's stack of cards.</returns>
    public static List<(Card, int)> GrabAllCardsFromLane(Lane lane)
    {
      List<(Card, int)> cardsAndRowIndexes = new();

      for (var rowIndex = 0; rowIndex < lane.Rows.Length; rowIndex++)
      {
        var row = lane.Rows[rowIndex];

        foreach (var card in row)
        {
          cardsAndRowIndexes.Add((card, rowIndex));
        }
      }

      lane.Rows = CreateEmptyRows();

      return cardsAndRowIndexes;
    }

    public static Lane CreateEmptyLane()
    {
      var rows = CreateEmptyRows();
      Lane lane = new(rows);

      return lane;
    }

    public static List<Card>[] CreateEmptyRows()
    {
      List<Card>[] rows = new List<Card>[NUMBER_OF_ROWS];

      for (int i = 0; i < rows.Length; i++)
      {
        var row = new List<Card>();
        rows[i] = row;
      }

      return rows;
    }
  }
}
