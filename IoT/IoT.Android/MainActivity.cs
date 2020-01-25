
using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Xamarin.Essentials;

namespace IoT.Droid
{
    [Activity(Label = "MainActivity", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        ToggleButton tb;
        SeekBar sb;
        TextView tvSet;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.MainActivity);

            // display user info
            Models.UserRecord userRecord = Models.userRecord;
            TextView tvWelcome = FindViewById<TextView>(Resource.Id.welcomeMA);
            tvWelcome.Text = "Welcome to Leany Heating\n" + userRecord.FirstName + " " + userRecord.LastName;

            // setup event listeners
            tb = FindViewById<ToggleButton>(Resource.Id.togButSystemState);
            tb.CheckedChange += ToggleButtonCheckedChanged;

            tvSet = FindViewById<TextView>(Resource.Id.textViewSetTemp);

            sb = FindViewById<SeekBar>(Resource.Id.seekBarTemp);
            sb.ProgressChanged += SeekBarProgressChanged;

            Button button = FindViewById<Button>(Resource.Id.buttonLogout);
            button.Click += ButtonLogoutClicked;

            ReadControlState();
        }

        private void SeekBarProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            tvSet.Text = sb.Progress.ToString();
        }

        private void ToggleButtonCheckedChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked) { tb.SetBackgroundColor(Color.ParseColor(Constants.systemOnBackColor)); }
            else { tb.SetBackgroundColor(Color.ParseColor(Constants.systemOffBackColor)); }
        }

        // read database values for controls
        private async void ReadControlState()
        {
            RestService _restService = new RestService();
            string requestUri = Constants.apiMarkGriffithsEndpoint;

            // get the logged in users' Arduino records
            requestUri += "/listArduino";
            var dataPoints = new List<Models.ArduinoRecord>(await _restService.GetArduinoRecordsByIDasync(requestUri, Models.userID));

            // system state
            tb = FindViewById<ToggleButton>(Resource.Id.togButSystemState);
            Models.ArduinoRecord aRec = dataPoints.Find(x => x.ValueName == "systemState");
            if (aRec != null)
            {
                if (aRec.ValueString == "on")
                { tb.Checked = true; }
                else { tb.Checked = false; }
            }

            // temperature setting
            
            aRec = dataPoints.Find(x => x.ValueName == "setTemperature");
            if (aRec != null)
            {
                tvSet.Text = aRec.ValueInt.ToString();

                sb = FindViewById<SeekBar>(Resource.Id.seekBarTemp);
                Int32.TryParse(aRec.ValueInt.ToString(), out int progress);
                sb.Progress = progress;
            }

            // current temperature 
            TextView tv = FindViewById<TextView>(Resource.Id.textViewCurrentTemp);
            aRec = dataPoints.Find(x => x.ValueName == "currentTemperature");
            if (aRec != null)
            { tv.Text = aRec.ValueInt.ToString(); }

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