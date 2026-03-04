using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual bool IsPersistent => false;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (IsPersistent)
            DontDestroyOnLoad(gameObject);

        OnInit();
    }

    protected virtual void OnInit() { }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
    }
}

public abstract class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
{
    protected override bool IsPersistent => true;
}