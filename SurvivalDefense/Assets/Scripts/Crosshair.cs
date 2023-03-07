using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private float gunAccuracy; //ũ�ν���� ���¿� ���� ���� ��Ȯ�� 

    //ũ�ν���� ��Ȱ���� ���� �θ� ��ü 
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

    public void FireAnimation() //���� �߻��� ��, ũ�ν���� ���� 
    {
        if (animator.GetBool("Walking")) //�Ȱ��ִ� ���¿��� 
        {
            animator.SetTrigger("Walk_Fire");
        }
        else if (animator.GetBool("Crouching")) //��ũ���� �ִ� ���¿��� 
        {
            animator.SetTrigger("Crouch_Fire");
        }
        else //������ �ִ� ���� 
        {
            animator.SetTrigger("Idle_Fire");
        }

    }

    public float getAccuracy() //ũ�ν���� ��Ȯ�� 
    {
        if (animator.GetBool("Walking")) //�Ȱ��ִ� ���¿��� 
        {
            gunAccuracy = 0.06f;
        }
        else if (animator.GetBool("Crouching")) //��ũ���� �ִ� ���¿��� 
        {
            gunAccuracy = 0.015f;
        }
        else if (theGunController.GetFineSightMode())//������ 
        {
            gunAccuracy = 0.01f;
        }
        else //������ �ִ� ���� (��ũ���� �ִ� ���º��� ��Ȯ���� �� ���ƾ� ��)
        {
            gunAccuracy = 0.035f;
        }

        return gunAccuracy;
    }
}
