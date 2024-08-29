using Marten.Events;
using Marten.Events.Aggregation;
using static Forse.MartenDemo.EventSourcing.ForseTripEvents;

namespace Forse.MartenDemo.EventSourcing.Projections;

public class ForseTripProjection : SingleStreamProjection<ForseTrip>
{
  public ForseTrip Create(IEvent<TripPlanned> @event)
  {
    return new()
    {
      Id = @event.StreamId,
      Year = @event.Data.Year,
      Destination = @event.Data.Destination,
      PlannedStart = @event.Data.PlannedStart,
      Ends = @event.Data.PlannedEnding,
      AttendeeIds = [],
      TotalDeathsRegistered = 0
    };
  }

  public void Apply(EmployeeSignsUpForTrip @event, ForseTrip trip)
  {
    trip.AttendeeIds.Add(@event.EmployeeId);
  }

  public void Apply(RegisterDeathlyInjury @event, ForseTrip trip)
  {
    trip.TotalDeathsRegistered++;
  }

  public void Apply(TripStartChanged @event, ForseTrip trip)
  {
    trip.PlannedStart = @event.NewStart;
  }

  public void Apply(TripEndChanged @event, ForseTrip trip)
  {
    trip.PlannedStart = @event.NewEnd;
  }
}
