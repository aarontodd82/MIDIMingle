using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MIDIMingle
{

    public partial class MainWindow : Window
    {

        private readonly IArduinoService _arduinoService;
        private readonly IMidiService _midiService;
        private readonly IButtonStateService _buttonStateService;

        public MainWindow(IArduinoService arduinoService, IMidiService midiService, IButtonStateService buttonStateService)
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            _midiService = midiService ?? throw new ArgumentNullException(nameof(midiService));
            AllowRetriggerCheckbox.IsChecked = _midiService.AllowRetrigger;
            var midiOutDevices = _midiService.GetMidiOutDevices().ToList();
            midiOutDevices.Insert(0, "Virtual MIDI Port"); // Assuming "Virtual MIDI Port" is the name of your virtual port
            MidiOutputDropdown.ItemsSource = midiOutDevices;
            MidiOutputDropdown.SelectedIndex = 0; // Default selection is the virtual port

            _arduinoService = arduinoService ?? throw new ArgumentNullException(nameof(arduinoService));
            Task.Run(() => _arduinoService.InitializeAsync());
            _arduinoService.DataReceivedEvent += HandleArduino_DataReceivedEvent;

            _buttonStateService = buttonStateService ?? throw new ArgumentNullException(nameof(buttonStateService));

            LoadDefaultSet();

        }

        private void LoadDefaultSet()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, "Combinations.json");
            var jsonData = File.ReadAllText(fullPath);
            var data = JsonConvert.DeserializeObject<FingeringData>(jsonData);
            string lastSet = data.LastSelectedSet ?? "DefaultSet";
            _buttonStateService.LoadSet(fullPath, lastSet);
        }

        private void HandleArduino_DataReceivedEvent(object sender, bool[] buttonStates)
        {
            PlayMidiNoteFromButtons(buttonStates);
        }

        private void PlayMidiNoteFromButtons(bool[] buttonStates)
        {
            var midiNote = _buttonStateService.GetMidiNoteFromButtons(buttonStates);
            Trace.WriteLine($"MIDI Note: {midiNote}");

            midiNote = _midiService.PlayMidiNote(midiNote);

            Dispatcher.Invoke(() =>
            {
                MidiNoteLabel.Content = midiNote.ToString();
            });

            
        }

        private void MidiChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MidiChannelComboBox.SelectedItem is ComboBoxItem selectedItem && _midiService != null)
            {
                int selectedChannel;
                if (int.TryParse(selectedItem.Content.ToString(), out selectedChannel))
                {
                    _midiService.Channel = selectedChannel - 1; // Subtracting 1 because MIDI channels in code are 0-15
                }
            }
        }


        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Handle the value change here
            int? newValue = e.NewValue as int?;
            if (newValue.HasValue && _midiService != null)
            {
                _midiService.Transpose = newValue.Value;
            }

        }





        private void OpenFingeringChartEditor(object sender, RoutedEventArgs e)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, "Combinations.json");
            var jsonData = File.ReadAllText(fullPath);
            var allSetsData = JsonConvert.DeserializeObject<AllSetsData>(jsonData);
            var editor = new FingeringChartEditor(fullPath, allSetsData.LastSelectedSet, _buttonStateService);
            editor.ShowDialog();
        }

        private void MidiOutputDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_midiService != null)
            {
                string selectedDevice = (string)MidiOutputDropdown.SelectedItem;
                _midiService.SwitchMidiOut(selectedDevice);
            }
        }

        private void AllowRetriggerCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            _midiService.AllowRetrigger = true;
        }

        private void AllowRetriggerCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            _midiService.AllowRetrigger = false;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _arduinoService.disconnectArduino();
        }

    }

}


