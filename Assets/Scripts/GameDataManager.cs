using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GameDataManager : MonoBehaviour {
    public static GameDataManager instance;

    private string filePath;
    [HideInInspector]
    public GameData gameData;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    public void initialize() {
        gameObject.SetActive(true);

        filePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        LoadGameData();
    }

    public void SaveGameData() {
        string json = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    public void LoadGameData() {
        if(File.Exists(filePath)) {
            string json = File.ReadAllText(filePath);
            gameData = JsonConvert.DeserializeObject<GameData>(json);
        } else {
            gameData = new GameData();
        }
    }
    // 이 안에서 사용되면 절대 안 됨!!! 순서 문제 때문!!!
    // -1 가능, 0 불가, 1 클리어
    public int getStageState(int world, int stage) {
        while (gameData.stageCleared.Count <= WorldManager.instance.worldCount) {
            gameData.stageCleared.Add(new List<int>());
        }
        while (gameData.stageCleared.Count <= world) {
            gameData.stageCleared.Add(new List<int>());
        }
        while (gameData.stageCleared[world].Count <= WorldManager.instance.worlds[world].stageCount) {
            gameData.stageCleared[world].Add(0);
        }
        while (gameData.stageCleared[world].Count <= stage) {
            gameData.stageCleared[world].Add(0);
        }
        if (gameData.stageCleared[world][stage] == 1) { // 클리어된 상태라면
            return 1;
        } else {
            // 스테이지 진입 가능한 상태인가?
            if (world == 1) {
                if (2 <= stage && stage <= 5) {
                    if (gameData.stageCleared[world][stage - 1] == 1) { // 이전 스테이지가 클리어된 상태라면
                        return -1;
                    }
                } else if (stage == 1) {
                    return -1;
                }
            } else if (world == 7 && stage >= 2) {
                if (gameData.stageCleared[world][stage - 1] == 1) {
                    return -1;
                }
            } else {
                int clearCount = 0;
                for (int i = 1; i <= WorldManager.instance.worlds[world].stageCount; i++) {
                    if (gameData.stageCleared[world - 1].Count >= i && gameData.stageCleared[world - 1][i] == 1) {
                        clearCount++;
                    }
                }
                if(world == 2) {
                    if (clearCount >= 5) {
                        return -1;
                    }
                } else {
                    if (clearCount >= 4) {
                        return -1;
                    }
                }
            }
        }
        return 0;
    }

    public float getVolume() {
        if(gameData == null) {
            return 0.5f;
        }
        return gameData.settingSoundVolume;
    }
    public void setVolume(float vol) {
        if(gameData == null) {
            return;
        }
        gameData.settingSoundVolume = vol;
    }

    public void deleteGameData() {
        gameData.stageCleared = new List<List<int>>();
    }

    void OnDestroy() {
        SaveGameData();
    }

    public void clearCurrentStage() {
        while(gameData.stageCleared.Count <= World.currentWorld.worldNumber) {
            gameData.stageCleared.Add(new List<int>());
        }
        while (gameData.stageCleared[World.currentWorld.worldNumber].Count <= Stage.currentStage.stageNumber) {
            gameData.stageCleared[World.currentWorld.worldNumber].Add(0);
        }
        gameData.stageCleared[World.currentWorld.worldNumber][Stage.currentStage.stageNumber] = 1;
        foreach(World _w in WorldManager.instance.worlds) {
            if(_w != null) {
                _w.updateConstellationAlphaBase();
            }
        }
    }
}

[System.Serializable]
public class GameData {
    public float settingSoundVolume;
    public List<List<int>> stageCleared;

    public GameData() {
        settingSoundVolume = 0.5f;
        stageCleared = new List<List<int>>();
    }
}
