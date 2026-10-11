using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Gameplay.Interaction
{
    public sealed class IncidentInteractionTarget : IInteractionTarget, IInteractionResultSource
    {
        private readonly IncidentController _incident;

        public IncidentInteractionTarget(IncidentController incident)
        {
            _incident = incident;
        }

        public string TargetId
        {
            get { return _incident.Id; }
        }

        public InteractionResult LastResult { get; private set; }

        public bool CanInteract(
            PlayerInteractionContext context, string actionId, out InteractionResultCode rejectionReason)
        {
            return _incident.CanAttempt(actionId, context, out rejectionReason);
        }

        public void HandleAcceptedInteraction(PlayerInteractionContext context, string actionId)
        {
            LastResult = _incident.TryRepair(
                new InteractionRequest(_incident.Id, actionId, context.PlayerId, 0u), context);
        }
    }
}
