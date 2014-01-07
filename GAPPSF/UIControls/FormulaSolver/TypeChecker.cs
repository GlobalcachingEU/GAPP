using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver
{
    static class TypeChecker
    {
        public static bool IsBoolean(object obj)
        {
            return (Type.GetTypeCode(obj.GetType()) == TypeCode.Boolean);
        }

        public static bool IsNumeric(object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return false;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                case TypeCode.DateTime:
                case TypeCode.String:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum GreatestCommonType
        {
            StringType,
            BooleanType,
            NumericType
        }

        public static GreatestCommonType GCT(params object[] toCheck)
        {
            int boolCount = 0;
            int stringCount = 0;
            int numCount = 0;

            foreach (var obj in toCheck)
            {
                if (IsBoolean(obj))
                    boolCount++;
                else if (IsNumeric(obj))
                    numCount++;
                else
                {
                    stringCount++;
                }
            }

            if (boolCount == toCheck.Length)
            {
                return GreatestCommonType.BooleanType;
            }

            if (numCount == toCheck.Length)
            {
                return GreatestCommonType.NumericType;
            }

            if (numCount > 0 && boolCount == 0)
            {
                return GreatestCommonType.NumericType;
            }

            return GreatestCommonType.StringType;

        }

    }
}
