using RicochetRobots.Domain;

namespace Domain
{
    public class GameSession
    {
        public string Code { get; }
        public Board Board { get; }
        public List<Bot> Bots { get; }
        public (int X, int Y, string BotId)? ActiveTarget { get; private set; }
        public int RoundNumber { get; private set; } = 1;
        public Dictionary<string, int> Scores { get; } = new();
        public Dictionary<string, (int X, int Y)> RoundStartPositions { get; private set; } = new();

        public ClaimWindow ClaimWindow { get; private set; } = new ();
        public ProvingAttempt? CurrentAttempt { get; private set; }
        public bool? RoundSucceeded { get; private set; }  // null = round still in progress

        private int _currentAttemptIndex;

        public RoundPhase Phase
        {
            get
            {
                if (RoundSucceeded.HasValue) return RoundPhase.RoundOver;
                if (CurrentAttempt != null) return RoundPhase.Proving;
                return RoundPhase.WaitingForClaims;
            }
        }

        public GameSession(string code, Board board, List<Bot> bots)
        {
            Code = code;
            Board = board;
            Bots = bots;
        }
        public void SubmitClaim(string playerId, int moveCount) => ClaimWindow.Submit(playerId, moveCount);

        public void EnsureProvingStarted()
        {
            if (Phase != RoundPhase.WaitingForClaims) return;
            if (!ClaimWindow.HasClosed) return;

            BeginAttempt(0);
        }

        public (int X, int Y) ApplyProvingMove(string playerId, string botId, Direction direction)
        {
            EnsureProvingStarted();

            if (Phase != RoundPhase.Proving)
                throw new InvalidOperationException("No proving turn is currently active.");

            if (CurrentAttempt!.PlayerId != playerId)
                throw new InvalidOperationException("It is not your turn to prove.");

            var bot = Bots.SingleOrDefault(b => b.Id == botId)
                ?? throw new InvalidOperationException($"No bot '{botId}' in this session.");

            var (x, y) = MoveResolver.Resolve(Board, Bots, bot, direction);
            bot.X = x;
            bot.Y = y;
            CurrentAttempt.RegisterMove();

            bool reachedTarget = ActiveTarget is { } t && t.BotId == bot.Id && bot.X == t.X && bot.Y == t.Y;

            if (reachedTarget && CurrentAttempt.Succeeds())
            {
                RoundSucceeded = true;
                Scores[playerId] = Scores.TryGetValue(playerId, out var points) ? points + 1 : 1;
            }
            else if (CurrentAttempt.HasUsedAllMoves)
            {
                AdvanceProver();
            }

            return (x, y);
        }

        public void ResetCurrentProverAttempt(string playerId)
        {
            EnsureProvingStarted();

            if (Phase != RoundPhase.Proving)
                throw new InvalidOperationException("No proving turn is currently active.");

            if (CurrentAttempt!.PlayerId != playerId)
                throw new InvalidOperationException("It is not your turn to prove.");

            CurrentAttempt.ResetMoveCount();
            ResetBotsToRoundStart();
        }

        public void BeginAttempt(int index)
        {
            if (index < 0 || index >= ClaimWindow.ClaimsInProvingOrder.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid claim index.");
            var claim = ClaimWindow.ClaimsInProvingOrder[index];
            CurrentAttempt = new ProvingAttempt(claim.PlayerId, claim.MoveCount);
            _currentAttemptIndex = index;
            
            ResetBotsToRoundStart();
        }

        private void AdvanceProver()
        {
            var order = ClaimWindow.ClaimsInProvingOrder;
            var nextIndex = _currentAttemptIndex + 1;

            if (nextIndex < order.Count)
            {
                BeginAttempt(nextIndex);
            }
            else
            {
                RoundSucceeded = false;
                CurrentAttempt = null;
            }
        }

        private void ResetBotsToRoundStart()
        {
            foreach (var bot in Bots)
            {
                if (RoundStartPositions.TryGetValue(bot.Id, out var pos))
                {
                    bot.X = pos.X;
                    bot.Y = pos.Y;
                }
            }
        }

        public void StartNextRound(Random random)
        {
            if (Phase != RoundPhase.RoundOver)
                throw new InvalidOperationException("The current round is not over yet.");

            if (RoundSucceeded == false)
                ResetBotsToRoundStart();

            RoundNumber++;
            SelectNewTarget(random);
        }

        public void SelectNewTarget(Random random)
        {
            if (Board.Targets.Count == 0 || Bots.Count == 0)
                throw new InvalidOperationException("Need at least one target cell and one bot to select a round target.");

            (int X, int Y) target;
            Bot bot;
            var maxAttempts = 100;
            var attempts = 0;
            do
            {
                if (attempts++ >= maxAttempts)
                    throw new InvalidOperationException("No valid target/bot combination available");
                
                target = Board.Targets[random.Next(Board.Targets.Count)];
                bot = Bots[random.Next(Bots.Count)];
            } while (bot.X == target.X && bot.Y == target.Y);

            ActiveTarget = (target.X, target.Y, bot.Id);

            RoundStartPositions = Bots.ToDictionary(b => b.Id, b => (b.X, b.Y));
            ClaimWindow = new ClaimWindow();
            CurrentAttempt = null;
            RoundSucceeded = null;
        }
    }
}
