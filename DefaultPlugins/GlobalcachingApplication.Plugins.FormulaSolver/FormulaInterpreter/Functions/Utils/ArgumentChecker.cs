using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions
{
    class ArgumentChecker
    {
        private string functionName;

        public ArgumentChecker(string functionName)
        {
            this.functionName = functionName;
        }

        /// <summary>
        /// <para>Checks for the right number of elements in object array.</para>
        /// <para>Throws ArgumentCountException if number of elements in args is less than minNumber or more than maxNumber</para>
        /// </summary>
        /// <param name="args">Array of objects to check</param>
        /// <param name="minNumber">Minimal number of elements expected in <paramref name="args"/> (may be null if no check)</param>
        /// <param name="maxNumber">Maximal number of elements expected in <paramref name="args"/> (may be null if no check)</param>
        public void CheckForNumberOfArguments(ref object[] args, int? minNumber, int? maxNumber)
        {
            if (minNumber != null)
            {
                if (args.Length < minNumber)
                {
                    throw new ArgumentCountException(
                        String.Format(StrRes.GetString(StrRes.STR_MISSING_ARGUMENT), functionName, minNumber)
                    );
                }
            }
            if (maxNumber != null)
            {
                if (args.Length > maxNumber)
                {
                    throw new ArgumentCountException(
                        String.Format(StrRes.GetString(StrRes.STR_TO_MUCH_ARGUMENTS), functionName, maxNumber)
                    );
                }
            }
        }


        /// <summary>
        /// Checks if an object is of zero size
        /// </summary>
        /// <param name="arg">The object to check</param>
        /// <returns>true if the length of the string representation of <paramref name="arg"/> is zero</returns>
        public bool ArgumentZeroSize(ref object arg)
        {
            return (arg.ToString().Length == 0);
        }


        /// <summary>
        /// Get an UInt64 value of an object with respect to a range given
        /// </summary>
        /// <param name="arg">Object to convert</param>
        /// <param name="minValue">Minimum expected value from <paramref name="arg"/></param>
        /// <param name="maxValue">Maximum expected value from <paramref name="arg"/></param>
        /// <returns></returns>
        public UInt64? GetRangedUInt64(ref object arg, UInt64 minValue, UInt64 maxValue)
        {
            UInt64 value = Convert.ToUInt64(arg.ToString());
            if ((value < minValue) || (value > maxValue))
            {
                throw new ArgumentRangeException(String.Format(
                    StrRes.GetString(StrRes.STR_VALUE_OUT_OF_RANGE),
                    functionName, minValue, maxValue));
            }
            return value;
        }
    }
}
