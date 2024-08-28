using Marten;
using Microsoft.AspNetCore.Mvc;
using static Forse.MartenDemo.EventSourcing.ForseEmployeeEvents;

namespace Forse.MartenDemo.EventSourcing.EndPoints;

public static class EmployeeEndPoints
{
  public static void MapForseEmployeeEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("employee").WithTags("Forse Employees");
    // Commands
    group.MapPost("/onboard", async ([FromBody] EmployeeHired @event,
                                   [FromServices] IDocumentSession session) =>
    {
      session.Events.Append(Guid.NewGuid(), @event);
      await session.SaveChangesAsync();
    });
    group.MapPost("/change-name", async ([FromBody] EmployeeNameChanged @event,
                                     [FromServices] IDocumentSession session) =>
    {
      session.Events.Append(Guid.NewGuid(), @event);
      await session.SaveChangesAsync();
    });

    //Queries
    group.MapGet("/{employeeNumber}", async ([FromRoute] int employeeNumber,
                                                    [FromServices] IQuerySession session) =>
    {
      var emp = await session.LoadAsync<ForseEmployee>(employeeNumber);
      return Results.Ok(emp);
    });

  }
}
