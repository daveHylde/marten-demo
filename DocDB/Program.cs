using Forse.MartenDemo.DocTypes;
using Marten;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(cfg =>
{
  cfg.Connection("Host=localhost;Port=5432;Database=demo;Username=admin;Password=admin;");

  cfg.UseSystemTextJsonForSerialization(configure: c =>
  {
    c.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
  });
  cfg.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;

  cfg.Schema.For<ForseEmployee>().Identity(x => x.EmployeeNumber);
  cfg.Schema.For<ForseEmployee>().Index(x => x.Name);
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/employee", async ([FromBody] ForseEmployee employee,
                                [FromServices] IDocumentSession session) =>
{
  session.Store(employee);
  await session.SaveChangesAsync();
  return Results.Created();
});

app.MapGet("/employee/{id}", async ([FromRoute] int id,
                                    [FromServices] IQuerySession session) =>
{
  return Results.Ok(await session.LoadAsync<ForseEmployee>(id));
  // Or like this
  //  return Results.Ok(await session.Query<ForseEmployee>()
  //                                 .Where(x => x.EmployeeNumber == id)
  //                                 .SingleAsync());
});

app.MapPost("/forse-trip/{year}/{destination}", async ([FromRoute] string year,
                                                       [FromRoute] string destination,
                                                       [FromQuery] DateTime starts,
                                                       [FromQuery] DateTime ends,
                                                       [FromServices] IDocumentSession session) =>
{
  var trip = await session.LoadAsync<ForseTrip>(year);
  if (trip is not null) return Results.BadRequest("Trip exists");

  session.Store(new ForseTrip()
  {
    Id = year,
    Destination = destination,
    Starts = starts,
    Ends = ends
  });
  await session.SaveChangesAsync();
  return Results.Ok();
});

app.MapPut("/forse-trip/join/{year}/{id}", async ([FromRoute] int id,
                                                  [FromRoute] string year,
                                                  [FromServices] IDocumentSession session) =>
{
  var tripT = session.LoadAsync<ForseTrip>(year);
  var employeeT = session.LoadAsync<ForseEmployee>(id);
  var trip = await tripT;
  var employee = await employeeT;

  if (trip is null || employee is null)
  {
    return Results.BadRequest();
  }

  trip.AttendeeIds.Add(id);
  session.Store(trip);
  await session.SaveChangesAsync();
  return Results.Ok();
});

app.MapPut("/forse-trip/register-deathly-injury/{year}", async ([FromRoute] string year,
                                                                [FromServices] IDocumentSession session) =>
{
  var trip = await session.LoadAsync<ForseTrip>(year);
  if (trip is null)
  {
    return Results.BadRequest();
  }

  trip.TotalDeathsRegistered++;
  session.Store(trip);
  await session.SaveChangesAsync();
  return Results.Ok();
});

app.MapGet("/forse-trip/{year}", async ([FromRoute] string year,
                                        [FromServices] IQuerySession session) =>
{
  var trip = await session.LoadAsync<ForseTrip>(year);
  if (trip is null) return Results.BadRequest();

  var attendees = await session.LoadManyAsync<ForseEmployee>(trip.AttendeeIds);

  var result = new ForseTripDto(trip.Id,
                                trip.Destination,
                                [.. attendees],
                                trip.Starts,
                                trip.Ends,
                                trip.TotalDeathsRegistered);

  return Results.Ok(result);
});

app.Run();

