using System;
using System.Threading.Tasks;
using PanicMechanics.Core;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace PanicMechanics.Runtime
{
    public sealed class MultiplayerServicesSessionService : IGameSessionService
    {
        public const string DefaultSessionName = "PanicMechanics";

        public SessionStatus Status { get; private set; } = SessionStatus.Offline;

        public event Action<SessionStatus> StatusChanged;

        public string JoinCode { get; private set; } = string.Empty;

        public string LastMessage { get; private set; } = string.Empty;

        private ISession _session;
        private bool _servicesReady;
        private bool _busy;

        public async Task<SessionOperationResult> CreatePrivateSessionAsync(int maxPlayers)
        {
            if (Status == SessionStatus.Hosting || Status == SessionStatus.Connected)
            {
                return SessionOperationResult.Fail(SessionErrorText.AlreadyHosting,
                    "Already in a session. Leave before hosting again.");
            }

            if (_busy)
            {
                return SessionOperationResult.Fail(SessionErrorText.Unavailable, "A session operation is already in progress.");
            }

            if (maxPlayers <= 0)
            {
                return SessionOperationResult.Fail(SessionErrorText.NetworkSetup, "Max players must be greater than zero.");
            }

            _busy = true;
            SetStatus(SessionStatus.Initializing);
            try
            {
                if (!await EnsureServicesReadyAsync())
                {
                    return SessionOperationResult.Fail(SessionErrorText.Unavailable,
                        "Multiplayer services are unavailable on this platform.");
                }

                SetStatus(SessionStatus.Creating);

                var options = new SessionOptions
                {
                    Name = DefaultSessionName,
                    MaxPlayers = maxPlayers,
                    IsPrivate = true
                }.WithRelayNetwork();

                var host = await MultiplayerService.Instance.CreateSessionAsync(options);
                Track(host);
                JoinCode = host.Code ?? string.Empty;
                SetStatus(SessionStatus.Hosting);
                LastMessage = "Hosting. Share code " + JoinCode;
                return SessionOperationResult.Ok(JoinCode, LastMessage);
            }
            catch (SessionException e)
            {
                await CleanupOrphanedSessionsAsync();
                JoinCode = string.Empty;
                LastMessage = SessionErrorText.MessageFor(e.Error.ToString());
                SetStatus(SessionStatus.Failed);
                return SessionOperationResult.Fail(SessionErrorText.CodeFor(e.Error.ToString()), LastMessage);
            }
            catch (Exception e)
            {
                await CleanupOrphanedSessionsAsync();
                JoinCode = string.Empty;
                LastMessage = "Session creation failed: " + e.Message;
                SetStatus(SessionStatus.Failed);
                return SessionOperationResult.Fail(SessionErrorText.Unknown, LastMessage);
            }
            finally
            {
                _busy = false;
            }
        }

        public async Task<SessionOperationResult> JoinByCodeAsync(string joinCode)
        {
            if (Status == SessionStatus.Hosting || Status == SessionStatus.Connected)
            {
                return SessionOperationResult.Fail(SessionErrorText.AlreadyHosting,
                    "Already in a session. Leave before joining another.");
            }

            if (_busy)
            {
                return SessionOperationResult.Fail(SessionErrorText.Unavailable, "A session operation is already in progress.");
            }

            if (SessionErrorText.IsEmptyCode(joinCode))
            {
                return SessionOperationResult.Fail(SessionErrorText.InvalidCode, "Enter a join code.");
            }

            string code = SessionErrorText.NormalizeCode(joinCode);
            _busy = true;
            SetStatus(SessionStatus.Initializing);
            try
            {
                if (!await EnsureServicesReadyAsync())
                {
                    return SessionOperationResult.Fail(SessionErrorText.Unavailable,
                        "Multiplayer services are unavailable on this platform.");
                }

                SetStatus(SessionStatus.Joining);

                var session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
                Track(session);
                JoinCode = code;
                SetStatus(SessionStatus.Connected);
                LastMessage = "Joined session " + code;
                return SessionOperationResult.Ok(code, LastMessage);
            }
            catch (SessionException e)
            {
                await CleanupOrphanedSessionsAsync();
                JoinCode = string.Empty;
                LastMessage = SessionErrorText.MessageFor(e.Error.ToString());
                SetStatus(SessionStatus.Failed);
                return SessionOperationResult.Fail(SessionErrorText.CodeFor(e.Error.ToString()), LastMessage);
            }
            catch (Exception e)
            {
                await CleanupOrphanedSessionsAsync();
                JoinCode = string.Empty;
                LastMessage = "Join failed: " + e.Message;
                SetStatus(SessionStatus.Failed);
                return SessionOperationResult.Fail(SessionErrorText.Unknown, LastMessage);
            }
            finally
            {
                _busy = false;
            }
        }

        public async Task LeaveSessionAsync()
        {
            var session = _session;
            _session = null;
            JoinCode = string.Empty;

            if (session != null)
            {
                try
                {
                    await session.LeaveAsync();
                }
                catch (Exception)
                {
                }
            }

            ShutdownNetworkManager();
            LastMessage = "Left session.";
            SetStatus(SessionStatus.Offline);
        }

        public string GetLocalPlayerId()
        {
            var auth = AuthenticationService.Instance;
            if (auth != null && auth.IsSignedIn && !string.IsNullOrEmpty(auth.PlayerId))
            {
                return auth.PlayerId;
            }

            return "0";
        }

        private async Task<bool> EnsureServicesReadyAsync()
        {
            if (_servicesReady)
            {
                return MultiplayerService.Instance != null;
            }

            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            var auth = AuthenticationService.Instance;
            if (auth == null || MultiplayerService.Instance == null)
            {
                return false;
            }

            if (!auth.IsSignedIn)
            {
                await auth.SignInAnonymouslyAsync();
            }

            _servicesReady = true;
            return true;
        }

        private void Track(ISession session)
        {
            _session = session;
            session.Deleted += OnSessionEnded;
            session.RemovedFromSession += OnRemovedFromSession;
        }

        private void Untrack(ISession session)
        {
            if (session == null)
            {
                return;
            }

            session.Deleted -= OnSessionEnded;
            session.RemovedFromSession -= OnRemovedFromSession;
        }

        private void OnSessionEnded()
        {
            HandleSessionLost("The host ended the session.");
        }

        private void OnRemovedFromSession()
        {
            HandleSessionLost("You were removed from the session.");
        }

        private void HandleSessionLost(string message)
        {
            var session = _session;
            _session = null;
            Untrack(session);
            JoinCode = string.Empty;
            ShutdownNetworkManager();
            LastMessage = message;
            SetStatus(SessionStatus.Disconnected);
        }

        private async Task CleanupOrphanedSessionsAsync()
        {
            Untrack(_session);
            _session = null;

            var service = MultiplayerService.Instance;
            if (service != null)
            {
                var active = new System.Collections.Generic.List<ISession>(service.Sessions.Values);
                for (int i = 0; i < active.Count; i++)
                {
                    try
                    {
                        await active[i].LeaveAsync();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            ShutdownNetworkManager();
        }

        private static void ShutdownNetworkManager()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null && manager.IsListening)
            {
                manager.Shutdown();
            }
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
