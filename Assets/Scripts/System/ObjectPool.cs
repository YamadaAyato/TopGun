using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly Queue<T> _pool = new();
    private readonly T _instance;
    private readonly Transform _parent;

    public ObjectPool(T instance, Transform parent, int initCount)
    {
        _instance = instance; 
        _parent = parent;

        for (int i = 0; i < initCount; i++)
        {
            Create();
        }
    }

    public T Get()
    {
        if (_pool.Count == 0)
            Create();

        T obj = _pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }

    private T Create()
    {
        T obj = Object.Instantiate(_instance, _parent);
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
        return obj;
    }
}
