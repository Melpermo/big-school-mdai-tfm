using UnityEngine;

public class CleanMemoryTest : MonoBehaviour
{

    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            Debug.Log("Do something special here");

        if (Application.platform == RuntimePlatform.OSXPlayer)
            Debug.Log("Do something special here");

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            Debug.Log("Do something special here");
    }

   
}
