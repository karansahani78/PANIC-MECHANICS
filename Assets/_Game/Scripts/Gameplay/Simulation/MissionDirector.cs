using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public sealed class MissionDirector
    {
        public GamePhase Phase { get; private set; }
        public float Elapsed { get; private set; }
        public string MissionId { get; private set; }
        public int Players { get; private set; }
        public float TargetDuration { get; private set; }
        public TrainSystemModel Model { get; private set; }
        public MissionEventLog Log { get; private set; }
        public MissionResult Result { get; private set; }

        private readonly List<IncidentController> _incidents = new List<IncidentController>();

        public event Action<GamePhase> PhaseChanged;
        public event Action<MissionResult> MissionEnded;
        public event Action<IncidentController> IncidentChanged;

        public MissionDirector(string missionId, int players, float targetDuration)
        {
            MissionId = missionId;
            Players = players;
            TargetDuration = targetDuration;
            Model = new TrainSystemModel();
            Log = new MissionEventLog();
            Phase = GamePhase.Boot;
        }

        public IReadOnlyList<IncidentController> Incidents
        {
            get { return _incidents; }
        }

        public MissionDirector AddIncident(IncidentController incident)
        {
            if (incident == null)
            {
                throw new ArgumentNullException("incident");
            }

            incident.Changed += OnIncidentChanged;
            _incidents.Add(incident);
            return this;
        }

        public void EnterLobby()
        {
            Elapsed = 0f;
            Result = null;
            Model.Reset();
            for (int i = 0; i < _incidents.Count; i++)
            {
                _incidents[i].Reset();
            }

            Log.Clear();
            SetPhase(GamePhase.Lobby);
        }

        public void StartMission()
        {
            Elapsed = 0f;
            Result = null;
            SetPhase(GamePhase.Countdown);
            SetPhase(GamePhase.Running);
            Log.Append(0f, 0L, MissionEventType.MissionStarted, 0UL, MissionId, TrainSubsystem.Communication);
        }

        public void Tick(float dt)
        {
            if (Phase != GamePhase.Running)
            {
                return;
            }

            if (dt <= 0f)
            {
                return;
            }

            Elapsed += dt;
            Model.Tick(dt);

            for (int i = 0; i < _incidents.Count; i++)
            {
                _incidents[i].Tick(dt, Elapsed);
            }

            EvaluateOutcome();
        }

        private void EvaluateOutcome()
        {
            for (int i = 0; i < _incidents.Count; i++)
            {
                if (_incidents[i].Phase == IncidentPhase.Failed)
                {
                    EndMission(MissionOutcome.Failure);
                    return;
                }
            }

            if (Model.Get(TrainSubsystem.Brakes).Pressure <= 0f)
            {
                EndMission(MissionOutcome.Failure);
                return;
            }

            if (Elapsed >= TargetDuration)
            {
                EndMission(MissionOutcome.Success);
            }
        }

        public void EndMission(MissionOutcome outcome)
        {
            if (Phase != GamePhase.Running)
            {
                return;
            }

            Log.Append(Elapsed, 0L, MissionEventType.MissionEnded, 0UL, MissionId,
                TrainSubsystem.Communication, "outcome", outcome.ToString());

            Result = new MissionResult
            {
                MissionId = MissionId,
                Outcome = outcome,
                Duration = Elapsed,
                Players = Players
            };

            for (int i = 0; i < _incidents.Count; i++)
            {
                var incident = _incidents[i];
                if (incident.Phase == IncidentPhase.Resolved)
                {
                    Result.ResolvedIncidents.Add(incident.Id);
                }
                else if (incident.Phase == IncidentPhase.Failed)
                {
                    Result.FailedIncidents.Add(incident.Id);
                }
            }

            Result.Score = MissionScorer.Score(outcome, _incidents, Elapsed, TargetDuration);

            SetPhase(GamePhase.Debrief);

            var report = IncidentReportBuilder.Build(Result, Log);
            Result.Summary.AddRange(report.Lines);

            var handler = MissionEnded;
            if (handler != null)
            {
                handler(Result);
            }
        }

        public void Abort()
        {
            EndMission(MissionOutcome.Aborted);
        }

        public void Reset()
        {
            Elapsed = 0f;
            Result = null;
            Model.Reset();
            for (int i = 0; i < _incidents.Count; i++)
            {
                _incidents[i].Reset();
            }

            Log.Clear();
            SetPhase(GamePhase.Boot);
        }

        private void OnIncidentChanged(IncidentController incident)
        {
            var handler = IncidentChanged;
            if (handler != null)
            {
                handler(incident);
            }
        }

        private void SetPhase(GamePhase phase)
        {
            if (Phase == phase)
            {
                return;
            }

            Phase = phase;
            var handler = PhaseChanged;
            if (handler != null)
            {
                handler(phase);
            }
        }
    }
}
