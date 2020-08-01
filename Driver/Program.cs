using QifApi;
using System;
using System.IO;
using System.Linq;

namespace AppliedHyperkinetics.Qif.Driver
{
    /// <summary>
    /// Shell driver for converting some specific transations that Fidelity netbenefits QIF export
    /// generates and that Quicken can't import correctly. 
    /// Conversions implemented:
    /// <list type="">
    ///     <item>Dividend reinvestments that have incorrect Buy action are converted to ReinvDiv action based on memo value Dividend
    ///     and action of Buy or Add</item>   
    ///     <item>Change in market value transactions with null action and memo value are converted from null action
    ///     to SharesIn action. Quicken ignores null actions on import. </item>
    /// </list>
    /// Command line params can be used to generate reports on the state of the file without converting. 
    /// <see cref="Usage"/> for details. 
    /// </summary>
    public class Program
    {
        static int Main(string[] args)
        {
            bool showMonthlyTotals = false;
            bool showSecurityTotals = false;
            bool showIndividualTransactions = false;
            bool generateIncomeQifFile = false;

            if (args.Length == 0 || args[0] == "-h" || args[0] == "/?")
            {
                Usage();
                return 0;
            }

            var importFD = args[0];

            for (int i = 1; i < args.Length; ++i)
            {
                if (args[i] == "-m") showMonthlyTotals = true;
                if (args[i] == "-s") showSecurityTotals = true;
                if (args[i] == "-i") showIndividualTransactions = true;
                if (args[i] == "-q") generateIncomeQifFile = true;
            }

            var qifDoc = QifDom.ImportFile(importFD);

            var dividendTransactions = qifDoc.InvestmentTransactions.Where(t => t.Memo.Trim() == "Dividends");

            var reporter = new ReportGenerator(
                dividendTransactions,
                showMonthlyTotals,
                showSecurityTotals,
                showIndividualTransactions);
            reporter.Generate();

            if (generateIncomeQifFile)
            {
                var newFileName = Path.GetFileNameWithoutExtension(importFD) + "_fixed" + Path.GetExtension(importFD);
                var outFD = Path.Combine(Path.GetDirectoryName(importFD), newFileName);
                if (File.Exists(outFD))
                {
                    Console.WriteLine($"Deleting existing copy of transformed output file {outFD}");
                    File.Delete(outFD);
                }

                Console.WriteLine();
                Console.WriteLine($"Generating DivInc QIF file {outFD}");

                IncomeQifGenerator qifGenerator = new IncomeQifGenerator();
                Console.WriteLine($"Converting dividend reinvestments");
                var reinvDivDoc = qifGenerator.ConvertDividendBuyToReinvest(qifDoc);

                Console.WriteLine($"Converting Mkt value change to SharesIn");
                var sharesInDoc = qifGenerator.ConvertMktValueChangeToSharesIn(reinvDivDoc);

                Console.WriteLine($"Writing generated DivInc qif file to {outFD}");
                sharesInDoc.Export(outFD);
            }
            Console.WriteLine("==================================================");
            Console.WriteLine();
            return 0;
        }

        public static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("QifDividendFix {fileDescriptor} [-s] [-m] [-i]");
            Console.WriteLine("\t{fileDescriptor} must be a single file");
            Console.WriteLine("\t-s displays per security totals for the run");
            Console.WriteLine("\t-m displays per month totals for the run");
            Console.WriteLine("\t-i displays details of each processed transaction");
            Console.WriteLine("\t-q generates a new QIF file with dividend Actions transformed from Buy -> ReinvDiv and market value change transactions from null action -> SharesIn");
        }
    }
}
