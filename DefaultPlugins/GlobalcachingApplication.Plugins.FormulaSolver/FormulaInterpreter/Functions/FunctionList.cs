using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions
{
    public class FunctionList
    {
        List<FunctionDescriptor> _functionList = new List<FunctionDescriptor>();

        public FunctionList()
        {
            InitFunctionList();
        }

        public List<FunctionDescriptor> GetList()
        {
            return _functionList;
        }

        private void InitFunctionList()
        {
            _functionList.Clear();
            AddTextFunctions();
            AddCoordinateFunctions();
            AddNumberFunctions();
        }

        public string GetFunctionGroupString(FunctionDescriptor.FunctionGroup grp)
        {
            switch (grp)
            {
                case FunctionDescriptor.FunctionGroup.NumberGroup:
                    return StrRes.GetString(StrRes.STR_NUMBER_GROUP);
                case FunctionDescriptor.FunctionGroup.CoordinateGroup:
                    return StrRes.GetString(StrRes.STR_COORDINATE_GROUP);
                case FunctionDescriptor.FunctionGroup.TextGroup:
                    return StrRes.GetString(StrRes.STR_TEXT_GROUP);
                default:
                    return StrRes.GetString(StrRes.STR_UNKNOWN_GROUP);
            }
        }

        private void AddNumberFunctions()
        {
            FunctionDescriptor.FunctionGroup grp = FunctionDescriptor.FunctionGroup.NumberGroup;

            AddFunction(
                "CrossTotal", grp, new[] { "Quersumme", "QS", "CT" }, 
                new Functions.NumberFunctions.CrossTotal(),
                StrRes.GetString(StrRes.STR_DESCR_CROSSTOTAL)
            );
            AddFunction(
                "ICrossTotal", grp, new[] { "IQuersumme", "IQS", "ICT" },
                new Functions.NumberFunctions.IteratedCrossTotal(),
                StrRes.GetString(StrRes.STR_DESCR_ICROSSTOTAL)
            );
            AddFunction(
                "CrossProduct", grp, new[] { "Querprodukt", "CP", "QP" },
                new Functions.NumberFunctions.CrossProduct(),
                StrRes.GetString(StrRes.STR_DESCR_CROSSPRODUCT)
            );
            AddFunction(
                "ICrossProduct", grp, new[] { "IQuerprodukt", "ICP", "IQP" },
                new Functions.NumberFunctions.IteratedCrossProduct(),
                StrRes.GetString(StrRes.STR_DESCR_ICROSSPRODUCT)
            );
            AddFunction(
                "PrimeNumber", grp, new[] { "Primzahl" },
                new Functions.NumberFunctions.PrimeNumber(),
                StrRes.GetString(StrRes.STR_DESCR_PRIMENUMBER)
            );
            AddFunction(
                "PrimeIndex", grp, new[] { "Primindex" }, 
                new Functions.NumberFunctions.PrimeIndex(),
                StrRes.GetString(StrRes.STR_DESCR_PRIMEINDEX)
            );
            AddFunction(
                "Int", grp, new[] { "Ganzzahl" },
                new Functions.NumberFunctions.IntFunction(),
                StrRes.GetString(StrRes.STR_DESCR_INT)
            );
            AddFunction(
                "Round", grp, new[] { "Runden" },
                new Functions.NumberFunctions.RoundFunction(),
                StrRes.GetString(StrRes.STR_DESCR_ROUND)
            );
            AddFunction(
                "Rom2Dec", grp, null,
                new Functions.NumberFunctions.Rom2Dec(),
                StrRes.GetString(StrRes.STR_DESCR_ROM2DEC)
            );
            AddFunction(
                "Pi", grp, null,
                new Functions.NumberFunctions.Pi(),
                StrRes.GetString(StrRes.STR_DESCR_PI)
            );
            AddFunction(
                "Pow", grp, new[] { "Power", "Potenz" },
                new Functions.NumberFunctions.PowerFunction(),
                StrRes.GetString(StrRes.STR_DESCR_POW)
            );
            AddFunction(
                "Fact", grp, new[] { "Factorial", "Fakultät" },
                new Functions.NumberFunctions.Factorial(),
                StrRes.GetString(StrRes.STR_DESCR_FACTORIAL)
            );
        }

        private void AddCoordinateFunctions()
        {
            FunctionDescriptor.FunctionGroup grp = FunctionDescriptor.FunctionGroup.CoordinateGroup;
            AddFunction(
                "Bearing", grp, new[] { "Peilung" },
                new Functions.CoordinateFunctions.Bearing(),
                StrRes.GetString(StrRes.STR_DESCR_BEARING)
            );
            AddFunction(
                "Distance", grp, new[] { "Abstand" },
                new Functions.CoordinateFunctions.Distance(),
                StrRes.GetString(StrRes.STR_DESCR_DISTANCE)
            );
            AddFunction(
                "CrossBearing", grp, new[] { "Kreuzpeilung" },
                new Functions.CoordinateFunctions.Crossbearing(),
                StrRes.GetString(StrRes.STR_DESCR_CROSSBEARING)
            );
            AddFunction(
                "Intersection", grp, new[] { "Schnittpunkt" },
                new Functions.CoordinateFunctions.Bearing(),
                StrRes.GetString(StrRes.STR_DESCR_INTERSECTION)
            );
            AddFunction(
                "Projection", grp, new[] { "Projektion" },
                new Functions.CoordinateFunctions.Projection(),
                StrRes.GetString(StrRes.STR_DESCR_PROJECTION)
            );
            AddFunction(
                "Waypoint", grp, new[] { "Wegpunkt", "WP" },
                new Functions.CoordinateFunctions.Waypoint(),
                StrRes.GetString(StrRes.STR_DESCR_WAYPOINT)
            );
        }

        private void AddTextFunctions()
        {
            FunctionDescriptor.FunctionGroup grp = FunctionDescriptor.FunctionGroup.TextGroup;
            AddFunction(
                "AlphaSum", grp, new[] { "AS" },
                new Functions.TextFunctions.AlphaSum(),
                StrRes.GetString(StrRes.STR_DESCR_ALPHASUM)
            );
            AddFunction(
                "AlphaPos", grp, new[] { "AP" },
                new Functions.TextFunctions.AlphaPos(),
                StrRes.GetString(StrRes.STR_DESCR_ALPHAPOS)
            );
            AddFunction(
                "PhoneCode", grp, new[] { "HandyCode", "PC", "HC" },
                new Functions.TextFunctions.PhoneCode(),
                StrRes.GetString(StrRes.STR_DESCR_PHONECODE)
            );
            AddFunction(
                "PhoneSum", grp, new[] { "HS", "PS", "HandySum" },
                new Functions.TextFunctions.PhoneSum(),
                StrRes.GetString(StrRes.STR_DESCR_PHONESUM)
            );
            AddFunction(
                "Length", grp, new[] { "Len", "Länge" },
                new Functions.TextFunctions.TextLen(),
                StrRes.GetString(StrRes.STR_DESCR_LEN)
            );
            AddFunction(
                "Mid", grp, null,
                new Functions.TextFunctions.Mid(),
                StrRes.GetString(StrRes.STR_DESCR_MID)
            );
            AddFunction(
                "Reverse", grp, new[] { "Umkehr" },
                new Functions.TextFunctions.Reverse(),
                StrRes.GetString(StrRes.STR_DESCR_REVERSE)
            );
            AddFunction(
                "Rot13", grp, null,
                new Functions.TextFunctions.Rot13(),
                StrRes.GetString(StrRes.STR_DESCR_ROT13)
            );
        }

        private void AddFunction(string name, FunctionDescriptor.FunctionGroup functionGroup, 
            string[] alternates, Functor functor, string description)
        {
            _functionList.Add(
                new FunctionDescriptor(
                    name,
                    functionGroup,
                    alternates,
                    functor,
                    description
                )
            );
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
