using UnityEngine;

namespace PanicMechanics.Runtime
{
    public sealed class InteractableStation : MonoBehaviour
    {
        [SerializeField] private string targetId = "overheat";
        [SerializeField] private string actionId = "open_cooling_valve";
        [SerializeField] private string prompt = "Open coolant valve [E]";

        public string TargetId
        {
            get { return targetId; }
        }

        public string ActionId
        {
            get { return actionId; }
        }

        public string Prompt
        {
            get { return prompt; }
        }
    }
}
