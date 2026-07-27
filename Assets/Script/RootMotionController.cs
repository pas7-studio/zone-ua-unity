using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionController : MonoBehaviour
{
    private Animator animator;
    private int enableRootMotionHash;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enableRootMotionHash = Animator.StringToHash("EnableRootMotion");
    }

    private void Update()
    {
        // check if the animation is playing
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            // set EnableRootMotion parameter to true
            animator.SetBool(enableRootMotionHash, true);
        }
        else
        {
            // set EnableRootMotion parameter to false
            animator.SetBool(enableRootMotionHash, false);
        }
    }
}