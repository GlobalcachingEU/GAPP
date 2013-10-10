using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    public class Optional<T>: GCToken where T: GCToken {
        private readonly T value;

        [Rule("<OptionalArgumentList> ::=", typeof(Sequence<Expression>))]
        public Optional() : this(null) { }

        public Optional(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get
            {
                return value;
            }
        }

        public bool HasValue
        {
            get
            {
                return value != null;
            }
        }
    }
}
