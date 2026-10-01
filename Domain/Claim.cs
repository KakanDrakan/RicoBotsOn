namespace Domain;

public record Claim(string PlayerId, int MoveCount, DateTime ClaimedAtUtc);