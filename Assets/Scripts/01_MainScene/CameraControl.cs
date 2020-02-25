using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("카메라기본속성")]
    private Transform myTransform = null;
    public GameObject Target = null;
    private Transform targetTransform = null;

    public enum CameraViewPointState { FIRST, SECOND, THIRD}
    public CameraViewPointState CameraState = CameraViewPointState.THIRD;

    [Header("3인칭 카메라")]
    public float Distance = 5.0f; //타켓으로부터 떨어진 거리.
    public float Height = 5f; //타켓의 위치보다 더 추가적인 높이.
    public float HeightDamping = 3.0f;
    public float RotationDamping = 2.0f;

    [Header("2인칭 카메라")]
    public float RotateSpeed = 10.0f;

    [Header("1인칭 카메라")]
    public float SensitivityX = 5.0f;
    public float SensitivityY = 5.0f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    public Transform FirstCameraSocket = null;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = GetComponent<Transform>();
        if (Target != null) {
            targetTransform = Target.transform;
        }
    }

    //3인칭 카메라
    void ThirdView() {
        float wantedRotationAngle = targetTransform.eulerAngles.y; //현재 타켓의 y축 각도 값.
        float wantedHeight = targetTransform.position.y + Height; //현재 타켓의 높이 + 우리가 추가로 높이고 싶은 높이.

        float currentRotationAngle = myTransform.eulerAngles.y; //현재 카메라의 y축 각도 값
        float currentHeight = myTransform.position.y; //현재 카메라의 높이값.

        //현재 각도에서 원하는 각도로 댐핑값을 얻게 됩니다.
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle,
                                                RotationDamping * Time.deltaTime);

        //현재 높이에서 원하는 높이로 댐핑값을 얻습니다.
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight,
                                    HeightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);

        myTransform.position = targetTransform.position;
        myTransform.position -= currentRotation * Vector3.forward * Distance;
        myTransform.position = new Vector3(myTransform.position.x, currentHeight, myTransform.position.z);
        myTransform.LookAt(targetTransform);
    }

    void SecondView() {
        myTransform.RotateAround(targetTransform.position, Vector3.up, RotateSpeed * Time.deltaTime);
        myTransform.LookAt(targetTransform);
    }

    void FirstView()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotationX = myTransform.localEulerAngles.y + mouseX * SensitivityX;
        //마이너스 각도를 조절하기 위한 연산
        rotationX = (rotationX > 180.0f) ? rotationX - 360.0f : rotationX;

        rotationY = rotationY + mouseX * SensitivityY;
        rotationY = (rotationY > 180.0f) ? rotationY - 360.0f : rotationY;

        myTransform.localEulerAngles = new Vector3(-rotationY, rotationX, 0f);
        myTransform.position = FirstCameraSocket.position;

    }


    // Update is called once per frame
    private void LateUpdate()
    {
        if (Target == null) {
            return;
        }
        if (targetTransform == null) {
            targetTransform = Target.transform;
        }

        switch (CameraState) {
            case CameraViewPointState.THIRD:
                ThirdView();
                break;
            case CameraViewPointState.SECOND:
                SecondView();
                break;
            case CameraViewPointState.FIRST:
                FirstView();
                break;
        }

    }
}
