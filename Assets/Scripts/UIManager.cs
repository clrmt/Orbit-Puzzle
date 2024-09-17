using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UIManager : MonoBehaviour {
    public static UIManager instance;

    private Camera mainCamera;
    private float initialOrthographicSize;
    private Vector3 initialScale;

    private GameObject backObj;
    private SpriteRenderer backObjSR;
    private CircleCollider2D backObjCC;
    private GameObject playbackObj;
    private SpriteRenderer playbackObjSR;
    private CircleCollider2D playbackObjCC;

    private Color playbackOriginalColor;

    private int playbackState;
    public Sprite playSprite;
    public Sprite stopSprite;
    public Sprite skipSprite;

    private float stageUIOpacity = 0.0f;
    private float stageUIOpacityDelta = 0.0f;

    private float previousScreenWidth = 0.0f;
    private float previousScreenHeight = 0.0f;

    public GameObject endingObj;

    // Space
    private bool spaceDown = false;

    // ESC메뉴
    public GameObject menuCanvas;
    [HideInInspector]
    public bool menuActive = false;

    private GameObject continueGO;
    private GameObject continueHighlightGO;

    private GameObject exitGO;
    private GameObject exitHighlightGO;

    private GameObject resetGO;
    private GameObject resetHighlightGO;

    private Slider volumeSlider;
    public AudioMixer audioMixer;

    // 바로 skipable되었을 때 클릭미스 안 되도록
    private int skipEnableTime = 0;

    void Awake() {
        if (instance == null) {
            gameObject.SetActive(false);
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void playStageMusic() {
        playbackObjSR.sprite = stopSprite;
        Color _color = playbackObjSR.color;
        _color.r = 1.0f;
        _color.g = 1.0f;
        _color.b = 1.0f;
        if (Stage.focused == 2) {
            if (_color.a < 1.0f) {
                _color.a = 1.0f;
            }
        }
        playbackObjSR.color = _color;
        playbackState = 2;

        skipEnableTime = 0;
    }
    public void stopStageMusic() {
        playbackObjSR.sprite = playSprite;
        Color _color = playbackObjSR.color;
        _color.r = 1.0f;
        _color.g = 1.0f;
        _color.b = 1.0f;
        if(Stage.focused == 2) {
            if (_color.a < 1.0f) {
                _color.a = 1.0f;
            }
        }
        playbackObjSR.color = _color;
        playbackState = 1;

        skipEnableTime = 0;
    }
    public void skipStageMusic() {

        
    }
    public void enableSkip() {
        playbackObjSR.sprite = skipSprite;
        Color _color = playbackObjSR.color;
        _color.r = 1.0f;
        _color.g = 0.3f;
        _color.b = 0.3f;
        playbackObjSR.color = _color;
        playbackState = 3;

        skipEnableTime = 30;
    }

    public void enableStageUI() {
        playbackObjCC.enabled = true;
        backObjCC.enabled = true;
    }

    public void disableStageUI() {
        playbackObjCC.enabled = false;
        backObjCC.enabled = false;
    }

    public void focusStage() {
        stageUIOpacityDelta = 0.04f;
    }
    public void blurStage() {
        stageUIOpacityDelta = -0.04f;
        disableStageUI();
    }

    // play UI
    public void playbackPointerEnter() {
        Color hdrGlowColor = playbackOriginalColor * Mathf.Pow(2, 1);
        playbackObjSR.material.SetColor("_GlowColor", hdrGlowColor);
    }
    public void playbackPointerExit() {
        Color hdrGlowColor = playbackOriginalColor * Mathf.Pow(2, 0);
        playbackObjSR.material.SetColor("_GlowColor", hdrGlowColor);
    }
    public void playbackPointerDown() {
        if (playbackState == 1) { // play
            Stage.currentStage?.play();
            playStageMusic();
        } else if (playbackState == 2) { // stop
            Stage.currentStage?.stop();
            stopStageMusic();
        } else if (playbackState == 3) { // skip
            if(skipEnableTime <= 0) {
                Stage.currentStage?.skip();
                skipStageMusic();
            }
        }
    }



    void Update() {

        if (Input.GetKeyDown(KeyCode.Escape)) {
            toggleMenu();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!spaceDown && !menuActive && Piece.currentlyDragging == null && Stage.focused == 2 && Stage.currentStage != null && !Stage.currentStage.clearing) {
                if (Stage.currentStage.playing) {
                    Stage.currentStage.stop();
                } else {
                    Stage.currentStage.play();
                }
            }
            spaceDown = true;
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            spaceDown = false;
        }

        UpdateObjectPosition(); // 고정되지 않은 UI 포지션이 카메라에 따라 이동

        float scaleMultiplier = mainCamera.orthographicSize / initialOrthographicSize;
        backObj.transform.localScale = initialScale * scaleMultiplier;
        playbackObj.transform.localScale = initialScale * scaleMultiplier;

        if (stageUIOpacityDelta != 0.0f) {
            if (stageUIOpacityDelta > 0.0f) {
                stageUIOpacity += stageUIOpacityDelta;
                if (stageUIOpacity >= 1.0f) {
                    stageUIOpacityDelta = 0.0f;
                    enableStageUI();
                }
            } else {
                stageUIOpacity += stageUIOpacityDelta;
                if (stageUIOpacity <= 0.0f) {
                    stageUIOpacityDelta = 0.0f;
                }
            }
            Color currentColor = playbackObjSR.color;
            if (stageUIOpacity < 0.7f) {
                currentColor.a = 0.0f;
            } else {
                currentColor.a = (stageUIOpacity - 0.7f) / 3.0f * 10.0f;
            }

            playbackObjSR.color = currentColor;
            backObjSR.color = currentColor;
        }

        if (Stage.focused == 2 && skipEnableTime > 0) {
            skipEnableTime--;
            Color _color = playbackObjSR.color;
            _color.a = Mathf.Pow((30.0f - (float)skipEnableTime) / 30.0f, 2f);
            playbackObjSR.color = _color;
        }
    }

    void OnDestroy() {
        volumeSlider.onValueChanged.RemoveListener(SetVolume);
    }

    public void initialize() {

        gameObject.SetActive(true);

        backObj = transform.Find("Back").gameObject;
        backObjSR = backObj.GetComponent<SpriteRenderer>();
        backObjCC = backObj.GetComponent<CircleCollider2D>();

        playbackObj = transform.Find("Playback").gameObject;
        playbackObjSR = playbackObj.GetComponent<SpriteRenderer>();
        playbackObjCC = playbackObj.GetComponent<CircleCollider2D>();

        playbackOriginalColor = playbackObjSR.color; // 기본 컬러를 저장
        playbackOriginalColor.a = 1.0f;

        playbackObjSR.sprite = playSprite;
        playbackState = 1;

        // 초기화
        mainCamera = Camera.main;
        initialOrthographicSize = mainCamera.orthographicSize; // 다른 클래스에서도 중복, 하지만 편의상 놔두기
        initialScale = new Vector3(1.5f, 1.5f, 1.0f);

        // 메뉴
        Transform _tr;

        _tr = menuCanvas.transform.Find("Panel").Find("Reset");
        resetGO = _tr.Find("Image").gameObject;
        resetHighlightGO = _tr.Find("ImageHighlight").gameObject;
        resetHighlightGO.SetActive(false);

        _tr = menuCanvas.transform.Find("Panel").Find("Exit");
        exitGO = _tr.Find("Image").gameObject;
        exitHighlightGO = _tr.Find("ImageHighlight").gameObject;
        exitHighlightGO.SetActive(false);

        volumeSlider = menuCanvas.transform.Find("Panel").Find("Volume").gameObject.GetComponent<Slider>();
        volumeSlider.onValueChanged.AddListener(SetVolume);

        float _vol = GameDataManager.instance.getVolume();
        volumeSlider.value = _vol;
        SetVolume(_vol);

        _tr = menuCanvas.transform.Find("Panel").Find("Continue");
        continueGO = _tr.Find("Image").gameObject;
        continueHighlightGO = _tr.Find("ImageHighlight").gameObject;
        continueHighlightGO.SetActive(false);

        menuCanvas.SetActive(false);
    }

    // 볼륨 설정 메서드
    void SetVolume(float volume) {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume * 1.3f + 0.001f) * 20);
        GameDataManager.instance.setVolume(volume);
    }

    void UpdateObjectPosition() {
        // 화면의 너비와 높이 가져오기
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (screenWidth == previousScreenWidth && screenHeight == previousScreenHeight) {
            return;
        }

        float marginLeft = (screenWidth - screenHeight / 9f * 16f) / 2f;
        float marginTop = (screenHeight - screenWidth / 16f * 9f) / 2f;
        if (marginLeft < 0f) {
            marginLeft = 0f;
        }
        if (marginTop < 0f) {
            marginTop = 0f;
        }

        if (screenWidth > screenHeight / 9f * 16f) {
            screenWidth = screenHeight / 9f * 16f;
        }
        if (screenHeight > screenWidth / 16f * 9f) {
            screenHeight = screenWidth / 16f * 9f;
        }

        //Screen.width

        // 화면 우하단 가장자리로부터 10% 안쪽 지점 계산
        Vector3 screenPosition = new Vector3(screenWidth * 0.95f + marginLeft, screenHeight * 0.1f + marginTop, mainCamera.nearClipPlane);

        // 화면 좌표를 월드 좌표로 변환
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        // z값을 0으로 고정 (2D 게임의 경우)
        worldPosition.z = 0;

        // 오브젝트 위치 설정
        playbackObj.transform.position = worldPosition;



        screenPosition = new Vector3(screenWidth * 0.95f + marginLeft, screenHeight * 0.9f + marginTop, mainCamera.nearClipPlane);
        worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0;
        backObj.transform.position = worldPosition;
    }

    public void toggleMenu() {

        if (FadeInEffect.isFading) {
            return;
        }
        if(Stage.focused == 1) {
            return;
        }
        if(Stage.focused == 0) {
            menuCanvas.transform.Find("Panel").Find("Reset").gameObject.SetActive(false);
        } else if(Stage.focused == 2) {
            menuCanvas.transform.Find("Panel").Find("Reset").gameObject.SetActive(true);
        }

        if(Piece.currentlyDragging != null) {
            Piece.currentlyDragging.drop();
        }

        // 메뉴 패널의 활성 상태를 토글
        if (menuActive) {
            menuActive = false;
            menuCanvas.SetActive(false);
            OnPointerExit_Exit(); // 마우스 올라간 상태로 닫힐 때 비활성화
            OnPointerExit_Continue(); // 마우스 올라간 상태로 닫힐 때 비활성화 대비
            OnPointerExit_Reset();
        } else {
            menuActive = true;
            menuCanvas.SetActive(true);
        }

        // 게임 일시정지
        if (menuCanvas.activeSelf) {
            Time.timeScale = 0f; // 게임 일시정지
        } else {
            Time.timeScale = 1f; // 게임 재개
        }
    }



    // 메뉴
    public void OnPointerEnter_Exit() {
        exitGO.SetActive(false);
        exitHighlightGO.SetActive(true);
    }

    public void OnPointerExit_Exit() {
        exitGO.SetActive(true);
        exitHighlightGO.SetActive(false);
    }

    public void OnPointerClick_Exit() {
        //GameDataManager.instance.deleteGameData();
        Application.Quit();
    }



    public void OnPointerEnter_Continue() {
        continueGO.SetActive(false);
        continueHighlightGO.SetActive(true);
    }

    public void OnPointerExit_Continue() {
        continueGO.SetActive(true);
        continueHighlightGO.SetActive(false);
    }

    public void OnPointerClick_Continue() {
        toggleMenu();
    }



    public void OnPointerEnter_Reset() {
        resetGO.SetActive(false);
        resetHighlightGO.SetActive(true);
    }

    public void OnPointerExit_Reset() {
        resetGO.SetActive(true);
        resetHighlightGO.SetActive(false);
    }

    public void OnPointerClick_Reset() {
        Stage.currentStage.reset();
        toggleMenu();
    }

    public void enableEnding() {
        foreach (Transform _tr in endingObj.transform.Find("T")) {
            _tr.gameObject.GetComponent<EndingNote>().enable();
        }
        foreach (Transform _tr in endingObj.transform.Find("H")) {
            _tr.gameObject.GetComponent<EndingNote>().enable();
        }
        foreach (Transform _tr in endingObj.transform.Find("E1")) {
            _tr.gameObject.GetComponent<EndingNote>().enable();
        }
        foreach (Transform _tr in endingObj.transform.Find("E2")) {
            _tr.gameObject.GetComponent<EndingNote>().enable();
        }
        foreach (Transform _tr in endingObj.transform.Find("N")) {
            _tr.gameObject.GetComponent<EndingNote>().enable();
        }
        foreach (Transform _tr in endingObj.transform.Find("D")) {
            _tr.gameObject.GetComponent<EndingNote>().enable();
        }
    }
    public void disableEnding() {
        foreach (Transform _tr in endingObj.transform.Find("T")) {
            _tr.gameObject.GetComponent<EndingNote>().disable();
        }
        foreach (Transform _tr in endingObj.transform.Find("H")) {
            _tr.gameObject.GetComponent<EndingNote>().disable();
        }
        foreach (Transform _tr in endingObj.transform.Find("E1")) {
            _tr.gameObject.GetComponent<EndingNote>().disable();
        }
        foreach (Transform _tr in endingObj.transform.Find("E2")) {
            _tr.gameObject.GetComponent<EndingNote>().disable();
        }
        foreach (Transform _tr in endingObj.transform.Find("N")) {
            _tr.gameObject.GetComponent<EndingNote>().disable();
        }
        foreach (Transform _tr in endingObj.transform.Find("D")) {
            _tr.gameObject.GetComponent<EndingNote>().disable();
        }
    }

}
