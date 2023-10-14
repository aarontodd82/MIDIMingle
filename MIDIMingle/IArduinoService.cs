using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIMingle
{
    public delegate void DataReceivedEventHandler(object sender, ReceivedData e);

    public interface IArduinoService
    {
        event DataReceivedEventHandler OnDataReceived;
        event EventHandler<bool> ConnectedEvent;

        void disconnectArduino();
        Task InitializeAsync();
        Task<int> GetDebounceTimeAsync();
        Task SetDebounceTimeAsync(int debounceTime);
        Task<int> GetOctavesAsync();
        Task SetOctavesAsync(int debounceTime);
    }
}
