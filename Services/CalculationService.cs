using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IncomeRelatedRent.Models;
using IncomeRelatedRent.Data;
using IncomeRelatedRent.Enums;

namespace IncomeRelatedRent.Services
{
    public class CalculationService
    {
        private readonly DatabaseHelper _taxDataHelper;
        private string _dbPath;

        public double Salary { get; private set; }
        public Frequency Frequency { get; private set; }
        public double HoursWorkedPerWeek { get; private set; }
        public bool IncludeKiwiSaver { get; private set; }
        public double KiwiSaverRate { get; private set; }
        public bool IncludeStudentLoan { get; private set; }
        public bool IncludeIncomeRelatedRent { get; private set; }
        public bool IncludeMarketRent { get; private set; }
        public double WeeklyRent { get; private set; }
        public string LivingSituation { get; private set; }

        public double WeeklySalary { get; private set; }
        public double MTaxCodePAYE { get; private set; }
        public double StudentLoanDeductions { get; private set; }
        public double KiwiSaverDeductions { get; private set; }
        public double TotalDeductions { get; private set; }
        public double Rent { get; private set; }
        public double NetSalary { get; private set; }
        public double NetSalaryMinusRent { get; private set; }

        public CalculationService(
            double salary, 
            Frequency frequency, 
            double hoursWorkedPerWeek,
            bool includeKiwiSaver, 
            double kiwiSaverRate, 
            bool includeStudentLoan,
            bool includeIncomeRelatedRent,
            bool includeMarketRent, 
            double weeklyRent, 
            string livingSituation
            )
        {
            _dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "WeeklyPAYEDeductions.db");
            _taxDataHelper = new DatabaseHelper(_dbPath);

            Salary = salary;
            Frequency = frequency;
            HoursWorkedPerWeek = hoursWorkedPerWeek;
            IncludeKiwiSaver = includeKiwiSaver;
            KiwiSaverRate = kiwiSaverRate / 100;
            IncludeStudentLoan = includeStudentLoan;
            IncludeIncomeRelatedRent = includeIncomeRelatedRent;
            IncludeMarketRent = includeMarketRent;
            WeeklyRent = weeklyRent;
            LivingSituation = livingSituation;
        }

        public async Task CalculateRentAndDeductionsAsync()
        {
            WeeklySalary = ConvertSalaryToWeekly();

            List<TaxDeductionsData> taxDataList = await _taxDataHelper.GetTaxDataAsync();

            TaxDeductionsData closestEarning = FindClosestEarning(WeeklySalary, taxDataList);

            (MTaxCodePAYE, StudentLoanDeductions, KiwiSaverDeductions) = GetDeductionsFromTaxData(WeeklySalary, closestEarning, IncludeStudentLoan, IncludeKiwiSaver, KiwiSaverRate);

            TotalDeductions = MTaxCodePAYE + StudentLoanDeductions + KiwiSaverDeductions;
            Rent = IncludeIncomeRelatedRent ? CalculateIncomeRelatedRent(WeeklySalary - MTaxCodePAYE, LivingSituation, IncludeMarketRent, WeeklyRent) : WeeklyRent;
            NetSalary = WeeklySalary - TotalDeductions;
            NetSalaryMinusRent = NetSalary - Rent;
        }

        private double ConvertSalaryToWeekly()
        {
            double weeklySalary = 0;

            switch (Frequency)
            {
                case Frequency.Hourly:
                    weeklySalary = Salary * HoursWorkedPerWeek;
                    break;
                case Frequency.Weekly:
                    weeklySalary = Salary;
                    break;
                case Frequency.Fortnightly:
                    weeklySalary = Salary / 2;
                    break;
                case Frequency.Monthly:
                    weeklySalary = Salary / ((365.25 / 12) / 7);
                    break;
                case Frequency.Yearly:
                    weeklySalary = Salary / 52;
                    break;
            }

            return weeklySalary;
        }

        private TaxDeductionsData FindClosestEarning(double weeklySalary, List<TaxDeductionsData> taxDataList)
        {
            var closestEarning = taxDataList.OrderBy(taxData => Math.Abs(weeklySalary - taxData.Earnings)).FirstOrDefault();

            if (closestEarning == null)
            {
                // Handle the case when the taxDataList is empty.
                throw new InvalidOperationException("Tax data is not available.");
            }

            return closestEarning;
        }

        private (double, double, double) GetDeductionsFromTaxData(double weeklySalary, TaxDeductionsData taxData, bool includeStudentLoan, bool includeKiwiSaverRate, double kiwiSaverRate)
        {
            double mTaxCodePAYE = 0;
            double studentLoan = 0;
            double kiwiSaver = 0;

            if (weeklySalary <= 3846)
            {
                mTaxCodePAYE = taxData.MTaxCodePAYE;
                studentLoan = includeStudentLoan ? taxData.StudentLoanDeductions : 0;

                if (includeKiwiSaverRate)
                {
                    kiwiSaverRate *= 100;
                }

                Dictionary<double, Func<TaxDeductionsData, double>> kiwiSaverRates = new Dictionary<double, Func<TaxDeductionsData, double>>
                {
                    { 3, taxData => taxData.KiwiSaverDeductionsAtThreePercent },
                    { 4, taxData => taxData.KiwiSaverDeductionsAtFourPercent },
                    { 6, taxData => taxData.KiwiSaverDeductionsAtSixPercent },
                    { 8, taxData => taxData.KiwiSaverDeductionsAtEightPercent },
                    { 10, taxData => taxData.KiwiSaverDeductionsAtTenPercent },
                };

                if (includeKiwiSaverRate && kiwiSaverRates.ContainsKey(kiwiSaverRate))
                {
                    kiwiSaver = kiwiSaverRates[kiwiSaverRate](taxData);
                }
            }
            else
            {
                // Calculate PAYE for earnings above $3,846
                double excessEarnings = weeklySalary - 3846;
                mTaxCodePAYE = 1158.64 + (excessEarnings * 0.39);

                if (includeStudentLoan)
                {
                    studentLoan = 408.84 + (excessEarnings * 0.12);
                }

                if (includeKiwiSaverRate)
                {
                    kiwiSaver = weeklySalary * kiwiSaverRate;
                }
            }

            return (mTaxCodePAYE, studentLoan, kiwiSaver);
        }

        private double CalculateIncomeRelatedRent(double weeklyIncomeMinusPAYE, string livingSituation, bool includeMarketRent, double marketRent)
        {
            Dictionary<string, double> livingSituationThresholds = new Dictionary<string, double>
            {
                { "Living by yourself", 496.37 },
                { "Single (without children) and living with others but you’re the only person on the tenancy agreement", 496.37 },
                { "Single with children", 763.64 },
                { "Living with your partner (with or without children)", 763.64 },
                { "Living with others (who are not your partner or your children), and there is at least one other person on the tenancy agreement", 763.64 }
            };

            double incomeThreshold = livingSituationThresholds[livingSituation];
            double incomeRelatedRent;

            if (weeklyIncomeMinusPAYE > incomeThreshold)
            {
                incomeRelatedRent = (incomeThreshold * 0.25) + ((weeklyIncomeMinusPAYE - incomeThreshold) * 0.5);
            }
            else
            {
                incomeRelatedRent = weeklyIncomeMinusPAYE * 0.25;
            }

            if (includeMarketRent)
            {
                if (incomeRelatedRent >= marketRent)
                {
                    incomeRelatedRent = marketRent;
                }
            }

            return Math.Round(incomeRelatedRent, MidpointRounding.ToEven);
        }
    }
}