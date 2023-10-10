using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;

namespace MIDIMingle
{
    public class HandleArduinoCom : IArduinoService
    {
        private bool[] buttonStates = new bool[3];
        SerialPort serialPort = new SerialPort();
        public event EventHandler<bool[]> DataReceivedEvent;

        public HandleArduinoCom(string portName = "COM14", int baudRate = 9600)
        {
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DtrEnable = true;

            serialPort.DataReceived += SerialPort_DataReceived;

            serialPort.Open();

            Trace.WriteLine($"Listening to Arduino on {portName}. Press any key to exit...");
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadLine().Trim();

            if (data.Length == 3)  // Ensure we have received exactly 3 bits
            {
                for (int i = 0; i < 3; i++)
                {
                    buttonStates[i] = data[i] == '0';
                }

                OnDataReceived(buttonStates);
            }
        }

        protected virtual void OnDataReceived(bool[] data)
        {
            DataReceivedEvent?.Invoke(this, data);
        }
    }
}
