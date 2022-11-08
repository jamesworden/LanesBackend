﻿using LanesBackend.Interfaces;

namespace LanesBackend.Logic
{
    public class GameCodeService : IGameCodeService
    {
        private readonly IPendingGameCache PendingGameCache;

        private static readonly Random Random = new();

        private static readonly string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public GameCodeService(IPendingGameCache pendingGameCache)
        {
            PendingGameCache = pendingGameCache;
        }

        public string GenerateUniqueGameCode()
        {
            var numRetries = 10;
            var currentRetry = 0;

            while (currentRetry < numRetries)
            {
                var gameCode = GenerateRandomLetterString(4).ToUpper();
                var gameCodeIsUnused = PendingGameCache.GetPendingGameByGameCode(gameCode) is null;

                if (gameCodeIsUnused)
                {
                    return gameCode;
                }
                else
                {
                    currentRetry++;
                }
            }

            throw new Exception("Unable to generate an unique game code.");
        }

        private static string GenerateRandomLetterString(int length)
        {
            var chars = Enumerable.Repeat(Alphabet, length).Select(s => s[Random.Next(s.Length)]);
            var charArray = chars.ToArray();
            return new string(charArray);
        }
    }
}
