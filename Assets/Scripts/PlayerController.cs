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
    [SerializeField] private float staminaDepletionMultiplier = 10;
    [SerializeField] private float chalkPerPiece = 50;
    
    [SerializeField] private Hold lhInitialHold;
    [SerializeField] private Hold rhInitialHold;
    [SerializeField] private Hold lfInitialHold;
    [SerializeField] private Hold rfInitialHold;
    
    private FullBodyBipedIK _ikController;
    private CustomInteractionSystem _interactionSystem;
    private Rigidbody _rigidbody;
    private Dictionary<KeyCode, Hold> _holdsInArea = new ();
    private Dictionary<Transform, FullBodyBipedEffector> _limbs = new ();
    private float _stamina;
    private float _chalk;
    private int _points;
    private int _highestPoints;
    private bool _initialPositionSet;
    private bool _loaded;
    private bool _switchingToHold;

    private List<KeyCode> _availableKeys = new ()
    {
        KeyCode.A, KeyCode.Q, KeyCode.Z,
        KeyCode.S, KeyCode.W, KeyCode.X,
        KeyCode.D, KeyCode.E, KeyCode.C,
        KeyCode.F, KeyCode.R, KeyCode.V,
        
        KeyCode.G, KeyCode.T, KeyCode.B,
        KeyCode.H, KeyCode.Y, KeyCode.N,
        KeyCode.J, KeyCode.U, KeyCode.M,
        KeyCode.K, KeyCode.I, KeyCode.L,
    };
    
    private void Start()
    {
        _loaded = false;
        _switchingToHold = false;
        _highestPoints = PlayerPrefs.GetInt("points");
        _stamina = maxStamina;
        _chalk = maxChalk;
        _ikController = GetComponent<FullBodyBipedIK>();
        _interactionSystem = GetComponent<CustomInteractionSystem>();
        _rigidbody = GetComponent<Rigidbody>();
        _limbs = new Dictionary<Transform, FullBodyBipedEffector>
        {
            { _ikController.references.leftHand, FullBodyBipedEffector.LeftHand },
            { _ikController.references.rightHand, FullBodyBipedEffector.RightHand },
            { _ikController.references.leftFoot, FullBodyBipedEffector.LeftFoot },
            { _ikController.references.rightFoot, FullBodyBipedEffector.RightFoot },
        };
        
        _holdsInArea = GetHoldsInArea(detectionRadius);
        _ikController.enabled = true;
        _rigidbody.isKinematic = true;
        _interactionSystem.onInteractionComplete.AddListener(() => UIManager.instance.CloseLoadingScreen());
        _interactionSystem.onInteractionComplete.AddListener(() => AudioManager.instance.PlaySongsRandomly());
        _interactionSystem.onInteractionComplete.AddListener(() => _loaded = true);
        _interactionSystem.onInteractionComplete.AddListener(() => _switchingToHold = false);
        _interactionSystem.onInteractionBegin.AddListener(() => _holdsInArea = GetHoldsInArea(detectionRadius));
        
        UIManager.instance.SetHighschore(_highestPoints);
    }

    private void Update()
    {
        if (!_initialPositionSet)
        {
            if (lhInitialHold) SwitchToHold(lhInitialHold, true);
            if (rhInitialHold) SwitchToHold(rhInitialHold, true);
            if (lfInitialHold) SwitchToHold(lfInitialHold, true);
            if (rfInitialHold) SwitchToHold(rfInitialHold, true);

            _initialPositionSet = true;
        }
        
        foreach (KeyCode key in _holdsInArea.Keys)
        {
            if (Input.GetKeyDown(key) && _loaded && !_switchingToHold)
            {
                _switchingToHold = true;
                SwitchToHold(_holdsInArea[key]);
            }
        }

        transform.position = RecalculateBodyPosition(_ikController.references.leftFoot.position, _ikController.references.rightFoot.position, _ikController.references.leftHand.position, _ikController.references.rightHand.position);
        if (_loaded)
        {
            UseStamina();
            CheckFailure();
        }
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

    private void SwitchToHold(Hold hold, bool initial = false)
    {
        List<Transform> available_limbs = _limbs.Keys.ToList().Except(hold.GetAttachedLimbs()).ToList();
        Transform closest_limb = GetClosestLimb(hold.transform, available_limbs);

        if (!closest_limb) return;

        foreach (Hold close_hold in _holdsInArea.Values) close_hold.DetachLimb(closest_limb);
        hold.AttachLimb(closest_limb);

        if (_stamina <= 3 * maxStamina / 5)
        {
            float distance = Vector3.Distance(closest_limb.position, hold.transform.position);
            _stamina -= distance * staminaDepletionMultiplier;
        }
        
        _interactionSystem.StartTransition(hold.transform.GetChild(0), _limbs[closest_limb]);
        
        if (!initial)
        {
            UseChalk(hold.GetChalkConsumption());
            AwardPoints(hold.GetPointsAwarded());
        }
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

    private Vector3 RecalculateBodyPosition(Vector3 left_foot_position, Vector3 right_foot_position, Vector3 left_hand_position, Vector3 right_hand_position)
    {
        Vector3 new_position = left_foot_position + right_foot_position + left_hand_position + right_hand_position;
        new_position /= 4f;
        new_position -= detectionOffset;

        return new_position;
    }

    private void UseStamina()
    {
        float stamina_consumption = staminaDecreaseSpeed * Time.deltaTime;
        if (_stamina <= maxStamina / 2f && _chalk > 0) stamina_consumption *= -0.5f;
        if (_chalk <= 0) stamina_consumption *= 2;

        _stamina -= stamina_consumption;
        if (_stamina < 0) _stamina = 0;
        
        UIManager.instance.SetStamina(_stamina, maxStamina);

        if (_stamina <= 0) Fall();
    }

    private void Fall()
    {
        foreach (FullBodyBipedEffector limb in _limbs.Values)
        {
            _interactionSystem.Detach(limb);
        }
        _ikController.enabled = false;
        _rigidbody.isKinematic = false;
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

        UIManager.instance.SetScore(_points);
        if (_points > _highestPoints) UIManager.instance.SetHighschore(_points);
    }

    private void CheckFailure()
    {
        if (transform.position.y >= GameManager.instance.failureHeight)
            return;
            
        if (_points > _highestPoints) PlayerPrefs.SetInt("points", _points);

        UIManager.instance.OpenFailScreen(_points);
        GameManager.instance.enabled = false;
        enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "chalk") return;

        _chalk += chalkPerPiece;
        if (_chalk > maxChalk) _chalk = maxChalk;
        
        Destroy(other.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + detectionOffset, detectionRadius);

        if (_ikController != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(RecalculateBodyPosition(_ikController.references.leftFoot.position, _ikController.references.rightFoot.position, _ikController.references.leftHand.position, _ikController.references.rightHand.position), .5f);
        }
    }
}
