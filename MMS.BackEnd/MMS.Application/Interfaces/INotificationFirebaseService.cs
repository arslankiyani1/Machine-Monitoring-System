namespace MMS.Application.Interfaces;

public interface INotificationFirebaseService
{
    Task<string> SendToDeviceAsync(string deviceToken, string title, string body, string channelId = "default_channel", Dictionary<string, string>? data = null);
    Task<string> SendToMultipleDevicesAsync(List<string> deviceTokens, string title, string body, string channelId = "default_channel");
    Task<string> SendToTopicAsync(string topic, string title, string body, string channelId = "default_channel", Dictionary<string, string>? data = null);
    Task SubscribeToTopicAsync(string token, string topic);
    Task UnsubscribeFromTopicAsync(string token, string topic);
}