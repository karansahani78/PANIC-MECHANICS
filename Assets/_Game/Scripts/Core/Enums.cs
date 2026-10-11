namespace PanicMechanics.Core
{
    public enum GamePhase
    {
        Boot,
        MainMenu,
        Lobby,
        Countdown,
        Running,
        Debrief,
        Results
    }

    public enum PlayerRole
    {
        Engineer,
        Electrician,
        Driver,
        Dispatcher
    }

    public enum TrainSubsystem
    {
        Engine,
        Electrical,
        Cooling,
        Brakes,
        Track,
        Communication
    }

    public enum IncidentPhase
    {
        Dormant,
        Active,
        Escalating,
        Stabilized,
        Resolved,
        Failed
    }

    public enum MissionOutcome
    {
        Success,
        PartialSuccess,
        Failure,
        Aborted
    }

    public enum InteractionResultCode
    {
        Accepted,
        InvalidTarget,
        TooFar,
        WrongPhase,
        InvalidAction,
        PreconditionsNotMet,
        Cooldown,
        AlreadyResolved,
        NotAuthorized
    }

    public enum IncidentKind
    {
        Overheat,
        PowerFault,
        BrakeLeak
    }

    public enum MissionEventType
    {
        MissionStarted,
        IncidentBegan,
        SymptomDiscovered,
        ActionAttempted,
        ActionAccepted,
        ActionRejected,
        SubsystemStateChanged,
        IncidentStabilized,
        IncidentEscalated,
        IncidentResolved,
        IncidentFailed,
        EmergencyBrakeUsed,
        MissionEnded
    }
}
