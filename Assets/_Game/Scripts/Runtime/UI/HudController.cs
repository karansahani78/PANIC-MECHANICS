using System.Collections.Generic;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Diagnostics;
using PanicMechanics.Gameplay.Interaction;
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
        [SerializeField] private TMP_Text roleLabel;
        [SerializeField] private float feedbackSeconds = 3f;

        private readonly RoleInformationService _roles = RoleInformationService.CreateDefault();
        private readonly List<TrainSubsystemState> _panelStates = new List<TrainSubsystemState>();

        private PlayerAvatar _localAvatar;
        private PlayerRole _role = PlayerRole.Dispatcher;
        private float _feedbackUntil;

        private void Start()
        {
            EnsureRoleLabel();
        }

        private void EnsureRoleLabel()
        {
            if (roleLabel != null)
            {
                return;
            }

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            var labelObject = new GameObject("RoleLabel", typeof(RectTransform));
            labelObject.transform.SetParent(canvas.transform, false);

            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -160f);
            rect.sizeDelta = new Vector2(320f, 40f);

            roleLabel = labelObject.AddComponent<TextMeshProUGUI>();
            roleLabel.fontSize = 28f;
            roleLabel.color = Color.white;
            roleLabel.alignment = TextAlignmentOptions.TopRight;
        }

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

            ResolveLocalRole();

            if (roleLabel != null)
            {
                roleLabel.text = "Role: " + _roles.Get(_role).DisplayName;
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

        private void ResolveLocalRole()
        {
            if (_localAvatar == null)
            {
                var avatars = FindObjectsByType<PlayerAvatar>(FindObjectsSortMode.None);
                for (int i = 0; i < avatars.Length; i++)
                {
                    if (avatars[i].IsOwner)
                    {
                        _localAvatar = avatars[i];
                        break;
                    }
                }
            }

            if (_localAvatar != null)
            {
                _role = (PlayerRole)_localAvatar.RoleValue;
            }
        }

        private string BuildSubsystemText(NetworkGameManager manager)
        {
            var list = manager.Subsystems;
            _panelStates.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                var state = list[i];
                _panelStates.Add(new TrainSubsystemState(
                    (TrainSubsystem)state.Subsystem,
                    state.Health,
                    state.Temperature,
                    state.Pressure,
                    state.Load,
                    state.Enabled,
                    state.Powered,
                    0));
            }

            return DiagnosticPanel.Format(_roles.Get(_role), _panelStates);
        }
    }
}
