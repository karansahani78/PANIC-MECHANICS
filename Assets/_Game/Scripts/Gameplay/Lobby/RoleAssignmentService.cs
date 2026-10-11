using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Lobby
{
    public static class RoleAssignmentService
    {
        public static readonly PlayerRole[] DefaultOrder =
        {
            PlayerRole.Engineer,
            PlayerRole.Electrician,
            PlayerRole.Driver,
            PlayerRole.Dispatcher
        };

        public static IReadOnlyDictionary<ulong, PlayerRole> Assign(
            IReadOnlyList<ulong> clientIds, IReadOnlyList<PlayerRole> roleOrder = null)
        {
            if (clientIds == null)
            {
                throw new ArgumentNullException(nameof(clientIds));
            }

            var order = roleOrder ?? DefaultOrder;
            if (clientIds.Count > order.Count)
            {
                throw new ArgumentException(
                    "More players (" + clientIds.Count + ") than available roles (" + order.Count + ")");
            }

            var assignments = new Dictionary<ulong, PlayerRole>();
            for (int i = 0; i < clientIds.Count; i++)
            {
                assignments[clientIds[i]] = order[i];
            }

            return assignments;
        }
    }
}
