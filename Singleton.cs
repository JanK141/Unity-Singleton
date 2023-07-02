using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
    /// Over engineered singleton base class.
    /// After creating first instance of it on the scene it will automatically create a prefab
    /// being instantiated every time when reffered to, while there is no instance present. 
    /// </summary>
    /// <typeparam name="T">Class deriving from singleton</typeparam>
    [ExecuteAlways]
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField, Tooltip("Should this instance persist between scenes?")] private bool _isPersistant = false;
        [SerializeField, Tooltip("If new instance is loaded, should this one be destroyed?")] private bool _isReplacable = false;
#if UNITY_EDITOR
        [SerializeField, Tooltip("When adding this component to an object on different scene, should a prefab variant be created to keep those instances separate?")] private bool _createVariants = false;
#endif

        private static T _instance;
        private static readonly object _instanceLock = new object();
        private static bool _quitting = false;

        public static T instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null && !_quitting)
                    {

                        _instance = GameObject.FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            GameObject prefab = Resources.Load<GameObject>($"Singletons/{typeof(T).Name}-{SceneManager.GetActiveScene().name}");
                            if(prefab == null)
                                prefab = Resources.Load<GameObject>($"Singletons/{typeof(T).Name}");
                            if (prefab != null)
                            {
                                _instance = Instantiate(prefab).GetComponent<T>();
                            }
                            else
                            {
                                GameObject go = new GameObject(typeof(T).ToString());
                                _instance = go.AddComponent<T>();
                            }
                        }
                    }

                    return _instance;
                }
            }
        }
        protected virtual void Reset()
        {
            if (gameObject.GetComponentsInChildren<MonoBehaviour>().Where(m => m != this && m.GetType().BaseType.IsGenericType && m.GetType().BaseType.GetGenericTypeDefinition() == typeof(Singleton<>)).Count() > 0
                || gameObject.GetComponentsInParent<MonoBehaviour>().Where(m => m != this && m.GetType().BaseType.IsGenericType && m.GetType().BaseType.GetGenericTypeDefinition() == typeof(Singleton<>)).Count() > 0)
            {
                Debug.LogError("Game object can hold only one singleton");
                DestroyImmediate(this);
                return;
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Singletons"))
                AssetDatabase.CreateFolder("Assets/Resources", "Singletons");
            GameObject prefab = Resources.Load<GameObject>($"Singletons/{GetType().Name}");

            if (prefab == null)
            {
                prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, $"Assets/Resources/Singletons/{GetType().Name}.prefab", InteractionMode.AutomatedAction);
            }
            else
            {
                if (FindObjectsOfType<T>().Count() > 1)
                {
                    Debug.LogError($"There can be only one {GetType().Name} in the scene");
                    DestroyImmediate(this);
                    return;
                }
                else if (PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != prefab)
                {
                    var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    Selection.activeGameObject = go;
                    if((prefab.GetComponent<T>() as Singleton<T>)._createVariants)
                    {
                        PrefabUtility.SaveAsPrefabAssetAndConnect(go, $"Assets/Resources/Singletons/{GetType().Name}-{EditorSceneManager.GetActiveScene().name}.prefab", InteractionMode.AutomatedAction);
                        Debug.LogWarning($"Created a prefab variant for {EditorSceneManager.GetActiveScene().name} scene. If no instance present, this scene will reffer to that variant instead of the original one.");
                    }
                    else
                    {
                        Debug.LogWarning($"Singleton of type {GetType().FullName} already exists in your project. " +
                        $"If you want this instance not to be connected to existing prefab, unpack it. If on top of that you want to make this instance into an original prefab, " +
                        $"delete existing one in \"Resources\\Singletons\" and reset (in context menu) this component");
                    }
                    if (gameObject.GetComponentsInChildren<Behaviour>().Where(m => m.GetType() != typeof(Transform) && m.GetType() != typeof(T)).Count() > 0)
                    {
                        DestroyImmediate(this);
                        return;
                    }
                    else
                    {
                        StartCoroutine(DestroyGO());
                        return;
                    }
                }
            }
        }
        private IEnumerator DestroyGO()
        {
            yield return null;
            DestroyImmediate(gameObject);
        }
        protected virtual void Awake()
        {
#if UNITY_EDITOR
            
            if (!Application.isPlaying) return;
#endif
            if (_instance == null)
            {
                _instance = gameObject.GetComponent<T>();
                if (_isPersistant) DontDestroyOnLoad(gameObject);
            }
            else if (_instance.GetInstanceID() != GetInstanceID())
            {
                if ((_instance as Singleton<T>)._isReplacable) Destroy(_instance.gameObject);
                else Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _quitting = true;
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += HandlePrefabInstanceUpdated;
#endif
        }
        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= HandlePrefabInstanceUpdated;
#endif
        }

#if UNITY_EDITOR
        private void HandlePrefabInstanceUpdated()
        {
            if (PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, false))
            {
                PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
            }
        }
#endif
    }
