using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupController : MonoBehaviour
{
    [SerializeField] private UIDocument _ui;

    private Label _titleLabel;
    private Label _contentLabel;
    private Button _okButton;
    private Button _cancelButton;

    private Action _onClickOk;
    private Action _onClickCancel;
    
    void Awake()
    {
        _titleLabel = _ui.rootVisualElement.Q<Label>("Title");
        _contentLabel = _ui.rootVisualElement.Q<Label>("Content");
        _okButton = _ui.rootVisualElement.Q<Button>("OkButton");
        _okButton.clicked += OnClickOk;
        _cancelButton = _ui.rootVisualElement.Q<Button>("CancelButton");
        _cancelButton.clicked += OnClickCancel;
        
        _ui.rootVisualElement.visible = false;
    }
    
    public void Show(string title, string content, Action onClickOk, Action onClickCancel = null)
    {
        _titleLabel.text = title;
        _contentLabel.text = content;

        _onClickOk = onClickOk;
        _onClickCancel = onClickCancel;
        
        _ui.rootVisualElement.visible = true;
    }

    private void OnClickOk()
    {
        _onClickOk?.Invoke();
        _ui.rootVisualElement.visible = false;
    }

    private void OnClickCancel()
    {
        _onClickCancel?.Invoke();
        _ui.rootVisualElement.visible = false;
    }
}
