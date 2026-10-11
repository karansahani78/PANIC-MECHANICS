namespace PanicMechanics.Gameplay.Simulation
{
    public readonly struct DifficultyProfile
    {
        public readonly int Players;
        public readonly float HeatGainMultiplier;
        public readonly float TimePressureMultiplier;
        public readonly float TargetDurationSeconds;

        public DifficultyProfile(
            int players, float heatGainMultiplier, float timePressureMultiplier, float targetDurationSeconds)
        {
            Players = players;
            HeatGainMultiplier = heatGainMultiplier;
            TimePressureMultiplier = timePressureMultiplier;
            TargetDurationSeconds = targetDurationSeconds;
        }
    }

    public static class DifficultyScaler
    {
        public const int MinPlayers = 2;
        public const int MaxPlayers = 4;

        public static DifficultyProfile ForPlayers(int players)
        {
            int clamped = players;
            if (clamped < MinPlayers)
            {
                clamped = MinPlayers;
            }
            else if (clamped > MaxPlayers)
            {
                clamped = MaxPlayers;
            }

            float heat = 0.90f + 0.10f * (clamped - MinPlayers);
            float pressure = 0.85f + 0.075f * (clamped - MinPlayers);
            float duration = 300f + 30f * (clamped - MinPlayers);
            return new DifficultyProfile(clamped, heat, pressure, duration);
        }
    }
}
