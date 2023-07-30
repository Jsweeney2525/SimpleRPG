using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Screens;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class CutsceneTests
    {
        private MockInput _input;
        private MockOutput _output;

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();
        }

        [TearDown]
        public void TearDown()
        {
            _input = null;
            _output = null;
        }

        [Test]
        public void CorrectlyPrintsCurscenes()
        {
            //Arrange
            ColorString[] firstSceneLines =
            {
                new ColorString("There once was a man named Gold Roger"),
                new ColorString("He had fame, wealth, and power beyond your wildest dreams"),
                new ColorString("Before they hung him from the gallows,"),
                new ColorString("These were the words he said:")
            };

            SingleScene firstScene = new SingleScene(firstSceneLines);

            ColorString[] secondSceneLines =
            {
                new ColorString("If you want my treasure, you can have it!"),
                new ColorString("I left everything I had in that place")
            };

            SingleScene secondScene = new SingleScene(secondSceneLines);

            SingleScene[] scenes =
            {
                firstScene,
                secondScene
            };

            Cutscene cutscene = new Cutscene(scenes);

            //Act
            cutscene.ExecuteCutscene(_input, _output, (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1), (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1));

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();
            
            int expectedNumberOfLines = firstSceneLines.Length + secondSceneLines.Length;

            Assert.AreEqual(expectedNumberOfLines, outputs.Length);

            int[] clearIndices = _output.GetClearIndices();
            Assert.AreEqual(scenes.Length, clearIndices.Length);

            int nextClearIndex = firstSceneLines.Length;
            Assert.AreEqual(nextClearIndex, clearIndices[0]);

            nextClearIndex += secondSceneLines.Length;
            Assert.AreEqual(nextClearIndex, clearIndices[1]);
        }
    }
}
