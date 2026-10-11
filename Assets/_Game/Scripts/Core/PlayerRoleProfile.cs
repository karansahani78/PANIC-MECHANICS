using System.Collections.Generic;

namespace PanicMechanics.Core
{
    public sealed class PlayerRoleProfile
    {
        public readonly PlayerRole Role;
        public readonly string DisplayName;
        private readonly HashSet<TrainSubsystem> _visibleSubsystems;

        public PlayerRoleProfile(PlayerRole role, string displayName, IEnumerable<TrainSubsystem> visibleSubsystems)
        {
            Role = role;
            DisplayName = displayName;
            _visibleSubsystems = new HashSet<TrainSubsystem>(visibleSubsystems);
        }

        public bool CanSeeDiagnosticsFor(TrainSubsystem subsystem)
        {
            return _visibleSubsystems.Contains(subsystem);
        }

        public IEnumerable<TrainSubsystem> VisibleSubsystems
        {
            get { return _visibleSubsystems; }
        }

        public static PlayerRoleProfile CreateDefault(PlayerRole role)
        {
            switch (role)
            {
                case PlayerRole.Engineer:
                    return new PlayerRoleProfile(role, "Engineer", new[]
                    {
                        TrainSubsystem.Engine, TrainSubsystem.Cooling
                    });
                case PlayerRole.Electrician:
                    return new PlayerRoleProfile(role, "Electrician", new[]
                    {
                        TrainSubsystem.Electrical
                    });
                case PlayerRole.Driver:
                    return new PlayerRoleProfile(role, "Driver", new[]
                    {
                        TrainSubsystem.Brakes, TrainSubsystem.Track
                    });
                case PlayerRole.Dispatcher:
                default:
                    return new PlayerRoleProfile(role, "Dispatcher", new[]
                    {
                        TrainSubsystem.Engine, TrainSubsystem.Electrical,
                        TrainSubsystem.Cooling, TrainSubsystem.Brakes,
                        TrainSubsystem.Track, TrainSubsystem.Communication
                    });
            }
        }
    }
}
