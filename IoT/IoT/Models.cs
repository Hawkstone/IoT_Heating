using System.Collections.Generic;

namespace IoT
{
    public class Models
    {
        public class UserRecord
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public PermissionLevel PermissionLevel { get; set; }
        }

        public class ArduinoRecord
        {
            public int Id { get; set; }
            public int UserID { get; set; }
            public string ValueName { get; set; }
            public int? ValueInt { get; set; }
            public string ValueString { get; set; }
        }

        public class ArduinoValues
        {
            public int? ValueInt { get; set; }
            public string ValueString { get; set; }
        }

        public interface IMessage
        {
            void LongAlert(string message);
            void ShortAlert(string message);
        }

        public static List<Models.ArduinoRecord> dataPoints;
        public static UserRecord userRecord = null;
        public static int userID = 0;
        public static bool loginSuccess = false;
        public static bool ignoreProgressChange;
    }
}
