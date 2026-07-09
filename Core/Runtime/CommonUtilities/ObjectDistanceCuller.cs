using UnityEngine;

namespace AbstractPixel.Core
{
    public class ObjectDistanceCuller : MonoBehaviour
    {
        [SerializeField] GameObject objectToCull;
        [SerializeField] float cullDistance = 10f;

        private CullingGroup cullingGroup;
        private BoundingSphere[] boundingSpheres;

        private void Start()
        {
            cullingGroup = new CullingGroup();
            boundingSpheres = new BoundingSphere[1];

            boundingSpheres[0] = new BoundingSphere(objectToCull.transform.position,1f);

            cullingGroup.targetCamera = Camera.main;
            cullingGroup.SetDistanceReferencePoint(Camera.main.transform);

            cullingGroup.SetBoundingSpheres(boundingSpheres);
            cullingGroup.SetBoundingSphereCount(1);

            float[] distanceBands = new float[] { cullDistance };
            cullingGroup.SetBoundingDistances(distanceBands);

            cullingGroup.onStateChanged += OnStateChanged;


        }

        void OnStateChanged(CullingGroupEvent _stateEvent)
        {
            if(_stateEvent.currentDistance <=0.05f)
            {
                objectToCull.SetActive(true);
            }
            else
            {
                objectToCull.SetActive(false);
            }
        }

        private void OnDestroy()
        {

            if (cullingGroup!=null)
            {
                cullingGroup.onStateChanged -= OnStateChanged;
                cullingGroup.Dispose();
                cullingGroup = null;
            }
            
        }

    }
}
