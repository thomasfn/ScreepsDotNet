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
                new object[] { new RoomPosition((1, 2), "sim"), 4227073 },
                new object[] { new RoomPosition((40, 2), "sim"), 5505025 },
                new object[] { new RoomPosition((40, 42), "sim"), 89391105 },
                new object[] { new RoomPosition((1, 42), "sim"), 88113153 },

                new object[] { new RoomPosition((1, 2), "E3N5"), 4244870 },
                new object[] { new RoomPosition((40, 2), "E3N5"), 5522822 },
                new object[] { new RoomPosition((40, 42), "E3N5"), 89408902 },
                new object[] { new RoomPosition((1, 42), "E3N5"), 88130950 },

                new object[] { new RoomPosition((1, 2), "E3N4"), 4244614 },
                new object[] { new RoomPosition((1, 2), "E3N45"), 4255110 },
                new object[] { new RoomPosition((1, 2), "E34N5"), 4244932 },
                new object[] { new RoomPosition((1, 2), "E34N56"), 4257988 },

                new object[] { new RoomPosition((1, 2), "E3S4"), 4242310 },
                new object[] { new RoomPosition((1, 2), "E3S45"), 4231814 },
                new object[] { new RoomPosition((1, 2), "E34S5"), 4242116 },
                new object[] { new RoomPosition((1, 2), "E34S56"), 4229060 },

                new object[] { new RoomPosition((1, 2), "W3N4"), 4244600 },
                new object[] { new RoomPosition((1, 2), "W3N45"), 4255096 },
                new object[] { new RoomPosition((1, 2), "W34N5"), 4244794 },
                new object[] { new RoomPosition((1, 2), "W34N56"), 4257850 },

                new object[] { new RoomPosition((1, 2), "W3S4"), 4242296 },
                new object[] { new RoomPosition((1, 2), "W3S45"), 4231800 },
                new object[] { new RoomPosition((1, 2), "W34S5"), 4241978 },
                new object[] { new RoomPosition((1, 2), "W34S56"), 4228922 },

            };
    }
}
