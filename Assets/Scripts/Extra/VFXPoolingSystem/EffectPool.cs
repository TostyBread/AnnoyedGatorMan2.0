using UnityEngine;
using System.Collections.Generic;

public class EffectPool : MonoBehaviour
{
    // To use the animation VFX on other script, type:
    // EffectPool.Instance.SpawnEffect("Explosion", transform.position, Quaternion.identity);
    public static EffectPool Instance { get; private set; }

    [System.Serializable]
    public class PooledEffect
    {
        public string effectName;
        public GameObject prefab;
        public int initialSize = 10;
    }

    [Header("Effect Pool Setup")]
    public List<PooledEffect> effectsToPool;

    private Dictionary<string, Queue<GameObject>> poolDictionary = new();
    private Dictionary<string, GameObject> prefabLookup = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        foreach (var effect in effectsToPool)
        {
            var queue = new Queue<GameObject>();

            for (int i = 0; i < effect.initialSize; i++)
            {
                GameObject obj = Instantiate(effect.prefab);
                obj.SetActive(false);
                DontDestroyOnLoad(obj);
                queue.Enqueue(obj);
            }

            poolDictionary[effect.effectName] = queue;
            prefabLookup[effect.effectName] = effect.prefab;
        }
    }

    public GameObject SpawnEffect(string effectName, Vector3 position, Quaternion rotation)
    {
        if (!prefabLookup.ContainsKey(effectName))
        {
            Debug.LogWarning($"[EffectPool] Effect '{effectName}' not registered.");
            return null;
        }

        GameObject obj = (poolDictionary.ContainsKey(effectName) && poolDictionary[effectName].Count > 0)
            ? poolDictionary[effectName].Dequeue()
            : CreateNewEffectInstance(effectName);
        //: Instantiate(prefabLookup[effectName]);

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    private GameObject CreateNewEffectInstance(string effectName)
    {
        var obj = Instantiate(prefabLookup[effectName]);
        DontDestroyOnLoad(obj);
        return obj;
    }

    public void ReturnEffect(string effectName, GameObject obj)
    {
        obj.SetActive(false);

        if (!poolDictionary.ContainsKey(effectName))
        {
            poolDictionary[effectName] = new Queue<GameObject>();
        }

        poolDictionary[effectName].Enqueue(obj);
    }

    // Optional: Expand effect pool dynamically
    public void Prewarm(string effectName, int count)
    {
        if (!prefabLookup.TryGetValue(effectName, out var prefab)) return;

        if (!poolDictionary.ContainsKey(effectName))
            poolDictionary[effectName] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab);
            DontDestroyOnLoad(obj);
            obj.SetActive(false);
            poolDictionary[effectName].Enqueue(obj);
        }
    }
}