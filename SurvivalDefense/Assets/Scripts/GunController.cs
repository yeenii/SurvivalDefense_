using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    //���� ������ ��
    [SerializeField]
    private Gun currentGun;

    //���� �ӵ� ��� 
    private float currentFireRate; //�� ���� 0�̵Ǹ� �߻��� �� �ְ� ���� ����

    //���� ����
    private bool isReload = false;
    
    [HideInInspector] //�ν����� â�� ���� �ʵ��� ��
    private bool isFineSightMode = false;

    //���� ������ ��
    [SerializeField]
    private Vector3 originPos;

    //ȿ���� ���
    private AudioSource audioSource;

    //������ �浹 ���� �޾ƿ�
    private RaycastHit hitInfo;

    [SerializeField]
    private Camera theCam;

    //�ǰ� ����Ʈ
    [SerializeField]
    private GameObject hit_effect_prefab;

    void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        //originPos = transform.localPosition; //�ν����� â���� ���� 
    }

    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();
        TryFire();
        TryReload();
        TryFineSight();
    }

    //���� ���� �ӵ�
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime; //1���� ����, 60���� 1/ 1�ʿ� 1�� ���� 
        }
    }

    //�߻�ð� 
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload) //�߻��
        {
            Fire();
        }

    }

    private void Fire() //�߻� ��
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
                Shoot(); //��Ƽ� ���� ������ �̺�Ʈ
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }
                
        }
        

    }

    private void Shoot() //�߻� ��
    {
        currentGun.currentBulletCount--; //�߻��ϸ� �Ѿ� ���� ��ƾ���
        currentFireRate = currentGun.fireRate; //���� �ӵ� ����
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();
        StopAllCoroutines(); //�ѱ� �ݵ� �ڷ�ƾ �� while�� �ΰ��� �������� ���� ���� ������ �ɸ��� ���� �����ϱ� ���� �ʿ�
        //�ѱ� �ݵ� �ڷ�ƾ ���� 
        StartCoroutine(RetroActionCoroutine());

       
    }

    private void Hit() //���� 
    {
        //��� ���� ���ߴ� �������,,
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward, out hitInfo, currentGun.range))
        {
            //Debug.Log(hitInfo.transform.name);
            GameObject clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal)); //Quaternion.LookRotation(hitInfo.normal) : ���� ���� �ִ� ���·� ��ü�� ������ 
            Destroy(clone,2f);
        }

    }

    //������ �õ� 
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    //������
    IEnumerator ReloadCoroutine() 
    {
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;

            currentGun.anim.SetTrigger("Reload"); //�ִϸ��̼� Parameters Reload

            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;


            yield return new WaitForSeconds(currentGun.reloadTime);

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount) //10�� ���� ������ �ִ� ������ ���� ��� 
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
            Debug.Log("�Ѿ��� �����ϴ�.");
        }
    }

    //������ �õ�
    private void TryFineSight()
    {
        if (Input.GetButtonDown("Fire2") && !isReload) //���� ���콺 Ŭ���ϸ� (���ε� ���� �ƴ� ���� )
        {
            FineSight(); //������
        }
    }

    //������ ���
    public void CancelFineSight()
    {
        if (isFineSightMode)
            FineSight();
    }

    //������ ���� ����
    private void FineSight() 
    {
        isFineSightMode = !isFineSightMode; //ó���� false�̹Ƿ�, ���⼱ true�� �� 
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

    //������ Ȱ��ȭ 
    IEnumerator FineSightActivateCoroutine() //ȭ�� ����� ������ ���� 
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos,0.2f); //Lerp : ��ǥ���� �Ű��� 
            yield return null; //1������ ��� 
        }
    }

    //������ ��Ȱ��ȭ
    IEnumerator FineSightDeactivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition,originPos, 0.2f); //���� ��ġ�� ���ƿ�
            yield return null; //1������ ��� 
        }
    }

    //�ݵ� �ڷ�ƾ
    IEnumerator RetroActionCoroutine() //�ݵ��� X���� �̷��� 
    {
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z );
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);

        if (!isFineSightMode) //������ ���°� �ƴ� ���
        {
            currentGun.transform.localPosition = originPos;

            //�ݵ� ����
            while (currentGun.transform.localPosition.x <= currentGun.retroActionForce -0.02f) //Lerp �Լ��� ��ǥ������ ������ �� ���°� �����̹Ƿ�, ��ó���� ���� �ݵ��� �̷�������� �ϱ� ���� -0.02f �ۼ���
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack,0.4f); //�ݵ��� �ִ� recoilBack����
                yield return null;
            }

            //����ġ(�ݵ� �� ����ġ)
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }

        }
        else //������ ������ ���
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

            //�ݵ� ����
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f) //Lerp �Լ��� ��ǥ������ ������ �� ���°� �����̹Ƿ�, ��ó���� ���� �ݵ��� �̷�������� �ϱ� ���� -0.02f �ۼ���
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f); //�ݵ��� �ִ� recoilBack����
                yield return null;
            }

            //����ġ(�ݵ� �� ����ġ)
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }

    //���� ���
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    //HUD.cs���� Gun �������� �ϱ� ���ؼ� 
    public Gun GetGun()
    {
        return currentGun;
    }
}


