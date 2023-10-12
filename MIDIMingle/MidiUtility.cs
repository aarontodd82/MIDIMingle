using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIMingle
{
    public static class MidiUtility
    {
        public static string MidiNoteToNoteName(int? midiNote)
        {
            if (!midiNote.HasValue)
            {
                return string.Empty; // Return blank string for null input
            }

            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

            int octave = (midiNote.Value / 12) - 1;
            int noteIndex = midiNote.Value % 12;

            return noteNames[noteIndex] + octave.ToString();
        }
    }
}
