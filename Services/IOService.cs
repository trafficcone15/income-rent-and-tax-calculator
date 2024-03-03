using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using IncomeRelatedRent.Enums;

namespace IncomeRelatedRent.Services
{
    public class IOService
    {
        private Activity _activity;

        private LinearLayout _linearLayoutResults;
        private LinearLayout _linearLayoutError;

        public IOService(Activity activity, LinearLayout linearLayoutResults, LinearLayout linearLayoutError)
        {
            _activity = activity;
            _linearLayoutResults = linearLayoutResults;
            _linearLayoutError = linearLayoutError;
        }

        public bool ValidateInputs(string salary, string hoursWorkedPerWeek, bool includeMarketRent, string marketRent)
        {
            bool inputsValidator = true;
            double salaryValidator,
                   marketRentValidator,
                   hoursWorkedPerWeekValidator;

            if (string.IsNullOrEmpty(salary))
            {
                ShowError("No salary detected. Please enter your salary.");
                inputsValidator = false;
            }
            else if ((!double.TryParse(salary, out salaryValidator) || salaryValidator <= 0))
            {
                ShowError("Invalid salary. Please enter a valid salary.");
                inputsValidator = false;
            }

            if (string.IsNullOrEmpty(hoursWorkedPerWeek))
            {
                ShowError("No hours worked per week detected. Please enter your hours worked per week.");
                inputsValidator = false;
            }
            else if ((!double.TryParse(hoursWorkedPerWeek, out hoursWorkedPerWeekValidator) || hoursWorkedPerWeekValidator <= 0))
            {
                ShowError("Invalid hours worked per week. Please enter a valid hours worked per week.");
                inputsValidator = false;
            }

            if (includeMarketRent)
            {
                if (string.IsNullOrEmpty(marketRent))
                {
                    ShowError("No market rent detected. Please enter the market rent amount.");
                    inputsValidator = false;
                }
                else if (!double.TryParse(marketRent, out marketRentValidator) || marketRentValidator <= 0)
                {
                    ShowError("Invalid market rent. Please enter a valid market rent amount.");
                    inputsValidator = false;
                }
            }

            return inputsValidator;
        }

        public void DisplayResults(Frequency inputFrequency, 
                                   Frequency outputFrequency, 
                                   double salary,
                                   double hoursWorkedPerWeek,
                                   double weeklySalary, 
                                   double mTaxCodePAYE, 
                                   bool includeStudentLoan, 
                                   double studentLoanDeductions,
                                   bool includeIncomeRelatedRent,
                                   bool includeKiwiSaver,
                                   double kiwiSaverDeductions,
                                   double totalDeductions,
                                   double netSalary,
                                   double rent,
                                   double netSalaryMinusRent)
        {
            double multiplier = 1;

            switch (outputFrequency)
            {
                case Frequency.Hourly:
                    multiplier = 1.0 / hoursWorkedPerWeek;
                    break;
                case Frequency.Weekly:
                    multiplier = 1;
                    break;
                case Frequency.Fortnightly:
                    multiplier = 2;
                    break;
                case Frequency.Monthly:
                    multiplier = 4;
                    break;
                case Frequency.Yearly:
                    multiplier = 52;
                    break;
            }

            TextView inputSalaryLabel = new TextView(_activity)
            {
                Text = $"Input salary: {(salary).ToString("C")} " + $"{inputFrequency.ToString()}",
                TextSize = 16
            };
            inputSalaryLabel.SetTypeface(null, TypefaceStyle.Bold);


            TextView calculatedLabel = new TextView(_activity)
            {
                Text = $"Calculated {outputFrequency.ToString()}",
                TextSize = 16
            };
            calculatedLabel.SetTypeface(null, TypefaceStyle.Italic);

            TextView newResults = new TextView(_activity)
            {
                Text = $"Gross salary: {(weeklySalary * multiplier).ToString("C")}\n" +
                       $"Hours worked: {(hoursWorkedPerWeek * multiplier).ToString()}\n" +
                       $"PAYE: {(mTaxCodePAYE * multiplier).ToString("C")}\n" +
                       (includeStudentLoan ? $"Student loan: {(studentLoanDeductions * multiplier).ToString("C")}\n" : "") +
                       (includeKiwiSaver ? $"KiwiSaver: {(kiwiSaverDeductions * multiplier).ToString("C")}\n" : "") +
                       $"Total deductions: {(totalDeductions * multiplier).ToString("C")}\n" +
                       $"Net salary: {(netSalary * multiplier).ToString("C")}\n" +
                       (includeIncomeRelatedRent ? "Income related rent: ": "Rent: ") + $"{(rent * multiplier).ToString("C")}\n" +
                       $"Net salary minus rent: {(netSalaryMinusRent * multiplier).ToString("C")}\n",
                TextSize = 16
            };

            LinearLayout singleResultLayout = new LinearLayout(_activity)
            {
                Orientation = Orientation.Vertical
            };

            singleResultLayout.AddView(inputSalaryLabel);
            singleResultLayout.AddView(calculatedLabel);
            singleResultLayout.AddView(newResults);

            _linearLayoutResults.AddView(singleResultLayout, 0); // Add the new result layout to the top
        }

        private void ShowError(string errorMessage)
        {
            TextView showErrorMessage= new TextView(_activity)
            {
                Text = $"{errorMessage}",
                TextSize = 16
            };
            showErrorMessage.SetTextColor(Color.Red);
            _linearLayoutError.AddView(showErrorMessage);
            _linearLayoutError.Visibility = ViewStates.Visible;
        }
    }
}