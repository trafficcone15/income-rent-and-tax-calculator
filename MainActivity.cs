using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using IncomeRelatedRent.Services;

namespace IncomeRelatedRent
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            SetupService setupService = new SetupService(this);

            setupService.InitializeUIElements();
            setupService.InitializeEventHandlers();
            setupService.LoadDefaultsOrPreviousSelections();

            await setupService.CopyDatabaseIfNeededAsync(Assets);

            setupService.OnCalculateClickEvent();
        }
    }
}