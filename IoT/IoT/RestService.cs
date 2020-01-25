using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace IoT
{
    public enum PermissionLevel
    {
        Admin = 1,
        User = 0
    }

    public class RootObjectDTO
    {
        public string Data { get; set; }
        public string Message { get; set; }
    }

    public class RestService
    {
        HttpClient _client;

        public RestService()
        {
            _client = new HttpClient();
            //_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>Get database user record</summary>
        /// <param name="uri">/getUserRecordByEmail</param>
        /// <returns>Models.UserRecord</returns>
        public async Task<Models.UserRecord> GetUserRecordByEmailAsync(string uri, string email)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri + "/" + email);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    RootObjectDTO r = JsonConvert.DeserializeObject<RootObjectDTO>(content);                // deserialise the data packet
                    var userRecord = JsonConvert.DeserializeObject<Models.UserRecord>(r.Data);       // deserialise user record objects

                    return userRecord;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }
            return null;
        }

        /// <summary>Get database user records</summary>
        /// <param name="uri">/getUserRecordByID</param>
        /// <returns>List of Models.UserRecord</returns>
        public async Task<List<Models.UserRecord>> GetUserRecordsByIDasync(string uri, int ID)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    RootObjectDTO r = JsonConvert.DeserializeObject<RootObjectDTO>(content);                // deserialise the data packet
                    var userRecords = JsonConvert.DeserializeObject<List<Models.UserRecord>>(r.Data);       // deserialise user record objects

                    return userRecords;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }
            return new List<Models.UserRecord>();
        }

        /// <summary>Get database arduino records</summary>
        /// <param name="uri">/listArduino/1</param>
        /// <returns>User password</returns>
        public async Task<List<Models.ArduinoRecord>> GetArduinoRecordsByIDasync(string uri, int userID)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri + "/" + userID);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    RootObjectDTO r = JsonConvert.DeserializeObject<RootObjectDTO>(content);                      // deserialise the data packet
                    var arduinoRecords = JsonConvert.DeserializeObject<List<Models.ArduinoRecord>>(r.Data);       // deserialise user record objects

                    return arduinoRecords;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }
            return new List<Models.ArduinoRecord>();
        }
               
        /// <summary>Get a datapoint</summary>
        /// <param name="uri">e.g. getPassword/:email</param>
        /// <returns>Single datapoint</returns>
        public async Task<String> GetDataPointAsync(string uri)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    RootObjectDTO r = JsonConvert.DeserializeObject<RootObjectDTO>(content);        // deserialise the data packet
                    return r.Data;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }
            return "";
        }
    }
}