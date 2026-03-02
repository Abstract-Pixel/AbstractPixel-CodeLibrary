using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public abstract class ValueTypeSO<T> : ScriptableObject where T : struct
{
    [SerializeField] private T currentValue;
    [field: SerializeField] public T StartValue { get; private set; }
    [field: SerializeField] public T MinValue { get; private set; }
    [field: SerializeField] public T MaxValue { get; private set; }
    [SerializeField] bool isResetable;
    public Action OnValueChanged;

    public T CurrentValue
    {
        get => currentValue;
        set
        {
            currentValue = ClampValue(value);
            OnValueChanged?.Invoke();
        }
    }

    protected abstract T ClampValue(T value);


    private void OnEnable()
    {
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged += ResetOnPlayModeExit;
        #endif
        SceneManager.sceneLoaded += ResetOnSceneChange;
    }

    private void OnDisable()
    {
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= ResetOnPlayModeExit;
        #endif
        SceneManager.sceneLoaded -= ResetOnSceneChange;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= ResetOnSceneChange;
    }

    private void OnValidate()
    {
        if (Application.isPlaying || !isResetable) return;
        CurrentValue = StartValue;
    }

    public void Reset()
    {
        CurrentValue = StartValue;
    }

#if UNITY_EDITOR
    void ResetOnPlayModeExit(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            Reset();
        }
    }
#endif

    void ResetOnSceneChange(Scene scene, LoadSceneMode mode)
    {
        Reset();
    }

}
