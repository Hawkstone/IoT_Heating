
using Android.App;
using Android.Content;
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
        bool loggedOut = false;

        protected override void OnCreate(Bundle savedInstanceState)
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

            CheckAndUpdate();
        }

        protected override void OnStart()
        {
            base.OnStart();
            Models.updateControls = true;

            Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(Constants.updateControlsIntervalSec), () =>
            {
                if (Models.updateControls == true)
                {
                    CheckAndUpdate();
                    return true; // return true to repeat counting, false to stop timer
                }
                else
                {
                    return false;
                }
            });
        }

        protected override void OnPause()
        {
            base.OnPause();
            Models.updateControls = false;
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckAndUpdate();
            Models.updateControls = true;
        }


        // Read control states from API and update activity controls
        private async void CheckAndUpdate()
        {
            // set control states
            await ReadArduinoParameters();

            Models.bypassAPIupdate = true;
            SetControls();
            Models.bypassAPIupdate = false;
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
                if (Models.nodeOffline)
                {
                    ShowNodeOffine();
                    SetControls();
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                sysDiag.Debug.WriteLine("\tERROR {0}", ex.Message);
            }
            return false;
        }

        /// <summary>Set controls values</summary>
        private async void SetControls()
        {
            // if the js node is not available, disable controls
            if (Models.nodeOffline)
            {
                tb.Enabled = false;
                sb.Enabled = false;
                tvSetTemp.Enabled = false;
                tvCurrTemp.Enabled = false;
                tvSetTemp.Text = "--";
                tvCurrTemp.Text = "--";
                ShowNodeOffine();
                return;
            }
            else
            {
                tb.Enabled = true;
                sb.Enabled = true;
                tvSetTemp.Enabled = true;
                tvCurrTemp.Enabled = true;
            }

            try
            {
                bool newParamsAdded = false;
                Models.ArduinoRecord aRec;

                // Min / Max temp
                var min = 0;
                var max = 0;
                aRec = GetMin(ref newParamsAdded);
                min = aRec.ValueInt.GetValueOrDefault(5);
                aRec = GetMax(ref newParamsAdded);
                max = aRec.ValueInt.GetValueOrDefault(30);
                tempOffset = min;
                if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.NMr1) { sb.Min = 0; }
                sb.Max = max - min;

                // system state
                tb = FindViewById<ToggleButton>(Resource.Id.togButSystemState);
                aRec = GetSystemState(ref newParamsAdded);
                if (aRec.ValueString == "on")
                { SetSystemState(true); }
                else
                { SetSystemState(false); }

                // temperature setting
                aRec = GetSetTemperature(ref newParamsAdded);
                this.tvSetTemp.Text = aRec.ValueInt.ToString();
                sb = FindViewById<SeekBar>(Resource.Id.seekBarTemp);
                Int32.TryParse((aRec.ValueInt - tempOffset).ToString(), out int progress);
                sb.Progress = progress;

                // current temperature 
                TextView tv = FindViewById<TextView>(Resource.Id.textViewCurrentTemp);
                aRec = GetCurrentTemperature(ref newParamsAdded);
                tv.Text = aRec.ValueInt.ToString();


                // write new values back to database
                if (newParamsAdded)
                {
                    bool r = await Services.WriteControlsState();
                    if (!r) { ShowNodeOffine(); }
                }
            }
            catch (Exception ex)
            {
                sysDiag.Debug.WriteLine("\tERROR {0}", ex.Message);
            }

            return;
        }

        private static Models.ArduinoRecord GetMin(ref bool newParamsAdded)
        {
            Models.ArduinoRecord aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cTempMin);
            if (aRec == null)
            {
                aRec = new Models.ArduinoRecord
                {
                    UserID = Models.userID,
                    ValueName = Constants.cTempMin,
                    ValueInt = Constants.defaultMin,
                    ValueString = ""
                };
                Models.dataPoints.Add(aRec);
                newParamsAdded = true;
            }

            return aRec;
        }

        private static Models.ArduinoRecord GetMax(ref bool newParamsAdded)
        {
            Models.ArduinoRecord aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cTempMax);
            if (aRec == null)
            {
                aRec = new Models.ArduinoRecord
                {
                    UserID = Models.userID,
                    ValueName = Constants.cTempMax,
                    ValueInt = Constants.defaultMax,
                    ValueString = ""
                };
                Models.dataPoints.Add(aRec);
                newParamsAdded = true;
            }

            return aRec;
        }

        private static Models.ArduinoRecord GetSystemState(ref bool newParamsAdded)
        {
            Models.ArduinoRecord aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cSystemState);
            if (aRec == null)
            {
                aRec = new Models.ArduinoRecord
                {
                    UserID = Models.userID,
                    ValueName = Constants.cSystemState,
                    ValueInt = null,
                    ValueString = Constants.defaultSystemState
                };
                Models.dataPoints.Add(aRec);
                newParamsAdded = true;
            }

            return aRec;
        }

        private static Models.ArduinoRecord GetSetTemperature(ref bool newParamsAdded)
        {
            Models.ArduinoRecord aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cSetTemperature);
            if (aRec == null)
            {
                aRec = new Models.ArduinoRecord
                {
                    UserID = Models.userID,
                    ValueName = Constants.cSetTemperature,
                    ValueInt = Constants.defaultSetTemperature,
                    ValueString = ""
                };
                Models.dataPoints.Add(aRec);
                newParamsAdded = true;
            }

            return aRec;
        }

        private static Models.ArduinoRecord GetCurrentTemperature(ref bool newParamsAdded)
        {
            Models.ArduinoRecord aRec = Models.dataPoints.Find(x => x.ValueName == Constants.cCurrentTemperature);
            if (aRec == null)
            {
                aRec = new Models.ArduinoRecord
                {
                    UserID = Models.userID,
                    ValueName = Constants.cCurrentTemperature,
                    ValueInt = Constants.defaultCurrentTemperature,
                    ValueString = ""
                };
                Models.dataPoints.Add(aRec);
                newParamsAdded = true;
            }

            return aRec;
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

        private async void SeekBarProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            tvSetTemp.Text = (sb.Progress + tempOffset).ToString();

            if (Models.bypassAPIupdate == false)
            {
                Models.ArduinoRecord dataPoint = Models.dataPoints.Find(x => x.ValueName == Constants.cSetTemperature);
                dataPoint.ValueInt = sb.Progress + tempOffset;
                bool r = await Services.WriteControlsState(Constants.cSetTemperature);
                if (!r) { ShowNodeOffine(); }
            }
        }

        private async void ToggleButtonCheckedChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (Models.bypassAPIupdate == false)
            {
                Models.ArduinoRecord dataPoint = Models.dataPoints.Find(x => x.ValueName == Constants.cSystemState);
                if (e.IsChecked) { dataPoint.ValueString = "on"; } else { dataPoint.ValueString = "off"; }

                SetSystemState(e.IsChecked);
                bool r = await Services.WriteControlsState(Constants.cSystemState);
                if (!r) { ShowNodeOffine(); }
            }
        }

        private void ButtonLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("userEmail");
            Preferences.Remove("userPassword");
            loggedOut = true;
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(typeof(LoginActivity));
            Finish();
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


