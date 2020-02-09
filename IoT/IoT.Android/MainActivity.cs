
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using sysDiag = System.Diagnostics;

namespace IoT.Droid
{
    [Activity(Label = "MainActivity", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        ToggleButton tb;
        SeekBar sb;
        TextView tvSetTemp;
        TextView tvCurrTemp;
        int tempOffset;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.MainActivity);

            // setup custom fonts
            var font = Typeface.CreateFromAsset(Assets, "Orbitron-VariableFont_wght.ttf");
            tvSetTemp = FindViewById<TextView>(Resource.Id.textViewSetTemp);
            tvCurrTemp = FindViewById<TextView>(Resource.Id.textViewCurrentTemp);
            tvSetTemp.Typeface = font;
            tvCurrTemp.Typeface = font;

            // display user info
            Models.UserRecord userRecord = Models.userRecord;
            TextView tvWelcome = FindViewById<TextView>(Resource.Id.welcomeMA);
            tvWelcome.Text = "Welcome to Leany Heating\n" + userRecord.FirstName + " " + userRecord.LastName;

            // setup event listeners 
            tb = FindViewById<ToggleButton>(Resource.Id.togButSystemState);
            tb.CheckedChange += ToggleButtonCheckedChanged;

            Button button = FindViewById<Button>(Resource.Id.buttonLogout);
            button.Click += ButtonLogoutClicked;

            sb = FindViewById<SeekBar>(Resource.Id.seekBarTemp);
            sb.ProgressChanged += SeekBarProgressChanged;

            // set control states
            if (await ReadArduinoParameters())
            {
                Models.ignoreProgressChange = true;
                SetControlsState();
                Models.ignoreProgressChange = false;
            }
            else
            {
                Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("No stored settings");
            }
        }

        private async void SeekBarProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            tvSetTemp.Text = (sb.Progress + tempOffset).ToString();

            if (Models.ignoreProgressChange == false)
            {
                Models.ArduinoRecord dataPoint = Models.dataPoints.Find(x => x.ValueName == Constants.cSetTemperature);
                dataPoint.ValueInt = sb.Progress + tempOffset;
                bool r = await Services.WriteControlsState(Constants.cSetTemperature);
                if (!r) { ShowNodeOffine(); }
            }
        }

        private async void ToggleButtonCheckedChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Models.ArduinoRecord dataPoint = Models.dataPoints.Find(x => x.ValueName == Constants.cSystemState);
            if (e.IsChecked) { dataPoint.ValueString = "on"; } else { dataPoint.ValueString = "off"; }

            SetSystemState(e.IsChecked);
            bool r = await Services.WriteControlsState(Constants.cSystemState);
            if (!r) { ShowNodeOffine(); }
        }

        /// <summary>Get database values for controls</summary>
        private void SetControlsState()
        {
            try
            {
                Models.ArduinoRecord aRec;

                // Min / Max temp
                var min = 0;
                var max = 0;
                aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cTempMin);
                if (aRec != null)
                { min = aRec.ValueInt.GetValueOrDefault(0); }
                aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cTempMax);
                if (aRec != null)
                { max = aRec.ValueInt.GetValueOrDefault(0); }
                tempOffset = min;
                if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.NMr1)
                {
                    sb.Min = 0;
                }
                sb.Max = max - min;

                // system state
                tb = FindViewById<ToggleButton>(Resource.Id.togButSystemState);
                aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cSystemState);
                if (aRec != null)
                {
                    if (aRec.ValueString == "on")
                    { SetSystemState(true); }
                    else { SetSystemState(false); }
                }

                // temperature setting
                aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cSetTemperature);
                if (aRec != null)
                {
                    this.tvSetTemp.Text = aRec.ValueInt.ToString();

                    sb = FindViewById<SeekBar>(Resource.Id.seekBarTemp);
                    Int32.TryParse((aRec.ValueInt - tempOffset).ToString(), out int progress);
                    sb.Progress = progress;
                }

                // current temperature 
                TextView tv = FindViewById<TextView>(Resource.Id.textViewCurrentTemp);
                aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cCurrentTemperature);
                if (aRec != null)
                { tv.Text = aRec.ValueInt.ToString(); }
            }
            catch (Exception ex)
            {
                sysDiag.Debug.WriteLine("\tERROR {0}", ex.Message);
            }

            return;
        }


        /// <summary>Get arduino database values for current user</summary>
        private async Task<bool> ReadArduinoParameters()
        {
            try
            {
                // get the logged in users' Arduino records
                RestService _restService = new RestService();
                string requestUri = Constants.apiMarkGriffithsEndpoint;
                requestUri += "/listArduino";
                Models.dataPoints = new List<Models.ArduinoRecord>(await _restService.GetArduinoRecordsByIDasync(requestUri, Models.userID));
            }
            catch (Exception ex)
            {
                sysDiag.Debug.WriteLine("\tERROR {0}", ex.Message);
            }

            return true;
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

        private void ButtonLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("userEmail");
            Preferences.Remove("userPassword");

            StartActivity(typeof(LoginActivity));
        }

        private void ShowNodeOffine()
        {
            if (Models.nodeOffline)
            {
                Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Connection Error. Node offline.");
            }
            //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}


