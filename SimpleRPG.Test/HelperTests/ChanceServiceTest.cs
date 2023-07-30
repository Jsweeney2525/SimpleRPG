using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Helpers;

namespace SimpleRPG.Test.HelperTests
{
    [TestFixture]
    class ChanceServiceTest
    {
        ChanceService _chanceService;

        [OneTimeSetUp]
        public void SetUp()
        {
            _chanceService = new ChanceService();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _chanceService = null;
        }

        [TestCase]
        public void TestEventOccurs()
        {
            decimal totalOccurances = 0;

            for (var i = 0; i < 10000; ++i)
            {
                if (_chanceService.EventOccurs(0.2)) {
                    totalOccurances += 1;
                }
            }

            totalOccurances /= 10000;

            Assert.LessOrEqual(.18, totalOccurances);
            Assert.GreaterOrEqual(.22, totalOccurances);
        }

        [TestCase]
        public void TestWhichEventOccurs_AtLeastOneEventOccurs()
        {
            double[] chanceEvents = { .1, .3, .6 };
            double[] occurances = { 0, 0, 0};

            for (var i = 0; i < 10000; ++i)
            {
                var eventIndex = _chanceService.WhichEventOccurs(chanceEvents);
                occurances[eventIndex] += 1;
            }

            for (var i = 0; i <3; ++i)
            {
                occurances[i] = occurances[i] / 10000;
            }

            Assert.LessOrEqual(.08, occurances[0]);
            Assert.GreaterOrEqual(.12, occurances[0]);

            Assert.LessOrEqual(.28, occurances[1]);
            Assert.GreaterOrEqual(.32, occurances[1]);

            Assert.LessOrEqual(.58, occurances[2]);
            Assert.GreaterOrEqual(.62, occurances[2]);
        }

        [Test]
        public void TestWhichEventOccurs_IntInput([Values(5, 10, 100)] int numEvents)
        {
            for (var i = 0; i < 10000; ++i)
            {
                var index = _chanceService.WhichEventOccurs(numEvents);
                Assert.Less(index, numEvents);
                Assert.GreaterOrEqual(index, 0);
            }
        }

        [TestCase]
        public void TestWhichEventOccurs_SometimesNoEventOccurs()
        {
            double[] chanceEvents = { .1, .1 };
            double[] occurances = { 0, 0 };
            double noOccurance = 0;

            for (var i = 0; i < 10000; ++i)
            {
                var eventIndex = _chanceService.WhichEventOccurs(chanceEvents);
                if (eventIndex == -1)
                {
                    noOccurance += 1;
                }
                else
                {
                    occurances[eventIndex] += 1;
                }
            }

            for (var i = 0; i < 2; ++i)
            {
                occurances[i] /= 10000;
            }
            noOccurance /= 10000;

            Assert.LessOrEqual(.08, occurances[0]);
            Assert.GreaterOrEqual(.12, occurances[0]);

            Assert.LessOrEqual(.08, occurances[1]);
            Assert.GreaterOrEqual(.12, occurances[1]);

            Assert.LessOrEqual(.78, noOccurance);
        }

        [Test]
        public void ShuffleMethod([Values(50, 100, 500)] int numberItems)
        {
            List<int> preShuffledNumbers = new List<int>();

            for(var i = 0; i < numberItems; ++i) { preShuffledNumbers.Add(i); }

            List<int> shuffledNumbers = _chanceService.Shuffle(preShuffledNumbers).ToList();

            Assert.AreEqual(preShuffledNumbers.Count, shuffledNumbers.Count);
            Assert.AreEqual(numberItems, shuffledNumbers.Count);

            //Assert original list unaffected
            for (var i = 0; i < numberItems; ++i) { Assert.AreEqual(i, preShuffledNumbers[i]); }

            int numberDifferent = 0;
            int index = 0;

            preShuffledNumbers.ForEach(number =>
            {
                //make sure no elements were duplicated/lost during the shuffle
                Assert.Contains(number, shuffledNumbers);

                if (index != shuffledNumbers.IndexOf(number))
                {
                    ++numberDifferent;
                }

                ++index;
            });

            Assert.True(numberDifferent > 0, "chances are at least one number is not in the same index as the other.");
        }
    }
}
