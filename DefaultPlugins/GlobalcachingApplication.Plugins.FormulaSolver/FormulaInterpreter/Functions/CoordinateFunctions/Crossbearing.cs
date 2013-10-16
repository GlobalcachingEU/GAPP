using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.CoordinateFunctions
{
    public class Crossbearing: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 4, null);
            Intersection intersection = new Intersection();
            Projection projection = new Projection();

            object[] projectionArgs1 = { args[0], "1000", args[1] };
            object p1 = projection.Execute(projectionArgs1, ctx);

            object[] projectionArgs2 = { args[2], "1000", args[3] };
            object p2 = projection.Execute(projectionArgs2, ctx);

            object[] intersectionArgs = { args[0], p1, args[2], p2 };
            return intersection.Execute(intersectionArgs, ctx).ToString();;
        }
    }
}
