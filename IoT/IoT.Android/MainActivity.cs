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
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        RestService _restService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            SetContentView(Resource.Layout.layout1);

            Button button = FindViewById<Button>(Resource.Id.buttonLogin);
            button.Click += ButtonLoginClicked;
        }

        async void ButtonLoginClicked(object sender, EventArgs eventArgs)
        {
            EditText txtEmail = FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText txtPassword = FindViewById<EditText>(Resource.Id.editTextPassword);
            Button button = FindViewById<Button>(Resource.Id.buttonLogin);

            txtEmail.Focusable = false;
            txtPassword.Focusable = false;
            button.Enabled = false;

            _restService = new RestService();
            string requestUri = Constants.apiMarkGriffithsEndpoint;

            // get a recordset from the webserver database
            requestUri += "/listUsers";
            var userRecords = new List<Models.UserRecord>(await _restService.GetUserRecordAsync(requestUri));
            Models.userRecord = userRecords[0];
            Models.userID = Models.userRecord.Id;

            // get the password for the given email address
            
            requestUri = Constants.apiMarkGriffithsEndpoint;
            requestUri += "/getPassword/" + txtEmail.Text.Trim();
            var dbpassword = await _restService.GetDataPointAsync(requestUri);

            txtEmail.Focusable = true;
            txtPassword.Focusable = true;
            button.Enabled = true;

            // compare returned password with entered password 
            if (dbpassword.Length > 0)
            {
                if (dbpassword == txtPassword.Text)
                {
                    Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Login suceeded");
                    return true;
                }
                else
                {
                    Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Login failed, email address or password was invalid");
                    return false;
                }
            }

        }
    }
}
