using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameTimeUIRoot : MonoBehaviour
{
    [SerializeField] private float _gameTime;
    
    private UIDocument _ui;
    private Label _gameTimeText;

    void Start()
    {
        _ui = GetComponent<UIDocument>();
        _gameTimeText = _ui.rootVisualElement.Q<Label>("TimeText");
    }

    void Update()
    {
        _gameTime += Time.deltaTime;

        var time = TimeSpan.FromSeconds(_gameTime);
        _gameTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds);
    }
}
