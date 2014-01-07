using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions
{
    public class CharMapPhoneCode : ICharacterMapper
    {
        private static string[] map = { " ", "ABC", "DEF", "GHI", "JKL", "MNO", "PQRS", "TUV", "WXYZ" };

        public int GetCharacterCode(char character)
        {
            char c = Char.ToUpper(character);
            for (int i = 0; i < map.Length; ++i)
            {
                if (map[i].IndexOf(c) >= 0)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
