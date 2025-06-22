using UnityEngine;
using UnityEngine.SceneManagement;

public class CinematicSceneManager : RuntimeSingleton<CinematicSceneManager>
{
    [Header("Common")]
    [SerializeField] private GameObject fadePanel;
    [SerializeField] private GameObject camPrefab;
    [SerializeField] private GameObject openingCanvas, endingCanvas, scoreBoard;
    [SerializeField] private GameObject receipt;
    [SerializeField] private GameObject logo;
    [SerializeField] private RectTransform[] paddings;
    [SerializeField] private GameObject[] covers;
    
    [Header("Track")]
    [SerializeField] private Transform trackTransform;
    [SerializeField] private CinematicHamsterController trackHamsterController;
    
    [Header("Town")]
    [SerializeField] private Transform townTransform;
    [SerializeField] private CinematicHamsterController townHamsterController;
    [SerializeField] private GameObject townPoliceParent, townLightsParent;
    
    [Header("Cage")]
    [SerializeField] private Transform cageTransform;

    [SerializeField] private GameObject skipButton;
    
    private GameManager.CinematicModes _cinematicMode;
    private CinematicSequence _cinematicSequence;

    private bool _skip;
    
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        skipButton.SetActive(true);
        
        _skip = false;
        CinematicCommonObject commonObject =
            new CinematicCommonObject(fadePanel, camPrefab, openingCanvas, logo, paddings, covers, endingCanvas,
                scoreBoard, receipt);
        CinematicTrackObject trackObject = new CinematicTrackObject(trackTransform, trackHamsterController);
        CinematicTownObject townObject =
            new CinematicTownObject(townTransform, townHamsterController, townPoliceParent, townLightsParent);
        CinematicCageObject cageObject = new CinematicCageObject(cageTransform);
        
        
        _cinematicMode = GameManager.Instance.cinematicMode;

        _cinematicSequence =
            _cinematicMode == GameManager.CinematicModes.Opening ? gameObject.GetComponent<OpeningManager>()
            : _cinematicMode == GameManager.CinematicModes.GoodEnding ? gameObject.GetComponent<GoodEndingManager>()
            : gameObject.GetComponent<BadEndingManager>();
        _cinematicSequence.Init(commonObject, trackObject, townObject, cageObject);
    }

    private void Start()
    {
        StartCoroutine(_cinematicSequence.Run());
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0)) SkipCinematicScene();
    // }

    public void GoBackToTitle()
    {
        AudioManager.Instance.StopBgm();
        Load("TitleScene");
    }

    public void PlayAgain()
    {
        Load("MainScene");
    }

    public void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SkipCinematicScene()
    {
        if (_skip) return;
        _skip = true;
        skipButton.SetActive(false);
        StopAllCoroutines();
        _cinematicSequence.Skip();
    }

    public void CinematicEnded()
    {
        _skip = true;
        skipButton.SetActive(false);
    }
}
