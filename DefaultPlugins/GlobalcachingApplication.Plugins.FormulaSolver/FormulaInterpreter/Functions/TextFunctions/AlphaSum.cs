using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class AlphaSum: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            int sum = 0;
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 1))
            {
                CharMapAlphabet map = new CharMapAlphabet();
                String s = args[0].ToString();
                for (int i = 0; i < s.Length; ++i)
                {
                    sum += map.GetCharacterCode(s[i]);
                }
            }
            return sum;
        }
    }
}
