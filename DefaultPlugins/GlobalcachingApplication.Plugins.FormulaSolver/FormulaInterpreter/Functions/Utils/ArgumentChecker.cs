using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions
{
    class ArgumentChecker
    {
        private string functionName;
        private ExecutionContext executionContext;

        public ArgumentChecker(ExecutionContext ctx, string functionName)
        {
            this.executionContext = ctx;
            this.functionName = functionName;
        }

        public bool CheckForMinimumArguments(ref object[] args, int minimumNumber) 
        {
            if (args.Length < minimumNumber)
            {
                throw new ArgumentCountException(
                    String.Format(StrRes.GetString(StrRes.STR_MISSING_ARGUMENT), functionName, minimumNumber)
                );
            }
            return true;
        }

        public bool ArgumentZeroSize(ref object arg)
        {
            return (arg.ToString().Length == 0);
        }

        public UInt64? GetRangedUInt64(ref object arg, UInt64 minValue, UInt64 maxValue)
        {
            UInt64 value = Convert.ToUInt64(arg.ToString());
            if ((value < minValue) || (value > maxValue))
            {
                throw new ArgumentOutOfRangeException("", value, String.Format(
                        StrRes.GetString(StrRes.STR_VALUE_OUT_OF_RANGE),
                        functionName, minValue, maxValue));
            }
            return value;
        }
    }
}
