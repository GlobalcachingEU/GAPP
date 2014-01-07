using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public abstract class BinaryOperator : GCToken
    {
        public abstract object Evaluate(object left, object right);
    }
}
