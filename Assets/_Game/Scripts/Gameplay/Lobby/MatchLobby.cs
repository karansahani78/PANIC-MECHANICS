using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Lobby
{
    public sealed class MatchLobby
    {
        private readonly List<ulong> _players = new List<ulong>();

        public int MinPlayers = 2;
        public int MaxPlayers = 4;

        public IReadOnlyList<ulong> Players
        {
            get { return _players; }
        }

        public int Count
        {
            get { return _players.Count; }
        }

        public bool CanStart
        {
            get { return _players.Count >= MinPlayers; }
        }

        public bool Contains(ulong clientId)
        {
            return _players.Contains(clientId);
        }

        public bool TryJoin(ulong clientId)
        {
            if (_players.Count >= MaxPlayers || _players.Contains(clientId))
            {
                return false;
            }

            _players.Add(clientId);
            return true;
        }

        public bool TryLeave(ulong clientId)
        {
            return _players.Remove(clientId);
        }

        public IReadOnlyDictionary<ulong, PlayerRole> AssignRoles()
        {
            return RoleAssignmentService.Assign(_players);
        }

        public void Clear()
        {
            _players.Clear();
        }
    }
}
