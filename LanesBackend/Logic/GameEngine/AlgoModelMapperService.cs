using LanesBackend.Interfaces.GameEngine;
using LanesBackend.Models;
using LanesBackend.Models.GameEngine;

namespace LanesBackend.Logic.GameEngine
{
    public class AlgoModelMapperService : IAlgoModelMapperService
    {

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

            if (!playerIsHost)
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

        public Move FromAlgoMove(AlgoMove algoMove, bool playerIsHost)
        {
            List<PlaceCardAttempt> placeCardAttempts = new();

            foreach (var algoPlaceCardAttempt in algoMove.PlaceCardAttempts)
            {
                PlaceCardAttempt placeCardAttempt = FromAlgoPlaceCardAttempt(algoPlaceCardAttempt, playerIsHost);
                placeCardAttempts.Add(placeCardAttempt);
            }

            Move move = new(placeCardAttempts);

            return move;
        }

        public AlgoPlaceCardAttempt ToAlgoPlaceCardAttempt(PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            AlgoCard algoCard = ToAlgoCard(placeCardAttempt.Card);
            int algoTargetLaneIndex = placeCardAttempt.TargetLaneIndex;
            int algoTargetRowIndex = placeCardAttempt.TargetRowIndex;

            if (!playerIsHost)
            {
                algoTargetLaneIndex =  4 - algoTargetLaneIndex;
                algoTargetRowIndex = 6 - algoTargetRowIndex;
            }

            AlgoPlaceCardAttempt algoPlaceCardAttempt = new(algoCard, algoTargetLaneIndex, algoTargetRowIndex);

            return algoPlaceCardAttempt;
        }

        public PlaceCardAttempt FromAlgoPlaceCardAttempt(AlgoPlaceCardAttempt algoPlaceCardAttempt, bool playerIsHost)
        {
            Card card = FromAlgoCard(algoPlaceCardAttempt.Card, playerIsHost);
            int targetLaneIndex = algoPlaceCardAttempt.TargetLaneIndex;
            int targetRowIndex = algoPlaceCardAttempt.TargetRowIndex;

            if (!playerIsHost)
            {
                targetLaneIndex = Math.Abs(targetLaneIndex - 4);
                targetRowIndex = Math.Abs(targetRowIndex - 6);
            }

            PlaceCardAttempt placeCardAttempt = new(card, targetLaneIndex, targetRowIndex);

            return placeCardAttempt;
        }

        public AlgoCard ToAlgoCard(Card card)
        {
            AlgoCard algoCard = new(card.Kind, card.Suit);
            algoCard.PlayedBy = ToAlgoPlayer(card.PlayedBy);

            return algoCard;
        }

        public Card FromAlgoCard(AlgoCard algoCard, bool playerIsHost)
        {
            Card card = new(algoCard.Kind, algoCard.Suit);
            card.PlayedBy = FromAlgoPlayer(algoCard.PlayedBy, playerIsHost);

            return card;
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

        public PlayerOrNone FromAlgoPlayer(AlgoPlayer algoPlayer, bool playerIsHost)
        {
            if (algoPlayer == AlgoPlayer.Player)
            {
                return playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            }

            if (algoPlayer == AlgoPlayer.Opponent)
            {
                return playerIsHost ? PlayerOrNone.Guest : PlayerOrNone.Host;
            }

            return PlayerOrNone.None;
        }

        public Lane FromAlgoLane(AlgoLane algoLane, bool playerIsHost)
        {
            List<Card>[] rows = new List<Card>[algoLane.Rows.Length];

            for (int i = 0; i < algoLane.Rows.Length; i++)
            {
                var algoRow = algoLane.Rows[i];

                List<Card> cards = new();

                foreach (var algoCard in algoRow)
                {
                    Card card = FromAlgoCard(algoCard, playerIsHost);
                    cards.Add(card);
                }

                rows[i] = cards;
            }

            if (!playerIsHost)
            {
                var reverseRowsList = rows.ToList();
                reverseRowsList.Reverse();
                var reverseRows = reverseRowsList.ToArray();

                rows = reverseRows;
            }

            Card? lastCardPlayed = algoLane.LastCardPlayed is null ?
                null :
                FromAlgoCard(algoLane.LastCardPlayed,playerIsHost);

            Lane lane = new(rows);
            lane.LastCardPlayed = lastCardPlayed;
            lane.LaneAdvantage = FromAlgoPlayer(algoLane.LaneAdvantage, playerIsHost);
            lane.WonBy = FromAlgoPlayer(algoLane.WonBy, playerIsHost);

            return lane;
        }
    }
}
