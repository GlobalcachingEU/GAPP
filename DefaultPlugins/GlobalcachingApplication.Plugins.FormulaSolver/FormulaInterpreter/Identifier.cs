using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    [Terminal("Identifier")]
    public class Identifier : Expression
    {
        internal readonly string _idName;

        public Identifier(string idName)
        {
            _idName = idName;
        }

        public override object GetValue(ExecutionContext ctx)
        {
            return ctx[_idName];
        }
    }
}
