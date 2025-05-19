using UnityEngine;

/// <summary>
/// 씬이 이동하여도 인스턴스가 파괴되지 않는 싱글톤입니다.
/// </summary>
/// <typeparam name="T">MonoBehaviour를 상속한 컴포넌트 타입</typeparam>
public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 현재 씬에 존재하는 T 타입의 오브젝트(컴포넌트)를 찾아 반환
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    var obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Instance로 그냥 접근하면 자동으로 인스턴스가 할당이 되어서
    /// Instance == null은 항상 false입니다.
    /// null 체크를 하려면 이 함수를 통해 체크해주세요
    /// </summary>
    protected bool IsInstanceNull()
    {
        return _instance == null;
    }
}
