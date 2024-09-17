using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public bool generating = false;
    private bool isSteamInitialized = false;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

            try {
                if (!SteamAPI.Init()) {
                    Debug.LogWarning("SteamAPI_Init() failed.");
                    return;
                }
            } catch (System.DllNotFoundException e) {
                Debug.LogError(e.Message + " - Steamworks DLL is missing.");
                return;
            }
            isSteamInitialized = true;

        } else {
            Destroy(gameObject);
        }

    }

    void Update() {
        if (isSteamInitialized) {
            SteamAPI.RunCallbacks();
        }
    }
    
    void Start() {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        StartCoroutine(WairForInitialization());
    }

    void OnDestroy() {
        if (isSteamInitialized) {
            SteamAPI.Shutdown();
        }
    }

    private IEnumerator WairForInitialization() {
        while (GameDataManager.instance == null) {
            yield return null;
        }
        GameDataManager.instance.initialize();
        
        while(AudioManager.instance == null) {
            yield return null;
        }
        AudioManager.instance.initialize();
        
        while (WorldManager.instance == null) {
            yield return null;
        }
        WorldManager.instance.initialize();
        
        while (UIManager.instance == null) {
            yield return null;
        }
        UIManager.instance.initialize();
        
        WorldDataManager.initialize(generating);
        
    }

    public void UnlockAchievement(string achievementID) {
        if (!isSteamInitialized) {
            return;
        }
        if (SteamUserStats.SetAchievement(achievementID)) {
            Debug.LogWarning(achievementID + " 등록 성공");
        } else {
            Debug.LogWarning(achievementID + " 등록 실패");
        }
        SteamUserStats.StoreStats();
    }

}
