using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    public static EventManager Instance => instance ??= FindAnyObjectByType<EventManager>();
    private Dictionary<Type, List<Action<IVillageEvent>>> eventListeners = new Dictionary<Type, List<Action<IVillageEvent>>>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddListener<T>(Action<T> listener) where T : IVillageEvent
    {
        var type = typeof(T);
        if (!eventListeners.ContainsKey(type)) eventListeners[type] = new List<Action<IVillageEvent>>();

        eventListeners[type].Add((evt) => listener((T)evt));
    }

    public void RemoveListener<T>(Action<T> listener) where T : IVillageEvent
    {
        var type = typeof(T);
        if (!eventListeners.ContainsKey(type)) return;

        eventListeners[type].RemoveAll(action =>
        action.Target == listener.Target &&
        action.Method == listener.Method);
    }

    public void TriggerEvent<T>(T eventData) where T : IVillageEvent
    {
        var type = typeof(T);
        if (!eventListeners.ContainsKey(type)) return;

        foreach (var listener in eventListeners[type])
        {
            listener.Invoke(eventData);
        }
    }
}
