namespace IBS_Backend.Sanitary
{
    public class LoggingFeaturesOptions
    {
        public bool CaptureBodies { get; set; } = true;
        public double SampleRate { get; set; } = 1.0;
        public int MaxBodyChars { get; set; } = 4000;
        public List<string> MaskKeys { get; set; } = new() // TODO load from appsettings
    {
        "password","pwd","token","authorization","apiKey","pin","cnic","bankId","bearer"
    };
        public bool IncludeHeaders { get; set; } = true;
        public List<string> ExcludePaths { get; set; } = new()
    {
        "/health","/metrics","/swagger"
    };
    }
}