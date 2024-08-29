namespace Forse.MartenDemo.EventSourcing;

public record ForseTripDto(string Year,
                           string Destination,
                           IList<ForseEmployee> Attendees,
                           DateTimeOffset Starts,
                           DateTimeOffset? Ends,
                           int? TotalDeathsRegistered);
