using System;
using UnityEngine;

namespace GJ.UnityToolbox.Localization
{
    public static class Localization
    {
        // PUBLIC PROPERTIES
        public static ELanguage CurrentLanguage { get; private set; }
        public static LocalizationSO LocalizationSO { get; private set; }

        // PUBLIC EVENTS
        public static event Action OnLanguageChanged;

        // PUBLIC METHODS
        public static void Init(ELanguage currentLanguage, LocalizationSO localizationSO)
        {
            Debug.Log("Localization.Init");

            CurrentLanguage = currentLanguage;
            LocalizationSO = localizationSO;

            // Remove all listeners
            OnLanguageChanged = null;
        }

        public static string Get(string key)
        {
            return LocalizationSO.Get(CurrentLanguage, key);
        }

        public static void SetLanguage(ELanguage language)
        {
            CurrentLanguage = language;
            OnLanguageChanged?.Invoke();
        }
    }
}