using System.Collections.Generic;

namespace ScreepsDotNet.API.World.Tests
{
    public class RoomCoordTests
    {
        public static IEnumerable<object[]> ParseTheoryData
           => new object[][]
           {
                ["sim", new RoomCoord(-128, -128)],

                ["E0S0", new RoomCoord(0, 0)],
                ["E5S7", new RoomCoord(5, 7)],
                ["E15S7", new RoomCoord(15, 7)],
                ["E5S17", new RoomCoord(5, 17)],
                ["E15S17", new RoomCoord(15, 17)],

                ["W0S0", new RoomCoord(-1, 0)],
                ["W5S7", new RoomCoord(-6, 7)],
                ["W15S7", new RoomCoord(-16, 7)],
                ["W5S17", new RoomCoord(-6, 17)],
                ["W15S17", new RoomCoord(-16, 17)],

                ["E0N0", new RoomCoord(0, -1)],
                ["E5N7", new RoomCoord(5, -8)],
                ["E15N7", new RoomCoord(15, -8)],
                ["E5N17", new RoomCoord(5, -18)],
                ["E15N17", new RoomCoord(15, -18)],

                ["W0N0", new RoomCoord(-1, -1)],
                ["W5N7", new RoomCoord(-6, -8)],
                ["W15N7", new RoomCoord(-16, -8)],
                ["W5N17", new RoomCoord(-6, -18)],
                ["W15N17", new RoomCoord(-16, -18)],
           };

        [Theory]
        [MemberData(nameof(ParseTheoryData))]
        public void ParseTheory(string str, RoomCoord coord)
        {
            Assert.Equal(coord, new RoomCoord(str));
        }

        [Theory]
        [MemberData(nameof(ParseTheoryData))]
        public void ToStringTheory(string str, RoomCoord coord)
        {
            Assert.Equal(str, coord.ToString());
        }

        public static IEnumerable<object[]> SubtractOperatorTheoryData
           => new object[][]
           {
                [new RoomCoord(5, 4), new RoomCoord(5, 5), ExitDirection.Top],
                [new RoomCoord(6, 5), new RoomCoord(5, 5), ExitDirection.Right],
                [new RoomCoord(5, 6), new RoomCoord(5, 5), ExitDirection.Bottom],
                [new RoomCoord(4, 5), new RoomCoord(5, 5), ExitDirection.Left],
           };

        [Theory]
        [MemberData(nameof(SubtractOperatorTheoryData))]
        public void SubtractOperatorTheory(RoomCoord lhs, RoomCoord rhs, ExitDirection expectedDirection)
        {
            var observedDirection = lhs - rhs;
            Assert.Equal(expectedDirection, observedDirection);
        }

        public static IEnumerable<object[]> AddOperatorByDirectionTheoryData
            => new object[][]
            {
                [new RoomCoord(1, 2), Direction.Top, new RoomCoord(1, 1)],
                [new RoomCoord(1, 2), Direction.Right, new RoomCoord(2, 2)],
                [new RoomCoord(1, 2), Direction.Bottom, new RoomCoord(1, 3)],
                [new RoomCoord(1, 2), Direction.Left, new RoomCoord(0, 2)],
            };

        [Theory]
        [MemberData(nameof(AddOperatorByDirectionTheoryData))]
        public void AddOperatorByDirectionTheory(RoomCoord lhs, ExitDirection rhs, RoomCoord expectedPosition)
        {
            var observedPosition = lhs + rhs;
            Assert.Equal(expectedPosition, observedPosition);
        }

        public static IEnumerable<object[]> SubtractOperatorByDirectionTheoryData
            => new object[][]
            {
                [new RoomCoord(1, 2), Direction.Top, new RoomCoord(1, 3)],
                [new RoomCoord(1, 2), Direction.Right, new RoomCoord(0, 2)],
                [new RoomCoord(1, 2), Direction.Bottom, new RoomCoord(1, 1)],
                [new RoomCoord(1, 2), Direction.Left, new RoomCoord(2, 2)],
            };

        [Theory]
        [MemberData(nameof(SubtractOperatorByDirectionTheoryData))]
        public void SubtractOperatorByDirectionTheory(RoomCoord lhs, ExitDirection rhs, RoomCoord expectedPosition)
        {
            var observedPosition = lhs - rhs;
            Assert.Equal(expectedPosition, observedPosition);
        }
    }
}
