namespace MIDIMingle
{
    public interface IMidiService
    {
        void PlayMidiNote(int? midiNote);
        bool AllowRetrigger { get; set; }
    }
}
