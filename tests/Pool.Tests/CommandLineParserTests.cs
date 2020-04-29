using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Pool.Tests
{
    [TestClass]
    public class CommandLineParserTests
    {
        [TestMethod]
        public void Parser()
        {
            var args = new string[]
            {
                "command1",
                "/?",
                "--flag1",
                "--arg1",
                "arg1Value",
                "--arg2",
                "va1 val2 val3",
                "--flag2",
            };

            var parser = CommandLineParser.FromArguments(args);

            Assert.IsTrue(parser.IsCommand("command1"));

            Assert.AreEqual(3, parser.Flags.Count);
            Assert.IsNotNull(parser.Flags.SingleOrDefault(f => f == "help"));
            Assert.IsNotNull(parser.Flags.SingleOrDefault(f => f == "flag1"));
            Assert.IsNotNull(parser.Flags.SingleOrDefault(f => f == "flag2"));

            Assert.AreEqual(2, parser.Args.Count);
            Assert.AreEqual("arg1Value", parser.Args["arg1"]);
            Assert.AreEqual("va1 val2 val3", parser.Args["arg2"]);
        }
    }
}