using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using Random = UnityEngine.Random;

public static class RectTransformExtensions
{
    public static bool Overlaps(this RectTransform a, RectTransform b)
    {
        return a.GetWorldSapceRect().Overlaps(b.GetWorldSapceRect());
    }

    public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
    {
        return a.GetWorldSapceRect().Overlaps(b.GetWorldSapceRect(), allowInverse);
    }

    static Rect GetWorldSapceRect(this RectTransform rt)
    {
        var r = rt.rect;
        r.center = rt.TransformPoint(r.center);
        //r.size = rt.TransformVector(new Vector2(rt.rect.width, rt.rect.height));
        return r;
    }
}

public static class Commons
{
    public static void SetActiveWithChecker(this GameObject gameObject, bool active)
    {
        if (gameObject.activeSelf != active)
        {
            gameObject.SetActive(active);
        }
    }

    public static Transform FindFirstChildWithName(this Transform t, string name,
        bool includeInactiveTransform = false)
    {
        Queue<Transform> q = new Queue<Transform>();
        q.Enqueue(t);
        while (q.Count > 0)
        {
            Transform u = q.Dequeue();
            if (u.name == name && (includeInactiveTransform || u.gameObject.activeInHierarchy)) return u;
            //visits u
            for (int i = 0; i < u.childCount; ++i)
                q.Enqueue(u.GetChild(i));
        }

        return null;
    }

    public static T PassTo<T>(this T obj, System.Action<T> action)
    {
        action.Invoke(obj);
        return obj;
    }

    public static bool IsInSet<T>(this T t, params T[] set)
    {
        int length = set.Length;
        for (int i = 0; i < length; ++i)
        {
            if (t.Equals(set[i])) return true;
        }

        return false;
    }

    public static T IfNotNull<T>(this T obj, System.Action<T> action)
    {
        if (obj != null && !obj.Equals(null))
            if (action != null)
                action.Invoke(obj);
        return obj;
    }

    public static Vector3 alterMember(this Vector3 v, float x = float.NaN, float y = float.NaN, float z = float.NaN)
    {
        Vector3 r = v;
        if (!float.IsNaN(x)) r.x = x;
        if (!float.IsNaN(y)) r.y = y;
        if (!float.IsNaN(z)) r.z = z;
        return r;
    }

    public static Color alterMember(this Color c, float r = float.NaN, float g = float.NaN, float b = float.NaN,
        float a = float.NaN)
    {
        Color ret = new Color(c.r, c.g, c.b, c.a);
        if (!float.IsNaN(r)) ret.r = r;
        if (!float.IsNaN(g)) ret.g = g;
        if (!float.IsNaN(b)) ret.b = b;
        if (!float.IsNaN(a)) ret.a = a;
        return ret;
    }

    public static Coroutine SetTimeout(this MonoBehaviour monoBehaviour, Action callback, float delay)
    {
        return monoBehaviour.StartCoroutine(Timeout(delay, callback));
    }

    public static Coroutine SetTimeoutUnrealTime(this MonoBehaviour monoBehaviour, Action callback, float delay)
    {
        return monoBehaviour.StartCoroutine(TimeoutUnrealTime(delay, callback));
    }

    private static IEnumerator TimeoutUnrealTime(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback();
    }

    private static IEnumerator Timeout(float delay, Action callback)
    {
        yield return new WaitForSecondsRealtime(delay);
        callback();
    }

    public static Coroutine SetInterval(this MonoBehaviour monoBehaviour, Action callback, float delay)
    {
        return monoBehaviour.StartCoroutine(Interval(delay, callback));
    }

    private static IEnumerator Interval(float delay, Action callback)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            callback();
        }
    }

    public static Coroutine WaitEndFrame(this MonoBehaviour monoBehaviour, Action callback)
    {
        return monoBehaviour.StartCoroutine(IWaitEndFrame(callback));
    }

    public static Coroutine WaitUntil(this MonoBehaviour monoBehaviour, Action callback, Func<bool> condition)
    {
        return monoBehaviour.StartCoroutine(IWaitUntil(callback, condition));
    }

    static IEnumerator IWaitUntil(Action callback, Func<bool> condition)
    {
        while (!condition())
        {
            yield return null;
        }

        callback();
    }

    static IEnumerator IWaitEndFrame(Action callback)
    {
        yield return new WaitForEndOfFrame();
        callback();
    }


    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static NestedList<T> ToNestedList<T>(this List<T> origin)
    {
        var temp = new NestedList<T>();
        temp.List = origin;
        return temp;
    }
}

[Serializable]
public class ValueKeyPair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public ValueKeyPair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

[Serializable]
public class ValueKeyPair<TKey, T1, T2>
{
    public TKey Key;
    public T1 V1;
    public T2 V2;

    public ValueKeyPair(TKey key, T1 v1, T2 v2)
    {
        Key = key;
        V1 = v1;
        V2 = v2;
    }
}

[Serializable]
public class SerializedDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [NonSerialized] Dictionary<TKey, TValue> map = new Dictionary<TKey, TValue>();
    [SerializeField] List<ValueKeyPair<TKey, TValue>> data = new List<ValueKeyPair<TKey, TValue>>();


    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        map.Clear();
        for (int i = 0; i < this.data.Count; i++)
        {
            var current = data[i];
            if (map.ContainsKey(current.Key))
            {
                map[current.Key] = current.Value;
            }
            else map.Add(current.Key, current.Value);
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        //data.Clear();

        //foreach (var item in map)
        //{
        //    data.Add(new ValueKeyPair<TKey, TValue>(item.Key, item.Value));
        //}
    }

    public TValue this[TKey key]
    {
        get { return map[key]; }
        set { map[key] = value; }
    }

    public int Count => map.Count;

    public bool ContainsKey(TKey key) => map.ContainsKey(key);

    public void Clear() => map.Clear();

    public IEnumerable<KeyValuePair< TKey, TValue>> AsEnumerable() => map.AsEnumerable();
}

[Serializable]
public struct NestedArray<T>
{
    public T[] List;

    public T this[int index]
    {
        get => List[index];
    }
}

[Serializable]
public struct NestedList<T>
{
    public List<T> List;

    public T this[int index]
    {
        get => List[index];
    }


    public int Count => List.Count;

    public IEnumerator<T> GetEnumerator()
    {
        return List.GetEnumerator();
    }
}