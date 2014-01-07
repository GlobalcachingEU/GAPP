using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class ExecutionContext
    {
        #region Variables

        private Dictionary<string, object> GlobalVariables = new Dictionary<string, object>();
        
        private HashSet<string> VariablesSet = new HashSet<string>();
        private HashSet<string> VariablesUndefinedWhenRead = new HashSet<string>();

        public object this[string idx]
        {
            get
            {
                string upperValue = idx.ToUpper();
                if (!VariablesSet.Contains(upperValue)) 
                {
                    VariablesUndefinedWhenRead.Add(upperValue);
                }
                return GlobalVariables.ContainsKey(upperValue)
                    ? GlobalVariables[upperValue]
                    : "";
            }
            set
            {
                string upperValue = idx.ToUpper();
                if (!VariablesSet.Contains(upperValue)) 
                {
                    VariablesSet.Add(upperValue);
                }
                GlobalVariables[upperValue] = value;
            }
        }

        public bool HasMissingVariables()
        {
            return (VariablesUndefinedWhenRead.Count > 0);
        }

        public string[] GetMissingVariableNames()
        {
            return VariablesUndefinedWhenRead.OrderBy(x => x).ToArray<string>();
        }

        #endregion

        #region Trigonometric Base

        public enum TrigonometricBase { Degree, Radians, Grad };
        public TrigonometricBase TrigonometricMeasure = TrigonometricBase.Radians;

        #endregion

        #region FunctionList

        private FunctionList _flist = new FunctionList();

        public ExecutionContext()
        {
        }

        internal Functor GetFunction(string functionName)
        {
            foreach (var fkt in from FunctionDescriptor fd in _flist.GetList() select fd)
            {
                if (fkt.Name.ToLower() == functionName)
                {
                    return fkt.Functor;
                }
                if (fkt.Alternates != null)
                {
                    foreach (string s in fkt.Alternates)
                    {
                        if (s.ToLower() == functionName)
                        {
                            return fkt.Functor;
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        #region Core property

        public Core.ApplicationData Core
        {
            get
            {
                return GAPPSF.Core.ApplicationData.Instance;
            }
            private set
            {
            }
        }

        #endregion

    }
}
