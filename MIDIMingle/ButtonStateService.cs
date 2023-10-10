using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MIDIMingle
{
    public class ButtonStateService : IButtonStateService
    {
        private Dictionary<string, Note> _currentMapping;
        private Dictionary<string, int> _alterationMapping;

        public ButtonStateService()
        {
            
        }

        public void LoadSet(string fileName, string setName)
        {
            var fileContent = File.ReadAllText(fileName);


            var data = JsonConvert.DeserializeObject<FingeringData>(fileContent);

            if (data.Sets.ContainsKey(setName))
            {
                _currentMapping = data.Sets[setName].Combinations.ToDictionary(pair => pair.Key, pair => new Note { BaseValue = pair.Value, Alteration = 0 });
                _alterationMapping = data.Sets[setName].Alterations;
            }
            else
            {
                if (!data.Sets.ContainsKey(setName))
                {
                    Console.WriteLine("Available keys in Sets: " + string.Join(", ", data.Sets.Keys));
                    throw new KeyNotFoundException($"The key set named {setName} was not found in the provided file.");
                }

                throw new KeyNotFoundException($"The key set named {setName} was not found in the provided file.");
            }
        }

        public int? GetMidiNoteFromButtons(bool[] buttons)
        {
            var key = string.Join("", buttons.Select(b => b ? "1" : "0"));

            // First, check for direct official combination match
            if (_currentMapping.ContainsKey(key))
            {
                return _currentMapping[key].BaseValue;
            }

            // If direct match is not found, then check for the best matching combination
            int? matchedNote = null;
            string matchedCombination = GetBestMatchingCombination(key);

            if (!string.IsNullOrEmpty(matchedCombination))
            {
                matchedNote = _currentMapping[matchedCombination].BaseValue;

                foreach (var alteration in _alterationMapping)
                {
                    if (key[GetIndexFromKey(alteration.Key)] == '1' && matchedCombination[GetIndexFromKey(alteration.Key)] == '0')
                    {
                        matchedNote += alteration.Value;
                    }
                }
            }

            return matchedNote;
        }

        private string GetBestMatchingCombination(string key)
        {
            string bestMatch = null;
            int maxMatchCount = -1;

            foreach (var combo in _currentMapping)
            {
                int matchCount = key.Where((ch, idx) => ch == combo.Key[idx] && ch == '1').Count();

                if (matchCount > maxMatchCount)
                {
                    maxMatchCount = matchCount;
                    bestMatch = combo.Key;
                }
            }

            return maxMatchCount > 0 ? bestMatch : null;
        }

        private bool IsSubset(string input, string combo)
        {
            return combo == string.Join("", input.Select((c, i) => c == '1' && combo[i] == '0' ? '0' : c));
        }

        private int GetIndexFromKey(string key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '1')
                    return i;
            }
            return -1;  // should not reach here if key is valid
        }


    }
}
