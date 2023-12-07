// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using GraphEventGrid.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;

namespace GraphEventGrid.Handlers;

/// <summary>
/// Implements handlers for incoming notifications
/// from Azure Event Grid.
/// </summary>
public static class NotificationEventHandler
{
    private static readonly string HandlerEndpoint = "/notifications";

    /// <summary>
    /// Maps the notification event endpoints.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to map endpoints with.</param>
    public static void Map(WebApplication app)
    {
        app.MapMethods(HandlerEndpoint, new[] { "OPTIONS" }, ValidateEndpoint);
        app.MapPost(HandlerEndpoint, HandleNotificationAsync);
    }

    private static void ValidateEndpoint(
        HttpContext context,
        [FromHeader(Name = "WEBHOOK-REQUEST-ORIGIN")] string? origin,
        [FromHeader(Name = "WEBHOOK-REQUEST-RATE")] string? rate)
    {
        // See https://github.com/cloudevents/spec/blob/v1.0/http-webhook.md#4-abuse-protection
        // Event Grid sends the host that emits events in this header as a request
        // for our webhook to allow them to send
        if (!string.IsNullOrEmpty(origin))
        {
            context.Response.Headers.Append("WebHook-Allowed-Origin", origin);
        }

        if (!string.IsNullOrEmpty(rate))
        {
            context.Response.Headers.Append("WebHook-Allowed-Rate", rate);
        }
    }

    private static async Task<IResult> HandleNotificationAsync(
        CloudEventNotification notification,
        [FromServices] GraphServiceClient graphClient,
        [FromServices] ILogger<Program> logger)
    {
        if (!string.IsNullOrEmpty(notification.Type))
        {
            logger.LogInformation("Received {type} notification from Event Grid", notification.Type);

            try
            {
                if (notification.Type.Equals("Microsoft.Graph.UserUpdated", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleUserUpdateAsync(
                        notification.GetChangeNotification(), graphClient, logger);
                }
                else if (notification.Type.Equals("Microsoft.Graph.UserDeleted", StringComparison.OrdinalIgnoreCase))
                {
                    HandleUserDelete(notification.GetChangeNotification(), logger);
                }
                else if (notification.Type.Equals("Microsoft.Graph.SubscriptionReauthorizationRequired", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleSubscriptionRenewalAsync(
                        notification.GetChangeNotification(), graphClient, logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing notification");
            }
        }

        return Results.Accepted();
    }

    private static async Task HandleUserUpdateAsync(
        Microsoft.Graph.Models.ChangeNotification? notification,
        GraphServiceClient graphClient,
        ILogger logger)
    {
        if (notification is not null)
        {
            // The user was either created, updated, or soft-deleted.
            // The notification only contains the user's ID, so
            // get the user from Microsoft Graph if other details are needed.
            // If the user isn't found, then it was likely deleted.

            // The notification has the relative URL to the user. The .WithUrl method
            // in the Graph client can use a URL to retrieve an object.
            try
            {
                var user = await graphClient.Users[string.Empty]
                    .WithUrl($"{graphClient.RequestAdapter.BaseUrl}/{notification.Resource}")
                    .GetAsync();

                logger.LogInformation(
                    "User {name} (ID: {id}) was created or updated",
                    user?.DisplayName,
                    user?.Id);
            }
            catch (ODataError oDataError)
            {
                if (oDataError.Error?.Code is string errorCode &&
                    errorCode.Contains("ResourceNotFound", StringComparison.OrdinalIgnoreCase))
                {
                    var userId = notification.Resource?.Split("/")[1];
                    logger.LogInformation("User with ID {userId} was soft-deleted", userId);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    private static void HandleUserDelete(
        Microsoft.Graph.Models.ChangeNotification? notification,
        ILogger logger)
    {
        if (notification is not null)
        {
            // The user was permanently deleted. The notification only contains
            // the user's ID, and we can no longer get the user from Graph.
            var userId = notification.Resource?.Split("/")[1];
            logger.LogInformation("User with ID {userId} was deleted", userId);
        }
    }

    private static async Task HandleSubscriptionRenewalAsync(
        Microsoft.Graph.Models.ChangeNotification? notification,
        GraphServiceClient graphClient,
        ILogger logger)
    {
        if (notification is not null)
        {
            // The subscription needs to be renewed.
            if (notification.SubscriptionId?.ToString() is string subscriptionId)
            {
                await graphClient.Subscriptions[subscriptionId]
                    .PatchAsync(new()
                    {
                        ExpirationDateTime = DateTimeOffset.UtcNow.AddHours(1),
                    });

                logger.LogInformation("Subscription with ID {id} renewed for another hour", subscriptionId);
            }
        }
    }
}
