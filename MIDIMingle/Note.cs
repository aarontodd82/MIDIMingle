using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIMingle
{
    public struct Note
    {
        public int BaseValue;   // The base MIDI note value
        public int Alteration;  // The alteration value (-1, 0, +1 for half step down, no change, half step up)

        public int ActualValue => BaseValue + Alteration;
    }
}
