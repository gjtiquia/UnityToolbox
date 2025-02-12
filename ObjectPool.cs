using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GJ.UnityToolbox
{
    // Referenced from Photon Fusion BR200 ObjectCache
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Create a new GameObject to hold the singleton instance
                    GameObject singletonObject = new GameObject(typeof(ObjectPool).Name);
                    _instance = singletonObject.AddComponent<ObjectPool>();
                    DontDestroyOnLoad(singletonObject);
                }
                return _instance;
            }
        }
        private static ObjectPool _instance;

        // PRIVATE MEMBERS
        private Transform _defaultParentTransform;
        private Dictionary<GameObject, Transform> _poolParentTransformDict;
        private Dictionary<int, Transform> _returnParentTransformDict;

        // MonoBehaviour INTERFACE
        private void Awake()
        {
            // Ensure that there is only one instance of the ObjectPool
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            GameObject defaultParentGO = new GameObject("Active Pooled Objects");
            _defaultParentTransform = defaultParentGO.transform;
            _defaultParentTransform.SetParent(this.transform);

            _poolParentTransformDict = new Dictionary<GameObject, Transform>();
            _returnParentTransformDict = new Dictionary<int, Transform>();

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            DestroyAllChildren(_defaultParentTransform);

            foreach (var pair in _poolParentTransformDict)
            {
                DestroyAllChildren(pair.Value);
            }
        }

        // PUBLIC METHODS
        public T Get<T>(T prefab, bool activate = true, bool createIfEmpty = true) where T : UnityEngine.Component
        {
            return Get(prefab, null, activate, createIfEmpty);
        }

        public GameObject Get(GameObject prefab, bool activate = true, bool createIfEmpty = true)
        {
            return Get(prefab, null, activate, createIfEmpty);
        }

        public T Get<T>(T prefab, Transform parent, bool activate = true, bool createIfEmpty = true) where T : UnityEngine.Component
        {
            GameObject instance = Get(prefab.gameObject, parent, activate, createIfEmpty);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public GameObject Get(GameObject prefab, Transform parent, bool activate = true, bool createIfEmpty = true)
        {
            if (prefab == null)
            {
                Debug.LogError("ObjectCache.Get: prefab null! Returning null...");
                return null;
            }

            // Create instance if pool is empty
            bool hasPool = _poolParentTransformDict.ContainsKey(prefab);
            bool isPoolEmpty = hasPool ? _poolParentTransformDict[prefab].childCount <= 0 : true;
            if (isPoolEmpty)
            {
                if (createIfEmpty)
                {
                    Debug.LogWarning($"ObjectCache.Get: Creating instance for {prefab.name}! Consider precaching!");
                    CreateInstance(prefab);
                }
                else
                {
                    Debug.LogError($"ObjectCache.Get: Prefab '{prefab.name}' not available in cache, returning null... [Debug Info] hasPool: {hasPool}, hasPoolChildren: {isPoolEmpty}");
                    return null;
                }
            }

            Transform instanceTransform = _poolParentTransformDict[prefab].GetChild(0);

            // Set parent
            if (parent == null)
                instanceTransform.SetParent(_defaultParentTransform, false);
            else
                instanceTransform.SetParent(parent, false);

            // Set transform
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = Quaternion.identity;
            instanceTransform.localScale = Vector3.one;

            GameObject instance = instanceTransform.gameObject;

            // Set active
            if (activate == true)
            {
                instance.SetActive(true);
            }

            return instance;
        }

        public void Return(UnityEngine.Component component, bool deactivate = true)
        {
            Return(component.gameObject, deactivate);
        }

        public void Return(GameObject instance, bool deactivate = true)
        {
            if (!_returnParentTransformDict.ContainsKey(instance.GetInstanceID()))
            {
                Debug.LogError($"ObjectCache.Return: Cannot find pool parent of '{instance.gameObject.name}'! Are you sure this object came from the ObjectCache? Destroying instead...");
                Destroy(instance);
                return;
            }

            if (deactivate)
                instance.SetActive(false);

            Transform parentTransform = _returnParentTransformDict[instance.GetInstanceID()];
            instance.transform.SetParent(parentTransform);
        }

        public void ReturnRange(List<GameObject> instances, bool deactivate = true)
        {
            for (int i = 0; i < instances.Count; i++)
            {
                Return(instances[i], deactivate);
            }
        }

        public void ReturnDeferred(UnityEngine.Component component, float delay)
        {
            ReturnDeferred(component.gameObject, delay);
        }

        public void ReturnDeferred(UnityEngine.Component component, float delay, Action callback)
        {
            ReturnDeferred(component.gameObject, delay, callback);
        }

        public void ReturnDeferred(GameObject instance, float delay)
        {
            ReturnDeferred(instance, delay, () => { });
        }

        /// <summary>
        /// Returns object after a specified delay duration.
        /// </summary>
        public void ReturnDeferred(GameObject instance, float delay, Action callback)
        {
            Debug.LogError("ObjectCache.ReturnDeferred: Not implemented! Install DOTween and uncomment the code to use");

            /*
            Sequence deferSequence = DOTween.Sequence();
            deferSequence
                .AppendInterval(delay)
                .AppendCallback(() => Return(instance));

            if (callback != null) deferSequence.AppendCallback(() => callback()); 
            */
        }

        public void Prepare(UnityEngine.Component component, int desiredCount)
        {
            Prepare(component.gameObject, desiredCount);
        }

        public void Prepare(GameObject prefab, int desiredCount)
        {
            for (int i = 0; i < desiredCount; i++)
            {
                CreateInstance(prefab);
            }
        }

        // PRIVATE METHODS

        private void CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, null, false);
            instance.SetActive(false);

            if (!_poolParentTransformDict.ContainsKey(prefab))
                CreateParentTransform(prefab);

            Transform parentTransform = _poolParentTransformDict[prefab];
            instance.transform.SetParent(parentTransform);
            _returnParentTransformDict.Add(instance.GetInstanceID(), parentTransform); // For return parent transform identification
        }

        private void CreateParentTransform(GameObject prefab)
        {
            GameObject parent = new GameObject($"Pool:{prefab.gameObject.name}");

            Transform parentTransform = parent.transform;
            parentTransform.SetParent(this.transform);
            _poolParentTransformDict.Add(prefab, parentTransform);
        }

        private void DestroyAllChildren(Transform parentTransform)
        {
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                Transform child = parentTransform.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        private void OnActiveSceneChanged(Scene current, Scene next)
        {
            // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-activeSceneChanged.html

            string currentName = current.name;
            if (currentName == null)
            {
                // Scene1 has been removed
                currentName = "Replaced";
            }

            Debug.Log($"ObjectCache.OnActiveSceneChanged: previous: {currentName}, next: {next.name}, Destroying all children...");

            DestroyAllChildren(_defaultParentTransform);

            foreach (var pair in _poolParentTransformDict)
            {
                DestroyAllChildren(pair.Value);
            }
        }
    }
}
