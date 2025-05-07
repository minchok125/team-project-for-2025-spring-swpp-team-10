using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    void Start()
    {
        // 마우스 커서 숨기기
        Cursor.visible = false;
        
        // 마우스 커서를 화면 중앙에 고정 (게임 창 내에서만 적용)
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ALT 키로 마우스 잠금 해제
        if (Input.GetKey(KeyCode.LeftAlt) || Time.timeScale == 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        // 마우스 잠금
        else 
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
