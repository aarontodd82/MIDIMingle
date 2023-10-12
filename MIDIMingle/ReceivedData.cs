using System;
namespace MIDIMingle
{
    public class ReceivedData : EventArgs
    {
        public bool[] ButtonStates { get; set; }
        public int Octave { get; set; }
    }


}

