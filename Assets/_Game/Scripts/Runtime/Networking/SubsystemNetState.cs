using System;
using PanicMechanics.Core;
using Unity.Netcode;

namespace PanicMechanics.Runtime
{
    public struct SubsystemNetState : INetworkSerializable, IEquatable<SubsystemNetState>
    {
        public byte Subsystem;
        public float Health;
        public float Temperature;
        public float Pressure;
        public float Load;
        public bool Enabled;
        public bool Powered;

        public SubsystemNetState(TrainSubsystemState state)
        {
            Subsystem = (byte)state.Subsystem;
            Health = state.Health;
            Temperature = state.Temperature;
            Pressure = state.Pressure;
            Load = state.Load;
            Enabled = state.Enabled;
            Powered = state.Powered;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Subsystem);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Temperature);
            serializer.SerializeValue(ref Pressure);
            serializer.SerializeValue(ref Load);
            serializer.SerializeValue(ref Enabled);
            serializer.SerializeValue(ref Powered);
        }

        public bool Equals(SubsystemNetState other)
        {
            return Subsystem == other.Subsystem
                && Health.Equals(other.Health)
                && Temperature.Equals(other.Temperature)
                && Pressure.Equals(other.Pressure)
                && Load.Equals(other.Load)
                && Enabled == other.Enabled
                && Powered == other.Powered;
        }

        public override bool Equals(object obj)
        {
            return obj is SubsystemNetState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Subsystem;
                hash = (hash * 397) ^ Health.GetHashCode();
                hash = (hash * 397) ^ Temperature.GetHashCode();
                hash = (hash * 397) ^ Pressure.GetHashCode();
                hash = (hash * 397) ^ Load.GetHashCode();
                hash = (hash * 397) ^ Enabled.GetHashCode();
                hash = (hash * 397) ^ Powered.GetHashCode();
                return hash;
            }
        }
    }
}
