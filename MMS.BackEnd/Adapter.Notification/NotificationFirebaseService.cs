namespace Adapter.Azure.Notification;

public class NotificationFirebaseService : INotificationFirebaseService
{
    private static bool _isInitialized = false;
    private readonly FirebaseConfig _config;

    public NotificationFirebaseService(IOptions<FirebaseConfig> options)
    {
        _config = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (!_isInitialized)
        {
            try
            {
                var fixedPrivateKey = _config.PrivateKey
                    .Replace("\\n", "\n") 
                    .Replace("\r\n", "\n")
                    .Trim();

                if (!fixedPrivateKey.StartsWith("-----BEGIN PRIVATE KEY-----"))
                {
                    fixedPrivateKey = $"-----BEGIN PRIVATE KEY-----\n{fixedPrivateKey}\n-----END PRIVATE KEY-----\n";
                }

                var base64Content = ExtractBase64FromPem(fixedPrivateKey);
                if (!IsValidBase64(base64Content))
                {
                    throw new InvalidOperationException("The private key contains invalid Base-64 characters or incorrect padding.");
                }

                var credentialJson = $@"{{
                        ""type"": ""service_account"",
                        ""project_id"": ""{_config.ProjectId}"",
                        ""private_key_id"": ""{_config.PrivateKey ?? "dummy-key-id"}"",
                        ""private_key"": ""{fixedPrivateKey}"",
                        ""client_email"": ""{_config.ClientEmail}"",
                        ""client_id"": ""101225797698771771999"",
                        ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                        ""token_uri"": ""https://oauth2.googleapis.com/token"",
                        ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                        ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/{Uri.EscapeDataString(_config.ClientEmail)}""
                    }}";

                var credential = GoogleCredential.FromJson(credentialJson);
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _config.ProjectId

                });

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize Firebase app. Check the private key format and configuration.", ex);
            }
        }
    }
   private string ExtractBase64FromPem(string pem)
    {
        var lines = pem.Split('\n');
        var base64 = string.Join("", lines
            .SkipWhile(line => line.StartsWith("-----BEGIN"))
            .TakeWhile(line => !line.StartsWith("-----END"))
            .Select(line => line.Trim()));
        return base64;
    }

    private bool IsValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return false;

        base64 = base64.Trim();
        if (base64.Length % 4 != 0)
            return false;

        return Regex.IsMatch(base64, @"^[A-Za-z0-9+/]+={0,2}$");
    }

    public async Task<string> SendToDeviceAsync(string deviceToken, string title, string body, string channelId = "default_channel", Dictionary<string, string>? data = null)
    {
        var message = new Message()
        {
            Token = deviceToken,
            Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
            Android = new AndroidConfig
            {
                Notification = new AndroidNotification
                {
                    ChannelId = channelId,
                    Priority = (NotificationPriority?)Priority.High
                }
            },
            Data = data ?? new Dictionary<string, string>()
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task<string> SendToMultipleDevicesAsync(List<string> deviceTokens, string title, string body, string channelId = "default_channel")
    {
        try
        {
            if (deviceTokens == null || deviceTokens.Count == 0 || deviceTokens.Any(t => string.IsNullOrWhiteSpace(t)))
            {
                return "Invalid or empty device tokens provided.";
            }
           
            if (!_isInitialized || FirebaseApp.DefaultInstance == null)
            {
                return "Firebase app is not initialized. Check service account credentials and project configuration.";
            }

            var message = new MulticastMessage()
            {
                Tokens = deviceTokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        ChannelId = channelId
                    },
                    Priority = Priority.High
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            return $"{response.SuccessCount} of {response.FailureCount + response.SuccessCount} sent successfully";
        }
        catch (Exception ex)
        {
            return $"An unexpected error occurred while sending notifications: {ex.Message}";
        }
    }

    public async Task<string> SendToTopicAsync(string topic, string title, string body, string channelId = "default_channel", Dictionary<string, string>? data = null)
    {
        var message = new Message()
        {
            Topic = topic,
            Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
            Android = new AndroidConfig
            {
                Notification = new AndroidNotification
                {
                    ChannelId = channelId,
                    Priority = (NotificationPriority?)Priority.High
                }
            },
            Data = data ?? new Dictionary<string, string>()
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task SubscribeToTopicAsync(string token, string topic)
    {
        await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(new List<string> { token }, topic);
    }

    public async Task UnsubscribeFromTopicAsync(string token, string topic)
    {
        await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(new List<string> { token }, topic);
    }
}
