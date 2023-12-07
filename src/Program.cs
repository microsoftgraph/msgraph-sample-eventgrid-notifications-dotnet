// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Identity;
using GraphEventGrid;
using GraphEventGrid.Handlers;
using Microsoft.Graph;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = builder.Configuration
    .GetSection("AppSettings")
    .Get<AppSettings>();

if (settings is null)
{
    throw new ApplicationException("Could not load settings from appsettings.json");
}

builder.Services.AddSingleton(settings);

var credential = new ClientSecretCredential(
    settings.TenantId,
    settings.ClientId,
    settings.ClientSecret);

var graphClient = new GraphServiceClient(credential);
builder.Services.AddSingleton(graphClient);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map event handler endpoints to receive
// notifications from Azure Event Grid.
NotificationEventHandler.Map(app);

// Register event handler to run after app has started
// to ensure the notification subscription exists
ApplicationEventHandler.Register(app.Lifetime, settings, graphClient, app.Logger);

app.Run();
