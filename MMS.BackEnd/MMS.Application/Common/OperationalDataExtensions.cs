//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MMS.Application.Common;

//public static class OperationalDataExtensions
//{
//    public static Dictionary<string, object> ToDictionary(this OperationalData data)
//    {
//        var dict = new Dictionary<string, object>();

//        if (data.FeedRate != null) dict["feedrate"] = data.FeedRate.Value;
//        if (data.SpindleSpeed != null) dict["SpindleSpeed"] = data.SpindleSpeed.Value;
//        if (data.Temperature != null) dict["temperature"] = data.Temperature.Value;
//        if (data.Vibration != null) dict["vibration"] = data.Vibration.Value;
//        if (data.PowerConsumption != null) dict["PowerConsumption"] = data.PowerConsumption.Value;
//        if (data.Torque != null) dict["torque"] = data.Torque.Value;
//        if (data.CoolantLevel != null) dict["CoolantLevel"] = data.CoolantLevel.Value;
//        if (data.AirPressure != null) dict["AirPressure"] = data.AirPressure.Value;

//        return dict;
//    }
//}
