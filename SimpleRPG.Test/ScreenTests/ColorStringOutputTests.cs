using System;
using System.Runtime.ExceptionServices;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleRPG.Screens;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class ColorStringOutputTests
    {
        private ConsoleOutput _output;
        private MockOutput _mockOutput;
        private TextMessageWriter _textWriter;

        [SetUp]
        public void SetUp()
        {
            _output = new ConsoleOutput();
            _mockOutput = new MockOutput();
            _textWriter = new TextMessageWriter();
            Console.SetOut(_textWriter);
        }

        [TearDown]
        public void TearDown()
        {
            _output = null;
            _mockOutput = null;
            _textWriter.Flush();
            _textWriter = null;
        }

        [Test]
        public void CorrectlyOutputsColorString_NoSubStrings()
        {
            const string expectedOutput = "Hello World!";
            ColorString colorString = new ColorString(expectedOutput, ConsoleColor.Red);

            _output.WriteLine(colorString);

            StringBuilder outputStringBuilder = _textWriter.GetStringBuilder();
            string output = outputStringBuilder.ToString();

            Assert.AreEqual(expectedOutput + "\r\n", output);
        }

        [Test]
        public void CorrectlyOutputsColorString_MultipleSubStrings()
        {
            const string firstOutput = "Hello ";
            const string secondOutput = "World!";
            const string expectedOutput = firstOutput + secondOutput;
            ColorString colorString = new ColorString(new ColorString(firstOutput, ConsoleColor.White), new ColorString(secondOutput, ConsoleColor.Red));
            
            _output.WriteLine(colorString);

            StringBuilder outputStringBuilder = _textWriter.GetStringBuilder();
            string output = outputStringBuilder.ToString();

            Assert.AreEqual(expectedOutput + "\r\n", output);
        }

        [Test]
        public void CorrectlyOutputsColorString_RecursiveSubStrings([Values(1, 2)] int whichStringIsRecursive)
        {
            const string firstOutput = "what if ";
            const string secondOutput = "there were ";
            const string thirdOutput = "no rules?";

            const string expectedOutput = firstOutput + secondOutput + thirdOutput;

            ColorString firstSubString = new ColorString(firstOutput, ConsoleColor.White);
            ColorString secondSubString = new ColorString(secondOutput, ConsoleColor.Green);
            ColorString thirdSubString = new ColorString(thirdOutput, ConsoleColor.Blue);
            
            ColorString subString;
            ColorString colorString;

            if (whichStringIsRecursive == 1)
            {
                //1 and 2 comprise the subString
                subString = new ColorString(firstSubString, secondSubString);
                colorString = new ColorString(subString, thirdSubString);
            }
            else
            {
                //2 and 3 comprise the subString
                subString = new ColorString(secondSubString, thirdSubString);
                colorString = new ColorString(firstSubString, subString);
            }

            _output.WriteLine(colorString);

            StringBuilder outputStringBuilder = _textWriter.GetStringBuilder();
            string output = outputStringBuilder.ToString();

            Assert.AreEqual(expectedOutput + "\r\n", output);
        }

        [Test]
        public void CorrectlyOutputsColors_NoSubStrings()
        {
            const string expectedOutput = "Hello World!";
            const ConsoleColor expectedColor = ConsoleColor.Red;
            ColorString colorString = new ColorString(expectedOutput, expectedColor);

            _mockOutput.WriteLine(colorString);

            MockOutputMessage[] outputs = _mockOutput.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            MockOutputMessage output = outputs[0];

            Assert.AreEqual(expectedOutput + "\n", output.Message);
            Assert.AreEqual(expectedColor, output.Color);
        }

        [Test]
        public void CorrectlyOutputsColors_MultipleSubStrings()
        {
            const string firstOutput = "Hello ";
            const string secondOutput = "World!";

            const ConsoleColor firstColor = ConsoleColor.White;
            const ConsoleColor secondColor = ConsoleColor.Red;

            ColorString colorString = new ColorString(new ColorString(firstOutput, firstColor), new ColorString(secondOutput, secondColor));

            _mockOutput.WriteLine(colorString);

            MockOutputMessage[] outputs = _mockOutput.GetOutputs();

            Assert.AreEqual(2, outputs.Length);

            MockOutputMessage output = outputs[0];

            Assert.AreEqual(firstOutput, output.Message);
            Assert.AreEqual(firstColor, output.Color);

            output = outputs[1];

            Assert.AreEqual(secondOutput + "\n", output.Message);
            Assert.AreEqual(secondColor, output.Color);
        }

        [Test]
        public void CorrectlyOutputsColors_RecursiveSubStrings()
        {
            const string firstOutput = "what if ";
            const string secondOutput = "there were ";
            const string thirdOutput = "no rules?";

            const ConsoleColor firstColor = ConsoleColor.White;
            const ConsoleColor secondColor = ConsoleColor.Green;
            const ConsoleColor thirdColor = ConsoleColor.Blue;

            ColorString subString = new ColorString(new ColorString(secondOutput, secondColor), new ColorString(thirdOutput, thirdColor));
            ColorString colorString = new ColorString(new ColorString(firstOutput, firstColor), subString);

            _mockOutput.WriteLine(colorString);

            MockOutputMessage[] outputs = _mockOutput.GetOutputs();

            Assert.AreEqual(3, outputs.Length);

            MockOutputMessage output = outputs[0];

            Assert.AreEqual(firstOutput, output.Message);
            Assert.AreEqual(firstColor, output.Color);

            output = outputs[1];

            Assert.AreEqual(secondOutput, output.Message);
            Assert.AreEqual(secondColor, output.Color);

            output = outputs[2];

            Assert.AreEqual(thirdOutput + "\n", output.Message);
            Assert.AreEqual(thirdColor, output.Color);
        }

        [Test]
        public void GetFullStringMethod_NoSubstrings()
        {
            //arrange
            const string expectedFullString = "you've got to fight, for your right, to party!";
            ColorString colorString = new ColorString(expectedFullString);

            //act
            string actualString = colorString.GetFullString();

            //assert
            Assert.AreEqual(expectedFullString, actualString);
        }

        [Test]
        public void GetFullStringMethod_SubStrings()
        {
            //arrange
            const string firstSubString = "you've got to fight, ";
            const string secondSubString = "for your right, ";
            const string thirdSubString = "to party!";

            const string expectedFullString =  firstSubString + secondSubString + thirdSubString;
            ColorString colorString = new ColorString(firstSubString, secondSubString, thirdSubString);

            //act
            string actualString = colorString.GetFullString();

            //assert
            Assert.AreEqual(expectedFullString, actualString);
        }

        [Test]
        public void GetFullStringMethod_RecursiveSubStrings()
        {
            //arrange
            const string firstSubString = "this";
            ColorString firstColorSubString = new ColorString(firstSubString, ConsoleColor.Red);
            const string secondSubString = "is";
            ColorString secondColorSubString = new ColorString(secondSubString, ConsoleColor.Blue);

            ColorString firstCompositeSubString = new ColorString(firstColorSubString, secondColorSubString);

            const string thirdSubString = "a";
            ColorString thirdColorSubString = new ColorString(thirdSubString, ConsoleColor.Green);
            const string fourthSubString = "test";
            ColorString fourthColorSubString = new ColorString(fourthSubString, ConsoleColor.Yellow);
            const string fifthSubString = "!";
            ColorString fifthColorSubString = new ColorString(fifthSubString, ConsoleColor.White);

            ColorString secondCompositeSubString = new ColorString(thirdColorSubString, fourthColorSubString, fifthColorSubString);

            const string expectedFullString = firstSubString + secondSubString + thirdSubString + fourthSubString + fifthSubString;
            ColorString colorString = new ColorString(firstCompositeSubString, secondCompositeSubString);

            //act
            string actualString = colorString.GetFullString();

            //assert
            Assert.AreEqual(expectedFullString, actualString);
        }
    }
}
