using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public Transform _moveObjTrn;

    public Transform _startTrn;

    public Transform _endTrn;

    public bool _isPong;

    public float _speed = 1f;
    public float _waitTime = 3f;

    private float _time = 0f;
    
    void Update()
    {
        if (_moveObjTrn == null || _startTrn == null || _endTrn == null) return;

        Vector3 targetPos = Vector3.zero;

        if (!_isPong)
            targetPos = _endTrn.position;
        else
            targetPos = _startTrn.position;
        
        _moveObjTrn.position = Vector3.MoveTowards(_moveObjTrn.position, targetPos, Time.deltaTime * _speed);
        if (Vector3.Distance(_moveObjTrn.position, targetPos) < 0.1f)
        {
            _time += Time.deltaTime;
            if(_time >= _waitTime)
            {
                _isPong = !_isPong;
                _time = 0f;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player");
            var player = other.gameObject.GetComponent<ThirdPersonController>();
            player.Grounded = false;
            player.SetVerticalVelocity(_speed);
        }
        
    }
}
