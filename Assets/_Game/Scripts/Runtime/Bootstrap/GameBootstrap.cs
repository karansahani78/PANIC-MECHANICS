using System;
using System.Collections.Generic;
using PanicMechanics.Core;
using PanicMechanics.Data;
using PanicMechanics.Gameplay.Interaction;
using PanicMechanics.Gameplay.Simulation;
using UnityEngine;

namespace PanicMechanics.Runtime
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [SerializeField] private MissionDefinitionSO mission;
        [SerializeField] private int fallbackPlayerCount = 2;

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
            Session = new LocalGameSessionService();

            int players = fallbackPlayerCount;
            float duration = mission != null ? mission.TargetDurationSeconds : 300f;
            string id = mission != null ? mission.Id : "mission.default";

            Director = new MissionDirector(id, players, duration);
            Interactions = new InteractionService(() => Director.Phase)
            {
                MaxDistance = IncidentController.MaxInteractionDistance
            };
            Director.PhaseChanged += OnPhaseChanged;

            if (mission != null && mission.Incidents != null && mission.Incidents.Count > 0)
            {
                for (int i = 0; i < mission.Incidents.Count; i++)
                {
                    var definition = mission.Incidents[i];
                    if (definition == null)
                    {
                        continue;
                    }

                    var incident = definition.Build(Director.Model, Director.Log);
                    if (incident != null)
                    {
                        Register(incident);
                    }
                }
            }
            else
            {
                Register(new OverheatIncident(Director.Model, Director.Log));
            }
        }

        private void Register(IncidentController incident)
        {
            Director.AddIncident(incident);
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
