using PanicMechanics.Core;
using Unity.Netcode;

namespace PanicMechanics.Runtime
{
    public struct SubsystemNetState : INetworkSerializable
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
    }
}
