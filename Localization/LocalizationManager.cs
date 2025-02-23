using UnityEngine;

namespace GJ.UnityToolbox.Localization
{
    /// <summary>
    /// This class is used to 
    /// - Get the reference to the LocalizationSO
    /// - Initialize the Localization static class
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        [SerializeField] private LocalizationSO _localizationSO;

        public void Awake()
        {
            Localization.Init(ELanguage.English, _localizationSO);
        }
    }
}
