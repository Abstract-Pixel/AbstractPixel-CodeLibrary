using System.Collections;
using UnityEngine;

public class CoroutineWorkHorse : MonoBehaviour
{
    public static CoroutineWorkHorse Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static Coroutine StartWork(IEnumerator coroutine)
    {
        if (Instance == null)
        {
            (new GameObject("CoroutineWorkHorse")).AddComponent<CoroutineWorkHorse>();
        }
        return Instance.StartCoroutine(coroutine);
    }
}