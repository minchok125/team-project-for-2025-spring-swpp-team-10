using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryItemPopup : MonoBehaviour
{
    // 현재 섹션 타입
    private ItemType _sectionType;
    public ItemType SectionType
    {
        get => _sectionType;
        set
        {
            if (_sectionType == value) return;
            if (SelectedItem != null)
            {
                SelectedItem.SetSelected(false);
                //SelectedItem = null;
            }

            _sectionType = value;
            SetSection(_sectionType);
        }
    }

    // 현재 선택된 아이템
    private UI_InventoryItem selectedItem;
    public UI_InventoryItem SelectedItem
    {
        get => selectedItem;
        set
        {
            if (selectedItem != null)
                selectedItem.SetSelected(false);

            if (value == null)
            {
                inventoryDetailView.gameObject.SetActive(false);
                return;
            }

            selectedItem = value;
            selectedItem.SetSelected(true);
            inventoryDetailView.gameObject.SetActive(true);
            inventoryDetailView.RefreshUI(selectedItem);
        }
    }

    // UI 요소
    [SerializeField] private UI_InventoryItem inventoryItem;
    [SerializeField] private GameObject lockItem;
    [SerializeField] private Transform itemParent;
    [SerializeField] private Button BasicButton;
    [SerializeField] private Button BonusButton;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button resetButton; // 초기화 버튼
    [SerializeField] private UI_InventoryDetailView inventoryDetailView;
    [SerializeField] private GameObject maxPopup;

    // 페이지 관련 변수
    private int currentPage = 0;
    private int itemsPerPage = 6;
    private List<UI_InventoryItem> currentSectionItems = new List<UI_InventoryItem>();

    // 데이터 관리
    private ItemInventory _inventoryData;
    private Dictionary<string, Item> _itemDic = new Dictionary<string, Item>();
    private Dictionary<ItemType, Button> sectionButtons = new Dictionary<ItemType, Button>();
    private Dictionary<ItemType, List<UI_InventoryItem>> sectionItemDic = new Dictionary<ItemType, List<UI_InventoryItem>>();

    // 아이템 선택 이벤트
    public static Action<UI_InventoryItem> OnItemSelected = (x) => { };
    private List<GameObject> lockItemPool = new List<GameObject>(); // lockItem 풀


    private List<UI_InventoryItem> selectedItems = new List<UI_InventoryItem>(); // 선택된 아이템 리스트
    private const int MaxSelectableItems = 3; // 최대 선택 가능 아이템 수

    private List<UI_InventoryItem> itemUIList = new List<UI_InventoryItem>(); // 생성된 UI 아이템 리스트

    // 초기화
    public void SetInfo(ItemInventory inventoryData)
    {
        _inventoryData = inventoryData;

        sectionItemDic.Add(ItemType.Basic, new List<UI_InventoryItem>());
        sectionItemDic.Add(ItemType.Bonus, new List<UI_InventoryItem>());


        foreach (var item in _inventoryData.items)
        {
            if (!_itemDic.ContainsKey(item.id))
            {
                _itemDic.Add(item.id, Item.Create(item.id, item.name, item.type, item.description, item.howToUse, item.count));
            }
        }

        // UI 아이템 생성
        foreach (var item in _itemDic)
        {
            var itemUI = Instantiate(inventoryItem, itemParent);
            itemUI.SetInfo(item.Value);
            itemUIList.Add(itemUI); // 리스트에 추가
            sectionItemDic[item.Value.type].Add(itemUI);
        }


        BasicButton.onClick.AddListener(() => SetSection(ItemType.Basic));
        BonusButton.onClick.AddListener(() => SetSection(ItemType.Bonus));


        sectionButtons.Add(ItemType.Basic, BasicButton);
        sectionButtons.Add(ItemType.Bonus, BonusButton);

        // 아이템 선택 이벤트
        OnItemSelected = (item) =>
        {
            SelectedItem = item;

            int currentIndex = currentSectionItems.IndexOf(SelectedItem);
            Debug.Log($"Current Index: {currentIndex}");

        };

        // 페이지 버튼 이벤트 연결
        leftArrowButton.onClick.AddListener(() => ChangePage(-1));
        rightArrowButton.onClick.AddListener(() => ChangePage(1));

        inventoryDetailView.gameObject.SetActive(false);
    }

    public void RefreshUI(ItemInventory inventoryData)
    {
        _inventoryData = inventoryData;
        sectionItemDic.Clear();
        _itemDic.Clear();
        currentSectionItems.Clear();

        // 기존 UI 아이템 비활성화 및 카운트가 0인 아이템 제거
        for (int i = itemUIList.Count - 1; i >= 0; i--)
        {
            var itemUI = itemUIList[i];
            var itemData = itemUI.Item;

            if (itemData.count <= 0)
            {
                // 카운트가 0인 아이템 제거
                Destroy(itemUI.gameObject); // UI에서 제거
                itemUIList.RemoveAt(i); // 리스트에서 제거
            }
            else
            {
                itemUI.gameObject.SetActive(false); // 비활성화
            }
        }

        // 섹션 초기화
        sectionItemDic.Add(ItemType.Basic, new List<UI_InventoryItem>());
        sectionItemDic.Add(ItemType.Bonus, new List<UI_InventoryItem>());

        // _inventoryData를 기준으로 UI 아이템 갱신
        foreach (var item in _inventoryData.items)
        {
            if (!_itemDic.ContainsKey(item.id) && item.count > 0)
            {
                _itemDic.Add(item.id, Item.Create(item.id, item.name, item.type, item.description, item.howToUse, item.count));
            }
        }

        foreach (var item in _itemDic)
        {
            var itemData = item.Value;
            var existingItem = itemUIList.Find(uiItem => uiItem.Item.id == itemData.id);

            if (existingItem == null)
            {
                // 새로운 UI 아이템 생성
                var newItemUI = Instantiate(inventoryItem, itemParent);
                newItemUI.SetInfo(itemData);
                itemUIList.Add(newItemUI);
                sectionItemDic[itemData.type].Add(newItemUI);
            }
            else
            {
                // 기존 아이템 활성화 및 갱신
                existingItem.gameObject.SetActive(true);
                existingItem.RefreshUI(itemData);
                sectionItemDic[itemData.type].Add(existingItem);
            }
        }

        // 현재 섹션 아이템 업데이트
        SetSection(_sectionType);

        // 페이지 업데이트
        UpdatePage();

    }

    private void Start()
    {
        InitializeLockItemPool(); // lockItem 풀 초기화
        resetButton.onClick.AddListener(ResetSelection);

    }

    void OnEnable()
    {
        SelectedItem = null;
    }

    void OnDisable()
    {
        SelectedItem = null;
    }

    // 섹션 변경
    public void SetSection(ItemType sectionType)
    {
        SectionType = sectionType;

        // 버튼 활성화 상태 업데이트
        foreach (var button in sectionButtons)
            button.Value.interactable = button.Key != sectionType;

        // 섹션에 해당하는 아이템 활성화
        foreach (var item in sectionItemDic)
        {
            bool isActive = item.Key == sectionType;
            item.Value.ForEach(uiItem => uiItem.gameObject.SetActive(isActive));
        }

        // 현재 섹션 아이템 업데이트
        currentSectionItems = sectionItemDic[sectionType];
        currentPage = 0; // 페이지 초기화
        UpdatePage();
    }

    // 페이지 변경
    private void ChangePage(int direction)
    {
        int totalPages = Mathf.CeilToInt((float)currentSectionItems.Count / itemsPerPage);
        currentPage = Mathf.Clamp(currentPage + direction, 0, totalPages - 1);
        UpdatePage();
    }

    private void InitializeLockItemPool()
    {
        // 필요한 lockItem 수만큼 미리 생성
        for (int i = 0; i < itemsPerPage; i++)
        {
            GameObject lockItemInstance = Instantiate(lockItem, itemParent);
            lockItemInstance.SetActive(false); // 초기에는 비활성화
            lockItemInstance.transform.SetAsLastSibling();

            lockItemPool.Add(lockItemInstance);
        }
    }

    private GameObject GetLockItemFromPool()
    {
        // 풀에서 비활성화된 lockItem 반환
        foreach (var lockItemInstance in lockItemPool)
        {
            if (!lockItemInstance.activeSelf)
            {
                lockItemInstance.SetActive(true);
                lockItemInstance.transform.SetAsLastSibling();
                return lockItemInstance;
            }
        }

        // 풀에 여유가 없으면 새로 생성 (예외 상황)
        GameObject newLockItem = Instantiate(lockItem, itemParent);
        newLockItem.transform.SetAsLastSibling(); // 부모의 맨 아래로 이동

        lockItemPool.Add(newLockItem);
        return newLockItem;
    }

    // 페이지 업데이트
    private void UpdatePage()
    {
        int totalPages = Mathf.CeilToInt((float)currentSectionItems.Count / itemsPerPage);

        // 좌/우 화살표 활성화 상태 업데이트
        leftArrowButton.gameObject.SetActive(currentPage > 0);
        rightArrowButton.gameObject.SetActive(currentPage < totalPages - 1);

        // 현재 페이지의 시작 인덱스와 끝 인덱스 계산
        int pageStartIndex = currentPage * itemsPerPage;
        int pageEndIndex = Mathf.Min(pageStartIndex + itemsPerPage, currentSectionItems.Count);

        // 현재 페이지에 해당하는 아이템만 활성화
        for (int i = 0; i < currentSectionItems.Count; i++)
        {
            bool isActive = i >= pageStartIndex && i < pageEndIndex;
            currentSectionItems[i].gameObject.SetActive(isActive);

            if (isActive)
            {
                currentSectionItems[i].RefreshUI();
            }
        }

        // 현재 페이지에 표시된 아이템 수 계산
        int itemsOnCurrentPage = pageEndIndex - pageStartIndex;

        // lockItem 풀 초기화
        foreach (var lockItemInstance in lockItemPool)
        {
            lockItemInstance.SetActive(false); // 모든 lockItem 비활성화
        }

        // 부족한 칸에 lockItem 활성화
        for (int i = itemsOnCurrentPage; i < itemsPerPage; i++)
        {
            GameObject lockItemInstance = GetLockItemFromPool();
            lockItemInstance.transform.SetAsLastSibling();
        }
    }

    [SerializeField] private int rows = 3; // 행 (N)
    [SerializeField] private int columns = 2; // 열 (M)

    private void Update()
    {
        HandleKeyboardInput();


        // S 키 입력 처리
        if (Input.GetKeyDown(KeyCode.S))
        {
            SelectCurrentItem();
        }
    }

    private void SelectCurrentItem()
    {
        if (SelectedItem == null) return;

        // 이미 선택된 아이템인지 확인
        if (selectedItems.Contains(SelectedItem))
        {
            Debug.Log("이미 선택된 아이템입니다.");
            return;
        }

        // 최대 선택 제한 확인
        if (selectedItems.Count >= MaxSelectableItems)
        {
            Debug.Log("최대 3개만 선택 가능합니다.");
            StartCoroutine(ShowMaxPopup()); // maxPopup 표시

            return;
        }

        // 아이템 선택
        selectedItems.Add(SelectedItem);
        SelectedItem.SetSelected(true); // 선택 상태 표시
    }

    // maxPopup을 3초 동안 표시하는 코루틴
    private IEnumerator ShowMaxPopup()
    {
        maxPopup.SetActive(true); // maxPopup 활성화
        yield return new WaitForSeconds(3f); // 3초 대기
        maxPopup.SetActive(false); // maxPopup 비활성화
    }

    private void ResetSelection()
    {
        // 선택된 모든 아이템 초기화
        foreach (var item in selectedItems)
        {
            item.SetSelected(false); // 선택 상태 해제
        }

        selectedItems.Clear(); // 선택된 아이템 리스트 초기화
    }

    private void HandleKeyboardInput()
    {
        if (SelectedItem == null || currentSectionItems.Count == 0) return;

        // 현재 페이지의 시작 인덱스 계산
        int pageStartIndex = currentPage * itemsPerPage;
        int pageEndIndex = Mathf.Min(pageStartIndex + itemsPerPage, currentSectionItems.Count);

        // 현재 선택된 아이템의 인덱스와 행/열 계산 (페이지 내 상대 인덱스)
        int currentIndex = currentSectionItems.IndexOf(SelectedItem) - pageStartIndex;
        int currentRow = currentIndex / columns; // 현재 행
        int currentCol = currentIndex % columns; // 현재 열

        // 현재 페이지에 표시된 아이템 수 계산
        int itemsOnCurrentPage = pageEndIndex - pageStartIndex;

        // 방향키 입력 처리
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentRow > 0) // 위로 이동 가능 여부 확인
            {
                int targetIndex = currentIndex - columns - 1;
                if (IsValidIndex(pageStartIndex + targetIndex)) // 유효한 인덱스인지 확인
                {
                    SelectedItem = currentSectionItems[pageStartIndex + targetIndex];
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentRow < (itemsOnCurrentPage - 1) / columns) // 아래로 이동 가능 여부 확인
            {
                int targetIndex = currentIndex + columns + 1;
                if (IsValidIndex(pageStartIndex + targetIndex)) // 유효한 인덱스인지 확인
                {
                    SelectedItem = currentSectionItems[pageStartIndex + targetIndex];
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {

            int targetIndex = currentIndex - 1;
            if (IsValidIndex(pageStartIndex + targetIndex)) // 유효한 인덱스인지 확인
            {
                SelectedItem = currentSectionItems[pageStartIndex + targetIndex];

            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentCol < columns && currentIndex + 1 <= itemsOnCurrentPage) // 오른쪽으로 이동 가능 여부 확인
            {
                int targetIndex = currentIndex + 1;
                if (IsValidIndex(pageStartIndex + targetIndex)) // 유효한 인덱스인지 확인
                {
                    SelectedItem = currentSectionItems[pageStartIndex + targetIndex];
                }
            }
        }
    }

    // 인덱스 유효성 검사 메서드
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < currentSectionItems.Count && currentSectionItems[index].gameObject.activeSelf;
    }

}