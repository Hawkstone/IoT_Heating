
using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Xamarin.Essentials;

namespace IoT.Droid
{
    [Activity(Label = "MainActivity", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MainActivity);

            Button button = FindViewById<Button>(Resource.Id.buttonLogout);
            button.Click += ButtonLogoutClicked;

            Models.UserRecord userRecord = Models.userRecord;
            TextView tvWelcome = FindViewById<TextView>(Resource.Id.welcomeMA);
            tvWelcome.Text = "Welcome " + userRecord.FirstName + " " + userRecord.LastName; 
        }

        private void ButtonLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("userEmail");
            Preferences.Remove("userPassword");

            StartActivity(typeof(LoginActivity));
        }
    }
}