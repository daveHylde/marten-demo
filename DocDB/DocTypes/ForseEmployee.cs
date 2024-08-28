namespace Forse.MartenDemo.DocTypes;

public record ForseEmployee
{
  public int EmployeeNumber { get; set; }
  public required string Name { get; set; }
}
