using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public Transform _moveObjTrn;

    public Transform _startTrn;

    public Transform _endTrn;

    // setParent�� y���� ����� ������ �ȵȴ�.
    public Transform _steppedPlayerTr;

    public bool _isPong;

    [SerializeField] float _originSpeed = 5.0f;
    public float _speed = 1f;
    public float _waitTime = 3f;

    private float _time = 0f;

    Vector3 _lastPos = Vector3.zero;
    Vector3 _moveVec = Vector3.zero;
    [SerializeField] float _offset;

    private void Start()
    {
        _lastPos = _moveObjTrn.position;
    }

    void LateUpdate()
    {
        if (_moveObjTrn == null || _startTrn == null || _endTrn == null) return;

        Vector3 targetPos = Vector3.zero;

        if (!_isPong)
            targetPos = _endTrn.position;
        else
            targetPos = _startTrn.position;
        
        _moveObjTrn.position = Vector3.MoveTowards(_moveObjTrn.position, targetPos, Time.deltaTime * _speed);

        _moveVec = _moveObjTrn.position - _lastPos;
        _lastPos = _moveObjTrn.position;

        if (Vector3.Distance(_moveObjTrn.position, targetPos) < 0.1f)
        {
            _speed = 0.0f;
            _time += Time.deltaTime;
            if(_time >= _waitTime)
            {
                _isPong = !_isPong;
                _time = 0f; 
                _speed = _originSpeed;
            }
        }
        else
        {
            _speed = _originSpeed;
        }

        AdjustSteppedPlayerTr();
    }

    void AdjustSteppedPlayerTr()
    {
        if (_steppedPlayerTr == null) return;

        Vector3 newPos = new Vector3(_steppedPlayerTr.position.x, _moveObjTrn.position.y + _offset, _steppedPlayerTr.position.z);
        _steppedPlayerTr.SetParent(_moveObjTrn);
        //_steppedPlayerTr.position = newPos;
        //_steppedPlayerTr.GetComponent<ThirdPersonController>().SetVerticalVelocity(-_speed);
        //_steppedPlayerTr.GetComponent<CharacterController>().SimpleMove(_moveVec);
    }
}
