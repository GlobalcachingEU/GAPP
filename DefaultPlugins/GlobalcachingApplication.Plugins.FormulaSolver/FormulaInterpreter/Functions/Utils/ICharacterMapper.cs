using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions
{
    interface ICharacterMapper
    {
        int GetCharacterCode(char character);
    }
}
