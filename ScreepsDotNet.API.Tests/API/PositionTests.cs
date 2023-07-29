using System.Collections.Generic;

namespace ScreepsDotNet.API.Tests
{
    public class PositionTests
    {
        public static IEnumerable<object[]> SubtractOperatorTheoryData
           => new object[][]
           {
                new object[] { new Position(5, 4), new Position(5, 5), Direction.Top },
                new object[] { new Position(6, 4), new Position(5, 5), Direction.TopRight },
                new object[] { new Position(6, 5), new Position(5, 5), Direction.Right },
                new object[] { new Position(6, 6), new Position(5, 5), Direction.BottomRight },
                new object[] { new Position(5, 6), new Position(5, 5), Direction.Bottom },
                new object[] { new Position(4, 6), new Position(5, 5), Direction.BottomLeft },
                new object[] { new Position(4, 5), new Position(5, 5), Direction.Left },
                new object[] { new Position(4, 4), new Position(5, 5), Direction.TopLeft },
           };

        [Theory]
        [MemberData(nameof(SubtractOperatorTheoryData))]
        public void SubtractOperatorTheory(Position lhs, Position rhs, Direction expectedDirection)
        {
            var observedDirection = lhs - rhs;
            Assert.Equal(expectedDirection, observedDirection);
        }

        public static IEnumerable<object[]> AddOperatorByDirectionTheoryData
            => new object[][]
            {
                new object[] { new Position(1, 2), Direction.Top, new Position(1, 1) },
                new object[] { new Position(1, 2), Direction.TopRight, new Position(2, 1) },
                new object[] { new Position(1, 2), Direction.Right, new Position(2, 2) },
                new object[] { new Position(1, 2), Direction.BottomRight, new Position(2, 3) },
                new object[] { new Position(1, 2), Direction.Bottom, new Position(1, 3) },
                new object[] { new Position(1, 2), Direction.BottomLeft, new Position(0, 3) },
                new object[] { new Position(1, 2), Direction.Left, new Position(0, 2) },
                new object[] { new Position(1, 2), Direction.TopLeft, new Position(0, 1) },
            };

        [Theory]
        [MemberData(nameof(AddOperatorByDirectionTheoryData))]
        public void AddOperatorByDirectionTheory(Position lhs, Direction rhs, Position expectedPosition)
        {
            var observedPosition = lhs + rhs;
            Assert.Equal(expectedPosition, observedPosition);
        }
    }
}
