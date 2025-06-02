using UnityEngine;

[System.Serializable]
public class UI_StoreStandItemData
{
    public int id;             // 고유 ID (아이템 구매/선택에 필요)
    public string title;          // 아이템 이름
    public string description;    // 아이템 설명
    public int price;             // 가격
    public Sprite icon;           // 아이콘
    public bool isEquipped;       // 현재 장착 여부
    public bool isLocked;         // 아직 해금되지 않았는가?
}