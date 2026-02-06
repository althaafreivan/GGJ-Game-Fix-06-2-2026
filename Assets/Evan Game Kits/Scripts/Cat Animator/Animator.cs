using UnityEngine;

public class catAnimator : MonoBehaviour
{
    private Animator animator;
    private static readonly int speedHash = Animator.StringToHash("isMove");
    private static readonly int runHash = Animator.StringToHash("isRun");
    private static readonly int jumpHash = Animator.StringToHash("onJump");

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
    }


    public void onMove(bool val) => animator.SetBool(speedHash, val);


    public void onRun(bool val) => animator.SetBool(runHash, val);

    public void onJump(bool val) => animator.SetTrigger(jumpHash);

    public void onPause(bool val) => animator.speed = val? 0 : 1;
}
