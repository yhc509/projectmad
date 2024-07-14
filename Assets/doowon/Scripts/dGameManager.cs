using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class dGameManager : MonoBehaviour
{
    public bool testMode = false;
    public Transform statueGroup;
    public GameObject[] statues;
    public Volume volume;
    public EnemyController enemyController;
    Coroutine vtCoroutine;

    public int winCount = 5;
    [SerializeField] private int currentCount = 0;

    public dUIManager uIManager;

    public static dGameManager instance;

    public void Start()
    {
        instance = this;
        StartGame();
    }

    public void StartGame()
    {
        currentCount = 0;
        UpdateEnvironment(currentCount);
        ShowMessage($"Collect {winCount} statues", 10f);
    }

    [ContextMenu("Update Statues")]
    void UpdateStatue()
    {
        if (statueGroup != null)
        {
            int count = statueGroup.childCount;
            statues = new GameObject[count];
            for(int i = 0; i < count; i++)
            {
                statues[i] = statueGroup.GetChild(i).gameObject;
            }
        }
    }

    private bool CheckStatue(GameObject statue, out int index)
    {
        index = -1;
        for (int i = 0; i < statues.Length; i++)
        {
            if (statues[i] == statue)
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    public void CollectStatue(GameObject statue)
    {
        if(CheckStatue(statue, out int index))
        {
            if(currentCount >= winCount)
            {
                ShowMessage("You have enough statues");
            }
            else
            {
                statues[index].SetActive(false);
                currentCount++;
                UpdateEnvironment(currentCount);
                if (currentCount >= winCount)
                {
                    ShowMessage("You collected all the statues.\n Go to couch.");
                }
                else
                {
                    ShowMessage($"You collect {currentCount}/{winCount} statues", 5f);
                } 
            }
        }
    }

    public void Finish()
    {
        if (currentCount < winCount)
        {
            ShowMessage($"You don't have enough statues.\n Need {winCount - currentCount} more statues.");
        }
        else
        {
            ShowMessage("You Win!!");
            GameEnd();
        }
    }

    public void UpdateEnvironment(int statueCount)
    {
        enemyController.gameObject.SetActive(statueCount > 0);
        switch (statueCount)
        {
            case 0:
                volume.weight = 0.5f;
                break;
            case 1:
                StartVolumeTransition(0.75f, 25f);
                break;
            case 2:
                StartVolumeTransition(0.85f, 15f);
                break;
            case 3:
                StartVolumeTransition(0.98f, 15f);
                break;
            default:
                break;
        }
        enemyController.SetSpeed(statueCount * 1 + 3f);
    }

    public void GameEnd()
    {
        enemyController.SetSpeed(0f);
        StartVolumeTransition(0.1f, 10f);
    }

    public void ShowMessage(string message, float duration = 5f)
    {
        if(uIManager != null)
        {
            uIManager.ShowMessage(message, duration);
        }
    }

    private void StartVolumeTransition(float targetWeight, float fadeTime)
    {
        if (vtCoroutine != null)
        {
            StopCoroutine(vtCoroutine);
        }
        vtCoroutine = StartCoroutine(VolumeTransitionCoroutine(targetWeight, fadeTime));
    }

    IEnumerator VolumeTransitionCoroutine(float targetWeight, float fadeTime)
    {
        if(fadeTime > 0)
        {
            float t = 0;
            float startWeight = volume.weight;
            while (t < fadeTime)
            {
                volume.weight = Mathf.Lerp(startWeight, targetWeight, t / fadeTime);
                yield return null;
                t += Time.deltaTime;
            }
        }
        volume.weight = targetWeight;
        vtCoroutine = null;
    }
}
