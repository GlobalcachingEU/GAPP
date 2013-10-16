using System;
using System.Collections.Generic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class PrimeIndex: Functor
    {
        private UInt32 FindNumberOfPrime(UInt64 value)
        {
            if (value == 2) { return 1; }

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

            try
            {
                // überprüft werden alle ungeraden Zahlen >= 3
                // gerade Zahlen < 2 haben die 2 als Teiler uns sind deswegen keine Primzahlen

                for (UInt64 zahl = 3; zahl <= value; zahl += 2)
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
                    if (isPrim)
                    {
                        primes.Add(zahl);
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
            }
            return (primes[primes.Count - 1] == value)? (UInt32)primes.Count: 0;
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            UInt64? value = null;
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            value = checker.GetRangedUInt64(ref args[0], 2, 10000000);
            return (value != null)? FindNumberOfPrime(Convert.ToUInt64(value)): 0;
        }
    }
}
