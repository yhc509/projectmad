using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class TimeController : MonoBehaviour
{
    private const float COMMON_GAME_SPEED = 1f;
    private const float SLOW_GAME_SPEED = 0.1f;
    private const float SPEED_FACTOR = 2f; // 속도 변화의 빠르기
    
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField] public Volume postProcessVolume;
    
    private ColorAdjustments _colorAdjustments;

    private float _gameSpeed
    {
        get { return Time.timeScale; }
        set { Time.timeScale = value;  }
    }

    void Start()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out _colorAdjustments);
        }
    }
    
    void Update()
    {
        CheckSlowInput();
    }

    private void CheckSlowInput()
    {
        if (_input != null)
        {
            if (_input.slow && Time.timeScale > SLOW_GAME_SPEED) Slow();
            else if (!_input.slow && Time.timeScale < COMMON_GAME_SPEED) Restore();
        }
    }

    private void Slow()
    {
        _gameSpeed = Mathf.Lerp(_gameSpeed, SLOW_GAME_SPEED, Time.deltaTime * SPEED_FACTOR); // 0.1 ~ 1
        if (_colorAdjustments != null)
        {
            _colorAdjustments.saturation.value = Map(_gameSpeed, -100f, 0f);
            _colorAdjustments.contrast.value = Map(_gameSpeed, 50f, 70f);
        }
    }
    
    private void Restore()
    {
        _gameSpeed = Mathf.Lerp(_gameSpeed, COMMON_GAME_SPEED, Time.deltaTime * SPEED_FACTOR);
        if (_colorAdjustments != null)
        {
            _colorAdjustments.saturation.value = Map(_gameSpeed, -100f, 0f);
            _colorAdjustments.contrast.value = Map(_gameSpeed, 50f, 70f);
        }
    }
    
    private float Map(float value, float toMin, float toMax)
    {
        return (value - SLOW_GAME_SPEED) * (toMax - toMin) / (COMMON_GAME_SPEED - SLOW_GAME_SPEED) + toMin;
    }
}
