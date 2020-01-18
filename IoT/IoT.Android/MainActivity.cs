using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace IoT.Droid
{
    [Activity(Label = "IoT", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            SetContentView(Resource.Layout.layout1);

            Button button = FindViewById<Button>(Resource.Id.buttonLogin);
            button.Click += Button1Clicked;
                       
        }

        private void Button1Clicked(object sender, EventArgs eventArgs)
        {
            TextView tv = FindViewById<TextView>(Resource.Id.textView1);
            tv.Text = "OK";
        }
        
    }
}