using System.Collections.Generic;
using PanicMechanics.Gameplay.Missions;
using UnityEngine;

namespace PanicMechanics.Data
{
    [CreateAssetMenu(menuName = "Panic Mechanics/Mission Definition", fileName = "MissionDefinition")]
    public sealed class MissionDefinitionSO : ScriptableObject
    {
        public string Id = "mission.derail";
        public string Title = "Runaway Freight";
        [TextArea(2, 4)]
        public string Description = "Keep the train intact until the next station.";
        public float TargetDurationSeconds = 180f;
        public int MinPlayers = 2;
        public int MaxPlayers = 4;

        [Header("Scheduled incidents")]
        public List<IncidentDefinitionSO> Incidents = new List<IncidentDefinitionSO>();

        [Header("Difficulty modifiers")]
        [Range(0.5f, 2f)]
        public float DifficultyScale = 1f;

        public MissionConfig ToConfig(int players)
        {
            var config = new MissionConfig
            {
                Id = Id,
                TargetDurationSeconds = TargetDurationSeconds,
                Players = players,
                DifficultyScale = DifficultyScale,
                Incidents = new List<IncidentConfig>()
            };

            for (int i = 0; i < Incidents.Count; i++)
            {
                if (Incidents[i] != null)
                {
                    config.Incidents.Add(Incidents[i].ToConfig());
                }
            }

            return config;
        }
    }
}
