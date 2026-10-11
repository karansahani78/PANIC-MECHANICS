using PanicMechanics.Core;
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

        private IGameSessionService _session;

        private void Start()
        {
            _session = GameBootstrap.Instance != null ? GameBootstrap.Instance.Session : null;

            if (hostButton != null)
            {
                hostButton.onClick.AddListener(OnHostClicked);
            }

            if (joinButton != null)
            {
                joinButton.onClick.AddListener(OnJoinClicked);
            }
        }

        private async void OnHostClicked()
        {
            if (_session != null)
            {
                await _session.CreatePrivateSessionAsync(maxPlayers);
            }
        }

        private async void OnJoinClicked()
        {
            if (_session != null)
            {
                await _session.JoinByCodeAsync(joinCode);
            }
        }
    }
}
