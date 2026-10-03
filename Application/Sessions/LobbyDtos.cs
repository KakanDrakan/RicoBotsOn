using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Sessions
{
    public record PlayerInfo(string Id, string Name);
    public record JoinRequest(string PlayerToken, string Name);
    public record JoinResponse(string PlayerToken, List<PlayerInfo> Players);
    public record CreateSessionRequest(string PlayerToken, string Name);
    public record CreateSessionResponse(string Code, List<PlayerInfo> Players);
}
