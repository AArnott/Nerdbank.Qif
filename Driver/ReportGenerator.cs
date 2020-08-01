using QifApi.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppliedHyperkinetics.Qif.Driver
{
    public class ReportGenerator
    {
        private IEnumerable<InvestmentTransaction> _transactions;
        private bool _showMonthlyTotals = false;
        private bool _showSecurityTotals = false;
        private bool _showIndividualTransactions = false;

        public ReportGenerator(
            IEnumerable<InvestmentTransaction> transactions, 
            bool showMonthlyTotals = false, 
            bool showSecurityTotals = false, 
            bool showIndividualTransactions = false)
        {
            _showMonthlyTotals = showMonthlyTotals;
            _showSecurityTotals = showSecurityTotals;
            _showIndividualTransactions = showIndividualTransactions;
            _transactions = transactions;
        }

        public void Generate()
        {
            Dictionary<string, List<InvestmentTransaction>> monthlyBuckets = new Dictionary<string, List<InvestmentTransaction>>();
            Dictionary<string, List<InvestmentTransaction>> securityBuckets = new Dictionary<string, List<InvestmentTransaction>>();
            
            foreach (var tx in _transactions)
            {
                if (_showIndividualTransactions)
                {
                    //Console.WriteLine(
                    //    $"{tx.Security} dividend of {tx.TransactionAmount:C2} on {tx.Date:d}");
                    Console.WriteLine(
                        $"{tx.Date:d}|{tx.TransactionAmount:C2}|{tx.Security}|{tx.Action}");
                }
                var monthkey = $"{tx.Date.Year}-{tx.Date.Month:D2}";

                if (!monthlyBuckets.ContainsKey(monthkey))
                {
                    monthlyBuckets[monthkey] = new List<InvestmentTransaction>();
                }

                monthlyBuckets[monthkey].Add(tx);

                if (!securityBuckets.ContainsKey(tx.Security))
                {
                    securityBuckets[tx.Security] = new List<InvestmentTransaction>();
                }

                securityBuckets[tx.Security].Add(tx);
            }

            var year = _transactions.FirstOrDefault()?.Date.Year;

            var yearlyTotal = monthlyBuckets.Values.Sum(v => v.Sum(t => t.TransactionAmount));

            Console.WriteLine();
            Console.WriteLine($"Total Dividends: {yearlyTotal:C2}");
            
            if (_showMonthlyTotals)
            {
                Console.WriteLine();
                Console.WriteLine("Total Dividends by month:");
                Console.WriteLine("-------------------------");
                foreach (var month in monthlyBuckets)
                {
                    var monthlyTotal = month.Value.Sum(t => t.TransactionAmount);
                    Console.WriteLine($"{month.Key} dividends: {monthlyTotal:C2}");
                }
                Console.WriteLine();
            }

            if (_showSecurityTotals)
            {
                Console.WriteLine();
                Console.WriteLine("Dividends by security:");
                Console.WriteLine("-----------------------------");
                foreach (var security in securityBuckets)
                {
                    var securityTotal = security.Value.Sum(t => t.TransactionAmount);
                    Console.WriteLine($"{security.Key} dividends: {securityTotal:C2}");
                }
            }
        }
    }
}
