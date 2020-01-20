
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



        //public async Task<List<Models.UserRecord>> GetUserRecord(int userID = -1)
        //{
        //    RestService _restService = new RestService();
        //    string requestUri = Constants.apiMarkGriffithsEndpoint;
        //}
    }
}