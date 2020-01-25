
using System;
using System.Collections.Generic;
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
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.MainActivity);

            Button button = FindViewById<Button>(Resource.Id.buttonLogout);
            button.Click += ButtonLogoutClicked;

            Models.UserRecord userRecord = Models.userRecord;
            TextView tvWelcome = FindViewById<TextView>(Resource.Id.welcomeMA);
            tvWelcome.Text = "Welcome to Leany Heating\n" + userRecord.FirstName + " " + userRecord.LastName;

            ReadControlState();



        }

        // read database values for controls
        private async void ReadControlState()
        {
            RestService _restService;
            _restService = new RestService();
            string requestUri = Constants.apiMarkGriffithsEndpoint;

            // get the user record from email address
            requestUri += "/listArduino";
            List<Models.ArduinoRecord> dataPoints = (await _restService.GetArduinoRecordsByIDasync(requestUri, Models.userID));

            return;
        }

        private void ButtonLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("userEmail");
            Preferences.Remove("userPassword");

            StartActivity(typeof(LoginActivity));
        }
    }
}