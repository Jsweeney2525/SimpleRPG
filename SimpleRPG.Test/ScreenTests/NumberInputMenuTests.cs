using System;
using NUnit.Framework;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuSelections;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class NumberInputMenuTests
    {
        private MockInput _input;
        private MockOutput _output;
        private NumberInputMenu _menu;

        private const int MinValue = 0;
        private const int MaxValue = 10;

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();

            _menu = new NumberInputMenu("foo", _input, _output, MinValue, MaxValue);
        }

        [Test]
        public void ReturnsAppopriateResponse()
        {
            _input.Push("1");

            MenuSelection menuSelection = _menu.GetInput();

            NumberInputMenuSelection numberInputSelection = menuSelection as NumberInputMenuSelection;

            Assert.NotNull(numberInputSelection);

            Assert.AreEqual(1, numberInputSelection.Number);
        }

        [Test]
        public void CorrectlyDisallowsEmptyInput()
        {
            _input.Push("", "2");

             _menu.GetInput();

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(3, outputs.Length);

            MockOutputMessage output = outputs[1];

            Assert.AreEqual("input is required\n", output.Message);
            Assert.AreEqual(ConsoleColor.Red, Globals.ErrorColor);
        }

        [Test]
        public void CorrectlyDisallowsOutOfRangeInput([Values(-1, -5, 1, 7)] int overUnderValue)
        {
            int value;

            if (overUnderValue < 0)
            {
                value = MinValue + overUnderValue;
            }
            else
            {
                value = MaxValue + overUnderValue;
            }

            _input.Push(value.ToString(), MinValue.ToString());

            _menu.GetInput();

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(3, outputs.Length);

            MockOutputMessage output = outputs[1];

            string expectedError = $"input must be between { MinValue} and { MaxValue}, inclusive\n";
            Assert.AreEqual(expectedError, output.Message);
            Assert.AreEqual(ConsoleColor.Red, Globals.ErrorColor);
        }

        [Test]
        public void CorrectlyDisallowsNonNumberInput()
        {
            _menu = new NumberInputMenu("", _input, _output, 0, 5);

            _input.Push("abc", "f12", "1");

            _menu.GetInput();

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(5, outputs.Length);

            MockOutputMessage output = outputs[1];

            const string nonNumberError = "input not recognized as a number\n";
            Assert.AreEqual(nonNumberError, output.Message);
            Assert.AreEqual(ConsoleColor.Red, Globals.ErrorColor);

            output = outputs[3];

            Assert.AreEqual(nonNumberError, output.Message);
            Assert.AreEqual(ConsoleColor.Red, Globals.ErrorColor);
        }

        [Test]
        public void CorrectlyReplacesMinAndMaxStringPlaceholders([Values(1, 3)] int minValue,
            [Values(4, 9)] int maxValue)
        {
            string promptBeforeReplacement = 
                $"min: {Globals.MinValueReplacementString}, max: {Globals.MaxValueReplacementString}";
            string minValueAsString = minValue.ToString();
            string maxValueAsString = maxValue.ToString();

            NumberInputMenu menu = new NumberInputMenu(promptBeforeReplacement, _input, _output, minValue, maxValue);

            _input.Push(minValueAsString);

            menu.GetInput();

            MockOutputMessage output = _output.GetOutputs()[0];

            string expectedOutput = promptBeforeReplacement.Replace(Globals.MinValueReplacementString, minValueAsString);
            expectedOutput = expectedOutput.Replace(Globals.MaxValueReplacementString, maxValueAsString);

            Assert.AreEqual(expectedOutput + "\n", output.Message);
        }
    }
}
