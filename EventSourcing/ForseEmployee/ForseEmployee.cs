namespace Forse.MartenDemo.EventSourcing;

public record ForseEmployee
{
  public int Id { get; set; }
  public required string Name { get; set; }
  //  public HashSet<string> ForseTrips { get; set; } = [];
}

public static class ForseEmployeeEvents
{
  public record EmployeeHired(int EmployeeNumber, string Name);
  public record EmployeeNameChanged(int EmployeeNumber, string NewName);
}
