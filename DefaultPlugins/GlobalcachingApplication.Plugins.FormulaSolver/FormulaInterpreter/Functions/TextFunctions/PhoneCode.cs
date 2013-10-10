using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class PhoneCode: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 1) && (!checker.ArgumentZeroSize(ref args[0])))
            {
                CharMapPhoneCode map = new CharMapPhoneCode();
                return map.GetCharacterCode(args[0].ToString()[0]);
            }
            return 0;
        }
    }
}
