using UnityEngine;

public class Hold : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    
    private void FixedUpdate()
    {
        Vector3 new_position = transform.localPosition;
        new_position.y -= Time.fixedDeltaTime * speed;
        transform.localPosition = new_position;
    }
}
