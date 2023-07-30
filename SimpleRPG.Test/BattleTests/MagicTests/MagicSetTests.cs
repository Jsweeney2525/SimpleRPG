using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.BattleTests.MagicTests
{
    [TestFixture]
    public class MagicSetTests
    {
        [Test]
        public void MagicSetGetsAppropriateDefaults_IntType()
        {
            var magicSet = new MagicSet<int>();

            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();

            foreach (MagicType magicType in magicTypes)
            {
                Assert.AreEqual(0, magicSet[magicType]);
            }
        }

        [Test]
        public void MagicSetGetsAppropriateDefaults_MagicRelationshipType()
        {
            var magicSet = new MagicSet<MagicRelationshipType>();

            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();

            foreach (MagicType magicType in magicTypes)
            {
                Assert.AreEqual(MagicRelationshipType.None, magicSet[magicType]);
            }
        }
    }
}
