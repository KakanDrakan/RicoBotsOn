using RicochetRobots.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Match
    {
        public Board Board { get; }
        public List<Bot> Bots { get; }
        public (int X, int Y, string BotId)? ActiveTarget { get; private set; }
        public int RoundNumber { get; private set; } = 1;
        public Dictionary<string, int> Scores { get; } = new();
        public Dictionary<string, (int X, int Y)> RoundStartPositions { get; private set; } = new();
        public List<MoveRecord> MoveHistory { get; } = new();
        public ClaimWindow ClaimWindow { get; private set; } = new();
        public ProvingAttempt? CurrentAttempt { get; private set; }
        public bool? RoundSucceeded { get; private set; }  // null = round still in progress

        private int _currentAttemptIndex;

        public int MinSolutionMoves { get; }
        private const int MaxQualityChecks = 50;

        public RoundPhase Phase
        {
            get
            {
                if (RoundSucceeded.HasValue) return RoundPhase.RoundOver;
                if (CurrentAttempt != null) return RoundPhase.Proving;
                return RoundPhase.WaitingForClaims;
            }
        }

        public Match(Board board, List<Bot> bots, int minSolutionMoves = 1)
        {
            Board = board;
            Bots = bots;
            MinSolutionMoves = minSolutionMoves;
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
            if (x == bot.X && y == bot.Y)
                return (x, y); //invalid move, no change

            var (fromX, fromY) = (bot.X, bot.Y);
            bot.X = x;
            bot.Y = y;
            MoveHistory.Add(new MoveRecord(bot.Id, fromX, fromY, x, y));
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

        public MoveRecord UndoProvingMove(string playerId)
        {
            EnsureProvingStarted();

            if (Phase != RoundPhase.Proving)
                throw new InvalidOperationException("No proving turn is currently active.");

            if (CurrentAttempt!.PlayerId != playerId)
                throw new InvalidOperationException("It is not your turn to prove.");

            if (MoveHistory.Count == 0)
                throw new InvalidOperationException("Nothing to undo.");

            var last = MoveHistory[^1];
            MoveHistory.RemoveAt(MoveHistory.Count - 1);

            var bot = Bots.Single(b => b.Id == last.BotId);
            bot.X = last.FromX;
            bot.Y = last.FromY;
            CurrentAttempt.UndoMove();

            return last;
        }

        public void ResetCurrentProverAttempt(string playerId)
        {
            EnsureProvingStarted();

            if (Phase != RoundPhase.Proving)
                throw new InvalidOperationException("No proving turn is currently active.");

            if (CurrentAttempt!.PlayerId != playerId)
                throw new InvalidOperationException("It is not your turn to prove.");

            CurrentAttempt.ResetMoveCount();
            MoveHistory.Clear();
            ResetBotsToRoundStart();
        }

        private void BeginAttempt(int index)
        {
            MoveHistory.Clear();
            var claim = ClaimWindow.ClaimsInProvingOrder[index];
            _currentAttemptIndex = index;
            CurrentAttempt = new ProvingAttempt(claim.PlayerId, claim.MoveCount);
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

        public void SelectNewTarget(Random random)
        {
            if (Board.Targets.Count == 0 || Bots.Count == 0)
                throw new InvalidOperationException("Need at least one target cell and one bot to select a round target.");

            (int X, int Y, Bot Bot)? chosen = null;
            (int X, int Y, Bot Bot)? fallback = null;
            var maxAttempts = Board.Targets.Count * Bots.Count * 10;

            var positions = Bots.Select(b => (b.X, b.Y)).ToArray();

            for (int attempts = 0; attempts < maxAttempts; attempts++)
            {
                var target = Board.Targets[random.Next(Board.Targets.Count)];
                var bot = Bots[random.Next(Bots.Count)];
                if (bot.X == target.X && bot.Y == target.Y) continue;

                fallback ??= (target.X, target.Y, bot);

                if (MinSolutionMoves <= 1 || !SolutionSearch.CanReach(Board, positions, Bots.IndexOf(bot), target, MinSolutionMoves - 1))
                {
                    chosen = (target.X, target.Y, bot);
                    break;
                }

                if (attempts >= MaxQualityChecks) break;  // fail-safe: use fallback
            }

            var pick = chosen ?? fallback
                ?? throw new InvalidOperationException("No valid target/bot combination available");

            ActiveTarget = (pick.X, pick.Y, pick.Bot.Id);

            RoundStartPositions = Bots.ToDictionary(b => b.Id, b => (b.X, b.Y));
            ClaimWindow = new ClaimWindow();
            CurrentAttempt = null;
            MoveHistory.Clear();
            RoundSucceeded = null;
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
    }
}
