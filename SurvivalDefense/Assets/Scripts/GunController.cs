using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    //현재 장착된 총
    [SerializeField]
    private Gun currentGun;

    //연사 속도 계산 
    private float currentFireRate; //이 값이 0이되면 발사할 수 있게 값을 깎음

    //상태 변수
    private bool isReload = false;
    
    [HideInInspector] //인스펙터 창에 뜨지 않도록 함
    private bool isFineSightMode = false;

    //본래 포지션 값
    [SerializeField]
    private Vector3 originPos;

    //효과음 재생
    private AudioSource audioSource;

    //레이저 충돌 정보 받아옴
    private RaycastHit hitInfo;

    [SerializeField]
    private Camera theCam;

    //피격 이펙트
    [SerializeField]
    private GameObject hit_effect_prefab;

    void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        //originPos = transform.localPosition; //인스펙터 창에서 지정 
    }

    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();
        TryFire();
        TryReload();
        TryFineSight();
    }

    //재계산 연사 속도
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime; //1초의 역수, 60분의 1/ 1초에 1씩 감소 
        }
    }

    //발사시고 
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload) //발사됨
        {
            Fire();
        }

    }

    private void Fire() //발사 전
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
                Shoot(); //방아쇠 당기고 나서의 이벤트
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }
                
        }
        

    }

    private void Shoot() //발사 후
    {
        currentGun.currentBulletCount--; //발사하면 총알 개수 깎아야함
        currentFireRate = currentGun.fireRate; //연사 속도 재계산
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();
        StopAllCoroutines(); //총기 반동 코루틴 내 while문 두개의 경쟁으로 인해 무한 루프에 걸리는 것을 방지하기 위해 필요
        //총기 반동 코루틴 실행 
        StartCoroutine(RetroActionCoroutine());

       
    }

    private void Hit() //명중 
    {
        //쏘는 족족 맞추는 방법으로,,
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward, out hitInfo, currentGun.range))
        {
            //Debug.Log(hitInfo.transform.name);
            GameObject clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal)); //Quaternion.LookRotation(hitInfo.normal) : 위를 보고 있는 상태로 객체가 생성됨 
            Destroy(clone,2f);
        }

    }

    //재장전 시도 
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    //재장전
    IEnumerator ReloadCoroutine() 
    {
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;

            currentGun.anim.SetTrigger("Reload"); //애니메이션 Parameters Reload

            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;


            yield return new WaitForSeconds(currentGun.reloadTime);

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount) //10발 보다 가지고 있는 개수가 많을 경우 
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;

            }

            isReload = false;
        }
        else {
            Debug.Log("총알이 없습니다.");
        }
    }

    //정조준 시도
    private void TryFineSight()
    {
        if (Input.GetButtonDown("Fire2") && !isReload) //우측 마우스 클릭하면 (리로드 중이 아닐 때만 )
        {
            FineSight(); //정조준
        }
    }

    //정조준 취소
    public void CancelFineSight()
    {
        if (isFineSightMode)
            FineSight();
    }

    //정조준 로직 가동
    private void FineSight() 
    {
        isFineSightMode = !isFineSightMode; //처음에 false이므로, 여기선 true가 됨 
        currentGun.anim.SetBool("FineSightMode",isFineSightMode);

        if (isFineSightMode)
        {
            StopAllCoroutines(); 
            StartCoroutine(FineSightActivateCoroutine());
        }
        else 
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeactivateCoroutine());
        }
    }

    //정조준 활성화 
    IEnumerator FineSightActivateCoroutine() //화면 가운데로 정조준 가동 
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos,0.2f); //Lerp : 좌표값을 옮겨줌 
            yield return null; //1프레임 대기 
        }
    }

    //정조준 비활성화
    IEnumerator FineSightDeactivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition,originPos, 0.2f); //원래 위치로 돌아옴
            yield return null; //1프레임 대기 
        }
    }

    //반동 코루틴
    IEnumerator RetroActionCoroutine() //반동이 X에서 이뤄짐 
    {
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z );
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);

        if (!isFineSightMode) //정조준 상태가 아닐 경우
        {
            currentGun.transform.localPosition = originPos;

            //반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionForce -0.02f) //Lerp 함수는 목표값까지 끝까지 못 가는게 단점이므로, 근처값에 가면 반동이 이루어지도록 하기 위해 -0.02f 작성함
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack,0.4f); //반동을 주는 recoilBack까지
                yield return null;
            }

            //원위치(반동 끝 원위치)
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }

        }
        else //정조준 상태일 경우
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

            //반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f) //Lerp 함수는 목표값까지 끝까지 못 가는게 단점이므로, 근처값에 가면 반동이 이루어지도록 하기 위해 -0.02f 작성함
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f); //반동을 주는 recoilBack까지
                yield return null;
            }

            //원위치(반동 끝 원위치)
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }

    //사운드 재생
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    //HUD.cs에서 Gun 가져오게 하기 위해서 
    public Gun GetGun()
    {
        return currentGun;
    }
}


