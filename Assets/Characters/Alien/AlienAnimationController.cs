using UnityEngine;
using System.Collections;

public class AlienAnimationController : MonoBehaviour
{
    private Animator animator;
    private bool isGreeting = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isGreeting)
        {
            isGreeting = true;
            animator.SetTrigger("Greet");
        }
    }

    private void Update()
    {
        // 判断动画是否播放完毕，回到Idle
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (isGreeting && stateInfo.IsName("Idle"))
        {
            isGreeting = false;
        }
    }
}
