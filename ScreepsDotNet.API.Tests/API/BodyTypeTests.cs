using System.Collections.Generic;

namespace ScreepsDotNet.API.Tests
{
    public class BodyTypeTests
    {
        [Fact]
        public void ConstructFromList()
        {
            var bodyType = new BodyType(new BodyPartType[] { BodyPartType.Move, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Attack });
            Assert.Equal(2, bodyType[BodyPartType.Move]);
            Assert.Equal(1, bodyType[BodyPartType.Carry]);
            Assert.Equal(1, bodyType[BodyPartType.Attack]);
            Assert.Equal(0, bodyType[BodyPartType.RangedAttack]);
        }

        [Fact]
        public void ConstructFromTuples()
        {
            var bodyType = new BodyType(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 2), (BodyPartType.Carry, 1), (BodyPartType.Attack, 1) });
            Assert.Equal(2, bodyType[BodyPartType.Move]);
            Assert.Equal(1, bodyType[BodyPartType.Carry]);
            Assert.Equal(1, bodyType[BodyPartType.Attack]);
            Assert.Equal(0, bodyType[BodyPartType.RangedAttack]);
        }

        [Fact]
        public void OperatorEquals()
        {
            var bodyTypeA = new BodyType(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 2), (BodyPartType.Carry, 1), (BodyPartType.Attack, 1) });
            var bodyTypeB = new BodyType(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 2), (BodyPartType.Carry, 1), (BodyPartType.Attack, 1), (BodyPartType.RangedAttack, 1) });
            var bodyTypeC = new BodyType(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 2), (BodyPartType.Carry, 1) });
            var bodyTypeD = new BodyType(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 2), (BodyPartType.Carry, 2), (BodyPartType.Attack, 1) });
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(bodyTypeA == bodyTypeA);
            Assert.True(bodyTypeB == bodyTypeB);
            Assert.True(bodyTypeC == bodyTypeC);
            Assert.True(bodyTypeD == bodyTypeD);
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.True(bodyTypeA != bodyTypeB);
            Assert.True(bodyTypeA != bodyTypeC);
            Assert.True(bodyTypeA != bodyTypeD);
            Assert.True(bodyTypeB != bodyTypeA);
            Assert.True(bodyTypeB != bodyTypeC);
            Assert.True(bodyTypeB != bodyTypeD);
            Assert.True(bodyTypeC != bodyTypeA);
            Assert.True(bodyTypeC != bodyTypeB);
            Assert.True(bodyTypeC != bodyTypeD);
            Assert.True(bodyTypeD != bodyTypeA);
            Assert.True(bodyTypeD != bodyTypeB);
            Assert.True(bodyTypeD != bodyTypeC);
        }
    }
}
