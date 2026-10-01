using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Sessions
{
    public record ClaimRequest(string PlayerId, int MoveCount);
    public record ClaimResponse(string PlayerId, int MoveCount, double ClaimWindowDeadlineUnixSeconds);
    public record ResetRequest(string PlayerId);
    public record ResetResponse(List<BotState> Bots);
}
