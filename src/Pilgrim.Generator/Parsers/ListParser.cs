using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Simple;

namespace Pilgrim.Generator.Parsers
{
    public class ListParser<T, P> : CommandParser<T, IEnumerable<P>>
    {
        public ListParser(bool mustBeExplicit, string name, Expression<Func<T, IEnumerable<P>>> expression) : base(mustBeExplicit, name, expression) { }
        protected override void ParseInternal(string match, IList<string> values, ICommand generator)
        {
            ExpressionExtensions.SetValue(Expression, generator,
                values.Select(x => ConvertValue<P>(x)).ToList());
        }
    }
}
