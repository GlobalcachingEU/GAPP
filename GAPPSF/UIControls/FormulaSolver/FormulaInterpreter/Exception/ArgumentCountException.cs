using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Exception;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class ArgumentCountException : FormulaSolverException
    {
        public ArgumentCountException(string msg) : base(msg) { }
        public ArgumentCountException(string msg, System.Exception innerException) : base(msg, innerException) { }
    }
}
