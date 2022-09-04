using LanesBackend.Models;
using LanesBackend.Utils;
using Xunit;

namespace UnitTest
{
    public class ModifyLaneFromHostPov
    {
        [Fact]
        public void Should_Not_Change_Whether_Player_Is_Truely_Host()
        {
            bool playerIsHost = false;
            LaneUtils.ModifyLaneFromHostPov(new Lane(), playerIsHost, (hostPovLane) => { });
            Assert.False(playerIsHost);

            playerIsHost = true;
            LaneUtils.ModifyLaneFromHostPov(new Lane(), playerIsHost, (hostPovLane) => { });
            Assert.True(playerIsHost);
        }

        [Fact]
        public void Should_Reverse_Rows_In_Lane_When_Modifying_The_Lane_And_Reverse_Back_When_Done()
        {
            Lane lane = new();
            bool playerIsHost = false;
            Card card = new(Kind.Ace, Suit.Spades);

            LaneUtils.ModifyLaneFromHostPov(lane, playerIsHost, (hostPovLane) =>
            {
                hostPovLane.Rows[0].Add(card);
                var cardIsInFirstRow = hostPovLane.Rows[0].Contains(card);
                var lastRowIsEmpty = hostPovLane.Rows[6].Count == 0;

                Assert.True(cardIsInFirstRow && lastRowIsEmpty);
            });

            var cardIsInLastRow = lane.Rows[6].Contains(card);
            var firstRowIsEmpty = lane.Rows[0].Count == 0;
            Assert.True(cardIsInLastRow && firstRowIsEmpty);
        }
    }
}