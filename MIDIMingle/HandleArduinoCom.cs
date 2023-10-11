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
        private SerialPort serialPort = new SerialPort();
        private const string HandshakeMessage = "HelloArduino";
        private const string HandshakeResponse = "HelloPC";
        private const int FastPollingInterval = 100;
        private const int SlowPollingInterval = 1000;
        private System.Timers.Timer pollTimer;
        private const string PingMessage = "AreYouThereArduino";
        private const string PingResponse = "YesIAm";
        private int failedPingCount = 0;
        private const int MaxFailedPings = 3;

        private Queue<Action<string>> responseHandlers = new Queue<Action<string>>();
        private StringBuilder receivedDataBuffer = new StringBuilder();

        public event EventHandler<bool[]> DataReceivedEvent;

        public bool Connected { get; private set; } = false;

        public HandleArduinoCom(int baudRate = 9600)
        {
            serialPort.BaudRate = baudRate;

            pollTimer = new System.Timers.Timer(2000);
            pollTimer.Elapsed += PollTimerElapsed;
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() => StartPortSearch());
        }

        private async void StartPortSearch()
        {
            Trace.WriteLine("Listening to Arduino. Press any key to exit...");
            while (!Connected)
            {
                foreach (var portName in SerialPort.GetPortNames())
                {
                    
                    serialPort.PortName = portName;

                    try
                    {
                        serialPort.DtrEnable = false;
                        serialPort.Open();
                        await Task.Delay(500);
                        serialPort.WriteLine(HandshakeMessage);
                           
                        serialPort.ReadTimeout = 500;  // 500 milliseconds, adjust as necessary

                        var response = serialPort.ReadLine().Trim();
                        Trace.WriteLine(response);

                        if (response == HandshakeResponse)
                        {
                            Connected = true;
                            Trace.WriteLine($"Connected to Arduino on {portName}");
                            break;
                        }
                    }
                    catch (TimeoutException)
                    {
                        // This will catch the timeout exception, allowing you to continue to the next port
                        Trace.WriteLine($"No response from {portName} within timeout.");
                    }
                    catch
                    {
                        Trace.WriteLine($"Error trying port {portName}");
                    }
                    finally
                    {
                        if (serialPort.IsOpen) serialPort.Close();
                    }
                }

                var pollingInterval = Connected ? SlowPollingInterval : FastPollingInterval;
                await Task.Delay(pollingInterval);
            }

            // Now that we're connected, start listening for data
            if (Connected)
            {
                try
                {
                    serialPort.DataReceived += SerialPort_DataReceived;
                    serialPort.Open();
                    pollTimer.Start();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception during StartPortSearch: {ex.Message}");
                    HandleDisconnection();
                }
            }
        }

        private async void PollTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                serialPort.WriteLine(PingMessage);
                responseHandlers.Enqueue((response) =>
                {
                    if (response != PingResponse)
                    {
                        failedPingCount++;
                        Trace.WriteLine("Failed ping");
                    }
                    else
                    {
                        failedPingCount = 0;
                        Trace.WriteLine("Successful ping");
                    }

                    if (failedPingCount >= MaxFailedPings)
                    {
                        // Consider Arduino as disconnected
                        pollTimer.Stop();
                        Connected = false;
                        failedPingCount = 0;
                        disconnectArduino();
                        StartPortSearch();
                    }
                });
            } catch (Exception ex)
            {
                Trace.WriteLine($"Exception during PollTimerElapsed: {ex.Message}");
                HandleDisconnection();
            }
        }

        public void disconnectArduino()
        {
            try
            {
                serialPort.WriteLine("GoodbyeArduino");
                serialPort.Close();
            } catch (Exception ex)
            {
                Trace.WriteLine($"Exception during disconnectArduino: {ex.Message}");
            }
            finally
            {
                HandleDisconnection();
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                receivedDataBuffer.Append(serialPort.ReadExisting());

                while (true)
                {
                    string bufferContent = receivedDataBuffer.ToString();

                    if (bufferContent.Contains("\n"))
                    {
                        string line = bufferContent.Substring(0, bufferContent.IndexOf("\n")).Trim();
                        receivedDataBuffer.Remove(0, bufferContent.IndexOf("\n") + 1);

                        if (responseHandlers.Count > 0)
                        {
                            var handler = responseHandlers.Dequeue();
                            handler(line);
                        }
                        else
                        {
                            ProcessDataLine(line);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception during SerialPort_DataReceived: {ex.Message}");
                HandleDisconnection();
            }
        }

        private void ProcessDataLine(string line)
        {
            if (line.Length == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    buttonStates[i] = line[i] == '0';
                }

                OnDataReceived(buttonStates);
            }
        }

        protected virtual void OnDataReceived(bool[] data)
        {
            DataReceivedEvent?.Invoke(this, data);
        }

        private void HandleDisconnection()
        {
            try
            {
                if (serialPort.IsOpen) serialPort.Close();
                pollTimer.Stop();
                Connected = false;
                StartPortSearch();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception during HandleDisconnection: {ex.Message}");
            }
        }
    }
}




























