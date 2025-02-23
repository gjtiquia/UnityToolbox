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
        [SerializeField] private string _csvFileName;
        [SerializeField] private bool _isSecondColumnComments = true;

        /// <summary>
        /// Note: a known limitation is the translations cannot have commas. If so, perhaps consider using TSVs instead (tab-separated values).
        /// </summary>
        [Button]
        private void ImportFromCSV()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            // Debug.Log("currentDirectory: " + currentDirectory);

            var assetFolder = AssetDatabase.GetAssetPath(_assetFolder);
            // Debug.Log("assetFolder: " + assetFolder);

            var absoluteDirectory = Path.Combine(currentDirectory, assetFolder);
            var absolutePath = Path.Combine(absoluteDirectory, _csvFileName);
            // Debug.Log("absolutePath: " + absolutePath);

            _localizedStrings.Clear();

            using (var reader = new StreamReader(absolutePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var key = values[0];
                    if (key == "Key") continue; // Skip first row
                    if (key == "") continue; // Skip empty rows

                    Debug.Log("line: " + line);
                    Debug.Log("Key: " + key);

                    // Optional comments are in the second column of the csv file
                    if (_isSecondColumnComments)
                        Debug.Log("Comments: " + key[1]);

                    var translations = new List<TranslationEntry>();
                    for (int i = 0; i < (int)ELanguage.COUNT; i++)
                    {
                        var index = i + 1;
                        if (_isSecondColumnComments) index++;

                        var language = (ELanguage)i;
                        var value = values[index];

                        Debug.Log($"language: {language}, value: {value}");

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
            }

            Debug.Log("SetDirty");
            EditorUtility.SetDirty(this);
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
            public string Value;
        }
    }
}