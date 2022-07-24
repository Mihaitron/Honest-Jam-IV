using System;
using RootMotion.FinalIK;
using TMPro;
using UnityEngine;

public class Hold : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private TMP_Text keyText;
    
    private InteractionObject _interactionObject;

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
}
