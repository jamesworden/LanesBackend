﻿using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameService
    {
        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode);

        public bool MakeMoveIfValid(Game game, Move move, bool playerIsHost);

        public void RemoveCardsFromHand(Game game, bool playerIsHost, Move move);

        public void DrawCardFromDeck(Game game, bool playerIsHost);
    }
}
