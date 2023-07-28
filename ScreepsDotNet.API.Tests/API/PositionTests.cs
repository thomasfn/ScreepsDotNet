using System.Collections.Generic;

namespace ScreepsDotNet.API.Tests
{
    public class PositionTests
    {
        [Theory]
        [MemberData(nameof(SubtractOperatorTheoryData))]
        public void SubtractOperatorTheory(Position lhs, Position rhs, Direction expectedDirection)
        {
            var observedDirection = lhs - rhs;
            Assert.Equal(expectedDirection, observedDirection);
        }

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
    }
}
