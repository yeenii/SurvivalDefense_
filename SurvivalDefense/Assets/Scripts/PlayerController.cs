using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수 
    [SerializeField] //private이지만 인스펙터 창에서 조정 가능 
    private float walkSpeed;

    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField] //점프 
    private float jumpForce;
    

    //땅 착지 여부 
    private CapsuleCollider capsuleCollider; //Ground와 맞닿아 있는 경우 점프되게 


    //상태변수
    private bool isRun = false; //뛰는지 안 뛰는지
    private bool isCrouch = false;
    private bool isGround = false;

    //앉았을 때 얼마나 앉을지 결정하는 변수 
    [SerializeField]
    private float crouchPosY; 
    private float originPosY; //현재 position y
    private float applyCrouchPosY;

    //카메라 민감도
    [SerializeField]
    private float lookSensitivity;

    //카메라 한계 
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0; //0 : 정면 

    //필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;

    private Rigidbody myRigid;

    private GunController theGunController; //GunController 스크립트

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        //theCamera = FindObjectType<Camera>();
        applySpeed = walkSpeed;
        theGunController = FindObjectOfType<GunController>();

        //초기화
        originPosY = theCamera.transform.localPosition.y; //capsule의 position으로 하면 땅에 박힐 수 있기 때문에, camera로 함 
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame 
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();//위아래 rotation
        CharacterRotation(); //좌우 rotation
    }

    //앉기 시도
    private void TryCrouch() //앉았다 일어나는 동작 구현 
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        isCrouch = !isCrouch; //isCrouch가 false일 겨우 true로 변경하고, isCrouch가 true일 경우 false로 변경

        if (isCrouch) 
        {
            //앉는 동작 
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY; //0으로 설정하면 0으로 Y이동 
        }
        else {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        
        }


        //딱딱 끊기지 않고, 자연스럽게 앉는 행동 구현 (코루틴 사용)
        StartCoroutine(CrouchCoroutine());

    }

    IEnumerator CrouchCoroutine()  //딱딱 끊기지 않고, 자연스럽게 앉는 행동 구현 (코루틴 사용)
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f); //보간. _posY에서 applyCrouchY로 
            theCamera.transform.localPosition = new Vector3(0,_posY,0);

            if (count > 15)
                break;
            yield return null; //한 프레임 대기 (while문에 없으면 순식간에 이루어져 원래랑 똑같이 딱딱 끊김)
        }

        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }

        

    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        //Vecotr3.down : capsule의 아래로 ray 쏨 / capsuleCollider.bounds.extents.y : y값의 절반(extends)만큼 / 0.1f는 오차를 상쇄 

    }

    //달리기 시도
    private void TryJump() //space바 누르면 점프
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround) //space바를 누르고, 바닥에 착지될 때만 점프
        {
            Jump();
        }
    }

    //점프 시도
    private void Jump()
    {
        //앉아있는 경우, 점프시 앉은 상태 해제
        if (isCrouch)
            Crouch();

        //현재 움직이는 속도
        myRigid.velocity = transform.up * jumpForce;
        
    }

    //달리기 시도
    private void TryRun()
    {
        //leftshift키를 누르면 Player가 뛰도록

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    private void Running() //뛸 때 
    {
        if (isCrouch)
            Crouch();

        theGunController.CancelFineSight(); //뛸 때, 정조준 해제 

        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;

    }


    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");//좌우 화살표
        //오 : 1, 왼 : -1, 안 누르면 : 0
        float _moveDirZ = Input.GetAxisRaw("Vertical");//위 아래 

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        //Vector3s는 (1,0,0)을 가지고 있음  / 오른쪽 방향으로 나가겠다. / moveDirX가 -1이면 곱해도 -1이 되니까 왼쪽으로 

        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed; //걷는지, 뛰는지 
        // (1,0,0) + (0,0,1) = (1,0,1)
        //(1,0,1) = 2
        //normalized해서 (0.5,0,0.5) = 1이 되게 함 

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); 

    }

    private void CharacterRotation() //좌우
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f)*lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY)); //Quaternion은 (0,0,0,0) / Euler
    }

    private void CameraRotation() //위아래
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //위아래로 고개 들기
        float _cameraRotationX = _xRotation * lookSensitivity; //민감도 곱하기 

        currentCameraRotationX += _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit,cameraRotationLimit); //-cameraRotationLimit와 cameraRotationLimit 사이에 값 고정
        
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        //localEulerAngles : Rotation x,y,z라고 생각하면 됨
    }
}
