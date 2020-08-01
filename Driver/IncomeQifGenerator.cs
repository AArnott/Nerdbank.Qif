using QifApi;
using QifApi.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppliedHyperkinetics.Qif.Driver
{
    public class IncomeQifGenerator
    {
        private const string REINVEST_DIV_ACTION = "ReinvDiv";
        private const string SHARES_IN_ACTION = "ShrsIn";
        public const string MKT_VALUE_SHARES_IN_MEMO = "Change on Market Value";

        /// <summary>
        /// Given a QifDom object containing Dividend Buy transactions, extract the Dividends
        /// and generate a QifDom object containing Dividend Income transactions for them.
        /// </summary>
        public QifDom GenerateIncomeTransactionsForDividends(QifDom inFile)
        {
            QifDom outFile = new QifDom();

            var dividendTransactions = inFile.InvestmentTransactions.Where(t => t.Memo.Trim() == "Dividends");

            foreach (var divTx in dividendTransactions)
            {
                var incTx = new InvestmentTransaction()
                {
                    Date = divTx.Date,
                    Action = "Div",
                    Security = divTx.Security,
                    Price = divTx.Price,
                    Quantity = divTx.Quantity,
                    TransactionAmount = divTx.TransactionAmount,
                    Memo = "Dividend income [generated transaction from qif Buy]"
                };

                outFile.InvestmentTransactions.Add(incTx);
            }

            return outFile;
        }

        /// <summary>
        /// Given a QifDom object containing Dividend Buy transactions, extract the Dividends
        /// and generate a QifDom object containing Dividend Income transactions for them.
        /// </summary>
        /// <param name="inFile"></param>
        /// <returns></returns>
        public QifDom ConvertDividendBuyToReinvest(QifDom inFile)
        {
            QifDom outFile = new QifDom();

            Console.WriteLine($"{nameof(ConvertDividendBuyToReinvest)}: {inFile.InvestmentTransactions.Count()} transactions for processing in input file");
            foreach (var tx in inFile.InvestmentTransactions)
            {
                if (tx.Memo.Trim() == "Dividends")
                {
                    if (tx.Action == "Buy" || tx.Action == "Div")
                    {
                        Console.WriteLine($"{nameof(ConvertDividendBuyToReinvest)}: {tx.Date} {tx.Security} {tx.Action} converting to {REINVEST_DIV_ACTION}");
                        tx.Memo = $"{tx.Memo.Trim()} [converted {tx.Action} -> {REINVEST_DIV_ACTION}]";
                        tx.Action = $"{REINVEST_DIV_ACTION}";
                    }
                }

                outFile.InvestmentTransactions.Add(tx);            
            }

            Console.WriteLine($"{nameof(ConvertDividendBuyToReinvest)}: {outFile.InvestmentTransactions.Count()} transactions being written to transformed output file");
            return outFile;
        }

        /// <summary>
        /// Converts transactions with no action and a specific memo string <see cref="MKT_VALUE_SHARES_IN_MEMO"/> indicating that 
        /// shares were added as a result of market value changes to a SharesIn transaction. Quicken ignores null actions, so why 
        /// Fidelity uses that is anybody's guess. 
        /// </summary>
        /// <param name="inFile">input <see cref="QifDom"/> object</param>
        /// <returns>Transformed <see cref="QifDom"/> object </returns>
        public QifDom ConvertMktValueChangeToSharesIn(QifDom inFile)
        {
            QifDom outFile = new QifDom();

            Console.WriteLine($"{nameof(ConvertMktValueChangeToSharesIn)}: {inFile.InvestmentTransactions.Count()} transactions for processing in input file");
            foreach (var tx in inFile.InvestmentTransactions)
            {
                if (tx.Memo.Trim() == MKT_VALUE_SHARES_IN_MEMO)
                {
                    if (tx.Action is null || tx.Action == "null")
                    {
                        Console.WriteLine($"{nameof(ConvertMktValueChangeToSharesIn)}: {tx.Date} {tx.Security} {tx.Action} converting to {SHARES_IN_ACTION}");
                        tx.Memo = $"{tx.Memo.Trim()} [converted {tx.Action} -> {SHARES_IN_ACTION}]";
                        tx.Action = $"{SHARES_IN_ACTION}";
                    }
                }

                outFile.InvestmentTransactions.Add(tx);
            }

            Console.WriteLine($"{nameof(ConvertMktValueChangeToSharesIn)}: {outFile.InvestmentTransactions.Count()} transactions being written to transformed output file");
            return outFile;
        }
    }
}
