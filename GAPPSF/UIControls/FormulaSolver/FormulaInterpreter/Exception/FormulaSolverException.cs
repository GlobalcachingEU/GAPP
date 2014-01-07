using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Exception
{
    public class FormulaSolverException: System.Exception
    {
        public FormulaSolverException(string msg) : base(msg) { }
        public FormulaSolverException(string msg, System.Exception innerException) : base(msg, innerException) { }
    }
}
