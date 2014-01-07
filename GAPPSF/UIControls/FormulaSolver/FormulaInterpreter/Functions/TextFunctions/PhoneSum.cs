using System;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class PhoneSum: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            int sum = 0;
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            CharMapPhoneCode map = new CharMapPhoneCode();
            String s = args[0].ToString();
            for (int i = 0; i < s.Length; ++i)
            {
                sum += map.GetCharacterCode(s[i]);
            }
            return sum;
        }
    }
}
