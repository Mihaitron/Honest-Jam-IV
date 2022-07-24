using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private Vector3 detectionOffset;

    private InteractionObject _lhInteractionObject;
    private InteractionSystem _interactionSystem;
    private FullBodyBipedIK _ikController;
    private Dictionary<KeyCode, Hold> _holdsInArea;
    private Dictionary<Transform, FullBodyBipedEffector> _limbs;

    private List<KeyCode> _availableKeys = new List<KeyCode>
    {
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.F,
        KeyCode.G,
        KeyCode.H,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
    };

    private void Start()
    {
        _interactionSystem = GetComponent<InteractionSystem>();
        _ikController = GetComponent<FullBodyBipedIK>();
        _holdsInArea = GetHoldsInArea(detectionRadius);
        _limbs = new Dictionary<Transform, FullBodyBipedEffector>
        {
            { _ikController.references.leftHand, FullBodyBipedEffector.LeftHand },
            { _ikController.references.rightHand, FullBodyBipedEffector.RightHand },
            { _ikController.references.leftFoot, FullBodyBipedEffector.LeftFoot },
            { _ikController.references.rightFoot, FullBodyBipedEffector.RightFoot },
        };
    }

    private void Update()
    {
        foreach (KeyCode key in _holdsInArea.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                SwitchToHold(_holdsInArea[key]);
                // _holdsInArea = GetHoldsInArea(detectionRadius);
            }
        }
    }

    private Dictionary<KeyCode, Hold> GetHoldsInArea(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + detectionOffset, radius, Utils.GetLayerMaskFromLayerIndex(31));
        Dictionary<KeyCode, Hold> holds = new Dictionary<KeyCode, Hold>();
        List<KeyCode> available_keys = new List<KeyCode>(_availableKeys);

        foreach (Collider collider in colliders)
        {
            Hold hold = collider.GetComponent<Hold>();
            KeyCode key = available_keys[Random.Range(0, available_keys.Count)];

            holds.Add(key, hold);
            hold.SetKey(key);
            available_keys.Remove(key);
        }

        return holds;
    }

    private void SwitchToHold(Hold hold)
    {
        InteractionObject interaction_object = hold.GetInteractionObject();
        Transform closestLimb = GetClosestLimb(hold.transform, _limbs.Keys.ToList());
        _interactionSystem.StartInteraction(_limbs[closestLimb], interaction_object, true);
    }

    private Transform GetClosestLimb(Transform obj, List<Transform> limbs)
    {
        Transform closest_limb = null;
        float closest_distance = float.MaxValue;

        foreach (Transform limb in limbs)
        {
            float limb_distance = Vector3.Distance(obj.position, limb.position);

            if (limb_distance < closest_distance)
            {
                closest_distance = limb_distance;
                closest_limb = limb;
            }
        }
        
        return closest_limb;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + detectionOffset, detectionRadius);
    }
}
