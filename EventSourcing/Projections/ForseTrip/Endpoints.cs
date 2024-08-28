using Marten;
using Microsoft.AspNetCore.Mvc;
using static Forse.MartenDemo.EventSourcing.ForseTripEvents;

namespace Forse.MartenDemo.EventSourcing.EndPoints;

public static class EndPoints
{
  public static void MapForseTripEndpoints(this WebApplication app)
  {
    // Commands
    app.MapPost("/plan-trip", async ([FromBody] TripPlanned @event,
                                     [FromServices] IDocumentSession session) =>
    {
      session.Events.Append(Guid.NewGuid(), @event);
      await session.SaveChangesAsync();
    });
    app.MapPost("/join-trip", async ([FromBody] EmployeeSignsUpForTrip @event,
                                     [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();

      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });
    app.MapPost("/register-death", async ([FromBody] RegisterDeathlyInjury @event,
                                          [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();
      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });
    app.MapPost("/change-start", async ([FromBody] TripStartChanged @event,
                                        [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();
      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });
    app.MapPost("/change-end", async ([FromBody] TripEndChanged @event,
                                      [FromServices] IDocumentSession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == @event.Year)
                              .SingleAsync();
      session.Events.Append(trip.Id, @event);
      await session.SaveChangesAsync();
    });

    //Queries
    app.MapGet("/forse-trip/{year}", async ([FromRoute] string year,
                                            [FromServices] IQuerySession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == year)
                              .SingleAsync();
      return Results.Ok(trip);
    });

    app.MapGet("/forse-trip/{year}/{version}", async ([FromRoute] string year,
                                                      [FromRoute] int version,
                                                      [FromServices] IQuerySession session) =>
    {
      var trip = await session.Query<ForseTrip>()
                              .Where(x => x.Year == year)
                              .SingleAsync();

      var tripVersioned = await session.Events.AggregateStreamAsync<ForseTrip>(trip.Id, version);
      return Results.Ok(tripVersioned);
    });

    app.MapGet("/forse-trips", async ([FromServices] IQuerySession session) =>
    {
      var trip = await session.Query<ForseTrip>().ToListAsync();
      return Results.Ok(trip);
    });
  }
}
