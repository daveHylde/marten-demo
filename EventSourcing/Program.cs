using Forse.MartenDemo.EventSourcing;
using Forse.MartenDemo.EventSourcing.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Mvc;
using Weasel.Core;
using Forse.MartenDemo.EventSourcing.EndPoints;
using Marten.Events.Daemon.Resiliency;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Setup Marten
builder.Services.AddMarten(cfg =>
{
  cfg.Connection("Host=localhost;Port=5432;Database=demo;Username=admin;Password=admin;");
  cfg.DatabaseSchemaName = "event_sourcing_demo";

  cfg.UseSystemTextJsonForSerialization(EnumStorage.AsString, Casing.CamelCase);

  cfg.Projections.Add<ForseTripProjection>(ProjectionLifecycle.Inline);
  cfg.Projections.Add<ForseEmployeeProjection>(ProjectionLifecycle.Async);
})
.UseLightweightSessions()
.AddAsyncDaemon(DaemonMode.Solo);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapForseTripEndpoints();
app.MapForseEmployeeEndpoints();

app.MapGet("/sandbox", async ([FromServices] IDocumentStore store, CancellationToken ct) =>
{
  var daemon = await store.BuildProjectionDaemonAsync();
  await daemon.RebuildProjectionAsync<ForseTrip>(ct);
});


app.Run();
