using System.Text;
using PanicMechanics.Core;
using TMPro;
using UnityEngine;

namespace PanicMechanics.Runtime
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text phaseLabel;
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private TMP_Text subsystemLabel;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private float feedbackSeconds = 3f;

        private float _feedbackUntil;

        private void OnEnable()
        {
            var manager = NetworkGameManager.Instance;
            if (manager != null)
            {
                manager.InteractionResolved += OnInteractionResolved;
            }
        }

        private void OnDisable()
        {
            var manager = NetworkGameManager.Instance;
            if (manager != null)
            {
                manager.InteractionResolved -= OnInteractionResolved;
            }
        }

        private void OnInteractionResolved(InteractionResultCode code, string feedbackKey)
        {
            if (feedbackLabel != null)
            {
                feedbackLabel.text = feedbackKey + "  (" + code + ")";
            }

            _feedbackUntil = Time.time + feedbackSeconds;
        }

        private void Update()
        {
            var manager = NetworkGameManager.Instance;
            if (manager == null)
            {
                return;
            }

            if (phaseLabel != null)
            {
                phaseLabel.text = manager.Phase.Value.ToString();
            }

            if (timerLabel != null)
            {
                timerLabel.text = manager.Elapsed.Value.ToString("0.0") + " s";
            }

            if (subsystemLabel != null)
            {
                subsystemLabel.text = BuildSubsystemText(manager);
            }

            if (feedbackLabel != null && _feedbackUntil > 0f && Time.time > _feedbackUntil)
            {
                feedbackLabel.text = string.Empty;
                _feedbackUntil = 0f;
            }
        }

        private static string BuildSubsystemText(NetworkGameManager manager)
        {
            var list = manager.Subsystems;
            var builder = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                var state = list[i];
                builder.Append((TrainSubsystem)state.Subsystem)
                    .Append("  h=").Append(state.Health.ToString("0.00"))
                    .Append("  t=").Append(state.Temperature.ToString("0.00"))
                    .Append("  p=").Append(state.Pressure.ToString("0.00"))
                    .Append('\n');
            }

            return builder.ToString();
        }
    }
}
