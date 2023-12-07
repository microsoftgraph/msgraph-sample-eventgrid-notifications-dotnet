// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;

namespace GraphEventGrid.Models;

/// <summary>
/// Represents the payload of a CloudEvent notification from
/// Azure Event Grid.
/// </summary>
public class CloudEventNotification
{
    /// <summary>
    /// Gets or sets the ID of the notification.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the notification.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the source of the notification.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the subject of the notification.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the date and time the notification was sent.
    /// </summary>
    public DateTimeOffset? Time { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type of the `Data` field.
    /// </summary>
    public string? DataContentType { get; set; }

    /// <summary>
    /// Gets or sets the spec version of the notification.
    /// </summary>
    public string? SpecVersion { get; set; }

    /// <summary>
    /// Gets or sets the Data field.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Converts the `Data` field to a <see cref="ChangeNotification"/>.
    /// </summary>
    /// <returns>A <see cref="ChangeNotification"/>.</returns>
    public ChangeNotification? GetChangeNotification()
    {
        return Data is null ? null :
            KiotaJsonSerializer.Deserialize<ChangeNotification>(JsonSerializer.Serialize(Data));
    }
}
