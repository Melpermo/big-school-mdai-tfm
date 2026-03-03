using UnityEngine;

public static class Bootstrapper
{
    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        // If Systems already exists, do nothing.
        if (Object.FindFirstObjectByType<SystemsRoot>() != null)
        {
            return;
        }

        var prefab = Resources.Load<GameObject>("Systems");
        if (prefab == null)
        {
            Debug.LogError("[Bootstrapper] Could not load 'Systems' prefab from Resources.");
            return;
        }

        var instance = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(instance);
    }
    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Systems")));
    }*/
}