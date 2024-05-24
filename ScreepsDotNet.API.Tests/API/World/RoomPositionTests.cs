using System.Collections.Generic;

namespace ScreepsDotNet.API.World.Tests
{
    public class RoomPositionTests
    {

        [Theory]
        [MemberData(nameof(EncodeToIntTheoryData))]
        public void EncodeToIntTheory(RoomPosition roomPosition, int expectedValue)
        {
            Assert.Equal(expectedValue, roomPosition.ToEncodedInt());
        }

        [Theory]
        [MemberData(nameof(EncodeToIntTheoryData))]
        public void FromEncodedIntTheory(RoomPosition expectedValue, int encodedInt)
        {
            Assert.Equal(expectedValue, RoomPosition.FromEncodedInt(encodedInt));
        }

        public static IEnumerable<object[]> EncodeToIntTheoryData
            => new object[][]
            {
                [new RoomPosition((1, 2), "sim"), 258],
                [new RoomPosition((40, 2), "sim"), 10242],
                [new RoomPosition((40, 42), "sim"), 10282],
                [new RoomPosition((1, 42), "sim"), 298],

                [new RoomPosition((1, 2), "E3N5"), -2089156350],
                [new RoomPosition((40, 2), "E3N5"), -2089146366],
                [new RoomPosition((40, 42), "E3N5"), -2089146326],
                [new RoomPosition((1, 42), "E3N5"), -2089156310],

                [new RoomPosition((1, 2), "E3N4"), -2089090814],
                [new RoomPosition((1, 2), "E3N45"), -2091777790],
                [new RoomPosition((1, 2), "E34N5"), -1569062654],
                [new RoomPosition((1, 2), "E34N56"), -1572404990],

                [new RoomPosition((1, 2), "E3S4"), -2088500990],
                [new RoomPosition((1, 2), "E3S45"), -2085814014],
                [new RoomPosition((1, 2), "E34S5"), -1568341758],
                [new RoomPosition((1, 2), "E34S56"), -1564999422],

                [new RoomPosition((1, 2), "W3N4"), 2088435970],
                [new RoomPosition((1, 2), "W3N45"), 2085748994],
                [new RoomPosition((1, 2), "W34N5"), 1568276738],
                [new RoomPosition((1, 2), "W34N56"), 1564934402],

                [new RoomPosition((1, 2), "W3S4"), 2089025794],
                [new RoomPosition((1, 2), "W3S45"), 2091712770],
                [new RoomPosition((1, 2), "W34S5"), 1568997634],
                [new RoomPosition((1, 2), "W34S56"), 1572339970],

            };
    }
}
