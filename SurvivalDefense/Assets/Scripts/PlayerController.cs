using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //���ǵ� ���� ���� 
    [SerializeField] //private������ �ν����� â���� ���� ���� 
    private float walkSpeed;

    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField] //���� 
    private float jumpForce;
    

    //�� ���� ���� 
    private CapsuleCollider capsuleCollider; //Ground�� �´�� �ִ� ��� �����ǰ� 


    //���º���
    private bool isRun = false; //�ٴ��� �� �ٴ���
    private bool isCrouch = false;
    private bool isGround = false;

    //�ɾ��� �� �󸶳� ������ �����ϴ� ���� 
    [SerializeField]
    private float crouchPosY; 
    private float originPosY; //���� position y
    private float applyCrouchPosY;

    //ī�޶� �ΰ���
    [SerializeField]
    private float lookSensitivity;

    //ī�޶� �Ѱ� 
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0; //0 : ���� 

    //�ʿ��� ������Ʈ
    [SerializeField]
    private Camera theCamera;

    private Rigidbody myRigid;

    private GunController theGunController; //GunController ��ũ��Ʈ

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        //theCamera = FindObjectType<Camera>();
        applySpeed = walkSpeed;
        theGunController = FindObjectOfType<GunController>();

        //�ʱ�ȭ
        originPosY = theCamera.transform.localPosition.y; //capsule�� position���� �ϸ� ���� ���� �� �ֱ� ������, camera�� �� 
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
        CameraRotation();//���Ʒ� rotation
        CharacterRotation(); //�¿� rotation
    }

    //�ɱ� �õ�
    private void TryCrouch() //�ɾҴ� �Ͼ�� ���� ���� 
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        isCrouch = !isCrouch; //isCrouch�� false�� �ܿ� true�� �����ϰ�, isCrouch�� true�� ��� false�� ����

        if (isCrouch) 
        {
            //�ɴ� ���� 
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY; //0���� �����ϸ� 0���� Y�̵� 
        }
        else {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        
        }


        //���� ������ �ʰ�, �ڿ������� �ɴ� �ൿ ���� (�ڷ�ƾ ���)
        StartCoroutine(CrouchCoroutine());

    }

    IEnumerator CrouchCoroutine()  //���� ������ �ʰ�, �ڿ������� �ɴ� �ൿ ���� (�ڷ�ƾ ���)
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f); //����. _posY���� applyCrouchY�� 
            theCamera.transform.localPosition = new Vector3(0,_posY,0);

            if (count > 15)
                break;
            yield return null; //�� ������ ��� (while���� ������ ���İ��� �̷���� ������ �Ȱ��� ���� ����)
        }

        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }

        

    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        //Vecotr3.down : capsule�� �Ʒ��� ray �� / capsuleCollider.bounds.extents.y : y���� ����(extends)��ŭ / 0.1f�� ������ ��� 

    }

    //�޸��� �õ�
    private void TryJump() //space�� ������ ����
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround) //space�ٸ� ������, �ٴڿ� ������ ���� ����
        {
            Jump();
        }
    }

    //���� �õ�
    private void Jump()
    {
        //�ɾ��ִ� ���, ������ ���� ���� ����
        if (isCrouch)
            Crouch();

        //���� �����̴� �ӵ�
        myRigid.velocity = transform.up * jumpForce;
        
    }

    //�޸��� �õ�
    private void TryRun()
    {
        //leftshiftŰ�� ������ Player�� �ٵ���

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    private void Running() //�� �� 
    {
        if (isCrouch)
            Crouch();

        theGunController.CancelFineSight(); //�� ��, ������ ���� 

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
        float _moveDirX = Input.GetAxisRaw("Horizontal");//�¿� ȭ��ǥ
        //�� : 1, �� : -1, �� ������ : 0
        float _moveDirZ = Input.GetAxisRaw("Vertical");//�� �Ʒ� 

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        //Vector3s�� (1,0,0)�� ������ ����  / ������ �������� �����ڴ�. / moveDirX�� -1�̸� ���ص� -1�� �Ǵϱ� �������� 

        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed; //�ȴ���, �ٴ��� 
        // (1,0,0) + (0,0,1) = (1,0,1)
        //(1,0,1) = 2
        //normalized�ؼ� (0.5,0,0.5) = 1�� �ǰ� �� 

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); 

    }

    private void CharacterRotation() //�¿�
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f)*lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY)); //Quaternion�� (0,0,0,0) / Euler
    }

    private void CameraRotation() //���Ʒ�
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //���Ʒ��� �� ���
        float _cameraRotationX = _xRotation * lookSensitivity; //�ΰ��� ���ϱ� 

        currentCameraRotationX += _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit,cameraRotationLimit); //-cameraRotationLimit�� cameraRotationLimit ���̿� �� ����
        
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        //localEulerAngles : Rotation x,y,z��� �����ϸ� ��
    }
}
