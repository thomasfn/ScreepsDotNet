using System.Collections.Generic;
using System.Text;

namespace ScreepsDotNet.API.World.Tests
{
    public class ObjectIdTests
    {

        [Theory]
        [MemberData(nameof(ToStringTheoryData))]
        public void ToStringTheory(string idStr)
        {
            var objId = new ObjectId(idStr);
            Assert.Equal(idStr, objId.ToString());
        }

        [Theory]
        [MemberData(nameof(ToStringTheoryData))]
        public void ParseFromBytesTheory(string idStr)
        {
            var bytes = Encoding.ASCII.GetBytes(idStr);
            var objId = new ObjectId(bytes);
            Assert.Equal(idStr, objId.ToString());
        }

        public static IEnumerable<object[]> ToStringTheoryData
            => new object[][]
            {
                new object[] { "64d06a2a1bc4141ed12a8609" },
                new object[] { "64e21b4945d7ba3cda2248bd" },
                new object[] { "64e77153329476abacf99c75" },
                new object[] { "5bbcafda9099fc012e63b498" },

            };
    }
}
