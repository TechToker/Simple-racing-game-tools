using UnityEngine;
using System.Collections;

public class MBSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_Instance;
    private static object m_Lock = new object();
    private static bool s_ApplicationStopping = false;

    public static T Instance()
    {
        lock (m_Lock)
        {
            if (s_Instance == null)
            {
                T[] objects = FindObjectsOfType<T>();
                if (objects.Length > 0)
                {
                    if (objects.Length > 1)
                    {
                        Debug.LogError("MBSingleton: Critical Error! There are more than one instance of" +
                                       typeof(T).ToString());
                    }

                    s_Instance = objects[0];
                    Debug.Log("MBSingleton: Using instance already created: " + s_Instance.gameObject.name);
                }
                else if (!s_ApplicationStopping)
                {
                    GameObject singleton = new GameObject();
                    s_Instance = singleton.AddComponent<T>();
                    singleton.name = typeof(T).ToString();
                    Debug.Log(
                        "MBSingleton: An instance of " + typeof(T) + " is needed in the scene, so it was created.");
                }
            }

            return s_Instance;
        }
    }

    private void OnDestroy()
    {
        s_Instance = null;
    }

    protected virtual void OnApplicationQuit()
    {
        s_ApplicationStopping = true;
    }
}