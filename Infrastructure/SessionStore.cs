using Application.Interfaces;
using Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class SessionStore : ISessionStore
    {
        private readonly ConcurrentDictionary<string, LobbySession> _sessions = new(StringComparer.OrdinalIgnoreCase);

        public LobbySession? Get(string code) =>
            _sessions.TryGetValue(code, out var session) ? session : null;

        public void Save(LobbySession session) =>
            _sessions[session.Code] = session;
    }
}
