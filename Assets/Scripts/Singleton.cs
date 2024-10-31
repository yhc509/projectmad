using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    // 인스턴스를 저장할 정적 변수
    private static T _instance;

    // 싱글톤 인스턴스에 접근할 수 있는 프로퍼티
    public static T Instance
    {
        get
        {
            // 만약 인스턴스가 없으면, 새로 생성
            if (_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾음
                _instance = FindObjectOfType<T>();

                // 씬에 없다면 새 GameObject를 생성하고 GameManager를 추가
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).ToString() + " (Singleton)";
                }

                // 인스턴스가 파괴되지 않도록 유지
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
}
