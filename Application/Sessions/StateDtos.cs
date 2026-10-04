using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Sessions
{
    public record SessionStateResponse(
    List<BotState> Bots,
    ActiveTarget? ActiveTarget,
    List<ClaimInfo> Claims,
    double? ClaimWindowDeadlineUnixSeconds,
    string Phase,
    string? CurrentProverPlayerId,
    int? CurrentProverClaimedMoves,
    int ProveMoveCount,
    bool? RoundSucceeded,
    int RoundNumber,
    List<ScoreInfo> Scores,
    List<PlayerInfo> Players,
    List<MoveInfo> MoveHistory);
    

    public record ClaimInfo(string PlayerId, int MoveCount, DateTime ClaimedAtUtc);
    public record ScoreInfo(string PlayerId, int Points);
    public record NextRoundResponse(int RoundNumber);
    public record MoveInfo(string BotId, int FromX, int FromY, int ToX, int ToY);
}
