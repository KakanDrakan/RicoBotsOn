using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class LobbySession
    {
        public string Code { get; }
        public Match Match { get; private set; }

        public const int MaxNameLength = 20;

        private readonly Dictionary<string, Player> _players = new();
        public IReadOnlyCollection<Player> Players => _players.Values;

        public LobbySession(string code, Match match)
        {
            Code = code;
            Match = match;
        }

        public void Join(string playerId, string name)
        {
            name = (name ?? "").Trim();

            if (name.Length == 0 || name.Length > MaxNameLength)
                throw new InvalidOperationException($"Name must be 1 to {MaxNameLength} characters.");

            if (_players.Values.Any(p => p.Id != playerId && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("That name is already taken in this lobby.");

            _players[playerId] = new Player(playerId, name);
        }
    }
}
