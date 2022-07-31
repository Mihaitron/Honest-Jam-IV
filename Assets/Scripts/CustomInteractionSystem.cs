using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;

public class CustomInteractionSystem : MonoBehaviour
{
    [SerializeField] private float interactionSpeed = 1f;
    
    private FullBodyBipedIK _ikController;
    
    [HideInInspector] public UnityEvent onInteractionComplete;

    private void Start()
    {
        _ikController = GetComponent<FullBodyBipedIK>();
    }

    public void StartTransition(Transform target, FullBodyBipedEffector type)
    {
        IKEffector effector;

        switch (type)
        {
            case FullBodyBipedEffector.LeftHand:
                effector = _ikController.solver.leftHandEffector;
                break;
            case FullBodyBipedEffector.RightHand:
                effector = _ikController.solver.rightHandEffector;
                break;
            case FullBodyBipedEffector.LeftFoot:
                effector = _ikController.solver.leftFootEffector;
                break;
            case FullBodyBipedEffector.RightFoot:
                effector = _ikController.solver.rightFootEffector;
                break;
            default:
                effector = null;
                break;
        }
        
        if (effector != null) StartCoroutine(TransitionToTarget(effector, target));
    }

    public void Detach(FullBodyBipedEffector type)
    {
        IKEffector effector;
        
        switch (type)
        {
            case FullBodyBipedEffector.LeftHand:
                effector = _ikController.solver.leftHandEffector;
                break;
            case FullBodyBipedEffector.RightHand:
                effector = _ikController.solver.rightHandEffector;
                break;
            case FullBodyBipedEffector.LeftFoot:
                effector = _ikController.solver.leftFootEffector;
                break;
            case FullBodyBipedEffector.RightFoot:
                effector = _ikController.solver.rightFootEffector;
                break;
            default:
                effector = null;
                break;
        }

        if (effector != null) StartCoroutine(DetachEffector(effector));
    }
    
    private IEnumerator TransitionToTarget(IKEffector effector, Transform target)
    {
        if (!effector.target)
        {
            while (effector.positionWeight > 0f)
            {
                effector.positionWeight -= Time.deltaTime * interactionSpeed;
                yield return new WaitForEndOfFrame();
            }
        }
        
        effector.positionWeight = 0f;
        effector.target = target;

        while (effector.positionWeight < 1f)
        {
            effector.positionWeight += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        effector.positionWeight = 1f;
        
        onInteractionComplete.Invoke();
    }

    private IEnumerator DetachEffector(IKEffector effector)
    {
        effector.target = null;
        yield return new WaitForEndOfFrame();
    }
}
