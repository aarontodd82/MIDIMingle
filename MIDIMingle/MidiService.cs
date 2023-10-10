using System;
using TobiasErichsen.teVirtualMIDI;

namespace MIDIMingle
{
    public class MidiService : IMidiService
    {
        private TeVirtualMIDI port;
        private int? lastNote = null;
        public bool AllowRetrigger { get; set; }

        public MidiService(string portName)
        {
            Guid manufacturer = new Guid("aa4e075f-3504-4aab-9b06-9a4104a91cf0");
            Guid product = new Guid("bb4e075f-3504-4aab-9b06-9a4104a91cf0");

            port = new TeVirtualMIDI(portName, 65535, TeVirtualMIDI.TE_VM_FLAGS_PARSE_RX, ref manufacturer, ref product);

            // Add any other initializations for the port if needed
        }

        public void PlayMidiNote(int? midiNote)
        {
            // If a note is currently playing and we receive a new note or null, stop the last note
            if (lastNote.HasValue && (!midiNote.HasValue || midiNote.Value != lastNote.Value))
            {
                SendNoteOff(lastNote.Value);
            }
            // If allowRetrigger is false and the same note is received again, do nothing
            else if (!AllowRetrigger && midiNote.HasValue && lastNote.HasValue && midiNote.Value == lastNote.Value)
            {
                return;
            }

            // If we receive a valid midiNote, play it
            if (midiNote.HasValue)
            {
                SendNoteOn(midiNote.Value);
                // Update the last played note
                lastNote = midiNote;
            }
            else
            {
                // Reset the last note
                lastNote = null;
            }
        }

        private void SendNoteOn(int note)
        {
            // Send a MIDI Note On message on channel 1 with a default velocity of 64
            byte[] midiData = { 0x90, (byte)note, 64 };
            port.sendCommand(midiData);
        }

        private void SendNoteOff(int note)
        {
            // Send a MIDI Note Off message on channel 1 with a default velocity of 64
            byte[] midiData = { 0x80, (byte)note, 64 };
            port.sendCommand(midiData);
        }
    }
}
