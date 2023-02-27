using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    public string gunName;
    public float range; //사정거리
    public float accuracy; //정확도
    public float fireRate; //높으면 높을 수록 연사 속도가 느려짐
    public float reloadTime; //재장전 속도

    public int damage; //총의 데미지

    public int reloadBulletCount; //총알 재장전 개수 
    public int currentBulletCount; //현재 탄알집에 남아있는 총알의 개수 
    public int maxBulletCount; //최대 소유 가능 총알 개수
    public int carryBulletCount; //현재 소유하고 있는 총알 개수

    public float retroActionForce; //반동세기 (뒤로 밀리는 정도)
    public float retroActionFineSightForce; //정조준시 최대 뒤로 밀릴 수 있는 세기  

    public Vector3 fineSightOriginPos;
    public Animator anim;
    public ParticleSystem muzzleFlash;

    public AudioClip fire_Sound; //총 쏠 때 나오는 사운드 
                            

}
