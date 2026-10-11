using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public sealed class MissionEventLog
    {
        private readonly List<MissionEvent> _events = new List<MissionEvent>();
        private long _nextId;

        public IReadOnlyList<MissionEvent> Events
        {
            get { return _events; }
        }

        public int Count
        {
            get { return _events.Count; }
        }

        public MissionEvent Append(
            float time,
            long tick,
            MissionEventType type,
            ulong actorId = 0UL,
            string incidentId = null,
            TrainSubsystem subsystem = TrainSubsystem.Communication,
            string key = null,
            string value = null)
        {
            var evt = new MissionEvent(
                _nextId++, time, tick, type, actorId,
                incidentId ?? string.Empty, subsystem,
                key ?? string.Empty, value ?? string.Empty);
            _events.Add(evt);
            return evt;
        }

        public int CountOf(MissionEventType type)
        {
            int count = 0;
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Type == type)
                {
                    count++;
                }
            }

            return count;
        }

        public int CountRejectedActions(string actionId)
        {
            int count = 0;
            for (int i = 0; i < _events.Count; i++)
            {
                var e = _events[i];
                if (e.Type == MissionEventType.ActionRejected &&
                    (actionId == null || e.Key == actionId))
                {
                    count++;
                }
            }

            return count;
        }

        public void Clear()
        {
            _events.Clear();
            _nextId = 0;
        }
    }
}
