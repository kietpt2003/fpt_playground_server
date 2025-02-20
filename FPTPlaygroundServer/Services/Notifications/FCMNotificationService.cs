using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace FPTPlaygroundServer.Services.Notifications;

public class FCMNotificationService
{
    public async Task<string> SendPersonalNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data)
    {
        // Xác định đường dẫn tương đối đến tệp `account_service.json`
        var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "Credentials", "account_service.json");
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(credentialsPath)
        });

        var message = new Message()
        {
            Data = data,
            Notification = new Notification
            {
                Title = title,
                Body = body,
            },
            Token = deviceToken,
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task<int> SendMultibleNotificationAsync(List<string> deviceTokens, string title, string body, Dictionary<string, string>? data)
    {
        // Xác định đường dẫn tương đối đến tệp `account_service.json`
        var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "Credentials", "account_service.json");

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(credentialsPath)
            });
        }

        var message = new MulticastMessage()
        {
            Data = data,
            Notification = new Notification
            {
                Title = title,
                Body = body,
            },

            Tokens = deviceTokens,
        };

        var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
        return response.SuccessCount;
    }
}
