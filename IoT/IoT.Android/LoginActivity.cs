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
    [Activity(Label = "Leany Heating", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LoginActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        async protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            SetContentView(Resource.Layout.LoginActivity);

            Button button = FindViewById<Button>(Resource.Id.buttonLogin);
            button.Click += ButtonLoginClicked;

            // see if the user has is already logged in
            var userEmail = Preferences.Get("userEmail", "");
            var userPassword = Preferences.Get("userPassword", "");
            if (userEmail.Length > 0 && userPassword.Length > 0)
            {
                SetContentView(Resource.Layout.ReloadCredentials);

                Models.loginSuccess = (await Login(userEmail, userPassword));
                if (Models.loginSuccess)
                {
                    StartActivity(typeof(MainActivity));
                }
                else
                {
                    SetContentView(Resource.Layout.LoginActivity);
                    button = FindViewById<Button>(Resource.Id.buttonLogin);
                    button.Click += ButtonLoginClicked;

                    EditText et = FindViewById<EditText>(Resource.Id.editTextEmail);
                    et.RequestFocus();
                }
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
            Models.loginSuccess = (await Login(etEmail.Text, etPassword.Text));
            if (Models.loginSuccess)
            {
                StartActivity(typeof(MainActivity));
            }
            btLogin.Enabled = true;
            btLogin.SetBackgroundColor(Color.ParseColor(Constants.buttonBackgroundColor));
        }

        /// <summary>check the user credentials against webserver data</summary>
        /// <param name="email">login email address</param>
        /// <param name="password">login password</param>
        public async Task<bool> Login(string email, string password)
        {
            RestService _restService;
            _restService = new RestService();
            string requestUri = Constants.apiMarkGriffithsEndpoint;

            // get the user record from email address
            requestUri += "/getUserRecordByEmail";
            Models.UserRecord userRecord = (await _restService.GetUserRecordByEmailAsync(requestUri, email.Trim()));

            if (userRecord != null)
            {
                Models.userRecord = userRecord;
                Models.userID = Models.userRecord.Id;

                // compare returned password with entered password 
                if (Models.userRecord.Password.Length > 0)
                {
                    if (Models.userRecord.Password == password) { Models.loginSuccess = true; }
                }
            }

            if (Models.loginSuccess)
            {
                Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Login succeeded");
                Preferences.Set("userEmail", userRecord.Email);
                Preferences.Set("userPassword", userRecord.Password);
                return true;
            }
            else
            {
                Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Login failed, please try again");
            }
            return false;
        }
    }
}
