using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Transform _cameraTr;
    [SerializeField] Vector3 _offset;
    [SerializeField] float _speed;

    [SerializeField] Transform _target;

    void Start()
    {
        _cameraTr = transform;
        _target = GameObject.Find("BouncingBall").transform;
        _offset = new Vector3(10.0f, 10.0f, 0.0f);
        //_cameraTr.SetParent(_target);
    }


    private void LateUpdate()
    {
        LookAround();
    }

    // �ϴ� ī�޶� �Ŵ������� Ÿ�ٵ� �������ش�... ���߿� ����
    void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 camAngle = _cameraTr.rotation.eulerAngles;

        //_cameraTr.position = _target.position + _offset;
        _cameraTr.rotation = Quaternion.Euler(camAngle.x - mouseDelta.y, camAngle.y + mouseDelta.x, camAngle.z);
    }
}
