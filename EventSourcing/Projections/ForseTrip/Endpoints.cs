using Marten;
using Microsoft.AspNetCore.Mvc;
using static Forse.MartenDemo.EventSourcing.ForseTripEvents;

namespace Forse.MartenDemo.EventSourcing.EndPoints;

public static class EndPoints
{
  public static void MapForseTripEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("trip").WithTags("Forse Trip");
    // Commands
    group.MapPost("/plan", async ([FromBody] TripPlanned @event,
                                  [FromServices] IDocumentSession session) =>
    {
      session.Events.Append(Guid.NewGuid(), @event);
      await session.SaveChangesAsync();
    });
    group.MapPost("/join", async ([FromBody] EmployeeSignsUpForTrip @event,
                                  [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();

      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });
    group.MapPost("/register-death", async ([FromBody] RegisterDeathlyInjury @event,
                                            [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();
      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });
    group.MapPost("/change-start", async ([FromBody] TripStartChanged @event,
                                          [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();
      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });
    group.MapPost("/change-end", async ([FromBody] TripEndChanged @event,
                                        [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();
      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });

    //Queries
    group.MapGet("/{year}", async ([FromRoute] string year,
                                   [FromServices] IQuerySession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == year)
                              .SingleAsync();
      return Results.Ok(trip);
    });

    group.MapGet("/{year}/{version}", async ([FromRoute] string year,
                                             [FromRoute] int version,
                                             [FromServices] IQuerySession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == year)
                              .SingleAsync();

      var tripVersioned = await session.Events.AggregateStreamAsync<ForseTrip>(trip.Id, version);
      return Results.Ok(tripVersioned);
    });

    group.MapGet("/", async ([FromServices] IQuerySession session) =>
    {
      var trip = await session.Query<ForseTrip>().ToListAsync();
      return Results.Ok(trip);
    });

    group.MapGet("/iknowwhatimdoing/{year}", async ([FromRoute] string year,
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
					WHERE (e.data->>'id')::int = ANY(
						SELECT jsonb_array_elements_text(t.data->'attendeeIds')::int
					)
				),
				'starts', t.data->>'plannedStart',
				'ends', t.data->>'ends',
				'totalDeathsRegistered', (t.data->>'totalDeathsRegistered')::int
			) AS result
			FROM {schema.For<ForseTrip>()} t
			WHERE t.data->>'year' = ?
			LIMIT 1;
		";

      var result = await session.AdvancedSql.QueryAsync<ForseTripDto>(q2, ct, year);
      return Results.Ok(result);
    });
  }
}
