using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class Rom2Dec: Functor
    {
        private readonly string[] roman = new string[] { "M", "CM", "CD", "D", "C", "XC", "XL", "L", "X", "IX", "IV", "V", "I" };
        private readonly int[] dec = new int[] { 1000, 900, 400, 500, 100, 90, 40, 50, 10, 9, 4, 5, 1 };

        private int Roman2Decimal(string i_string)
        {
            int value = 0;
            int index = 0;
            for (int i = 0; i < dec.Length; ++i) {
                while ((index + roman[i].Length <= i_string.Length) &&
                    i_string.Substring(index, roman[i].Length) == roman[i]) 
                {
                    value += dec[i];
                    index += roman[i].Length;
                }
            }
            return value;
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            return Roman2Decimal(Convert.ToString(args[0]));
        }
    }
}
