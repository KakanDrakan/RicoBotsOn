using Domain;
using RicochetRobots.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RicoBotsOn.Tests
{
    internal static class TestHelpers
    {
        public static Board FiveByFiveNoWalls() => new(5, 5);

        public static void CloseClaimWindow(GameSession session)
        {
            typeof(ClaimWindow)
                .GetProperty(nameof(ClaimWindow.DeadlineUtc))!
                .SetValue(session.ClaimWindow, DateTime.UtcNow.AddSeconds(-1));
        }
    }
}
