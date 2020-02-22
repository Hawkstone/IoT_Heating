
using Android.App;
using Android.Widget;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IoT
{
    public class Services
    {
        /// <summary>check the user credentials against webserver data</summary>
        /// <param name="email">login email address</param>
        /// <param name="password">login password</param>
        public static async Task<bool> Login(string email, string password)
        {
            RestService _restService = new RestService();
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
                Xamarin.Essentials.Preferences.Set("userEmail", userRecord.Email);
                Xamarin.Essentials.Preferences.Set("userPassword", userRecord.Password);
                return true;
            }
            else
            {
                Xamarin.Forms.DependencyService.Get<Models.IMessage>().LongAlert("Login failed, please try again");
            }
            return false;
        }

        /// <summary>Write the current state of a single control or all controls</summary>
        /// <param name="valueName">optional: If given, write a single control value</param>
        public static async Task<bool> WriteControlsState(string valueName = "")
        {
            bool allSucceeded = true;

            // write named value
            if (valueName != "")
            {
                Models.ArduinoRecord dataPoint = Models.dataPoints.Find(x => x.ValueName == valueName);
                if (dataPoint != null)
                {
                    bool r = await Services.WriteControlState(dataPoint);
                    if (!r) { allSucceeded = false; }
                }
            }
            else
            // write all values
            {
                foreach (Models.ArduinoRecord dataPoint in Models.dataPoints)
                {
                    bool r = await Services.WriteControlState(dataPoint);
                    if (!r) { allSucceeded = false; }
                }
            }

            return allSucceeded;
        }

        /// <summary>Write a single datapoint to the database. If dataPoint has no .Id, a new dataPoint is created.</summary>
        /// <param name="dataPoint">parameter to write</param>
        private static async Task<bool> WriteControlState(Models.ArduinoRecord dataPoint)
        {
            RestService _restService = new RestService();
            bool response = false;

            string requestUri = Constants.apiMarkGriffithsEndpoint;
            if (dataPoint.Id == 0)
            {
                // create new parameter
                requestUri += "/addArduino";

                Models.ArduinoRecord ardVal = new Models.ArduinoRecord
                {
                    UserID = dataPoint.UserID,
                    ValueName = dataPoint.ValueName,
                    ValueInt = dataPoint.ValueInt,
                    ValueString = dataPoint.ValueString
                };

                response = false;
                try
                {
                    response = await _restService.PostArduinoValuesAsync(requestUri, ardVal);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine("\tERROR {0}", ex.Message);
                }
            }
            else
            {
                // update parameter
                requestUri += "/updateArduino/" + dataPoint.Id;

                Models.ArduinoValues ardVal = new Models.ArduinoValues
                {
                    ValueInt = dataPoint.ValueInt,
                    ValueString = dataPoint.ValueString
                };

                response = false;
                try
                {
                    response = await _restService.PutArduinoValuesAsync(requestUri, ardVal);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine("\tERROR {0}", ex.Message);
                }
            }
            return response;
        }

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
    }
}