using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIMingle
{
    public interface IArduinoService
    {
        event EventHandler<bool[]> DataReceivedEvent;
        event EventHandler<bool> ConnectedEvent;

        void disconnectArduino();
        Task InitializeAsync();
        Task<int> GetDebounceTimeAsync();
        Task SetDebounceTimeAsync(int debounceTime);
    }
}
