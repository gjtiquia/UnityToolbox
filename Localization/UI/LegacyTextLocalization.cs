using UnityEngine;
using UnityEngine.UI;

namespace GJ.UnityToolbox.Localization.UI
{
    [RequireComponent(typeof(Text))]
    public class LegacyTextLocalization : MonoBehaviour
    {
        [SerializeField] private string _key;

        private Text _text
        {
            get
            {
                // Lazy initialize
                if (m_text == null)
                    m_text = GetComponent<Text>();

                return m_text;
            }
        }
        private Text m_text;

        // Cant be OnEnable/OnDisable because Localization.Init is called in Awake, which may/may not be before OnEnable in other game objects
        private void Start()
        {
            UpdateText();
            Localization.OnLanguageChanged += UpdateText;
        }

        // Cant be OnEnable/OnDisable because Localization.Init is called in Awake, which may/may not be before OnEnable in other game objects
        private void OnDestroy()
        {
            Localization.OnLanguageChanged -= UpdateText;
        }

        private void UpdateText()
        {
            var translation = Localization.Get(_key);
            // Debug.Log($"LegacyTextLocalization: {_key} = {translation}");

            _text.text = translation;
        }
    }
}