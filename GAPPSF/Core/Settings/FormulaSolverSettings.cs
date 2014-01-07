using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int FormulaSolverWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int FormulaSolverWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int FormulaSolverWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int FormulaSolverWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public string GetFormula(string gcCode)
        {
            return _settingsStorage.GetFormula(gcCode);
        }
        public void SetFormula(string gcCode, string formula)
        {
            _settingsStorage.SetFormula(gcCode, formula);
        }

    }
}
