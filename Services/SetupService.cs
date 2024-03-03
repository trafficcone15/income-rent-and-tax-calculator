using Android.App;
using Android.Content.Res;
using Android.Widget;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using System;

namespace IncomeRelatedRent.Services
{
    public class SetupService
    {
        private Activity _activity;

        private EditText _editTextSalary;
        private Spinner _spinnerFrequency;
        private EditText _editTextHoursWorkedPerWeek;
        private CheckBox _checkBoxStudentLoan;
        private CheckBox _checkBoxKiwiSaver;
        private TextView _kiwiSaverRateTextViewLabel;
        private CheckBox _checkboxCalculateIncomeRelatedRent;
        private CheckBox _checkBoxMarketRent;
        private EditText _editTextWeeklyRent;
        private Spinner _spinnerKiwiSaver;
        private Spinner _spinnerLivingSituation;
        private Spinner _spinnerOutputFrequency;
        private Button _buttonCalculate;
        private Button _buttonClearResults;
        private LinearLayout _linearLayoutError;
        private LinearLayout _linearLayoutResults;

        public SetupService(Activity activity)
        {
            _activity = activity;
        }

        public void InitializeUIElements()
        {
            _editTextSalary = _activity.FindViewById<EditText>(Resource.Id.editTextSalary);
            _spinnerFrequency = _activity.FindViewById<Spinner>(Resource.Id.spinnerFrequency);
            _editTextHoursWorkedPerWeek = _activity.FindViewById<EditText>(Resource.Id.editTextHoursWorkedPerWeek);
            _checkBoxStudentLoan = _activity.FindViewById<CheckBox>(Resource.Id.checkBoxStudentLoan);
            _checkBoxKiwiSaver = _activity.FindViewById<CheckBox>(Resource.Id.checkBoxKiwiSaver);
            _kiwiSaverRateTextViewLabel = _activity.FindViewById<TextView>(Resource.Id.kiwiSaverRateTextViewLabel);
            _checkboxCalculateIncomeRelatedRent = _activity.FindViewById<CheckBox>(Resource.Id.checkboxCalculateIncomeRelatedRent);
            _spinnerKiwiSaver = _activity.FindViewById<Spinner>(Resource.Id.spinnerKiwiSaver);
            _checkBoxMarketRent = _activity.FindViewById<CheckBox>(Resource.Id.checkBoxMarketRent);
            _editTextWeeklyRent = _activity.FindViewById<EditText>(Resource.Id.editTextWeeklyRent);
            _spinnerLivingSituation = _activity.FindViewById<Spinner>(Resource.Id.spinnerLivingSituation);
            _spinnerOutputFrequency = _activity.FindViewById<Spinner>(Resource.Id.spinnerOutputFrequency);
            _buttonCalculate = _activity.FindViewById<Button>(Resource.Id.buttonCalculate);
            _buttonClearResults = _activity.FindViewById<Button>(Resource.Id.buttonClearResults);
            _linearLayoutError = _activity.FindViewById<LinearLayout>(Resource.Id.linearLayoutError);
            _linearLayoutResults = _activity.FindViewById<LinearLayout>(Resource.Id.linearLayoutResults);

            ArrayAdapter adapterFrequency = ArrayAdapter<string>.CreateFromResource(
                _activity, Resource.Array.frequency_array, Android.Resource.Layout.SimpleSpinnerItem);
            ArrayAdapter adapterKiwiSaver = ArrayAdapter<string>.CreateFromResource(
                _activity, Resource.Array.kiwisaver_rate_array, Android.Resource.Layout.SimpleSpinnerItem);
            ArrayAdapter adapterLivingSituation = ArrayAdapter<string>.CreateFromResource(
                _activity, Resource.Array.living_situation_array, Android.Resource.Layout.SimpleSpinnerItem);
            ArrayAdapter adapterOutputFrequency = ArrayAdapter<string>.CreateFromResource(
                _activity, Resource.Array.output_frequency_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapterFrequency.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            adapterKiwiSaver.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            adapterLivingSituation.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            adapterOutputFrequency.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _spinnerFrequency.Adapter = adapterFrequency;
            _spinnerOutputFrequency.Adapter = adapterOutputFrequency;
            _spinnerKiwiSaver.Adapter = adapterKiwiSaver;
            _spinnerLivingSituation.Adapter = adapterLivingSituation;
        }

        public void InitializeEventHandlers()
        {
            _checkBoxKiwiSaver.CheckedChange += OnKiwiSaverCheckBoxChanged;
            _checkboxCalculateIncomeRelatedRent.CheckedChange += OnCaclulateIncomeRelatedRentCheckBoxChanged;
            _buttonClearResults.Click += ClearResults;
        }

        private void OnKiwiSaverCheckBoxChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                _spinnerKiwiSaver.Visibility = ViewStates.Visible;
                _kiwiSaverRateTextViewLabel.Visibility = ViewStates.Visible;
            }
            else
            {
                _spinnerKiwiSaver.Visibility = ViewStates.Gone;
                _kiwiSaverRateTextViewLabel.Visibility = ViewStates.Gone;
            }
        }

