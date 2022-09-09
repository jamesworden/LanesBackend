using LanesBackend.Interfaces.GameEngine;
using LanesBackend.Models;
using LanesBackend.Models.GameEngine;

namespace LanesBackend.Logic.GameEngine
{
    public class AlgoModelMapperService : IAlgoModelMapperService
    {
        private readonly int MAX_ROW_INDEX = 6; // Replace with number of rows constant - 1;

        public AlgoLane ToAlgoLane(Lane lane, bool playerIsHost)
        {
            List<AlgoCard>[] algoRows = new List<AlgoCard>[lane.Rows.Length];

            for (int i = 0; i < lane.Rows.Length; i++)
            {
                var row = lane.Rows[i];

                List<AlgoCard> algoCards = new();

                foreach (var card in row)
                {
                    AlgoCard algoCard = ToAlgoCard(card);
                    algoCards.Add(algoCard);
                }

                algoRows[i] = algoCards;
            }

            if (playerIsHost)
            {
                var reverseAlgoRowsList = algoRows.ToList();
                reverseAlgoRowsList.Reverse();
                var reverseAlgoRows = reverseAlgoRowsList.ToArray();

                algoRows = reverseAlgoRows;
            }

            AlgoCard? lastCardPlayed = lane.LastCardPlayed is null ? 
                null : 
                ToAlgoCard(lane.LastCardPlayed);

            AlgoLane algoLane = new(algoRows);
            algoLane.LastCardPlayed = lastCardPlayed;
            algoLane.LaneAdvantage = ToAlgoPlayer(lane.LaneAdvantage);
            algoLane.WonBy = ToAlgoPlayer(lane.WonBy);

            return algoLane;
        }

        public AlgoMove ToAlgoMove(Move move, bool playerIsHost)
        {
            List<AlgoPlaceCardAttempt> algoPlaceCardAttempts = new();

            foreach(var placeCardAttempt in move.PlaceCardAttempts)
            {
                AlgoPlaceCardAttempt algoPlaceCardAttempt = ToAlgoPlaceCardAttempt(placeCardAttempt, playerIsHost);
                algoPlaceCardAttempts.Add(algoPlaceCardAttempt);
            }

            AlgoMove algoMove = new(algoPlaceCardAttempts);

            return algoMove;
        }

        public AlgoPlaceCardAttempt ToAlgoPlaceCardAttempt(PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            AlgoCard algoCard = ToAlgoCard(placeCardAttempt.Card);

            int targetRowIndex = playerIsHost ? 
                placeCardAttempt.TargetRowIndex : 
                MAX_ROW_INDEX - placeCardAttempt.TargetRowIndex;

            AlgoPlaceCardAttempt algoPlaceCardAttempt = new(algoCard, placeCardAttempt.TargetLaneIndex, targetRowIndex);

            return algoPlaceCardAttempt;
        }

        public AlgoCard ToAlgoCard(Card card)
        {
            AlgoCard algoCard = new(card.Kind, card.Suit);
            algoCard.PlayedBy = ToAlgoPlayer(card.PlayedBy);

            return algoCard;
        }

        public AlgoPlayer ToAlgoPlayer(PlayerOrNone playerOrNone)
        {            
            if (playerOrNone == PlayerOrNone.Host)
            {
                return AlgoPlayer.Player;
            }

            if (playerOrNone == PlayerOrNone.Guest)
            {
                return AlgoPlayer.Opponent;
            }

            return AlgoPlayer.None;
        }
    }
}
