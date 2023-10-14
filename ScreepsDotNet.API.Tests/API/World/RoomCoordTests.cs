using System.Collections.Generic;

namespace ScreepsDotNet.API.World.Tests
{
    public class RoomCoordTests
    {
        public static IEnumerable<object[]> SubtractOperatorTheoryData
           => new object[][]
           {
                new object[] { new RoomCoord(5, 6), new RoomCoord(5, 5), ExitDirection.Top },
                new object[] { new RoomCoord(6, 5), new RoomCoord(5, 5), ExitDirection.Right },
                new object[] { new RoomCoord(5, 4), new RoomCoord(5, 5), ExitDirection.Bottom },
                new object[] { new RoomCoord(4, 5), new RoomCoord(5, 5), ExitDirection.Left },
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
                new object[] { new RoomCoord(1, 2), Direction.Top, new RoomCoord(1, 3) },
                new object[] { new RoomCoord(1, 2), Direction.Right, new RoomCoord(2, 2) },
                new object[] { new RoomCoord(1, 2), Direction.Bottom, new RoomCoord(1, 1) },
                new object[] { new RoomCoord(1, 2), Direction.Left, new RoomCoord(0, 2) },
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
                new object[] { new RoomCoord(1, 2), Direction.Top, new RoomCoord(1, 1) },
                new object[] { new RoomCoord(1, 2), Direction.Right, new RoomCoord(0, 2) },
                new object[] { new RoomCoord(1, 2), Direction.Bottom, new RoomCoord(1, 3) },
                new object[] { new RoomCoord(1, 2), Direction.Left, new RoomCoord(2, 2) },
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
