using System;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class PhoneCode: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            
            if (!checker.ArgumentZeroSize(ref args[0]))
            {
                CharMapPhoneCode map = new CharMapPhoneCode();
                return map.GetCharacterCode(args[0].ToString()[0]);
            }
            return 0;
        }
    }
}
