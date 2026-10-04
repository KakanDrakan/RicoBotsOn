using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Sessions
{
    public record MoveRequest(string BotId, string Direction, string PlayerId);
    public record MoveResponse(string BotId, int X, int Y);
    public record UndoRequest(string PlayerId);
}
