using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class PrimeNumber: Functor
    {
        public static UInt64 FindNthPrimeNumber(UInt64 upperLimit, UInt32 nth)
        {
            // Jede Zahl, die nicht Primzahl ist, hat (mindestens) einen Primteiler
            // Der kleinste Primteiler einer Zahl ist höchstens die Wurzel der Zahl selbst

            List<UInt64> primes = new List<UInt64>();
            primes.Add(2);

            // primteiler durchläuft die gefundenen Primzahlen
            UInt64 primteiler;
            // testlimit ist die Grenze, bis zu der die Teilbarkeit geprüft werden muss
            double testlimit;
            // laufindex ist die Stelle an der wir uns in der Primzahl-Liste befinden
            int laufindex;
            // isPrim bleibt solange wahr, bis ein Primteiler gefunden wird
            bool isPrim;

            if (nth == 1) { return 2; }
            UInt32 count = 1;

            try
            {
                // überprüft werden alle ungeraden Zahlen >= 3
                // gerade Zahlen < 2 haben die 2 als Teiler uns sind deswegen keine Primzahlen

                for (UInt64 zahl = 3; zahl <= upperLimit; zahl += 2)
                {
                    // zunächst wird für jede Zahl angenommen sie sei eine Primzahl
                    isPrim = true;
                    // Wir starten bei der drei als Primteiler, weil wir gegen 2 als Primteiler nicht prüfen müssen, da wir nur ungerade Zahlen testen
                    primteiler = 3;
                    // Weil der neue Primteiler erst am Ende der Schleife festgelegt wird, startet startet der Laufindex bei 2 (der dritten Primzahl).
                    laufindex = 2;
                    // wie oben schon erwähnt müssen wir nur bis Wurzel zahl prüfen
                    testlimit = Math.Sqrt(zahl);

                    while (primteiler <= testlimit)
                    {
                        // Der Modulo ist gleich 0, wenn primteiler ein Teiler von zahl ist. Dann ist zahl keine Primzahl und die Schleife wird abgebrochen.
                        if (zahl % primteiler == 0)
                        {
                            isPrim = false;
                            break;
                        }
                        primteiler = primes[laufindex++];
                    }
                    if (isPrim) {
                        primes.Add(zahl);
                        count++;
                        if (count == nth) {
                            return zahl;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Write(e.Message);
            }
            return 0;
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            UInt64? nth = null;

            checker.CheckForNumberOfArguments(ref args, 1, null);
            nth = checker.GetRangedUInt64(ref args[0], 0, 664579);

            return (nth != null)? FindNthPrimeNumber(10000000, Convert.ToUInt32(nth)).ToString(): "";
        }
    }
}
