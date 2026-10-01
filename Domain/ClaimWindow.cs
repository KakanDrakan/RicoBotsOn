using Domain;

namespace RicochetRobots.Domain;

public class ClaimWindow
{
    private readonly List<Claim> _claims = new();

    public IReadOnlyList<Claim> Claims => _claims;
    public DateTime? DeadlineUtc { get; private set; }

    // null when expired
    public DateTime? ActiveDeadlineUtc =>
        DeadlineUtc.HasValue && DeadlineUtc.Value > DateTime.UtcNow ? DeadlineUtc : null;

    public bool HasClosed => _claims.Count > 0 && ActiveDeadlineUtc == null;

    public void Submit(string playerId, int moveCount)
    {
        if (moveCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(moveCount), "Move count must be positive.");

        if (_claims.Count == 0)
        {
            DeadlineUtc = DateTime.UtcNow.AddSeconds(30);
        }
        else if (ActiveDeadlineUtc == null)
        {
            throw new InvalidOperationException("The claim window has already closed.");
        }

        // Ignores duplicate claims for the same move count by the same player.
        bool alreadyClaimedThisNumber = _claims.Any(c => c.PlayerId == playerId && c.MoveCount == moveCount);
        if (alreadyClaimedThisNumber)
            return;

        _claims.Add(new Claim(playerId, moveCount, DateTime.UtcNow));
    }

    // Sortst by lowest, then fastest
    public IReadOnlyList<Claim> ClaimsInProvingOrder =>
        _claims.OrderBy(c => c.MoveCount).ThenBy(c => c.ClaimedAtUtc).ToList();
}