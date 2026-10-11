using PanicMechanics.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PanicMechanics.Runtime
{
    public sealed class SessionMenuController : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private string joinCode = "LOCAL";
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private TMP_Text statusLabel;

        private IGameSessionService _session;

        public string JoinCode
        {
            get { return joinCode; }
        }

        private void Start()
        {
            _session = GameBootstrap.Instance != null ? GameBootstrap.Instance.Session : null;

            if (_session != null)
            {
                _session.StatusChanged += OnSessionStatusChanged;
            }

            EnsureStatusLabel();

            if (hostButton != null)
            {
                hostButton.onClick.AddListener(OnHostClicked);
            }

            if (joinButton != null)
            {
                joinButton.onClick.AddListener(OnJoinClicked);
            }
        }

        private void EnsureStatusLabel()
        {
            if (statusLabel != null)
            {
                return;
            }

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            var labelObject = new GameObject("SessionStatusLabel", typeof(RectTransform));
            labelObject.transform.SetParent(canvas.transform, false);

            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(20f, 20f);
            rect.sizeDelta = new Vector2(720f, 80f);

            statusLabel = labelObject.AddComponent<TextMeshProUGUI>();
            statusLabel.fontSize = 24f;
            statusLabel.color = Color.white;
            statusLabel.alignment = TextAlignmentOptions.BottomLeft;
        }

        private void OnDestroy()
        {
            if (_session != null)
            {
                _session.StatusChanged -= OnSessionStatusChanged;
            }
        }

        public void SetJoinCode(string code)
        {
            joinCode = code ?? string.Empty;
        }

        private async void OnHostClicked()
        {
            if (_session == null)
            {
                return;
            }

            SetStatus("Hosting...");
            var result = await _session.CreatePrivateSessionAsync(maxPlayers);
            SetStatus(result.Success ? "Join code: " + result.JoinCode : "Host failed: " + result.Message);
        }

        private async void OnJoinClicked()
        {
            if (_session == null)
            {
                return;
            }

            SetStatus("Joining...");
            var result = await _session.JoinByCodeAsync(joinCode);
            SetStatus(result.Success ? "Joined: " + result.JoinCode : "Join failed: " + result.Message);
        }

        private void OnSessionStatusChanged(SessionStatus status)
        {
            SetStatus(status.ToString());
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null)
            {
                statusLabel.text = text;
            }

            Debug.Log("[SessionMenu] " + text);
        }
    }
}
