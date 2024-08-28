namespace Forse.MartenDemo.DocTypes;

public record ForseTrip
{
  public required string Id { get; set; }
  public required string Destination { get; set; }
  public HashSet<int> AttendeeIds { get; set; } = [];
  public DateTimeOffset PlannedStart { get; set; }
  public DateTimeOffset? Ends { get; set; }
  public int TotalDeathsRegistered { get; set; }
}
