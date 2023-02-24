using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField]
    private Gun currentGun;

    private float currentFireRate; //�� ���� 0�̵Ǹ� �߻��� �� �ְ� ���� ����

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();
        TryFire();
    }


    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime; //1���� ����, 60���� 1/ 1�ʿ� 1�� ���� 
        }
    }

    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0) //�߻��
        {
            Fire();
        }

    }

    private void Fire() //�߻� ��
    {
        currentFireRate -= currentGun.fireRate;
        shoot(); //��Ƽ� ���� ������ �̺�Ʈ
    }

    private void shoot() //�߻� ��
    {
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Debug.Log("�Ѿ� �߻�");
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }
}


