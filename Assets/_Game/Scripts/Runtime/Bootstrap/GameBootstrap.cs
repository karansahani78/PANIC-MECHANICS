using System;
using System.Collections.Generic;
using PanicMechanics.Core;
using PanicMechanics.Data;
using PanicMechanics.Gameplay.Interaction;
using PanicMechanics.Gameplay.Missions;
using PanicMechanics.Gameplay.Simulation;
using UnityEngine;

namespace PanicMechanics.Runtime
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        public const string SessionBackendEnvVar = "PM_SESSION_BACKEND";

        public enum SessionBackend
        {
            Local,
            MultiplayerServices
        }

        [SerializeField] private MissionDefinitionSO mission;
        [SerializeField] private int fallbackPlayerCount = 2;
        [SerializeField] private SessionBackend sessionBackend = SessionBackend.Local;

        public MissionDirector Director { get; private set; }
        public InteractionService Interactions { get; private set; }
        public IGameSessionService Session { get; private set; }

        private readonly Dictionary<string, IncidentInteractionTarget> _targets =
            new Dictionary<string, IncidentInteractionTarget>(StringComparer.Ordinal);

        public event Action<GamePhase> PhaseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Build();
        }

        private void Build()
        {
            Session = CreateSessionService();

            int players = fallbackPlayerCount;
            MissionConfig config = mission != null
                ? mission.ToConfig(players)
                : new MissionConfig { Players = players };

            if (config.Incidents.Count == 0)
            {
                config.Incidents.Add(IncidentConfig.Overheat());
            }

            Director = MissionBuilder.Build(config);
            Interactions = new InteractionService(() => Director.Phase)
            {
                MaxDistance = IncidentController.MaxInteractionDistance,
                DefaultCooldownSeconds = 0.8f,
                ExpertCooldownMultiplier = 0.25f,
                IsExpertFor = RoleExpertise.IsExpert
            };
            Director.PhaseChanged += OnPhaseChanged;

            for (int i = 0; i < Director.Incidents.Count; i++)
            {
                Register(Director.Incidents[i]);
            }
        }

        private IGameSessionService CreateSessionService()
        {
            bool useMultiplayerServices = sessionBackend == SessionBackend.MultiplayerServices;

            string overrideValue = Environment.GetEnvironmentVariable(SessionBackendEnvVar);
            if (!string.IsNullOrEmpty(overrideValue))
            {
                useMultiplayerServices = string.Equals(overrideValue, "ugs", StringComparison.OrdinalIgnoreCase);
            }

            if (useMultiplayerServices)
            {
                return new MultiplayerServicesSessionService();
            }

            return new LocalGameSessionService();
        }

        private void Register(IncidentController incident)
        {
            var target = new IncidentInteractionTarget(incident);
            _targets[target.TargetId] = target;
            Interactions.RegisterTarget(target);
        }

        public bool TryGetTargetResult(string targetId, out InteractionResult result)
        {
            IncidentInteractionTarget target;
            if (_targets.TryGetValue(targetId, out target))
            {
                result = target.LastResult;
                return true;
            }

            result = default;
            return false;
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            var handler = PhaseChanged;
            if (handler != null)
            {
                handler(phase);
            }
        }
    }
}
