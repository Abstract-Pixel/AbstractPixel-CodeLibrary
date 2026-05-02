using UnityEngine;

[CreateAssetMenu(fileName = "TransitionProfile", menuName = "Utility/SceneRelated/TransitionProfile", order = 1)]
public abstract class TransitionProfile : ScriptableObject
{
    [SerializeField] public GameObject TransitionControllerPrefab;

}