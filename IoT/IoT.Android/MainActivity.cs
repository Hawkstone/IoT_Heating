
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        List<Models.ArduinoRecord> dataPoints;

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

        private async void ToggleButtonCheckedChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            SetSystemState(e.IsChecked);
            bool x = await WriteControlsState("systemState");
        }

        private void SetSystemState(bool isChecked)
        {
            if (isChecked)
            {
                tb.Checked = true;
                tb.SetBackgroundColor(Color.ParseColor(Constants.systemOnBackColor));
                tb.Text = "System is on";
            }
            else
            {
                tb.Checked = false;
                tb.SetBackgroundColor(Color.ParseColor(Constants.systemOffBackColor));
                tb.Text = "System is off";
            }
        }

        // read database values for controls
        private async void ReadControlState()
        {
            RestService _restService = new RestService();
            string requestUri = Constants.apiMarkGriffithsEndpoint;

            // get the logged in users' Arduino records
            requestUri += "/listArduino";
            dataPoints = new List<Models.ArduinoRecord>(await _restService.GetArduinoRecordsByIDasync(requestUri, Models.userID));

            // system state
            tb = FindViewById<ToggleButton>(Resource.Id.togButSystemState);
            Models.ArduinoRecord aRec = dataPoints.Find(x => x.ValueName == "systemState");
            if (aRec != null)
            {
                if (aRec.ValueString == "on")
                { SetSystemState(true); }
                else { SetSystemState(false); }
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

        private async Task<bool> WriteControlsState(string valueName = "")
        {
            bool allSucceeded = true;

            if (valueName == "")
            {
                // write all values
                foreach (Models.ArduinoRecord dataPoint in dataPoints)
                {
                    bool x = await WriteControlState(dataPoint);
                    if (!x) { allSucceeded = false; }
                }
            }
            else
            {
                // write named value
                Models.ArduinoRecord dataPoint = dataPoints.Find(x => x.ValueName == valueName);
                if (dataPoint != null)
                {
                    bool x = await WriteControlState(dataPoint);
                    if (!x) { allSucceeded = false; }
                }
            }

            return allSucceeded;
        }

        private async Task<bool> WriteControlState(Models.ArduinoRecord dataPoint)
        {
            RestService _restService = new RestService();

            string requestUri = Constants.apiMarkGriffithsEndpoint;
            requestUri += "/arduinoParameter/" + dataPoint.Id;

            Models.ArduinoValues ardVal = new Models.ArduinoValues
            {
                ValueInt = dataPoint.ValueInt,
                ValueString = dataPoint.ValueString
            };
                        
            await _restService.PostArduinoValues(requestUri, ardVal);
            return true;
        }

        private void ButtonLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("userEmail");
            Preferences.Remove("userPassword");

            StartActivity(typeof(LoginActivity));
        }
    }
}