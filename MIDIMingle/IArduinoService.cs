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

        void disconnectArduino();
        Task InitializeAsync();
    }
}
