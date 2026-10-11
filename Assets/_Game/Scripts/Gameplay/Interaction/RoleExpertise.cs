using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Interaction
{
    public static class RoleExpertise
    {
        public static readonly IReadOnlyDictionary<string, PlayerRole> TargetExpertRoles =
            new Dictionary<string, PlayerRole>(StringComparer.Ordinal)
            {
                { "overheat", PlayerRole.Engineer },
                { "powerfault", PlayerRole.Electrician },
                { "brakeleak", PlayerRole.Driver }
            };

        public static bool IsExpert(PlayerRole role, string targetId)
        {
            if (role == PlayerRole.Dispatcher)
            {
                return true;
            }

            PlayerRole expert;
            if (!TargetExpertRoles.TryGetValue(targetId, out expert))
            {
                return true;
            }

            return role == expert;
        }
    }
}