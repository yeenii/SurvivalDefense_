using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    //���� ������ Hand�� Ÿ�� ���� 
    [SerializeField]
    private Hand currentHand;

    //������
    private bool isAttack = false;
    private bool isSwing = false;

    private RaycastHit hitInfo; //RaycastHit : Raycast�� ���� �༮�� ������ ���� �� �ִ� �� 


    // Update is called once per frame
    void Update()
    {
        TryAttack();

    }

    private void TryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isAttack) //isAttack�� false�� ��� 
            {
                //�ڷ�ƾ ����
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentHand.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentHand.attackDelayA); //�ణ�� ������ 
        isSwing = true;

        //���� Ȱ��ȭ ����(�ڷ�ƾ)
        StartCoroutine(HitCoroutine());


        yield return new WaitForSeconds(currentHand.attackDelayB); //�����ð� ������
        isSwing = false; //�ڷ�ƾ ���� 

        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false;

    }

    IEnumerator HitCoroutine()
    {
        while (isSwing) //isSwing�� true�̸� 
        {
            if (CheckObject())
            {
                //�浹����
                isSwing = false; //�ѹ� �����ϸ� ������ 
                Debug.Log(hitInfo.transform.name);
            }

            yield return null;

        }
    }

    private bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range)) //�������� ���� ������ true
        {
            return true;
        }
        return false;
    }
}