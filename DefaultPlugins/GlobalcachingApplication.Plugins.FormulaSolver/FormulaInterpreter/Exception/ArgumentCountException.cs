using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Exception;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    public class ArgumentCountException : FormulaSolverException
    {
        public ArgumentCountException(string msg) : base(msg) { }
        public ArgumentCountException(string msg, System.Exception innerException) : base(msg, innerException) { }
    }
}
