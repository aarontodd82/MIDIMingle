using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

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

            _arduinoService = arduinoService ?? throw new ArgumentNullException(nameof(arduinoService));
            _midiService = midiService ?? throw new ArgumentNullException(nameof(midiService));


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
           
            _midiService.PlayMidiNote(midiNote);
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

    }

}


