﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Pilgrim.Generator;

namespace Pilgrim.Tests.Generator
{
    public class GeneratorResolverFixture
    {
        [Test]
        public void CanSelectCorrectGenerator()
        {
            var resolver = new CommandResolver();

            resolver.Register(() => new SampleStringList(), "test1");
            resolver.Register(() => new SampleDateTimeList(), "test2");

            var generator1 = resolver.Resolve("test1 test", true);
            var generator2 = resolver.Resolve("test2 test", true);


            generator1.Should().Be.InstanceOf<SampleStringList>();
            generator2.Should().Be.InstanceOf<SampleDateTimeList>();
        }

        [Test]
        public void ShowErrorWhenStringOnlyStartsWithCorrectValue()
        {
            var resolver = new CommandResolver();
            resolver.Register(() => new SampleStringList(), "test1");

            Assert.Throws<InvalidCommandException>(() => resolver.Resolve("test1t(test)", true));
        }


        [Test]
        public void CanSelectCorrectGeneratorStartingWithSpace()
        {
            var resolver = new CommandResolver();
            resolver.Register(() => new SampleStringList(), "test1");
            resolver.Register(() => new SampleDateTimeList(), "test2");

            var generator1 = resolver.Resolve(" test1 test", true);
            var generator2 = resolver.Resolve(" test2 test", true);


            generator1.Should().Be.InstanceOf<SampleStringList>();
            generator2.Should().Be.InstanceOf<SampleDateTimeList>();
        }

        [Test]
        public void CanSelectCorrectGeneratorEndingWithSpace()
        {
            var resolver = new CommandResolver();
            resolver.Register(() => new SampleStringList(), "test1");
            resolver.Register(() => new SampleDateTimeList(), "test2");

            var generator1 = resolver.Resolve("test1 test ", true);
            var generator2 = resolver.Resolve("test2 test ", true);

            generator1.Should().Be.InstanceOf<SampleStringList>();
            generator2.Should().Be.InstanceOf<SampleDateTimeList>();
        }


        [Test]
        public void CanSelectCorrectGeneratorWithSpacedInput()
        {
            var resolver = new CommandResolver();
            resolver.Register(() => new SampleStringList(), "test1 a");
            resolver.Register(() => new SampleDateTimeList(), "test2 b");

            var generator1 = resolver.Resolve("test1    a", true);
            var generator2 = resolver.Resolve("test2    b", true);


            generator1.Should().Be.InstanceOf<SampleStringList>();
            generator2.Should().Be.InstanceOf<SampleDateTimeList>();
        }

        [Test]
        public void CanSelectCorrectGeneratorWithSpacedInputAndParenthesisEnd()
        {
            var resolver = new CommandResolver();
            resolver.Register(() => new SampleStringList(), "test1 a");
            resolver.Register(() => new SampleDateTimeList(), "test2 b");

            var generator1 = resolver.Resolve("test1    a(test)", true);
            var generator2 = resolver.Resolve("test2    b(test)", true);


            generator1.Should().Be.InstanceOf<SampleStringList>();
            generator2.Should().Be.InstanceOf<SampleDateTimeList>();
        }

        [Test]
        public void CanSelectCorrectGeneratorWithSpacedInputAndSpaceEnd()
        {
            var resolver = new CommandResolver();
            resolver.Register(() => new SampleStringList(), "test1 a");
            resolver.Register(() => new SampleDateTimeList(), "test2 b");

            var generator1 = resolver.Resolve("test1    a (test)", true);
            var generator2 = resolver.Resolve("test2    b (test)", true);


            generator1.Should().Be.InstanceOf<SampleStringList>();
            generator2.Should().Be.InstanceOf<SampleDateTimeList>();
        }

        [Test, ExpectedException(typeof(AmbiguousCommandException),
            ExpectedMessage = "GeneratorResolverFixture.SampleStringList, GeneratorResolverFixture.SampleDateTimeList",
            MatchType=MessageMatch.Contains)]
        public void CannotSelectCorrectGeneratorWhenFoundMultiple()
        {
            var resolver = new CommandResolver();
            resolver.Register(() => new SampleStringList(), "test1 a");
            resolver.Register(() => new SampleDateTimeList(), "test1");
            resolver.Resolve("test1    a (test)", true);
        }


     

        public class SampleStringList : ICommand
        {
            public void Execute() { }
        }

        public class SampleDateTimeList : ICommand
        {
            public void Execute() { }
        }

    }

}
