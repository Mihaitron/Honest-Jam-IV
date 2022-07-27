using System;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private Vector3 detectionOffset;
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float maxChalk = 100;
    [SerializeField] private float staminaDecreaseSpeed = 2;

    private InteractionObject _lhInteractionObject;
    private InteractionSystem _interactionSystem;
    private FullBodyBipedIK _ikController;
    private Dictionary<KeyCode, Hold> _holdsInArea = new ();
    private Dictionary<Transform, FullBodyBipedEffector> _limbs = new ();
    private float _stamina;
    private float _chalk;
    private int _points;

    private List<KeyCode> _availableKeys = new ()
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
        _stamina = maxStamina;
        _chalk = maxChalk;
        _interactionSystem = GetComponent<InteractionSystem>();
        _ikController = GetComponent<FullBodyBipedIK>();
        _limbs = new Dictionary<Transform, FullBodyBipedEffector>
        {
            { _ikController.references.leftHand, FullBodyBipedEffector.LeftHand },
            { _ikController.references.rightHand, FullBodyBipedEffector.RightHand },
            { _ikController.references.leftFoot, FullBodyBipedEffector.LeftFoot },
            { _ikController.references.rightFoot, FullBodyBipedEffector.RightFoot },
        };
        
        _holdsInArea = GetHoldsInArea(detectionRadius);

        _interactionSystem.OnInteractionStop += (effector, interaction_object) => transform.position = RecalculateBodyPosition(_ikController.references.leftFoot.position, _ikController.references.rightFoot.position);
        _interactionSystem.OnInteractionStop += (effector, interaction_object) => _holdsInArea = GetHoldsInArea(detectionRadius);
    }

    private void Update()
    {
        foreach (KeyCode key in _holdsInArea.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                SwitchToHold(_holdsInArea[key]);
            }
        }
        
        UseStamina();
    }

    private Dictionary<KeyCode, Hold> GetHoldsInArea(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + detectionOffset, radius, Utils.GetLayerMaskFromLayerIndex(31));
        Dictionary<KeyCode, Hold> holds = new Dictionary<KeyCode, Hold>();
        List<KeyCode> available_keys = new List<KeyCode>(_availableKeys);

        foreach (Hold hold in FindObjectsOfType<Hold>())
        {
            hold.UnsetKey();
        }

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
        List<Transform> available_limbs = _limbs.Keys.ToList().Except(hold.GetAttachedLimbs()).ToList();
        Transform closest_limb = GetClosestLimb(hold.transform, available_limbs);

        if (!closest_limb) return;

        foreach (Hold close_hold in _holdsInArea.Values) close_hold.DetachLimb(closest_limb);
        hold.AttachLimb(closest_limb);
        
        _interactionSystem.StartInteraction(_limbs[closest_limb], interaction_object, true);
        
        UseChalk(hold.GetChalkConsumption());
        AwardPoints(hold.GetPointsAwarded());
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

    private Vector3 RecalculateBodyPosition(Vector3 left_foot_position, Vector3 right_foot_position)
    {
        Vector3 new_position = left_foot_position + right_foot_position;

        return new_position / 2;
    }

    private void UseStamina()
    {
        float stamina_consumption = staminaDecreaseSpeed * Time.deltaTime;
        if (_chalk <= 0) stamina_consumption *= 2;

        _stamina -= stamina_consumption;
        if (_stamina < 0) _stamina = 0;
        
        UIManager.instance.SetStamina(_stamina, maxStamina);
    }

    private void UseChalk(float chalk_consumption)
    {
        _chalk -= chalk_consumption;
        if (_chalk < 0) _chalk = 0;
        
        UIManager.instance.SetChalk(_chalk, maxChalk);
    }

    private void AwardPoints(int points_awarded)
    {
        _points += points_awarded;

        UIManager.instance.SetPoints(_points);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + detectionOffset, detectionRadius);
        
        if (_limbs.Count > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(RecalculateBodyPosition(_ikController.references.leftFoot.position, _ikController.references.rightFoot.position), .1f);
        }
    }
}
