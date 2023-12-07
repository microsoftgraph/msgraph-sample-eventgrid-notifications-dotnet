// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace GraphEventGrid.Handlers;

/// <summary>
/// Implements handler for IHostApplicationLifetime.ApplicationStarted.
/// </summary>
public static class ApplicationEventHandler
{
    /// <summary>
    /// Registers a handler for the IHostApplicationLifetime.ApplicationStarted event.
    /// </summary>
    /// <param name="lifetime">The app's instance of <see cref="IHostApplicationLifetime"/>.</param>
    /// <param name="settings">The application settings.</param>
    /// <param name="graphClient">The Microsoft Graph client.</param>
    /// <param name="logger">The app's <see cref="ILogger"/> instance.</param>
    public static void Register(
        IHostApplicationLifetime lifetime,
        AppSettings settings,
        GraphServiceClient graphClient,
        ILogger logger)
    {
        lifetime.ApplicationStarted.Register(async () =>
        {
            // Ensure a subscription is in place
            var subscriptions = await graphClient.Subscriptions.GetAsync();
            if (subscriptions?.Value?.Count > 0)
            {
                logger.LogInformation("Subscription already exists");
                return;
            }

            try
            {
                // Create a subscription
                logger.LogInformation("No existing subscription found");

                var eventGridUrl =
                    $"EventGrid:?azuresubscriptionid={settings.SubscriptionId}" +
                    $"&resourcegroup={settings.ResourceGroup}" +
                    $"&partnertopic={settings.EventGridTopic}" +
                    $"&location={settings.Location}";

                var newSubscription = await graphClient.Subscriptions.PostAsync(new Subscription
                {
                    ChangeType = "updated,deleted,created",
                    Resource = "users",
                    ClientState = "SomeSecretValue",
                    NotificationUrl = eventGridUrl,
                    LifecycleNotificationUrl = eventGridUrl,

                    // Setting a short expire time for testing purposes
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddHours(1),
                });

                if (newSubscription is null)
                {
                    logger.LogError("Could not create subscription - the API returned null");
                }
                else
                {
                    logger.LogInformation(
                        "Created new subscription with ID {subscriptionId}", newSubscription.Id);
                    logger.LogInformation(
                        "Please activate the {topicName} partner topic in the Azure portal and create an event subscription. See README for details.",
                        settings.EventGridTopic);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating subscription");
            }
        });
    }
}
