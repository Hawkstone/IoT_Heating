
using Android.App;
using Android.Widget;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
        
        /// <summary>Write a single datapoint to the database</summary>
        /// <param name="dataPoint">parameter to write</param>
        public static async Task<bool> WriteControlState(Models.ArduinoRecord dataPoint)
        {
            RestService _restService = new RestService();

            string requestUri = Constants.apiMarkGriffithsEndpoint;
            requestUri += "/arduinoParameter/" + dataPoint.Id;

            Models.ArduinoValues ardVal = new Models.ArduinoValues
            {
                ValueInt = dataPoint.ValueInt,
                ValueString = dataPoint.ValueString
            };

            bool response = false;
            try
            {
                response = await _restService.PostArduinoValues(requestUri, ardVal);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }
            
            return response;
        }
    }
}