using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class Mid: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            string res = "";
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 2, null);
            string s = args[0].ToString();
            int start = int.Parse(args[1].ToString());
            if ((start > 0) && (start <= s.Length))
            {
                int len = 1;
                if (args.Length > 2)
                {
                    len = Math.Min(int.Parse(args[2].ToString()), s.Length + 1 - start);
                }
                res = s.Substring(start - 1, len);
            }
            return res;
        }
    }
}
