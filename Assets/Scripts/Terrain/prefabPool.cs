using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PrefabPool : MonoBehaviour
{
    [System.Serializable]
    private struct PrewarmEntry
    {
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(0)] private int count;

        public GameObject Prefab => prefab;
        public int Count => count;
    }

    [Header("Prewarm (Optional)")]
    [SerializeField] private PrewarmEntry[] prewarm;

    [Header("Hierarchy")]
    [Tooltip("If not set, a child named 'PoolRoot' is created.")]
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<GameObject, Stack<GameObject>> pools = new();
    private readonly Dictionary<GameObject, Transform> poolContainers = new();

    private void Awake()
    {
        if (poolRoot == null)
        {
            Transform existing = transform.Find("PoolRoot");
            if (existing != null) poolRoot = existing;
            else
            {
                GameObject root = new GameObject("PoolRoot");
                root.transform.SetParent(transform, false);
                poolRoot = root.transform;
            }
        }

        Prewarm();
    }

    private void Prewarm()
    {
        if (prewarm == null) return;

        for (int i = 0; i < prewarm.Length; i++)
        {
            GameObject prefab = prewarm[i].Prefab;
            int count = prewarm[i].Count;

            if (prefab == null || count <= 0) continue;

            for (int n = 0; n < count; n++)
            {
                GameObject obj = CreateNew(prefab);
                Release(obj);
            }
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null) return null;

        Stack<GameObject> stack = GetStack(prefab);
        GameObject obj = stack.Count > 0 ? stack.Pop() : CreateNew(prefab);

        PooledObject marker = obj.GetComponent<PooledObject>();
        if (marker != null)
            marker.ResetToBaseScale();

        Transform t = obj.transform;
        t.SetParent(parent, worldPositionStays: true);
        t.SetPositionAndRotation(position, rotation);

        obj.SetActive(true);
        return obj;
    }

    public void Release(GameObject obj)
    {
        if (obj == null) return;

        PooledObject marker = obj.GetComponent<PooledObject>();
        if (marker == null || marker.Prefab == null)
        {
            Destroy(obj);
            return;
        }

        GameObject prefab = marker.Prefab;

        obj.SetActive(false);

        Transform container = GetOrCreateContainer(prefab);
        obj.transform.SetParent(container, worldPositionStays: false);

        GetStack(prefab).Push(obj);
    }

    private Stack<GameObject> GetStack(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>(32);
            pools.Add(prefab, stack);
        }
        return stack;
    }

    private Transform GetOrCreateContainer(GameObject prefab)
    {
        if (poolContainers.TryGetValue(prefab, out Transform t) && t != null)
            return t;

        GameObject go = new GameObject(prefab.name + "_Pool");
        go.transform.SetParent(poolRoot, false);
        poolContainers[prefab] = go.transform;
        return go.transform;
    }

    private GameObject CreateNew(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);

        PooledObject marker = obj.GetComponent<PooledObject>();
        if (marker == null) marker = obj.AddComponent<PooledObject>();
        marker.SetPrefab(prefab);

        return obj;
    }

    private sealed class PooledObject : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector3 baseLocalScale = Vector3.one;

        public GameObject Prefab => prefab;

        public void SetPrefab(GameObject p)
        {
            prefab = p;
            baseLocalScale = transform.localScale;
        }

        public void ResetToBaseScale()
        {
            transform.localScale = baseLocalScale;
        }
    }
}
