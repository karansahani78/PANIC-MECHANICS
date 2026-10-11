using System;
using System.Text;
using System.Threading.Tasks;
using PanicMechanics.Core;
using Unity.Netcode;
using UnityEngine;

namespace PanicMechanics.Runtime
{
    public sealed class LocalGameSessionService : IGameSessionService
    {
        public const string LoopbackCode = "LOCAL";

        public SessionStatus Status { get; private set; } = SessionStatus.Offline;

        public event Action<SessionStatus> StatusChanged;

        public Task<SessionOperationResult> CreatePrivateSessionAsync(int maxPlayers)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                return Task.FromResult(SessionOperationResult.Fail("no_manager", "No NetworkManager in scene"));
            }

            SetStatus(SessionStatus.Creating);
            bool started = nm.StartHost();
            SetStatus(started ? SessionStatus.Hosting : SessionStatus.Failed);

            return Task.FromResult(started
                ? SessionOperationResult.Ok(LoopbackCode, "Hosting locally on loopback")
                : SessionOperationResult.Fail("host_failed", "Could not start host"));
        }

        public Task<SessionOperationResult> JoinByCodeAsync(string joinCode)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                return Task.FromResult(SessionOperationResult.Fail("no_manager", "No NetworkManager in scene"));
            }

            SetStatus(SessionStatus.Joining);
            nm.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(joinCode ?? LoopbackCode);
            bool started = nm.StartClient();
            SetStatus(started ? SessionStatus.Connected : SessionStatus.Failed);

            return Task.FromResult(started
                ? SessionOperationResult.Ok(joinCode, "Connecting to loopback host")
                : SessionOperationResult.Fail("join_failed", "Could not start client"));
        }

        public Task LeaveSessionAsync()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null)
            {
                nm.Shutdown();
            }

            SetStatus(SessionStatus.Offline);
            return Task.CompletedTask;
        }

        public string GetLocalPlayerId()
        {
            var nm = NetworkManager.Singleton;
            return nm != null ? nm.LocalClientId.ToString() : "0";
        }

        private void SetStatus(SessionStatus status)
        {
            Status = status;
            var handler = StatusChanged;
            if (handler != null)
            {
                handler(status);
            }
        }
    }
}
