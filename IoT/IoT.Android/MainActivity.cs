
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using sysDiag = System.Diagnostics;

namespace IoT.Droid
{
    [Activity(Label = "MainActivity", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        ToggleButton tb;
        SeekBar sb;
        TextView tv;

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

            tv = FindViewById<TextView>(Resource.Id.textViewSetTemp);

            sb = FindViewById<SeekBar>(Resource.Id.seekBarTemp);
            sb.ProgressChanged += SeekBarProgressChanged;

            Button button = FindViewById<Button>(Resource.Id.buttonLogout);
            button.Click += ButtonLogoutClicked;

            ReadControlsState();
        }

        private async void SeekBarProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            tv.Text = sb.Progress.ToString();
        }

        private async void ToggleButtonCheckedChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Models.ArduinoRecord dataPoint = Models.dataPoints.Find(x => x.ValueName == Models.cSystemState);
            if (e.IsChecked) { dataPoint.ValueString = "on"; } else { dataPoint.ValueString = "off"; }

            SetSystemState(e.IsChecked);
            bool r = await Services.WriteControlsState(Models.cSystemState);
        }

        /// <summary>Set the System On / System Off control state</summary>
        /// <param name="isChecked"></param>
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

        /// <summary>Get database values for controls</summary>
        private async void ReadControlsState()
        {
            try
            {
                // get the logged in users' Arduino records
                RestService _restService = new RestService();
                string requestUri = Constants.apiMarkGriffithsEndpoint;
                requestUri += "/listArduino";
                Models.dataPoints = new List<Models.ArduinoRecord>(await _restService.GetArduinoRecordsByIDasync(requestUri, Models.userID));


                // system state
                tb = FindViewById<ToggleButton>(Resource.Id.togButSystemState);
                Models.ArduinoRecord aRec = Models.dataPoints.Find(x => x.ValueName == "systemState");
                if (aRec != null)
                {
                    if (aRec.ValueString == "on")
                    { SetSystemState(true); }
                    else { SetSystemState(false); }
                }

                // temperature setting
                aRec = Models.dataPoints.Find(x => x.ValueName == "setTemperature");
                if (aRec != null)
                {
                    this.tv.Text = aRec.ValueInt.ToString();

                    sb = FindViewById<SeekBar>(Resource.Id.seekBarTemp);
                    Int32.TryParse(aRec.ValueInt.ToString(), out int progress);
                    sb.Progress = progress;
                }

                // current temperature 
                TextView tv = FindViewById<TextView>(Resource.Id.textViewCurrentTemp);
                aRec = Models.dataPoints.Find(x => x.ValueName == "currentTemperature");
                if (aRec != null)
                { tv.Text = aRec.ValueInt.ToString(); }
            }
            catch (Exception ex)
            {
                sysDiag.Debug.WriteLine("\tERROR {0}", ex.Message);
            }

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