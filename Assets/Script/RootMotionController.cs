using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class RootMotionController : MonoBehaviour
{
    private static readonly int EnableRootMotionHash =
        Animator.StringToHash("EnableRootMotion");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        bool animationIsPlaying =
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;

        animator.SetBool(EnableRootMotionHash, animationIsPlaying);
    }
}
