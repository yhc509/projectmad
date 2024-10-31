using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TitleController : MonoBehaviour
{
    [SerializeField] private UIDocument _ui;
    [SerializeField] private PopupController _popupController;
    
    void Awake()
    {
        var startButton = _ui.rootVisualElement.Q<Button>("StartButton");
        startButton.clicked += OnClickStartButton;
        
        var optionButton = _ui.rootVisualElement.Q<Button>("OptionButton");
        optionButton.clicked += OnClickOptionButton;
        
        var exitButton = _ui.rootVisualElement.Q<Button>("ExitButton");
        exitButton.clicked += OnClickExitButton;
    }

    public void OnClickStartButton()
    {
        SceneLoader.Instance.LoadGameScene();
    }

    public void OnClickOptionButton()
    {
        Debug.Log("OnClickOptionButton");
    }
    
    public void OnClickRankingButton()
    {
        Debug.Log("OnClickRankingButton");
    }
    
    public void OnClickExitButton()
    {
        _popupController.Show(
            "Exit Game?", 
            "Are you sure you want to quit the game?", 
            ExitGame);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
