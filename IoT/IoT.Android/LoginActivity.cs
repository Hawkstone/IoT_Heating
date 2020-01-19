using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using IoT;
using System;
using System.Collections.Generic;

[assembly: Xamarin.Forms.Dependency(typeof(Services.MessageAndroid))]

namespace IoT.Droid
{
    [Activity(Label = "Leany Heating", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LoginActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            SetContentView(Resource.Layout.LoginActivity);

            Button button = FindViewById<Button>(Resource.Id.buttonLogin);
            button.Click += ButtonLoginClicked;
        }

        async void ButtonLoginClicked(object sender, EventArgs eventArgs)
        {
            EditText txtEmail = FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText txtPassword = FindViewById<EditText>(Resource.Id.editTextPassword);
            Button button = FindViewById<Button>(Resource.Id.buttonLogin);

            //txtEmail.Focusable = false;
            //txtPassword.Focusable = false;
            button.Enabled = false;

            RestService _restService;
            _restService = new RestService();
            string requestUri = Constants.apiMarkGriffithsEndpoint;

            // get the user record from email address
            requestUri += "/getUserRecordByEmail";
            Models.UserRecord userRecord = (await _restService.GetUserRecordByEmailAsync(requestUri, txtEmail.Text.Trim()));

            bool loginSuccess = false;

            if (userRecord != null)
            {
                Models.userRecord = userRecord;
                Models.userID = Models.userRecord.Id;

                // compare returned password with entered password 
                if (Models.userRecord.Password.Length > 0)
                {
                    if (Models.userRecord.Password == txtPassword.Text) { loginSuccess = true; }
                }
            }

            if (loginSuccess)
            {
                Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Login succeeded");
                SetContentView(Resource.Layout.MainActivity);
            }
            else
            {
                Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Login failed, please try again");
                //txtEmail.Focusable = true;
                //txtPassword.Focusable = true;
                button.Enabled = true;
            }

        }
    }
}
