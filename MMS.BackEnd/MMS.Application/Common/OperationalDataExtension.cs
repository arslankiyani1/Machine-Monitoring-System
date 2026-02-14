//namespace MMS.Application.Common;

//public static class OperationalDataExtension
//{
//    public static bool HasData(this OperationalData data)
//    {
//        if (data == null)
//            return false;

//        return
//            (data.FeedRate?.Value ?? 0) != 0 ||
//            (data.SpindleSpeed?.Value ?? 0) != 0 ||
//            data.SpindleStatus == true ||
//            (data.Temperature?.Value ?? 0) != 0 ||
//            (data.Vibration?.Value ?? 0) != 0 ||
//            (data.PowerConsumption?.Value ?? 0) != 0 ||
//            (data.Torque?.Value ?? 0) != 0 ||
//            (data.CoolantLevel?.Value ?? 0) != 0 ||
//            (data.AirPressure?.Value ?? 0) != 0 ||
//            data.CycleTime != null;
//    }
//}
