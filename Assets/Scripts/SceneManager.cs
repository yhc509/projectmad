using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public void LoadGameScene()
    {
        StartCoroutine(LoadGameSceneRoutine());
    }
    
    public IEnumerator LoadGameSceneRoutine()
    {       
        yield return SceneManager.LoadSceneAsync("Scenes/EmptyScene");
        
        yield return SceneManager.LoadSceneAsync("Scenes/Stages/Stage_yhj/Stage_yhj_improved");
    }
}
