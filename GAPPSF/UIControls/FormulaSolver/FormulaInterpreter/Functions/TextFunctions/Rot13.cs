using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class Rot13: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            char[] array = args[0].ToString().ToCharArray();
            for (int i = 0; i < array.Length; ++i)
            {
                int number = (int)array[i];
                if (number >= 'a' && number <= 'z')
                {
                    number += (number > 'm')? -13: 13;
                }
                else if (number >= 'A' && number <= 'Z')
                {
                    number += (number > 'M') ? -13 : 13;
                }
                array[i] = (char)number;
            }
            return new string(array);
        }
    }
}
