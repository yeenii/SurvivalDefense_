using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private float gunAccuracy; //크로스헤어 상태에 따른 총의 정확도 

    //크로스헤어 비활성를 위한 부모 객체 
    [SerializeField]
    private GameObject go_crosshairHUD;

    [SerializeField]
    private GunController theGunController;

    public void WalkingAnimation(bool _flag)
    {
        animator.SetBool("Walking", _flag);
    }

    public void RunningAnimation(bool _flag)
    {
        animator.SetBool("Running",_flag);
    }

    public void CrouchingAnimation(bool _flag)
    {
        animator.SetBool("Crouching", _flag);
    }

    public void FineSightAnimation(bool _flag)
    {
        animator.SetBool("FineSight", _flag);
    }

    public void FireAnimation() //총을 발사할 때, 크로스헤어 변경 
    {
        if (animator.GetBool("Walking")) //걷고있는 상태에서 
        {
            animator.SetTrigger("Walk_Fire");
        }
        else if (animator.GetBool("Crouching")) //웅크리고 있는 상태에서 
        {
            animator.SetTrigger("Crouch_Fire");
        }
        else //가만히 있는 상태 
        {
            animator.SetTrigger("Idle_Fire");
        }

    }

    public float getAccuracy() //크로스헤어 정확도 
    {
        if (animator.GetBool("Walking")) //걷고있는 상태에서 
        {
            gunAccuracy = 0.06f;
        }
        else if (animator.GetBool("Crouching")) //웅크리고 있는 상태에서 
        {
            gunAccuracy = 0.015f;
        }
        else if (theGunController.GetFineSightMode())//정조준 
        {
            gunAccuracy = 0.01f;
        }
        else //가만히 있는 상태 (웅크리고 있는 상태보다 정확도가 안 좋아야 함)
        {
            gunAccuracy = 0.035f;
        }

        return gunAccuracy;
    }
}
