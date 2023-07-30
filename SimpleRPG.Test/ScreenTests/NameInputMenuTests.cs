using NUnit.Framework;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    class NameInputMenuTests : ScreenTestBase
    {
        private MockInput _input;
        private MockOutput _output;
        private NameInputMenu _menu;
        private const string Prompt = NameInputMenu.DefaultPrompt;
        private string _outputtedPrompt;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _outputtedPrompt = Prompt + "\n";
        }

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();

            _menu = new NameInputMenu(_input, _output);
        }

        [TearDown]
        public void TearDown()
        {
            _input = null;
            _output = null;
        }

        [Test]
        public void TestGetName_AppropriatelyDisplaysDefaultPrompt()
        {
            const string expected = "Alkeeros";

            _input.Push(expected);

            _menu.GetName();

            var outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(_outputtedPrompt, outputs[0].Message);
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[0].Type);
        }

        [Test]
        public void TestGetName_AppropriatelyDisplaysGivenPrompt()
        {
            const string expected = "Alkeeros";
            const string newPrompt = "this is a test prompt";

            _input.Push(expected);

            _menu.GetName(newPrompt);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);
            Assert.AreNotEqual(_outputtedPrompt, outputs[0].Message);
            Assert.AreEqual(newPrompt + "\n", outputs[0].Message);
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[0].Type);
        }

        [Test]
        public void TestGetName_AppropriatelyReturnsName()
        {
            const string expected = "Alkeeros";
            _input.Push(expected);

            var name = _menu.GetName();

            Assert.AreEqual(expected, name);
        }

        [Test]
        public void TestGetName_AppropriatelyPrintsError_NoNameSpecified()
        {
            _input.Push("");
            const string expected = "Alkeeros";
            _input.Push(expected);

            var name = _menu.GetName();

            Assert.AreEqual(expected, name);

            var outputs = _output.GetOutputs();
            var clearIndices = _output.GetClearIndices();

            Assert.AreEqual(3, outputs.Length);

            //0 and 2 index are the prompt
            Assert.AreEqual(NameInputMenu.NameRequiredErrorMessage + "\n", outputs[1].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[1].Type);

            Assert.AreEqual(1, clearIndices.Length);
            Assert.AreEqual(2, clearIndices[0]);
        }

        [Test]
        public void TestGetName_AppropriatelyPrintsError_NameTooLong()
        {
            _input.Push("01234567890123456789abcde");
            const string expected = "Alkeeros";
            _input.Push(expected);

            var name = _menu.GetName();

            Assert.AreEqual(expected, name);

            var outputs = _output.GetOutputs();
            var clearIndices = _output.GetClearIndices();

            Assert.AreEqual(3, outputs.Length);

            //0 and 2 index are the prompt
            Assert.AreEqual(NameInputMenu.NameTooLongErrorMessage + "\n", outputs[1].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[1].Type);

            Assert.AreEqual(1, clearIndices.Length);
            Assert.AreEqual(2, clearIndices[0]);
        }

        [Test]
        public void TestGetName_AppropriatelyPrintsError_DisallowedName([Values(null, "duplicate names are not allowed")] string disallowedErrorMessage)
        {
            const string disallowedName = "Alan";
            _input.Push(disallowedName);
            _input.Push(disallowedName.ToUpperInvariant());
            _input.Push(disallowedName.ToLowerInvariant());
            const string expected = "Alkeeros";
            _input.Push(expected);

            var name = _menu.GetName(disallowedErrorMessage: disallowedErrorMessage, disallowedNames: disallowedName);

            Assert.AreEqual(expected, name);

            var outputs = _output.GetOutputs();
            var clearIndices = _output.GetClearIndices();

            Assert.AreEqual(7, outputs.Length);

            //even indices are the prompts
            string expectedErrorMessage = disallowedErrorMessage ?? NameInputMenu.DefaultDisallowedNameErrorMessage;
            Assert.AreEqual(expectedErrorMessage + "\n", outputs[1].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[1].Type);

            expectedErrorMessage = disallowedErrorMessage ?? NameInputMenu.DefaultDisallowedNameErrorMessage;
            Assert.AreEqual(expectedErrorMessage + "\n", outputs[3].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[3].Type);

            expectedErrorMessage = disallowedErrorMessage ?? NameInputMenu.DefaultDisallowedNameErrorMessage;
            Assert.AreEqual(expectedErrorMessage + "\n", outputs[5].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[5].Type);

            Assert.AreEqual(3, clearIndices.Length);
            Assert.AreEqual(2, clearIndices[0]);
            Assert.AreEqual(4, clearIndices[1]);
            Assert.AreEqual(6, clearIndices[2]);
        }

        [Test]
        public void TestGetName_MultipleErrors()
        {
            _input.Push("01234567890123456789abcde");
            _input.Push("edcba98765432109876543210");
            _input.Push("");
            const string expected = "Alkeeros";
            _input.Push(expected);

            var name = _menu.GetName();

            Assert.AreEqual(expected, name);

            var outputs = _output.GetOutputs();
            var clearIndices = _output.GetClearIndices();

            Assert.AreEqual(7, outputs.Length);
            Assert.AreEqual(3, clearIndices.Length);

            //even number indices are the prompt
            for (int i = 1, j = 0; i < 7; i += 2, ++j)
            {
                var expectedError = ((i < 4) ? 
                    NameInputMenu.NameTooLongErrorMessage : 
                    NameInputMenu.NameRequiredErrorMessage);
                Assert.AreEqual(expectedError + "\n", outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Error, outputs[i].Type);

                Assert.AreEqual(i + 1, clearIndices[j]);
            }
        }
    }
}
