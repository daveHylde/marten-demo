namespace Forse.MartenDemo.EventSourcing;

public record ForseTrip
{
  public required Guid Id { get; set; }
  public required string Year { get; set; }
  public required string Destination { get; set; }
  public HashSet<int> AttendeeIds { get; set; } = [];
  public DateTimeOffset PlannedStart { get; set; }
  public DateTimeOffset? Ends { get; set; }
  public int TotalDeathsRegistered { get; set; }
}

public static class ForseTripEvents
{
  public record TripPlanned(string Year,
                            string Destination,
                            DateTimeOffset PlannedStart,
                            DateTimeOffset? PlannedEnding);

  public record EmployeeSignsUpForTrip(string Year,
                                       int EmployeeId);

  public record RegisterDeathlyInjury(string Year,
                                      int EmployeeId,
                                      string Description);

  public record TripStartChanged(string Year, DateTimeOffset NewStart);
  public record TripEndChanged(string Year, DateTimeOffset NewEnd);
}
