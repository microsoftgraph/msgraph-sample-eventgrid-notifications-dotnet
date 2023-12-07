// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace GraphEventGrid;

/// <summary>
/// Represents the settings for the application.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the "Application (client) ID" of the app registration in Azure.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret of the app registration in Azure.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the "Directory (tenant) ID" of the app registration in Azure.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the Azure subscription where the Event Grid
    /// partner topic should be created.
    /// </summary>
    public string? SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the name of the Azure resource group where the Event Grid
    /// partner topic should be created.
    /// </summary>
    public string? ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the name of the Event Grid topic to be created.
    /// </summary>
    public string? EventGridTopic { get; set; }

    /// <summary>
    /// Gets or sets the Azure location name.
    /// </summary>
    public string? Location { get; set; }
}
