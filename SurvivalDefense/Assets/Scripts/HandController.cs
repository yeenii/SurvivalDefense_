using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    //현재 장착된 Hand형 타입 무기 
    [SerializeField]
    private Hand currentHand;

    //공격중
    private bool isAttack = false;
    private bool isSwing = false;

    private RaycastHit hitInfo; //RaycastHit : Raycast가 닿은 녀석의 정보를 얻어올 수 있는 것 


    // Update is called once per frame
    void Update()
    {
        TryAttack();

    }

    private void TryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isAttack) //isAttack이 false일 경우 
            {
                //코루틴 실행
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentHand.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentHand.attackDelayA); //약간의 딜레이 
        isSwing = true;

        //공격 활성화 시점(코루틴)
        StartCoroutine(HitCoroutine());


        yield return new WaitForSeconds(currentHand.attackDelayB); //일정시간 지나면
        isSwing = false; //코루틴 중지 

        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false;

    }

    IEnumerator HitCoroutine()
    {
        while (isSwing) //isSwing이 true이면 
        {
            if (CheckObject())
            {
                //충돌했음
                isSwing = false; //한번 적중하면 끝나게 
                Debug.Log(hitInfo.transform.name);
            }

            yield return null;

        }
    }

    private bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range)) //레이저를 쏴서 있으면 true
        {
            return true;
        }
        return false;
    }
}