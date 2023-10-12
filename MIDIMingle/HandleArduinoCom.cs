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

        public event DataReceivedEventHandler OnDataReceived;
        public event EventHandler<bool> ConnectedEvent;

        

        public bool Connected { get; private set; } = false;

        public HandleArduinoCom(int baudRate = 57600)
        {
            serialPort.BaudRate = baudRate;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;

            pollTimer = new System.Timers.Timer(3000);
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
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }

                    serialPort.PortName = portName;

                    try
                    {
                        serialPort.DtrEnable = false;
                        serialPort.Open();
                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();
                        await Task.Delay(500);
                        serialPort.WriteLine(HandshakeMessage);


                        serialPort.ReadTimeout = 500;  // 500 milliseconds, adjust as necessary

                        var response2 = serialPort.ReadLine().Trim();
                        

                        if (response2 == HandshakeResponse)
                        {
                            Connected = true;
                            Trace.WriteLine($"Connected to Arduino on {portName}");
                            break;
                        } else
                        {
                            Trace.WriteLine($"Wrong but received {response2}");
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
                    OnConnectionChanged(true);
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

        public async Task SetDebounceTimeAsync(int milliseconds)
        {
            await SendCommandAndWaitForResponse($"SetDebounceTime:{milliseconds}", null);
        }

        public async Task<int> GetDebounceTimeAsync()
        {
            var tcs = new TaskCompletionSource<int>();

            await SendCommandAndWaitForResponse("GetDebounceTime", (response) =>
            {
                try
                {
                    int result = int.Parse(response);
                    Trace.WriteLine($"Debounce time from arduino handler: {result}");
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error parsing response: {ex.Message}");
                    tcs.SetException(ex);
                }
            });

            return await tcs.Task;
        }

        private async Task SendCommandAndWaitForResponse(string command, Action<string> responseHandler)
        {
            try
            {
                serialPort.WriteLine(command);
                if (responseHandler != null)
                {
                    responseHandlers.Enqueue(responseHandler);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception during SendCommandAndWaitForResponse: {ex.Message}");
                HandleDisconnection();
            }
        }

        private void ProcessDataLine(string line)
        {
            if (line.Length >= 3)
            {
                int octaveValue = 0;
                bool[] localButtonStates = new bool[3];

                // Extract the button states
                for (int i = 0; i < 3; i++)
                {
                    localButtonStates[i] = line[i] == '0';
                }

                // Extract the octave if present
                if (line.Contains("oct:"))
                {
                    int startIndex = line.IndexOf("oct:") + 4;
                    if (int.TryParse(line.Substring(startIndex), out int parsedOctave))
                    {
                        octaveValue = parsedOctave;
                    }
                }

                RaiseDataReceivedEvent(new ReceivedData { ButtonStates = localButtonStates, Octave = octaveValue });
            }
        }

        protected virtual void RaiseDataReceivedEvent(ReceivedData data)
        {
            OnDataReceived?.Invoke(this, new ReceivedData
            {
                ButtonStates = data.ButtonStates,
                Octave = data.Octave
            });
        }

        protected virtual void OnConnectionChanged(bool isConnected)
        {
            ConnectedEvent?.Invoke(this, isConnected);
        }

        public void disconnectArduino()
        {
            try
            {
                serialPort.WriteLine("GoodbyeArduino");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception during disconnectArduino: {ex.Message}");
            }
            finally
            {
                HandleDisconnection();
            }
        }

        private object disconnectionLock = new object();

        private void HandleDisconnection()
        {
            lock (disconnectionLock)
            {
                try
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();
                        serialPort.Close();
                    }
                    pollTimer.Stop();
                    Connected = false;
                    OnConnectionChanged(false);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception during HandleDisconnection: {ex.Message}");
                }
            }
            StartPortSearch();  // Start the search outside of the lock.
        }
    }
}
