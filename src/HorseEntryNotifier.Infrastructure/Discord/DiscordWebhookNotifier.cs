using System.Text;
using HorseEntryNotifier.Core.Interfaces;
using HorseEntryNotifier.Core.Models;
using HorseEntryNotifier.Infrastructure.Configuration;

namespace HorseEntryNotifier.Infrastructure.Discord;

public sealed class DiscordWebhookNotifier(
    HttpClient httpClient,
    DiscordOptions options,
    DiscordPayloadBuilder payloadBuilder) : IDiscordNotifier
{
    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(options.WebhookUrl, UriKind.Absolute, out var webhookUri) ||
            webhookUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Discord Webhook URLが未設定または不正です。DISCORD_WEBHOOK_URLを設定してください。");
        }

        using var content = new StringContent(
            payloadBuilder.BuildJson(message),
            Encoding.UTF8,
            "application/json");
        using var response = await httpClient.PostAsync(webhookUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
