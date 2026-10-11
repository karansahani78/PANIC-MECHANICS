using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Interaction
{
    public sealed class InteractionService : IInteractionService
    {
        private readonly Dictionary<string, IInteractionTarget> _targets =
            new Dictionary<string, IInteractionTarget>(StringComparer.Ordinal);

        private readonly Dictionary<string, float> _readyAt =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private readonly Func<GamePhase> _phaseProvider;

        public float MaxDistance = 3f;
        public float DefaultCooldownSeconds = 0f;
        public float ExpertCooldownMultiplier = 1f;
        public GamePhase RequiredPhase = GamePhase.Running;

        public Func<PlayerRole, string, bool> IsExpertFor;

        public InteractionService(Func<GamePhase> phaseProvider)
        {
            if (phaseProvider == null)
            {
                throw new ArgumentNullException(nameof(phaseProvider));
            }

            _phaseProvider = phaseProvider;
        }

        public event Action<InteractionRequest, InteractionResult> Resolved;

        public int TargetCount
        {
            get { return _targets.Count; }
        }

        public void RegisterTarget(IInteractionTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            _targets[target.TargetId] = target;
        }

        public bool UnregisterTarget(string targetId)
        {
            return targetId != null && _targets.Remove(targetId);
        }

        public InteractionResult Submit(InteractionRequest request, PlayerInteractionContext context)
        {
            var result = Evaluate(request, context);
            var handler = Resolved;
            if (handler != null)
            {
                handler(request, result);
            }

            return result;
        }

        private InteractionResult Evaluate(InteractionRequest request, PlayerInteractionContext context)
        {
            IInteractionTarget target;
            if (request.TargetId == null || !_targets.TryGetValue(request.TargetId, out target))
            {
                return Reject(InteractionResultCode.InvalidTarget, "interaction.invalid_target");
            }

            if (_phaseProvider() != RequiredPhase)
            {
                return Reject(InteractionResultCode.WrongPhase, "interaction.wrong_phase");
            }

            if (context.Distance > MaxDistance)
            {
                return Reject(InteractionResultCode.TooFar, "interaction.too_far");
            }

            string key = CooldownKey(context.PlayerId, request.TargetId, request.ActionId);
            float readyAt;
            if (_readyAt.TryGetValue(key, out readyAt) && context.Time < readyAt)
            {
                return Reject(InteractionResultCode.Cooldown, "interaction.cooldown");
            }

            InteractionResultCode reason;
            if (!target.CanInteract(context, request.ActionId, out reason))
            {
                return Reject(reason, FeedbackKeyFor(reason));
            }

            target.HandleAcceptedInteraction(context, request.ActionId);

            var source = target as IInteractionResultSource;
            var outcome = source != null
                ? source.LastResult
                : InteractionResult.Ok("interaction.accepted", 0);

            if (outcome.Accepted)
            {
                float cooldown = CooldownFor(context, request.TargetId);
                if (cooldown > 0f)
                {
                    _readyAt[key] = context.Time + cooldown;
                }
            }

            return outcome;
        }

        private float CooldownFor(PlayerInteractionContext context, string targetId)
        {
            float cooldown = DefaultCooldownSeconds;
            if (cooldown > 0f && IsExpertFor != null && IsExpertFor(context.Role, targetId))
            {
                cooldown *= ExpertCooldownMultiplier;
            }

            return cooldown;
        }

        public void ClearCooldowns()
        {
            _readyAt.Clear();
        }

        private static InteractionResult Reject(InteractionResultCode code, string feedbackKey)
        {
            return InteractionResult.Rejected(code, feedbackKey, 0);
        }

        private static string CooldownKey(ulong playerId, string targetId, string actionId)
        {
            return playerId.ToString() + "|" + targetId + "|" + actionId;
        }

        private static string FeedbackKeyFor(InteractionResultCode code)
        {
            switch (code)
            {
                case InteractionResultCode.InvalidTarget:
                    return "interaction.invalid_target";
                case InteractionResultCode.TooFar:
                    return "interaction.too_far";
                case InteractionResultCode.WrongPhase:
                    return "interaction.wrong_phase";
                case InteractionResultCode.InvalidAction:
                    return "interaction.invalid_action";
                case InteractionResultCode.PreconditionsNotMet:
                    return "interaction.preconditions";
                case InteractionResultCode.Cooldown:
                    return "interaction.cooldown";
                case InteractionResultCode.AlreadyResolved:
                    return "interaction.already_resolved";
                case InteractionResultCode.NotAuthorized:
                    return "interaction.not_authorized";
                default:
                    return "interaction.rejected";
            }
        }
    }
}
