using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public record MoveRecord(string BotId, int FromX, int FromY, int ToX, int ToY);
}
