namespace ZoneUA.Persistence
{
    public interface IPersistentSaveParticipant
    {
        string ParticipantKey { get; }
        int ParticipantVersion { get; }
        string CaptureState();
        void RestoreState(string payload, int version);
    }
}