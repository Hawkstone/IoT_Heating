using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using IoT;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(Services.MessageAndroid))]

namespace IoT.Droid
{
    [Activity(Label = "Leany Heating", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, NoHistory = true)]
    public class LoginActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        async protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            bool showLogin = false;

            // see if the user has is already logged in
            var userEmail = Preferences.Get("userEmail", "");
            var userPassword = Preferences.Get("userPassword", "");
            if (userEmail.Length > 0 && userPassword.Length > 0)
            {
                SetContentView(Resource.Layout.ReloadCredentials);

                Models.loginSuccess = (await Services.Login(userEmail, userPassword));
                if (Models.loginSuccess)
                {
                    StartActivity(typeof(MainActivity));
                }
                else
                {
                    showLogin = true;
                }
            }
            else
            {
                showLogin = true;
            }

            if (showLogin)
            {
                // show login activity
                Button button = FindViewById<Button>(Resource.Id.buttonLogin);

                SetContentView(Resource.Layout.LoginActivity);
                button = FindViewById<Button>(Resource.Id.buttonLogin);
                button.Click += ButtonLoginClicked;

                EditText et = FindViewById<EditText>(Resource.Id.editTextEmail);
                et.RequestFocus();
            }
        }

        async void ButtonLoginClicked(object sender, EventArgs eventArgs)
        {
            Button btLogin = FindViewById<Button>(Resource.Id.buttonLogin);
            btLogin.Enabled = false;
            btLogin.SetBackgroundColor(Color.ParseColor(Constants.buttonDisabledColor));
            
            // login using entered credentials
            EditText etEmail = FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText etPassword = FindViewById<EditText>(Resource.Id.editTextPassword);
            Models.loginSuccess = (await Services.Login(etEmail.Text, etPassword.Text));
            if (Models.loginSuccess)
            {
                StartActivity(typeof(MainActivity));
            }
            btLogin.Enabled = true;
            btLogin.SetBackgroundColor(Color.ParseColor(Constants.buttonBackgroundColor));
            return;
        }
    }
}
