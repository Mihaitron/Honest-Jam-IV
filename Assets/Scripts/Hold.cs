using System.Collections.Generic;
using RootMotion.FinalIK;
using TMPro;
using UnityEngine;

public class Hold : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private float chalkConsumption = 10;
    [SerializeField] private int pointsAwarded = 100;

    private InteractionObject _interactionObject;
    private List<Transform> _attachedLimbs = new ();

    private void Start()
    {
        _interactionObject = GetComponent<InteractionObject>();
    }

    private void FixedUpdate()
    {
        Vector3 new_position = transform.localPosition;
        new_position.y -= Time.fixedDeltaTime * speed;
        transform.localPosition = new_position;
    }

    public void SetKey(KeyCode key)
    {
        keyText.gameObject.SetActive(true);
        keyText.text = key.ToString();
    }
    
    public void UnsetKey()
    {
        keyText.text = "";
        keyText.gameObject.SetActive(false);
    }

    public InteractionObject GetInteractionObject()
    {
        return _interactionObject;
    }

    public void AttachLimb(Transform limb)
    {
        _attachedLimbs.Add(limb);
    }

    public void DetachLimb(Transform limb)
    {
        _attachedLimbs.Remove(limb);
    }

    public List<Transform> GetAttachedLimbs()
    {
        return _attachedLimbs;
    }

    public float GetChalkConsumption()
    {
        return chalkConsumption;
    }

    public int GetPointsAwarded()
    {
        return pointsAwarded;
    }

    public void SetSpeed(float new_speed)
    {
        speed = new_speed;
    }
    
    public void Accelerate(float acceleration)
    {
        speed += acceleration;
    }
}
