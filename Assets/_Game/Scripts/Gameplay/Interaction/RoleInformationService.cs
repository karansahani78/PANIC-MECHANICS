using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Interaction
{
    public sealed class RoleInformationService
    {
        private readonly Dictionary<PlayerRole, PlayerRoleProfile> _profiles =
            new Dictionary<PlayerRole, PlayerRoleProfile>();

        public RoleInformationService(IEnumerable<PlayerRoleProfile> profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException(nameof(profiles));
            }

            foreach (var profile in profiles)
            {
                _profiles[profile.Role] = profile;
            }
        }

        public static RoleInformationService CreateDefault()
        {
            return new RoleInformationService(new[]
            {
                PlayerRoleProfile.CreateDefault(PlayerRole.Engineer),
                PlayerRoleProfile.CreateDefault(PlayerRole.Electrician),
                PlayerRoleProfile.CreateDefault(PlayerRole.Driver),
                PlayerRoleProfile.CreateDefault(PlayerRole.Dispatcher)
            });
        }

        public PlayerRoleProfile Get(PlayerRole role)
        {
            PlayerRoleProfile profile;
            if (_profiles.TryGetValue(role, out profile))
            {
                return profile;
            }

            return PlayerRoleProfile.CreateDefault(role);
        }

        public bool CanSee(PlayerRole role, TrainSubsystem subsystem)
        {
            return Get(role).CanSeeDiagnosticsFor(subsystem);
        }
    }
}
