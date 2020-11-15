
namespace IoT
{
    public class Constants
    {
        public const string apiMarkGriffithsEndpoint = "http://markgriffiths.info:3000";
        public const int apiReqTimeoutSecs = 10;
        public const int updateControlsIntervalSec = 3;

        public const string buttonBackgroundColor = "#FF2189AD";
        public const string buttonDisabledColor = "#FF7B7B7B";
        public const string systemOffBackColor = "#FFFFDDE6";
        public const string systemOnBackColor = "#FFCFFF9E";

        public const string cSystemState = "systemState";
        public const string cSetTemperature = "setTemperature";
        public const string cCurrentTemperature = "currentTemperature";
        public const string cTempMin = "tempMin";
        public const string cTempMax = "tempMax";

        public const int defaultMin = 5;
        public const int defaultMax = 30;
        public const string defaultSystemState = "off";
        public const int defaultSetTemperature = 20;
        public const int defaultCurrentTemperature = 20;
    }
}