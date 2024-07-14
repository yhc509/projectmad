using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dUIManager : MonoBehaviour
{
    public CanvasGroup _messageCanvasGroup;
    public TMP_Text _messageText;
    private Coroutine _fadeCoroutine;
    public float _messageFadeTime = 3f;

    public void Start()
    {
        _messageCanvasGroup.gameObject.SetActive(false);
    }
    public void ShowMessage(string message, float delay)
    {
        _messageCanvasGroup.gameObject.SetActive(true);
        _messageCanvasGroup.alpha = 1f;
        _messageText.text = message;
        if(_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeCoroutine(delay));
    }
    
    IEnumerator FadeCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        float t = 0;
        while (t < delay)
        {
            _messageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / delay);
            t += Time.deltaTime;
            yield return null;
        }
        _messageCanvasGroup.alpha = 0f;
        _messageCanvasGroup.gameObject.SetActive(false);
    }
}
