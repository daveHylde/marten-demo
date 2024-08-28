using Forse.MartenDemo.DocTypes;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Setup Marten
builder.Services.AddMarten(cfg =>
{
  cfg.Connection("Host=localhost;Port=5432;Database=demo;Username=admin;Password=admin;");

  cfg.UseSystemTextJsonForSerialization(EnumStorage.AsString, Casing.CamelCase);

  cfg.Schema.For<ForseEmployee>().Identity(x => x.EmployeeNumber);
  cfg.Schema.For<ForseEmployee>().Index(x => x.Name);
}).UseLightweightSessions();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// Upsert employee
app.MapPost("/employee", async ([FromBody] ForseEmployee employee,
                                [FromServices] IDocumentSession session) =>
{
  session.Store(employee);
  await session.SaveChangesAsync();
  return Results.Created();
});

// Get employee
app.MapGet("/employee/{id}", async ([FromRoute] int id,
                                    [FromServices] IQuerySession session) =>
{
  return Results.Ok(await session.LoadAsync<ForseEmployee>(id));
  // Or like this
  //  return Results.Ok(await session.Query<ForseEmployee>()
  //                                 .Where(x => x.EmployeeNumber == id)
  //                                 .SingleAsync());
});

// Upsert forse trip
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
    PlannedStart = starts,
    Ends = ends
  });
  await session.SaveChangesAsync();
  return Results.Ok();
});

// Employee joins trip
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

// Ups - someone died
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

// Get trip - the easy way
app.MapGet("/forse-trip/{year}", async ([FromRoute] string year,
                                        [FromServices] IQuerySession session) =>
{
  var trip = await session.LoadAsync<ForseTrip>(year);
  if (trip is null) return Results.BadRequest();

  var attendees = await session.LoadManyAsync<ForseEmployee>(trip.AttendeeIds);

  var result = new ForseTripDto(trip.Id,
                                trip.Destination,
                                [.. attendees],
                                trip.PlannedStart,
                                trip.Ends,
                                trip.TotalDeathsRegistered);

  return Results.Ok(result);
});

// Get trip - the "I know what I'm doing"-way
app.MapGet("/forse-trip-iknowwhatimdoing/{year}", async ([FromRoute] string year,
                                                         [FromServices] IQuerySession session,
                                                         CancellationToken ct) =>
{
  var schema = session.DocumentStore.Options.Schema;

  var q2 = @$"
			SELECT json_build_object(
				'year', t.data->>'id',
				'destination', t.data->>'destination',
				'attendees', (
					SELECT json_agg(e.data)
					FROM {schema.For<ForseEmployee>()} e
					WHERE (e.data->>'employeeNumber')::int = ANY(
						SELECT jsonb_array_elements_text(t.data->'attendeeIds')::int
					)
				),
				'starts', t.data->>'plannedStart',
				'ends', t.data->>'ends',
				'totalDeathsRegistered', (t.data->>'totalDeathsRegistered')::int
			) AS result
			FROM {schema.For<ForseTrip>()} t
			WHERE t.data->>'id' = ?
			LIMIT 1;
		";

  var result = await session.AdvancedSql.QueryAsync<ForseTripDto>(q2, ct, year);
  return Results.Ok(result);
});

app.Run();

