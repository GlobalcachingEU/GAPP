using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions
{
    public abstract class Functor
    {
        public abstract object Execute(object[] args, ExecutionContext ctx);
    }
}
