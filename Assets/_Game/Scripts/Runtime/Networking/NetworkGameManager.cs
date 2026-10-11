using PanicMechanics.Core;
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

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

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
                clientId, PlayerRole.Dispatcher, distance, boot.Director.Elapsed);
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
