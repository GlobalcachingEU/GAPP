using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions
{
    public class CharMapAlphabet: ICharacterMapper
    {
        public int GetCharacterCode(char character) 
        {
            char c = Char.ToUpper(character);
            if ((c >= 'A') && (c <= 'Z'))
            {
                return c - 'A' + 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
