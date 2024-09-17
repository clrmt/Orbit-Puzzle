using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    public int worldNumber;
    public static World currentWorld = null;

    private List<Stage> stages;
    [HideInInspector]
    public int stageCount = 0;

    // private bool needUpdate = false;

    private SpriteRenderer constellationSR;
    //private float constellationAlpha = 1.0f;
    private float constellationAlphaBase = 1.0f;
    private float constellationAlphaMultiplier = 1.0f;

    public void initialize() {
        constellationSR = transform.Find("constellation").gameObject.GetComponent<SpriteRenderer>();
        stages = new List<Stage>();
        
        foreach (Transform child in transform.Find("Stages")) {
            Stage cp = child.gameObject.GetComponent<Stage>();
            if (cp == null) {
                continue;
            }
            stageCount++;
            while (stages.Count <= cp.stageNumber) {
                stages.Add(null);
            }
            stages[cp.stageNumber] = cp;
            cp.initialize(this);
        }
        updateConstellationAlphaBase();
        Color _color = constellationSR.color;
        _color.a = Mathf.Pow(constellationAlphaMultiplier, 5f);
        constellationSR.material.SetColor("_GlowColor", _color * constellationAlphaBase);
    }

    void Update() {
        if (Stage.focused > 0) {
            if (constellationAlphaMultiplier > 0.0f) {
                constellationAlphaMultiplier -= 0.008f;
                Color _color = constellationSR.color;
                _color.a = Mathf.Pow(constellationAlphaMultiplier, 4f);
                constellationSR.material.SetColor("_GlowColor", _color * constellationAlphaBase);
            }
        } else {
            if (constellationAlphaMultiplier < 1.0f) {
                constellationAlphaMultiplier += 0.012f;
                Color _color = constellationSR.color;
                _color.a = Mathf.Pow(constellationAlphaMultiplier, 4f);
                constellationSR.material.SetColor("_GlowColor", _color * constellationAlphaBase);
            }
        }

    }

    public void updateConstellationAlphaBase() {
        int clearCount = 0;
        int notClearCount = 0;
        for(int i = 1;i<=stageCount;i++) {
            if(GameDataManager.instance.getStageState(worldNumber, i) == 1) {
                clearCount++;
            } else {
                notClearCount++;
            }
        }
        int availableCount = 0;
        for (int i = 1; i <= stageCount; i++) {
            if (GameDataManager.instance.getStageState(worldNumber, i) == -1) {
                availableCount++;
            }
        }

        if (clearCount == stageCount) {
            //constellationAlphaBase = 0.2f;
            constellationAlphaBase = Mathf.Pow(2f, 0.3f);
        } else if(availableCount == 0 && clearCount == 0){
            constellationAlphaBase = 0.0f;
        } else {
            constellationAlphaBase = 0.2f;
        }

        WorldManager.instance.notClearCounts[worldNumber] = notClearCount;

        int sum = 0;
        for(int i = 1; i < WorldManager.instance.worlds.Count; i++) {
            for(int j = 1; j <= 5; j++) {
                sum += GameDataManager.instance.getStageState(i, j);
            }
        }
        
        if (sum == 35) {
            GameManager.instance.UnlockAchievement("ACHIEVEMENT_COMPLETE");
        }

    }
}