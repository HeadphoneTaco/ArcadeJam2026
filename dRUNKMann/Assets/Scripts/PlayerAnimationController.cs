using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private BasicMovementScript movementScript;
    [SerializeField] private string tumbleParameterName = "IsTumbling";

    private int tumbleParameterHash;
    private bool hasTumbleParameter;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<BasicMovementScript>();
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (movementScript == null)
        {
            movementScript = GetComponent<BasicMovementScript>();
        }

        tumbleParameterHash = Animator.StringToHash(tumbleParameterName);
        hasTumbleParameter = HasAnimatorBool(tumbleParameterHash);
    }

    private void Update()
    {
        if (animator == null || movementScript == null || !hasTumbleParameter)
        {
            return;
        }

        animator.SetBool(tumbleParameterHash, movementScript.isPlayerRagdoll);
    }

    private bool HasAnimatorBool(int parameterHash)
    {
        if (animator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == parameterHash && parameter.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }

        Debug.LogWarning($"{name} needs a bool animator parameter named '{tumbleParameterName}'.", this);
        return false;
    }
}
