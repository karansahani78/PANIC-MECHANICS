using System;
using System.Threading.Tasks;

namespace PanicMechanics.Core
{
    public enum SessionStatus
    {
        Offline,
        Initializing,
        Creating,
        Joining,
        Hosting,
        Connected,
        Disconnected,
        Failed
    }

    public readonly struct SessionOperationResult
    {
        public readonly bool Success;
        public readonly string Code;
        public readonly string Message;
        public readonly string JoinCode;

        public SessionOperationResult(bool success, string code, string message, string joinCode)
        {
            Success = success;
            Code = code;
            Message = message;
            JoinCode = joinCode;
        }

        public static SessionOperationResult Ok(string joinCode, string message)
        {
            return new SessionOperationResult(true, "ok", message, joinCode);
        }

        public static SessionOperationResult Fail(string code, string message)
        {
            return new SessionOperationResult(false, code, message, string.Empty);
        }
    }

    public interface IGameSessionService
    {
        Task<SessionOperationResult> CreatePrivateSessionAsync(int maxPlayers);
        Task<SessionOperationResult> JoinByCodeAsync(string joinCode);
        Task LeaveSessionAsync();
        string GetLocalPlayerId();
        SessionStatus Status { get; }
        event Action<SessionStatus> StatusChanged;
    }

    public interface IInteractionTarget
    {
        string TargetId { get; }
        bool CanInteract(PlayerInteractionContext context, string actionId, out InteractionResultCode rejectionReason);
        void HandleAcceptedInteraction(PlayerInteractionContext context, string actionId);
    }

    public interface IInteractionResultSource
    {
        InteractionResult LastResult { get; }
    }

    public interface IMissionService
    {
        GamePhase CurrentPhase { get; }
        string CurrentMissionId { get; }
        event Action<GamePhase> PhaseChanged;
        event Action<MissionResult> MissionEnded;
    }

    public interface ITrainSystemService
    {
        TrainSubsystemState GetSubsystemState(TrainSubsystem subsystem);
        event Action<TrainSubsystemState> SubsystemStateChanged;
        InteractionResult ApplyRepairAction(InteractionRequest request, PlayerInteractionContext context);
    }

    public interface IInteractionService
    {
        InteractionResult Submit(InteractionRequest request, PlayerInteractionContext context);
    }
}
