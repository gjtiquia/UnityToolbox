using System.Collections.Generic;
using UnityEngine;

// Project needs to have Odin Inspector installed
// Otherwise, find a way to trigger the import from CSV method
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace GJ.UnityToolbox.Localization
{
    [CreateAssetMenu(fileName = "LocalizationSO", menuName = "ScriptableObjects/LocalizationSO")]
    public class LocalizationSO : ScriptableObject
    {
        [SerializeField] private List<LocalizedString> _localizedStrings;

        public string Get(ELanguage currentLanguage, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Localization: Key is empty!");
                return "-";
            }

            var result = _localizedStrings.Find(x => x.Key == key);
            if (result == null)
            {
                Debug.LogError($"Localization: Cannot find '{key}'!");
                return "-";
            }

            var entry = result.Translations.Find(x => x.Language == currentLanguage);
            if (entry == null)
            {
                Debug.LogError($"Localization: No {currentLanguage} translation found for '{key}'!");
                return "-";
            }

            // \n in excel automatically changed to \\n to render correctlyin excel. Need to change back to \n.
            // https://forum.unity.com/threads/newline-n-not-parsing-correctly.470634/
            return entry.Value.Replace("\\n", "\n");
        }

#if UNITY_EDITOR
        [SerializeField] private DefaultAsset _assetFolder;
        [SerializeField] private string _fileName;
        [SerializeField] private bool _isSecondColumnComments = true;

        [Button]
        private void ImportFromFile()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            // Debug.Log("currentDirectory: " + currentDirectory);

            var assetFolder = AssetDatabase.GetAssetPath(_assetFolder);
            // Debug.Log("assetFolder: " + assetFolder);

            var absoluteDirectory = Path.Combine(currentDirectory, assetFolder);
            var absolutePath = Path.Combine(absoluteDirectory, _fileName);
            // Debug.Log("absolutePath: " + absolutePath);

            _localizedStrings.Clear();

            List<List<string>> parsedData = ParseCSVFile(absolutePath, ',');

            // Skip the header row
            for (int rowIndex = 1; rowIndex < parsedData.Count; rowIndex++)
            {
                var values = parsedData[rowIndex];

                var key = values[0];
                if (key == "") continue; // Skip empty rows

                var translations = new List<TranslationEntry>();
                for (int i = 0; i < (int)ELanguage.COUNT; i++)
                {
                    var index = i + 1;

                    // Optional comments are in the second column of the csv file
                    if (_isSecondColumnComments) index++;

                    // Make sure we don't go out of bounds
                    if (index >= values.Count)
                    {
                        Debug.LogError($"Missing translation for key '{key}', language {(ELanguage)i}");
                        continue;
                    }

                    var language = (ELanguage)i;
                    var value = values[index];

                    Debug.Log($"key: {key}, language: {language}, value: {value}");

                    translations.Add(new TranslationEntry()
                    {
                        Language = language,
                        Value = value
                    });
                }

                _localizedStrings.Add(new LocalizedString()
                {
                    Key = key,
                    Translations = translations,
                });
            }

            Debug.Log("SetDirty");
            EditorUtility.SetDirty(this);
        }

        private List<List<string>> ParseCSVFile(string filePath, char delimiter)
        {
            List<List<string>> result = new List<List<string>>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string pendingField = "";
                bool insideQuotes = false;
                List<string> currentLine = new List<string>();

                // Read entire file content
                string fileContent = reader.ReadToEnd();
                int index = 0;

                while (index < fileContent.Length)
                {
                    char c = fileContent[index];

                    // Check for quotes (handles escaping)
                    if (c == '"')
                    {
                        // Check if it's an escaped quote
                        if (index + 1 < fileContent.Length && fileContent[index + 1] == '"')
                        {
                            pendingField += '"';
                            index += 2; // Skip both quotes
                            continue;
                        }

                        // Toggle quote state
                        insideQuotes = !insideQuotes;
                        index++;
                        continue;
                    }

                    // Process delimiter (only if not inside quotes)
                    if (c == delimiter && !insideQuotes)
                    {
                        currentLine.Add(pendingField);
                        pendingField = "";
                        index++;
                        continue;
                    }

                    // Process newline (only if not inside quotes)
                    if ((c == '\n' || (c == '\r' && index + 1 < fileContent.Length && fileContent[index + 1] == '\n')) && !insideQuotes)
                    {
                        currentLine.Add(pendingField);
                        result.Add(new List<string>(currentLine));

                        // Reset for next line
                        currentLine.Clear();
                        pendingField = "";

                        // Skip the newline character(s)
                        if (c == '\r')
                            index += 2; // Skip \r\n
                        else
                            index++; // Skip \n

                        continue;
                    }

                    // Handle newlines inside quotes (preserve them)
                    if ((c == '\n' || c == '\r') && insideQuotes)
                    {
                        if (c == '\r' && index + 1 < fileContent.Length && fileContent[index + 1] == '\n')
                        {
                            pendingField += '\n';
                            index += 2; // Skip \r\n
                        }
                        else
                        {
                            pendingField += '\n';
                            index++; // Skip \n or \r
                        }
                        continue;
                    }

                    // Add character to pending field
                    pendingField += c;
                    index++;
                }

                // Handle last field if exists
                if (!string.IsNullOrEmpty(pendingField) || currentLine.Count > 0)
                {
                    currentLine.Add(pendingField);
                    result.Add(currentLine);
                }
            }

            return result;
        }
#endif

        [System.Serializable]
        private class LocalizedString
        {
            public string Key;
            public List<TranslationEntry> Translations;
        }

        [System.Serializable]
        private class TranslationEntry
        {
            public ELanguage Language;
            [TextArea] public string Value;
        }
    }
}
