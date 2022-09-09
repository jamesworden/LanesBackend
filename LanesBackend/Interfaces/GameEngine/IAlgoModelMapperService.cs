using LanesBackend.Models;
using LanesBackend.Models.GameEngine;

namespace LanesBackend.Interfaces.GameEngine
{
    public interface IAlgoModelMapperService
    {
        public AlgoLane ToAlgoLane(Lane lane, bool playerIsHost);

        public Lane FromAlgoLane(AlgoLane algoLane, bool playerIsHost);

        public AlgoMove ToAlgoMove(Move move, bool playerIsHost);

        public Move FromAlgoMove(AlgoMove algoMove, bool playerIsHost);

        public AlgoPlaceCardAttempt ToAlgoPlaceCardAttempt(PlaceCardAttempt placeCardAttempt, bool playerIsHost);

        public PlaceCardAttempt FromAlgoPlaceCardAttempt(AlgoPlaceCardAttempt algoPlaceCardAttempt, bool playerIsHost);

        public AlgoCard ToAlgoCard(Card card);

        public Card FromAlgoCard(AlgoCard algoCard, bool playerIsHost);

        public AlgoPlayer ToAlgoPlayer(PlayerOrNone playerOrNone);

        public PlayerOrNone FromAlgoPlayer(AlgoPlayer algoPlayer, bool playerIsHost);
    }
}
