using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MIDIMingle
{
    public partial class FingeringChartEditor : Window
    {
        private string _filePath;
        private FingeringData _allData;
        private ObservableCollection<CombinationEntry> _currentData;
        private string _selectedSetName;
        private IButtonStateService _buttonStateService;

        public FingeringChartEditor(string filePath, string selectedSetName, IButtonStateService buttonStateService)
        {
            InitializeComponent();
            _filePath = filePath;
            _selectedSetName = selectedSetName;
            _buttonStateService = buttonStateService;
            _currentData = new ObservableCollection<CombinationEntry>();
            CombinationsListView.ItemsSource = _currentData;
            
            LoadData();
            
        }

        private void LoadSelectedSet()
        {
            if (SetsComboBox.SelectedItem is string setName)
            {
                _currentData.Clear();

                // Load combinations
                foreach (var entry in _allData.Sets[setName].Combinations)
                {
                    _currentData.Add(new CombinationEntry
                    {
                        Key1 = entry.Key[0] == '1',
                        Key2 = entry.Key.Length > 1 && entry.Key[1] == '1',
                        Key3 = entry.Key.Length > 2 && entry.Key[2] == '1',
                        MidiNote = entry.Value,
                        IsAlteration = false
                    });
                }

                // Load alterations
                foreach (var entry in _allData.Sets[setName].Alterations)
                {
                    _currentData.Add(new CombinationEntry
                    {
                        RadioKey1 = entry.Key[0] == '1',
                        RadioKey2 = entry.Key.Length > 1 && entry.Key[1] == '1',
                        RadioKey3 = entry.Key.Length > 2 && entry.Key[2] == '1',
                        IsAlteration = true,
                        AlterationValue = entry.Value
                    });
                }

                _buttonStateService.LoadSet(_filePath, setName);
            }
        }


        private void LoadData()
        {
            var json = File.ReadAllText(_filePath);
            _allData = JsonConvert.DeserializeObject<FingeringData>(json);
            SetsComboBox.ItemsSource = _allData.Sets.Keys;

            if (!string.IsNullOrEmpty(_selectedSetName) && _allData.Sets.ContainsKey(_selectedSetName))
            {
                SetsComboBox.SelectedItem = _selectedSetName;
                LoadSelectedSet();
            }
            else
            {
                SetsComboBox.ItemsSource = _allData.Sets.Keys;

                LoadSelectedSet();
            }
        }

        private void OnSetSelected(object sender, RoutedEventArgs e)
        {
            LoadSelectedSet();

            // Save the last selected set to JSON
            if (SetsComboBox.SelectedItem is string setName)
            {
                _allData.LastSelectedSet = setName;
                var updatedJson = JsonConvert.SerializeObject(_allData, Formatting.Indented);
                File.WriteAllText(_filePath, updatedJson);
            }

        }

        private void OnAddEntryRow(object sender, RoutedEventArgs e)
        {
            var entry = new CombinationEntry();
            if (sender is Button btn && btn.Tag is string tag && tag == "Alteration")
            {
                entry.IsAlteration = true;
            } else
            {
                entry.IsAlteration = false;
            }
            _currentData.Add(entry);
        }

        private void OnDeleteRow(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CombinationEntry entry)
            {
                _currentData.Remove(entry);
            }
        }


        private void OnApply(object sender, RoutedEventArgs e)
        {
            if (SetsComboBox.SelectedItem is string setName)
            {
                _allData.Sets[setName].Combinations.Clear();
                _allData.Sets[setName].Alterations.Clear();

                foreach (var entry in _currentData)
                {
                    string keyCombination =
                ((entry.Key1 || entry.RadioKey1) ? "1" : "0") +
                ((entry.Key2 || entry.RadioKey2) ? "1" : "0") +
                ((entry.Key3 || entry.RadioKey3) ? "1" : "0");

                    if (entry.IsAlteration.HasValue && entry.IsAlteration.Value)
                    {
                        _allData.Sets[setName].Alterations[keyCombination] = entry.AlterationValue.HasValue ? entry.AlterationValue.Value : 0;
                    }
                    else
                    {
                        _allData.Sets[setName].Combinations[keyCombination] = entry.MidiNote;
                    }

                }

                // Serialize the updated _allData and write it back to the JSON file
                var updatedJson = JsonConvert.SerializeObject(_allData, Formatting.Indented);
                File.WriteAllText(_filePath, updatedJson);
                _buttonStateService.LoadSet(_filePath, setName);
            }
        }

        private void OnAddSet(object sender, RoutedEventArgs e)
        {
            var newSetName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new set name:", "New Set", "");
            if (string.IsNullOrEmpty(newSetName))
                return;

            if (_allData.Sets.ContainsKey(newSetName))
            {
                MessageBox.Show("Set name already exists.");
                return;
            }

            _allData.Sets[newSetName] = new SetData();
            var updatedJson = JsonConvert.SerializeObject(_allData, Formatting.Indented);
            File.WriteAllText(_filePath, updatedJson);
            LoadData();
            SetsComboBox.SelectedItem = newSetName;
        }

        private void OnDeleteSet(object sender, RoutedEventArgs e)
        {
            if (SetsComboBox.SelectedItem is string setName)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the set '{setName}'?", "Delete Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _allData.Sets.Remove(setName);
                    var updatedJson = JsonConvert.SerializeObject(_allData, Formatting.Indented);
                    File.WriteAllText(_filePath, updatedJson);
                    LoadData();
                    SetsComboBox.SelectedItem = 0;
                }
            }
        }

        private void OnIsAlterationChanged(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var combinationEntry = checkbox?.DataContext as CombinationEntry;
            if (combinationEntry != null)
            {
                combinationEntry.Key1 = false;
                combinationEntry.Key2 = false;
                combinationEntry.Key3 = false;
                combinationEntry.RadioKey1 = false;
                combinationEntry.RadioKey2 = false;
                combinationEntry.RadioKey3 = false;
            }
        }






    }
}
