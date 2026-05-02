using UnityEngine;
using AbstractPixel.Core;
using System.Threading.Tasks;

public class TransitionManager : PersistentSingleton<TransitionManager>
{
    [SerializeField] private TransitionProfile transitionInProfile;
    [SerializeField] private TransitionProfile transitionOutProfile;

    private ITransitionController transitionInController;
    private ITransitionController transitionOutController;

    internal bool IsTransitioning { get; private set; } = false;
    internal bool IsInitialized { get; private set; } = false;

    internal void Initialize(TransitionProfile _transitionInProfile, TransitionProfile _transitionOutProfile)
    {
        transitionInProfile = _transitionInProfile;
        transitionOutProfile = _transitionOutProfile;

        SetTransitionInProfile(transitionInProfile);
        SetTransitionOutProfile(transitionOutProfile);
       
        if(transitionInController != null && transitionOutController != null)
        {
            IsInitialized = true;
        }
    }

    internal async Task PlayTransitionIn()
    {
        if (IsTransitioning || !IsInitialized) return;
        if (transitionInProfile == null || transitionInController == null) return;

        IsTransitioning = true;
        await transitionInController.PlayTransitionIn();
        IsTransitioning = false;
    }

    internal async Task PlayTransitionOut()
    {
        if (IsTransitioning || !IsInitialized) return;
        if (transitionOutProfile == null || transitionOutController == null) return;
        IsTransitioning = true;
        await transitionOutController.PlayTransitionOut();
        IsTransitioning = false;
    }


    internal void SetTransitionInProfile(TransitionProfile _transitionInProfile)
    {
        
        if (transitionInController != null)
        {
            Destroy(transitionInController.gameObject);
        }
        if (_transitionInProfile == null)
        {
            IsInitialized = false;
            return;
        }
        transitionInController = Instantiate(_transitionInProfile.TransitionControllerPrefab).GetComponent<ITransitionController>();
    }

    internal void SetTransitionOutProfile(TransitionProfile _transitionOutProfile)
    {
        if (transitionOutController != null)
        {
            Destroy(transitionOutController.gameObject);
        }
        if(transitionOutProfile == null)
        {
            IsInitialized = false;
            return;
        }
        transitionOutController = Instantiate(_transitionOutProfile.TransitionControllerPrefab).GetComponent<ITransitionController>();
    }
}