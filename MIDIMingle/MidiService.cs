using NAudio.Midi;
using System;
using System.Windows.Media;
using TobiasErichsen.teVirtualMIDI;


namespace MIDIMingle
{
    public class MidiService : IMidiService
    {
        private TeVirtualMIDI virtualPort;
        private MidiOut midiOutDevice;
        private int? lastNote = null;
        public bool AllowRetrigger { get; set; }
        public int Transpose { get; set; } = 0;
        public int Channel { get; set; } = 0;

        public MidiService(string portName)
        {


            InitVirtualMidiPort(portName);
        }

        private void InitVirtualMidiPort(string portName)
        {
            Guid manufacturer = new Guid("aa4e075f-3504-4aab-9b06-9a4104a91cf0");
            Guid product = new Guid("bb4e075f-3504-4aab-9b06-9a4104a91cf0");

            

            virtualPort = new TeVirtualMIDI(portName, 65535, TeVirtualMIDI.TE_VM_FLAGS_INSTANTIATE_TX_ONLY, ref manufacturer, ref product);
        }

        public string[] GetMidiOutDevices()
        {
            int numOfDevices = MidiOut.NumberOfDevices;
            string[] devices = new string[numOfDevices];

            for (int i = 0; i < numOfDevices; i++)
            {
                devices[i] = MidiOut.DeviceInfo(i).ProductName;
            }

            return devices;
        }

        public void SwitchMidiOut(string deviceName)
        {
            if (deviceName == "Virtual MIDI Port") // Assume virtualPortName is a constant or member that has the name of your virtual port
            {
                if (midiOutDevice != null)
                {
                    midiOutDevice.Close();
                    midiOutDevice = null;
                }

                // If not initialized, initialize the virtual port
                if (virtualPort == null)
                {
                    InitVirtualMidiPort(App.VIRTUALPORTNAME);
                }
            }
            else
            {
                if (virtualPort != null)
                {
                    virtualPort.shutdown();
                    virtualPort = null;
                }

                if (midiOutDevice != null)
                {
                    midiOutDevice.Close();
                }

                int deviceId = Array.IndexOf(GetMidiOutDevices(), deviceName);
                if (deviceId >= 0)
                {
                    midiOutDevice = new MidiOut(deviceId);
                }
            }
        }

        public int? PlayMidiNote(int? midiNote)
        {
            // If a note is currently playing and we receive a new note or null, stop the last note
            if (lastNote.HasValue && (!midiNote.HasValue || midiNote.Value != lastNote.Value))
            {
                SendNoteOff(lastNote.Value);
            }
            // If allowRetrigger is false and the same note is received again, do nothing
            else if (!AllowRetrigger && midiNote.HasValue && lastNote.HasValue && midiNote.Value == lastNote.Value)
            {
                return null;
            }

            // If we receive a valid midiNote, play it
            if (midiNote.HasValue)
              {
                midiNote += Transpose;

                // Ensure that midiNote is within valid MIDI range
                midiNote = Math.Clamp(midiNote.Value, 0, 127);
                SendNoteOn(midiNote.Value);
                // Update the last played note
                lastNote = midiNote;
                return midiNote;
            }
            else
            {
                // Reset the last note
                lastNote = null;
                return null;
            }
        }

        private void SendNoteOn(int note)
        {
            // Send a MIDI Note On message on channel 1 with a default velocity of 64
            byte noteOnStatus = GetMidiStatusByte(MidiMessageType.NoteOn, Channel);
            byte[] midiData = { noteOnStatus, (byte)note, 64 };
            SendMidiData(midiData);
        }

        private void SendNoteOff(int note)
        {
            // Send a MIDI Note Off message on channel 1 with a default velocity of 64
            byte noteOffStatus = GetMidiStatusByte(MidiMessageType.NoteOff, Channel);
            byte[] midiData = { noteOffStatus, (byte)note, 64 };
            SendMidiData(midiData);
        }

        private void SendMidiData(byte[] midiData)
        {
            if (virtualPort != null)
            {
                virtualPort.sendCommand(midiData);
            }
            else if (midiOutDevice != null)
            {
                midiOutDevice.Send(ConvertToNAudioMidiFormat(midiData));
            }
        }

        private int ConvertToNAudioMidiFormat(byte[] midiData)
        {
            if (midiData == null || midiData.Length < 3)
                throw new ArgumentException("Expected a 3-byte MIDI data array.");

            return (midiData[0] & 0xFF) | ((midiData[1] & 0xFF) << 8) | ((midiData[2] & 0xFF) << 16);
        }


        public byte GetMidiStatusByte(MidiMessageType type, int channel)
        {
            if (channel < 0 || channel > 15) throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 0 and 15.");

            byte statusByte = (byte)((int)type << 4 | channel);

            return statusByte;
        }
    }
}
