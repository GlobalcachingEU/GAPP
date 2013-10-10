
namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class TextLen: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            int len = 0;
            if (args.Length > 0)
            {
                len = args[0].ToString().Length;
            }
            return len;
        }
    }
}
