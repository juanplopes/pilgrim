﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Simple.Patterns;
using System.Text.RegularExpressions;
using Pilgrim.Generator.Parsers;
using Simple;

namespace Pilgrim.Generator
{
    public class CommandOptions<T> : ICommandOptions
        where T : ICommand
    {
        Func<T> _generator;
        protected ICommandParser ArgumentParser = null;
        protected List<ICommandParser> OptionParsers = new List<ICommandParser>();
        public CommandOptions(Func<T> generator)
        {
            this._generator = generator;
        }


        public CommandOptions<T> WithOption<P>(string name, Expression<Func<T, P>> into)
        {
            OptionParsers.Add(new ValueParser<T, P>(true, name, into));
            return this;
        }

        public CommandOptions<T> WithOptionList<P>(string name, Expression<Func<T, IEnumerable<P>>> into)
        {
            OptionParsers.Add(new ListParser<T, P>(true, name, into));
            return this;
        }




        public ICommand Parse(string parameters, bool ignoreExceedingArgs)
        {
            var result = _generator();

            if (ArgumentParser != null)
                parameters = Argument.Parse(parameters, result);

            foreach (var parser in OptionParsers)
                parameters = parser.Parse(parameters, result);

            parameters = parameters.Trim();
            if (parameters.Length > 0 && !ignoreExceedingArgs)
                throw new UnrecognizedOptionsException(parameters);

            return result;
        }

        public string GeneratorType
        {
            get { return typeof(T).GetRealClassName(); }
        }

        #region IGeneratorOptions Members

        public IEnumerable<ICommandParser> Options
        {
            get { return OptionParsers; }
        }

        public ICommandParser Argument
        {
            get { return ArgumentParser; }
        }

        #endregion
    }
}
