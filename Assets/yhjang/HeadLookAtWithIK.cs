using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLookAtWithIK : MonoBehaviour
{
    public Animator _animator;
    public bool _isActive = false;
    public Transform _objTarget;
    public float _lookWeight;
    public float desireDistance = 4f;
    
    // dummy pivot
    private GameObject _objPivot;
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();

        _objPivot = new GameObject("DummyPivot");
        _objPivot.transform.SetParent(transform);
        _objPivot.transform.localPosition = new Vector3(0, 1.6f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        _objPivot.transform.LookAt(_objTarget);

        float pivotRotY = _objPivot.transform.localRotation.y;
        float dist = Vector3.Distance(_objPivot.transform.position, _objTarget.position);
        
        if (pivotRotY < 0.6f && pivotRotY > -0.6f && dist < desireDistance)
        {
            // target tracking
            _lookWeight = Mathf.Lerp(_lookWeight, 1, Time.deltaTime * 2.5f);
        }
        else
        {
            // target release
            _lookWeight = Mathf.Lerp(_lookWeight, 0, Time.deltaTime * 2.5f);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator)
        {
            if (_isActive)
            {
                if (_objTarget != null)
                {
                    _animator.SetLookAtWeight(_lookWeight);
                    _animator.SetLookAtPosition(_objTarget.position);
                }
            }
            else
            {
                _animator.SetLookAtWeight(0);
            }
        }
    }
}
