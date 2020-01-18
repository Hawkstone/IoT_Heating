
using Android.App;
using Android.Widget;

namespace IoT
{
    public class Services
    {
        public class MessageAndroid : Models.IMessage
        {
            public void LongAlert(string message)
            {
                Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
            }

            public void ShortAlert(string message)
            {
                Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
            }
        }

        public class GetUsers
        {

        }

    }
}