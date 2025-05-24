using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StorePanelController : MonoBehaviour
{
    [SerializeField] private GameObject storeItemPrefab;
    [SerializeField] private Transform storePanelGrid;
    [SerializeField] private List<UI_StoreItemData> itemList;

#if UNITY_EDITOR
    [ContextMenu("Generate Dummy Store Items In Editor")]
    public void GenerateInEditor()
    {
        InitializeDummyData();
        RenderItems();
    }
#endif

    public void RenderItems()
    {
        foreach (Transform child in storePanelGrid)
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
#endif
                Destroy(child.gameObject);
        }

        foreach (var itemData in itemList)
        {
            var itemGO = Instantiate(storeItemPrefab, storePanelGrid);
            var view = itemGO.GetComponent<UI_StoreItem>();
            view?.Bind(itemData);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(itemGO, "Create StoreItem");
#endif
        }
    }

    private void InitializeDummyData()
    {
        Sprite sharedIcon = Resources.Load<Sprite>("Images/icon_boosterfh");

        itemList = new List<UI_StoreItemData>
        {
            new UI_StoreItemData { title = "Booster A", description = "속도 증가", price = 100, icon = sharedIcon, isEquipped = false, isLocked = false },
            new UI_StoreItemData { title = "Booster B", description = "방어력 증가", price = 200, icon = sharedIcon, isEquipped = true, isLocked = false },
            new UI_StoreItemData { title = "Booster C", description = "에너지 회복", price = 150, icon = sharedIcon, isEquipped = false, isLocked = true },
            new UI_StoreItemData { title = "Booster D", description = "점프력 향상", price = 180, icon = sharedIcon, isEquipped = false, isLocked = false },
            new UI_StoreItemData { title = "Booster E", description = "속도 대폭 증가", price = 300, icon = sharedIcon, isEquipped = false, isLocked = false },
            new UI_StoreItemData { title = "Booster F", description = "무적 시간 증가", price = 500, icon = sharedIcon, isEquipped = false, isLocked = true }
        };
    }
}

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StorePanelController : MonoBehaviour
{
    [SerializeField] private GameObject storeItemPrefab;
    [SerializeField] private Transform storePanelGrid;
    [SerializeField] private List<UI_StoreItemData> itemList;

#if UNITY_EDITOR
    [ContextMenu("Generate Dummy Store Items In Editor")]
    public void GenerateInEditor()
    {
        InitializeDummyData();
        RenderItems();
    }
#endif

    public void RenderItems()
    {
        foreach (Transform child in storePanelGrid)
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
#endif
                Destroy(child.gameObject);
        }

        foreach (var itemData in itemList)
        {
            var itemGO = Instantiate(storeItemPrefab, storePanelGrid);
            var view = itemGO.GetComponent<UI_StoreItem>();
            view?.Bind(itemData);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(itemGO, "Create StoreItem");
#endif
        }
    }

    private void InitializeDummyData()
    {
        Sprite sharedIcon = Resources.Load<Sprite>("Images/icon_boosterfh");

        itemList = new List<UI_StoreItemData>
        {
            new UI_StoreItemData { title = "Booster A", description = "속도 증가", price = 100, icon = sharedIcon, isEquipped = false, isLocked = false },
            new UI_StoreItemData { title = "Booster B", description = "방어력 증가", price = 200, icon = sharedIcon, isEquipped = true, isLocked = false },
            new UI_StoreItemData { title = "Booster C", description = "에너지 회복", price = 150, icon = sharedIcon, isEquipped = false, isLocked = true },
            new UI_StoreItemData { title = "Booster D", description = "점프력 향상", price = 180, icon = sharedIcon, isEquipped = false, isLocked = false },
            new UI_StoreItemData { title = "Booster E", description = "속도 대폭 증가", price = 300, icon = sharedIcon, isEquipped = false, isLocked = false },
            new UI_StoreItemData { title = "Booster F", description = "무적 시간 증가", price = 500, icon = sharedIcon, isEquipped = false, isLocked = true }
        };
    }
}