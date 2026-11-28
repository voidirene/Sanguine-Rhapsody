using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MoveWithAnimation : MonoBehaviour // MUST be placed in the same gameobject as the animator
{
    [SerializeField] private CharacterController controller;
    
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    void OnAnimatorMove()
    {
        if (IsUsingRootMotion())
            controller.Move(_animator.deltaPosition);
    }

    bool IsUsingRootMotion()
    {
        AnimatorStateInfo info = _animator.IsInTransition(0)? _animator.GetNextAnimatorStateInfo(0) : _animator.GetCurrentAnimatorStateInfo(0);
        return info.IsTag("RootMotion");
    }
}
