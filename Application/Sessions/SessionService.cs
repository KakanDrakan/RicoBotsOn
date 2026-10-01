using Application.Exceptions;
using Application.Interfaces;
using Domain;

namespace Application.Sessions
{
    public class SessionService
    {
        private readonly ISessionStore _sessionStore;

        public SessionService(ISessionStore sessionStore)
        {
            _sessionStore = sessionStore;
        }

        public MoveResponse SubmitMove(string sessionCode, MoveRequest request)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new InvalidOperationException($"No session found for code '{sessionCode}'.");


            var direction = Enum.Parse<Direction>(request.Direction, ignoreCase: true);

            var (x, y) = session.ApplyProvingMove(request.PlayerId, request.BotId, direction);

            _sessionStore.Save(session);

            return new MoveResponse(request.BotId, x, y);
        }

        public ClaimResponse SubmitClaim(string sessionCode, ClaimRequest request)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.SubmitClaim(request.PlayerId, request.MoveCount);
            _sessionStore.Save(session);

            return new ClaimResponse(request.PlayerId, request.MoveCount, ToUnixSeconds(session.ClaimWindow.DeadlineUtc)!.Value);
        }


        public BoardStateResponse GetBoardState(string sessionCode)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new InvalidOperationException($"No session found for code '{sessionCode}'.");

            var walls = new List<CellWalls>();
            for (int x = 0; x < session.Board.Width; x++)
            {
                for (int y = 0; y < session.Board.Height; y++)
                {
                    walls.Add(new CellWalls(
                        x, y,
                        session.Board.HasWall(x, y, Direction.North),
                        session.Board.HasWall(x, y, Direction.East),
                        session.Board.HasWall(x, y, Direction.South),
                        session.Board.HasWall(x, y, Direction.West)));
                }
            }

            var bots = session.Bots.Select(b => new BotState(b.Id, b.X, b.Y)).ToList();
            var targets = session.Board.Targets.Select(t => new TargetCell(t.X, t.Y)).ToList();

            ActiveTarget? activeTarget = session.ActiveTarget is { } t2
            ? new ActiveTarget(t2.X, t2.Y, t2.BotId)
            : null;

            return new BoardStateResponse(session.Board.Width, session.Board.Height, walls, bots, targets, activeTarget);
        }

        public SessionStateResponse GetSessionState(string sessionCode)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.EnsureProvingStarted();
            _sessionStore.Save(session);

            var bots = session.Bots.Select(r => new BotState(r.Id, r.X, r.Y)).ToList();

            ActiveTarget? activeTarget = session.ActiveTarget is { } t
                ? new ActiveTarget(t.X, t.Y, t.BotId)
                : null;

            var claims = session.ClaimWindow.ClaimsInProvingOrder
            .Select(c => new ClaimInfo(c.PlayerId, c.MoveCount, c.ClaimedAtUtc))
            .ToList();

            string? currentProverPlayerId = session.CurrentAttempt?.PlayerId;
            int? currentProverClaimedMoves = session.CurrentAttempt?.ClaimedMoveCount;

            var scores = session.Scores
            .OrderByDescending(s => s.Value)
            .Select(s => new ScoreInfo(s.Key, s.Value))
            .ToList();

            return new SessionStateResponse(
            bots,
            activeTarget,
            claims,
            ToUnixSeconds(session.ClaimWindow.ActiveDeadlineUtc),
            session.Phase.ToString(),
            currentProverPlayerId,
            currentProverClaimedMoves,
            session.CurrentAttempt?.MoveCount ?? 0,
            session.RoundSucceeded,
            session.RoundNumber,
            scores);
        }

        public NextRoundResponse StartNextRound(string sessionCode)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.StartNextRound(Random.Shared);
            _sessionStore.Save(session);

            return new NextRoundResponse(session.RoundNumber);
        }

        public ResetResponse ResetProvingAttempt(string sessionCode, ResetRequest request)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.ResetCurrentProverAttempt(request.PlayerId);
            _sessionStore.Save(session);

            var bots = session.Bots.Select(b => new BotState(b.Id, b.X, b.Y)).ToList();
            return new ResetResponse(bots);
        }

        private static double? ToUnixSeconds(DateTime? utc) =>
        utc is { } value ? new DateTimeOffset(value, TimeSpan.Zero).ToUnixTimeMilliseconds() / 1000.0 : null;
    }
}
