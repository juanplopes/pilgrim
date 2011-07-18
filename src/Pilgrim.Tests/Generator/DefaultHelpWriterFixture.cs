﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Pilgrim.Generator;
using System.IO;

namespace Pilgrim.Tests.Generator
{
    public class DefaultHelpWriterFixture
    {
        [Test]
        public void CannotWriteHelpUnlessConfigured()
        {
            var resolver = new CommandResolver();

            Assert.Throws<InvalidCommandException>(() => resolver.Resolve("help"));
            Assert.Throws<InvalidCommandException>(() => resolver.Resolve("help test"));
        }

        [Test]
        public void CanWriteHelpSummary()
        {
            var writer = new StringWriter();
            var r = Sample(writer);
            r.Resolve("help").Execute();

            var text = writer.ToString();

            AssertTexts(
@"Available commands: (say 'help <commands>' for more info)

help <commands> (IList<String>)
> Generator: HelpTextGenerator

cmd1 a <cmd1_arguments> (IList<String>)
> Generator: DefaultHelpWriterFixture.Sample1

cmd1 b <cmd1_arguments> (IList<String>)
> Generator: DefaultHelpWriterFixture.Sample1

cmd2
> Generator: DefaultHelpWriterFixture.Sample2

noopt
> Generator: DefaultHelpWriterFixture.Sample2", text);

        }

        [Test]
        public void CanWriteHelpInfoForCmd1A()
        {
            var writer = new StringWriter();
            var r = Sample(writer);
            r.Resolve("help 'cmd1 a'").Execute();

            var text = writer.ToString();
            AssertTexts(
@"Commands found: 1

Command: cmd1 a <cmd1_arguments> (IList<String>)
Available options:
> opt1 [Int32]
> opt2 [String]", text);
        }

        [Test]
        public void CanWriteHelpInfoForNoOptionCommand()
        {
            var writer = new StringWriter();
            var r = Sample(writer);
            r.Resolve("help noopt").Execute();

            var text = writer.ToString();
            AssertTexts(
@"Commands found: 1

Command: noopt
No available options.", text);
        }


        [Test]
        public void CanWriteHelpInfoForCmd1Generic()
        {
            var writer = new StringWriter();
            var r = Sample(writer);
            r.Resolve("help 'cmd1'").Execute();

            var text = writer.ToString();
            AssertTexts(
@"Commands found: 2

Command: cmd1 a <cmd1_arguments> (IList<String>)
Available options:
> opt1 [Int32]
> opt2 [String]

Command: cmd1 b <cmd1_arguments> (IList<String>)
Available options:
> opt1 [Int32]
> opt2 [String]", text);
        }

        [Test]
        public void CanWriteHelpInfoForCmdWithoutNumberGeneric()
        {
            var writer = new StringWriter();
            var r = Sample(writer);
            r.Resolve("help 'cmd'").Execute();

            var text = writer.ToString();
            AssertTexts(
@"Commands found: 3

Command: cmd1 a <cmd1_arguments> (IList<String>)
Available options:
> opt1 [Int32]
> opt2 [String]

Command: cmd1 b <cmd1_arguments> (IList<String>)
Available options:
> opt1 [Int32]
> opt2 [String]

Command: cmd2
Available options:
> opt1 [Int32]
> opt2 [String]", text);
        }

        [Test]
        public void CanWriteHelpWithInvalidCommandName()
        {
            var writer = new StringWriter();
            var r = Sample(writer);
            r.Resolve("help 'asd qwe'").Execute();

            var text = writer.ToString();
            AssertTexts(@"Commands found: 0", text);
        }

        public void AssertTexts(string expected, string actual)
        {
            Clean(actual).Should().Be(Clean(expected));
        }

        public string Clean(string text)
        {
            return text.Replace("\n", "").Replace("\r", "");
        }

        public CommandResolver Sample(TextWriter writer)
        {
            var r = new CommandResolver().WithHelp(writer);

            r.Register<Sample1>("cmd1 a", "cmd1 b")
                .WithArgumentList("cmd1_arguments", x => x.Arguments)
                .WithOption("opt1", x => x.Option1)
                .WithOption("opt2", x => x.Option2);

            r.Register<Sample2>("cmd2")
              .WithOption("opt1", x => x.Option1)
              .WithOption("opt2", x => x.Option2);

            r.Register<Sample2>("noopt");

            return r;
        }

        public class Sample1 : ICommand
        {
            public IList<string> Arguments { get; set; }
            public int Option1 { get; set; }
            public string Option2 { get; set; }

            public void Execute() { }
        }

        public class Sample2 : ICommand
        {
            public int Option1 { get; set; }
            public string Option2 { get; set; }

            public void Execute() { }
        }
    }
}

