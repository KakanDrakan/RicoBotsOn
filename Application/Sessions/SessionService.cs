using Application.Exceptions;
using Application.Interfaces;
using Domain;

namespace Application.Sessions
{
    public class SessionService
    {
        private readonly ISessionStore _sessionStore;
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";  // no 0/O/1/I
        private const int CodeLength = 5;

        public SessionService(ISessionStore sessionStore)
        {
            _sessionStore = sessionStore;
        }

        public CreateSessionResponse CreateSession(CreateSessionRequest request)
        {
            string code;
            var attempts = 0;
            do
            {
                if (attempts++ >= 20)
                    throw new InvalidOperationException("Could not generate a unique lobby code.");

                code = new string(Enumerable.Range(0, CodeLength)
                    .Select(_ => CodeAlphabet[Random.Shared.Next(CodeAlphabet.Length)])
                    .ToArray());
            } while (_sessionStore.Get(code) != null);

            var session = new LobbySession(code, MatchFactory.CreateDefault(Random.Shared));
            session.Join(request.PlayerToken, request.Name);
            _sessionStore.Save(session);

            return new CreateSessionResponse(code, ToPlayerInfos(session));
        }

        public JoinResponse Join(string sessionCode, JoinRequest request)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.Join(request.PlayerToken, request.Name);
            _sessionStore.Save(session);

            return new JoinResponse(request.PlayerToken, ToPlayerInfos(session));
        }

        private static List<PlayerInfo> ToPlayerInfos(LobbySession session) =>
            session.Players.Select(p => new PlayerInfo(p.Id, p.Name)).ToList();

        public MoveResponse SubmitMove(string sessionCode, MoveRequest request)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);


            var direction = Enum.Parse<Direction>(request.Direction, ignoreCase: true);

            var (x, y) = session.Match.ApplyProvingMove(request.PlayerId, request.BotId, direction);

            _sessionStore.Save(session);

            return new MoveResponse(request.BotId, x, y);
        }

        public ClaimResponse SubmitClaim(string sessionCode, ClaimRequest request)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.Match.SubmitClaim(request.PlayerId, request.MoveCount);
            _sessionStore.Save(session);

            return new ClaimResponse(request.PlayerId, request.MoveCount, ToUnixSeconds(session.Match.ClaimWindow.DeadlineUtc)!.Value);
        }


        public BoardStateResponse GetBoardState(string sessionCode)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            var board = session.Match.Board;

            var walls = new List<CellWalls>();
            for (int x = 0; x < session.Match.Board.Width; x++)
            {
                for (int y = 0; y < session.Match.Board.Height; y++)
                {
                    walls.Add(new CellWalls(
                        x, y,
                        session.Match.Board.HasWall(x, y, Direction.North),
                        session.Match.Board.HasWall(x, y, Direction.East),
                        session.Match.Board.HasWall(x, y, Direction.South),
                        session.Match.Board.HasWall(x, y, Direction.West)));
                }
            }

            var targets = session.Match.Board.Targets.Select(t => new TargetCell(t.X, t.Y)).ToList();

            return new BoardStateResponse(session.Match.Board.Width, session.Match.Board.Height, walls, targets);
        }

        public SessionStateResponse GetSessionState(string sessionCode)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);
            
            var match = session.Match;

            match.EnsureProvingStarted();
            _sessionStore.Save(session);

            var bots = match.Bots.Select(r => new BotState(r.Id, r.X, r.Y)).ToList();

            ActiveTarget? activeTarget = match.ActiveTarget is { } t
            ? new ActiveTarget(t.X, t.Y, t.BotId)
            : null;

            var claims = match.ClaimWindow.ClaimsInProvingOrder
            .Select(c => new ClaimInfo(c.PlayerId, c.MoveCount, c.ClaimedAtUtc))
            .ToList();

            string? currentProverPlayerId = match.CurrentAttempt?.PlayerId;
            int? currentProverClaimedMoves = match.CurrentAttempt?.ClaimedMoveCount;

            var scores = match.Scores
            .OrderByDescending(s => s.Value)
            .Select(s => new ScoreInfo(s.Key, s.Value))
            .ToList();

            return new SessionStateResponse(
            bots,
            activeTarget,
            claims,
            ToUnixSeconds(match.ClaimWindow.ActiveDeadlineUtc),
            match.Phase.ToString(),
            currentProverPlayerId,
            currentProverClaimedMoves,
            match.CurrentAttempt?.MoveCount ?? 0,
            match.RoundSucceeded,
            match.RoundNumber,
            scores,
            ToPlayerInfos(session));
        }

        public NextRoundResponse StartNextRound(string sessionCode)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.Match.StartNextRound(Random.Shared);
            _sessionStore.Save(session);

            return new NextRoundResponse(session.Match.RoundNumber);
        }

        public ResetResponse ResetProvingAttempt(string sessionCode, ResetRequest request)
        {
            var session = _sessionStore.Get(sessionCode)
                ?? throw new SessionNotFoundException(sessionCode);

            session.Match.ResetCurrentProverAttempt(request.PlayerId);
            _sessionStore.Save(session);

            var bots = session.Match.Bots.Select(b => new BotState(b.Id, b.X, b.Y)).ToList();
            return new ResetResponse(bots);
        }

        private static double? ToUnixSeconds(DateTime? utc) =>
        utc is { } value ? new DateTimeOffset(value, TimeSpan.Zero).ToUnixTimeMilliseconds() / 1000.0 : null;
    }
}
