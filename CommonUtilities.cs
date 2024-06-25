using UnityEngine;
using UnityEngine.Assertions;

namespace GJ.UnityToolbox
{
    public static class CommonUtilities
    {
        public static T CheckNull<T>(T property, GameObject contextGameObject = null) where T : class
        {
            WarnIfNull(contextGameObject, property);
            return property;
        }

        public static void AssertIsNotNull<T>(GameObject contextGameObject, T property) where T : class
        {
            WarnIfNull(contextGameObject, property);
            Assert.IsNotNull(property);
        }

        public static void AssertIsNotNull<T>(ScriptableObject contextScriptableObject, T property) where T : class
        {
            if (property == null)
                Debug.LogError($"{contextScriptableObject.name}: The property of type {typeof(T)} is null!", contextScriptableObject);

            Assert.IsNotNull(property);
        }

        public static void WarnIfNull<T>(GameObject contextGameObject, T property) where T : class
        {
            if (property == null)
                Warn(contextGameObject, $"The property of type {typeof(T)} is null!");
        }

        public static void WarnIfNull<T>(string prefix, T property) where T : class
        {
            if (property == null)
                Warn(prefix, $"The property of type {typeof(T)} is null!");
        }

        public static void WarnIfTrue(GameObject contextGameObject, bool condition)
        {
            WarnIfTrue(contextGameObject, condition, "The condition is true!");
        }

        public static void WarnIfTrue(GameObject contextGameObject, bool condition, string customErrorMessage)
        {
            if (condition != true)
                return;

            Warn(contextGameObject, customErrorMessage);
        }

        public static void WarnIfTrue(ScriptableObject contextScriptableObject, bool condition)
        {
            WarnIfTrue(contextScriptableObject, condition, "The condition is true!");
        }

        public static void WarnIfTrue(ScriptableObject contextScriptableObject, bool condition, string customErrorMessage)
        {
            if (condition != true)
                return;

            Warn(contextScriptableObject, customErrorMessage);
        }

        public static void Warn(GameObject contextGameObject, string message)
        {
            string gameObjectName = contextGameObject != null ? $"{GetParentGameObjectNameRecursively(contextGameObject)}: " : "";
            Debug.LogError($"{gameObjectName}{message}", contextGameObject);
        }

        public static void Warn(ScriptableObject contextScriptableObject, string message)
        {
            Debug.LogError($"{contextScriptableObject.name}: {message}", contextScriptableObject);
        }

        public static void Warn(string prefix, string message)
        {
            Debug.LogError($"{prefix}: {message}");
        }

        public static void AssertIsTrue(GameObject contextGameObject, bool assertionToBeTrue)
        {
            AssertIsTrue(contextGameObject, assertionToBeTrue, "");
        }

        public static void AssertIsTrue(GameObject contextGameObject, bool assertionToBeTrue, string customErrorMessage)
        {
            string gameObjectName = contextGameObject != null ? $"{GetParentGameObjectNameRecursively(contextGameObject)}: " : "";
            string errorMessage = $"{gameObjectName}Assertion is False!" + " " + customErrorMessage;

            if (assertionToBeTrue == false)
                Debug.LogError(errorMessage, contextGameObject);

            Assert.IsTrue(assertionToBeTrue, errorMessage);
        }

        public static void AssertIsTrue(ScriptableObject contextScriptableObject, bool assertionToBeTrue)
        {
            AssertIsTrue(contextScriptableObject, assertionToBeTrue, "");
        }

        public static void AssertIsTrue(ScriptableObject contextScriptableObject, bool assertionToBeTrue, string customErrorMessage)
        {
            string errorMessage = $"{contextScriptableObject.name}: Assertion is False!" + " " + customErrorMessage;

            if (assertionToBeTrue == false)
                Debug.LogError(errorMessage, contextScriptableObject);

            Assert.IsTrue(assertionToBeTrue, errorMessage);
        }

        public static void AssertIsFalse(GameObject contextGameObject, bool assertionToBeFalse)
        {
            string gameObjectName = contextGameObject != null ? $"{GetParentGameObjectNameRecursively(contextGameObject)}: " : "";
            string errorMessage = $"{gameObjectName}Assertion is True!";

            if (assertionToBeFalse == true)
                Debug.LogError(errorMessage, contextGameObject);

            Assert.IsFalse(assertionToBeFalse, errorMessage);
        }

        public static string GetParentGameObjectNameRecursively(GameObject gameObject)
        {
            return GetParentGameObjectNameRecursively("", gameObject);
        }

        private static string GetParentGameObjectNameRecursively(string currentName, GameObject gameObject)
        {
            if (gameObject.transform.parent != null)
            {
                string parentName = GetParentGameObjectNameRecursively(currentName, gameObject.transform.parent.gameObject);
                return $"{parentName}.{gameObject.name}";
            }

            return gameObject.name;
        }
    }
}