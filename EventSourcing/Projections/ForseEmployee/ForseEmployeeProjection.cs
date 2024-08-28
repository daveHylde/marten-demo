using Marten.Events.Projections;
using static Forse.MartenDemo.EventSourcing.ForseEmployeeEvents;
using static Forse.MartenDemo.EventSourcing.ForseTripEvents;

namespace Forse.MartenDemo.EventSourcing;

public class ForseEmployeeProjection : MultiStreamProjection<ForseEmployee, int>
{

  public ForseEmployeeProjection()
  {
    Identity<EmployeeHired>(x => x.EmployeeNumber);
    Identity<EmployeeNameChanged>(x => x.EmployeeNumber);

    //    Identity<EmployeeSignsUpForTrip>(x => x.EmployeeId);
    Identity<RegisterDeathlyInjury>(x => x.EmployeeId);
  }

  public ForseEmployee Create(EmployeeHired @event)
  {
    return new()
    {
      Id = @event.EmployeeNumber,
      Name = @event.Name
    };
  }

  public void Apply(EmployeeNameChanged @event, ForseEmployee employee)
  {
    employee.Name = @event.NewName;
  }

  //  public void Apply(EmployeeSignsUpForTrip @event, ForseEmployee employee)
  //  {
  //    employee.ForseTrips.Add(@event.Year);
  //  }

  public bool ShouldDelete(RegisterDeathlyInjury @event, ForseEmployee employee) => true;
}
