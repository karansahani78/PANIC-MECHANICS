using System.Collections.Generic;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Lobby;
using Unity.Netcode;
using UnityEngine;

namespace PanicMechanics.Runtime
{
    public sealed class NetworkGameManager : NetworkBehaviour
    {
        public const int SubsystemCount = 6;

        public static NetworkGameManager Instance { get; private set; }

        private readonly NetworkVariable<GamePhase> _phase = new NetworkVariable<GamePhase>(
            GamePhase.Boot,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _elapsed = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkList<SubsystemNetState> _subsystems =
            new NetworkList<SubsystemNetState>();

        public NetworkVariable<GamePhase> Phase
        {
            get { return _phase; }
        }

        public NetworkVariable<float> Elapsed
        {
            get { return _elapsed; }
        }

        public NetworkList<SubsystemNetState> Subsystems
        {
            get { return _subsystems; }
        }

        public event System.Action<InteractionResultCode, string> InteractionResolved;

        private readonly Dictionary<ulong, PlayerRole> _authoritativeRoles =
            new Dictionary<ulong, PlayerRole>();

        private void Awake()
        {
            Instance = this;
        }

        public PlayerRole GetAuthoritativeRole(ulong clientId)
        {
            PlayerRole role;
            if (_authoritativeRoles.TryGetValue(clientId, out role))
            {
                return role;
            }

            return PlayerRole.Dispatcher;
        }

        public void EnsureAuthoritativeRoles()
        {
            if (IsServer)
            {
                ReassignRoles();
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }

            ReassignRoles();

            for (int i = 0; i < SubsystemCount; i++)
            {
                _subsystems.Add(default);
            }

            var boot = GameBootstrap.Instance;
            if (boot == null)
            {
                return;
            }

            boot.Director.EnterLobby();
            boot.Director.StartMission();
            _phase.Value = boot.Director.Phase;
            RefreshSubsystems();
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                ReassignRoles();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

            _authoritativeRoles.Remove(clientId);
            ReassignRoles();
        }

        private void ReassignRoles()
        {
            if (!IsServer || NetworkManager == null)
            {
                return;
            }

            var ids = new List<ulong>();
            foreach (var pair in NetworkManager.ConnectedClients)
            {
                ids.Add(pair.Key);
            }

            ids.Sort();

            var assignments = RoleAssignmentService.Assign(ids);
            _authoritativeRoles.Clear();
            foreach (var pair in assignments)
            {
                _authoritativeRoles[pair.Key] = pair.Value;
            }

            foreach (var id in ids)
            {
                NetworkClient client;
                if (NetworkManager.ConnectedClients.TryGetValue(id, out client) &&
                    client.PlayerObject != null)
                {
                    var avatar = client.PlayerObject.GetComponent<PlayerAvatar>();
                    if (avatar != null)
                    {
                        avatar.AuthorizeRole(GetAuthoritativeRole(id));
                    }
                }
            }
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            var boot = GameBootstrap.Instance;
            if (boot == null)
            {
                return;
            }

            var director = boot.Director;
            if (director.Phase == GamePhase.Running)
            {
                director.Tick(Time.deltaTime);
            }

            _phase.Value = director.Phase;
            _elapsed.Value = director.Elapsed;
            RefreshSubsystems();
        }

        private void RefreshSubsystems()
        {
            var boot = GameBootstrap.Instance;
            if (boot == null)
            {
                return;
            }

            var model = boot.Director.Model;
            for (int i = 0; i < SubsystemCount; i++)
            {
                if (i >= _subsystems.Count)
                {
                    break;
                }

                _subsystems[i] = new SubsystemNetState(model.Get((TrainSubsystem)i));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitInteractionServerRpc(
            string targetId, string actionId, float distance, ServerRpcParams rpcParams = default)
        {
            var boot = GameBootstrap.Instance;
            if (boot == null)
            {
                return;
            }

            ulong clientId = rpcParams.Receive.SenderClientId;
            var context = new PlayerInteractionContext(
                clientId, GetAuthoritativeRole(clientId), distance, boot.Director.Elapsed);
            var request = new InteractionRequest(targetId, actionId, clientId, 0u);

            var result = boot.Interactions.Submit(request, context);
            SendResultClientRpc(
                (int)result.Code, result.FeedbackKey,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
                });
        }

        [ClientRpc]
        private void SendResultClientRpc(int code, string feedbackKey, ClientRpcParams rpcParams = default)
        {
            var handler = InteractionResolved;
            if (handler != null)
            {
                handler((InteractionResultCode)code, feedbackKey);
            }
        }
    }
}