        private void OnCaclulateIncomeRelatedRentCheckBoxChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                _checkBoxMarketRent.Visibility = ViewStates.Visible;
            }
            else
            {
                _checkBoxMarketRent.Visibility = ViewStates.Gone;
            }
        }

        private void ClearResults(object sender, EventArgs e)
        {
            _linearLayoutResults.RemoveAllViews();
            _buttonClearResults.Visibility = ViewStates.Gone;
        }

        public void OnCalculateClickEvent()
        {
            IOService ioService = new IOService(_activity, _linearLayoutResults, _linearLayoutError);

            _buttonCalculate.Click += async (sender, e) =>
            {
                SaveUserSelections(double.TryParse(_editTextSalary.Text, out double salary) ? salary : 0,
                                   _spinnerFrequency.SelectedItemPosition,
                                   double.TryParse(_editTextHoursWorkedPerWeek.Text, out double hoursWorkedPerWeek) ? hoursWorkedPerWeek : 0,
                                   _checkBoxStudentLoan.Checked,
                                   _checkBoxKiwiSaver.Checked,
                                   _spinnerKiwiSaver.SelectedItemPosition,
                                   _checkboxCalculateIncomeRelatedRent.Checked,
                                   _checkBoxMarketRent.Checked,
                                   double.TryParse(_editTextWeeklyRent.Text, out double marketRent) ? marketRent : 0,
                                   _spinnerLivingSituation.SelectedItemPosition,
                                   _spinnerOutputFrequency.SelectedItemPosition);

                _linearLayoutError.RemoveAllViews();
                if (!ioService.ValidateInputs(_editTextSalary.Text, _editTextHoursWorkedPerWeek.Text, _checkBoxMarketRent.Checked, _editTextWeeklyRent.Text))
                    return;

                _buttonClearResults.Visibility = ViewStates.Visible;

                CalculationService calculationService = new CalculationService(double.Parse(_editTextSalary.Text),                                       // Input salary 
                                                                               (Enums.Frequency)_spinnerFrequency.SelectedItemPosition,                  // Input salary frequency
                                                                               double.Parse(_editTextHoursWorkedPerWeek.Text),                           // Hours worked per week
                                                                               _checkBoxKiwiSaver.Checked,                                               // Include KiwiSaver
                                                                               double.Parse(_spinnerKiwiSaver.SelectedItem.ToString().Replace("%", "")), // KiwiSaver rate
                                                                               _checkBoxStudentLoan.Checked,                                             // Include Student Loan
                                                                               _checkboxCalculateIncomeRelatedRent.Checked,                              // Include income related rent
                                                                               _checkBoxMarketRent.Checked,                                              // Include market rent
                                                                               double.Parse(_editTextWeeklyRent.Text),                                   // Weekly rent
                                                                               _spinnerLivingSituation.SelectedItem.ToString()                           // Living situation
                                                                               );

                await calculationService.CalculateRentAndDeductionsAsync();

                ioService.DisplayResults((Enums.Frequency)_spinnerFrequency.SelectedItemPosition,
                                         (Enums.Frequency)_spinnerOutputFrequency.SelectedItemPosition,
                                         calculationService.Salary,
                                         calculationService.HoursWorkedPerWeek,
                                         calculationService.WeeklySalary,
                                         calculationService.MTaxCodePAYE,
                                         calculationService.IncludeStudentLoan,
                                         calculationService.StudentLoanDeductions,
                                         calculationService.IncludeIncomeRelatedRent,
                                         calculationService.IncludeKiwiSaver,
                                         calculationService.KiwiSaverDeductions,
                                         calculationService.TotalDeductions,
                                         calculationService.NetSalary,
                                         calculationService.Rent,
                                         calculationService.NetSalaryMinusRent);
            };
        }

        public void LoadDefaultsOrPreviousSelections()
        {
            // Check if it's the first run
            var sharedPreferences = _activity.GetSharedPreferences("MyAppPreferences", FileCreationMode.Private);
            bool isFirstRun = sharedPreferences.GetBoolean("IsFirstRun", true);

            double salary;
            int frequencyIndex;
            double hoursWorkedPerWeek;
            bool studentLoan;
            bool kiwiSaver;
            int kiwiSaverRateIndex;
            bool includeIncomeRelatedRent;
            double weeklyRent;
            bool includeMarketRent;
            int livingSituationIndex;
            int outputFrequencyIndex;

            if (isFirstRun)
            {
                var editor = sharedPreferences.Edit();
                editor.PutBoolean("IsFirstRun", false);
                editor.Apply();

                (salary,
                 frequencyIndex,
                 hoursWorkedPerWeek,
                 studentLoan,
                 kiwiSaver,
                 kiwiSaverRateIndex,
                 includeIncomeRelatedRent,
                 includeMarketRent,
                 weeklyRent,
                 livingSituationIndex,
                 outputFrequencyIndex) = GetDefaultUserSelections();
            }
            else
            {
                (salary,
                 frequencyIndex,
                 hoursWorkedPerWeek,
                 studentLoan,
                 kiwiSaver,
                 kiwiSaverRateIndex,
                 includeIncomeRelatedRent,
                 includeMarketRent,
                 weeklyRent,
                 livingSituationIndex,
                 outputFrequencyIndex) = GetUserSelections();
            }

            // Set the EditText fields based on their values
            _editTextSalary.Text = salary == 0 ? "" : salary.ToString();
            _editTextHoursWorkedPerWeek.Text = hoursWorkedPerWeek == 0 ? "" : hoursWorkedPerWeek.ToString();
            _editTextWeeklyRent.Text = weeklyRent == 0 ? "" : weeklyRent.ToString();

            _spinnerFrequency.SetSelection(frequencyIndex);
            _checkBoxStudentLoan.Checked = studentLoan;
            _checkBoxKiwiSaver.Checked = kiwiSaver;
            _checkboxCalculateIncomeRelatedRent.Checked = includeIncomeRelatedRent;
            _checkBoxMarketRent.Checked = includeMarketRent;
            _spinnerKiwiSaver.SetSelection(kiwiSaverRateIndex);
            _spinnerLivingSituation.SetSelection(livingSituationIndex);
            _spinnerOutputFrequency.SetSelection(outputFrequencyIndex);
        }

        private (double salary, 
                 int frequencyIndex, 
                 double hoursWorkedPerWeek,
                 bool studentLoan, 
                 bool kiwiSaver, 
                 int kiwiSaverRateIndex,
                 bool includeIncomeRelatedRent,
                 bool inludeMarketRent, 
                 double weeklyRent, 
                 int livingSituationIndex, 
                 int outputFrequencyIndex) GetUserSelections()
        {
            var sharedPreferences = _activity.GetSharedPreferences("MyAppPreferences", FileCreationMode.Private);

            double salary = double.TryParse(sharedPreferences.GetString("Salary", "0"), out double tempSalary) ? tempSalary : 0;
            int frequencyIndex = sharedPreferences.GetInt("FrequencyIndex", 4);
            double hoursWorkedPerWeek = double.TryParse(sharedPreferences.GetString("HoursWorkedPerWeek", "40"), out double tempHoursWorkedPerWeek) ? tempHoursWorkedPerWeek : 0;
            bool studentLoan = sharedPreferences.GetBoolean("StudentLoan", false);
            bool kiwiSaver = sharedPreferences.GetBoolean("KiwiSaver", false);
            int kiwiSaverRateIndex = sharedPreferences.GetInt("KiwiSaverRateIndex", 0);
            bool includeIncomeRelatedRent = sharedPreferences.GetBoolean("CalculateIncomeRelatedRent", true);
            bool includeMarketRent = sharedPreferences.GetBoolean("IncludeMarketRent", true);
            double weeklyRent = double.TryParse(sharedPreferences.GetString("WeeklyRent", "0"), out double tempWeeklyRent) ? tempWeeklyRent : 0;
            int livingSituationIndex = sharedPreferences.GetInt("LivingSituationIndex", 0);
            int outputFrequencyIndex = sharedPreferences.GetInt("OutputFrequencyIndex", 1);

            return (salary, 
                    frequencyIndex,
                    hoursWorkedPerWeek,
                    studentLoan, 
                    kiwiSaver, 
                    kiwiSaverRateIndex,
                    includeIncomeRelatedRent,
                    includeMarketRent, 
                    weeklyRent, 
                    livingSituationIndex, 
                    outputFrequencyIndex);
        }

        private (double salary, 
                 int frequencyIndex,
                 double hoursWorkedPerWeek,
                 bool studentLoan, 
                 bool kiwiSaver, 
                 int kiwiSaverRateIndex,
                 bool includeIncomeRelatedRent,
                 bool includeMarketRent, 
                 double weeklyRent, 
                 int livingSituationIndex, 
                 int outputFrequencyIndex) GetDefaultUserSelections()
        {
            return (0,      // Set salary to 0                    
                    4,      // Set input frequency to Yearly
                    40,     // Set hours worked per week to 40 hours per week
                    false,  // Do not include Student Loan
                    false,  // Do not include KiwiSaver
                    0,      // Set KiwiSaver rate to 3%
                    true,   // Will calculate income related rent
                    true,   // Include market rent
                    0,      // Set market rent to 0
                    0,      // Set living situation to living by yourself
                    1       // Set output frequency to Weekly
                    ); // Default values for the first run.
        }

        public async Task CopyDatabaseIfNeededAsync(AssetManager assetManager)
        {
            var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "WeeklyPAYEDeductions.db");
            if (!File.Exists(dbPath))
            {
                // Use the passed AssetManager to open the asset
                using (var assetStream = assetManager.Open("WeeklyPAYEDeductions.db"))
                using (var fileStream = File.Create(dbPath))
                {
                    await assetStream.CopyToAsync(fileStream);
                }
            }
        }

        public void SaveUserSelections(double salary,
                                       int frequencyIndex,
                                       double hoursWorkedPerWeek,
                                       bool includeSudentLoan,
                                       bool includeKiwiSaver,
                                       int kiwiSaverRateIndex,
                                       bool includeIncomeRelatedRent,
                                       bool includeMarketRent,
                                       double weeklyRent,
                                       int livingSituationIndex,
                                       int outputFrequencyIndex)
        {
            var sharedPreferences = _activity.GetSharedPreferences("MyAppPreferences", FileCreationMode.Private);
            var editor = sharedPreferences.Edit();

            editor.PutString("Salary", salary == 0 ? "" : salary.ToString());
            editor.PutInt("FrequencyIndex", frequencyIndex);
            editor.PutString("HoursWorkedPerWeek", hoursWorkedPerWeek == 0 ? "" : hoursWorkedPerWeek.ToString());
            editor.PutBoolean("StudentLoan", includeSudentLoan);
            editor.PutBoolean("KiwiSaver", includeKiwiSaver);
            editor.PutInt("KiwiSaverRateIndex", kiwiSaverRateIndex);
            editor.PutBoolean("CalculateIncomeRelatedRent", includeIncomeRelatedRent);
            editor.PutBoolean("IncludeMarketRent", includeMarketRent);
            editor.PutString("WeeklyRent", weeklyRent == 0 ? "" : weeklyRent.ToString());
            editor.PutInt("LivingSituationIndex", livingSituationIndex);
            editor.PutInt("OutputFrequencyIndex", outputFrequencyIndex);

            editor.Apply();
        }
    }
}