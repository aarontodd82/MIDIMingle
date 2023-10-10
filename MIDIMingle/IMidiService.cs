namespace MIDIMingle
{
    public interface IMidiService
    {
        int Transpose { get; set; }
        int? PlayMidiNote(int? midiNote);
        bool AllowRetrigger { get; set; }
        int Channel { get; set; }

        void SwitchMidiOut(string deviceName);

        // Static method to get the list of MIDI out devices
        string[] GetMidiOutDevices();


    }
}
