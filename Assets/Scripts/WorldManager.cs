using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
    public static WorldManager instance;

    [HideInInspector]
    public List<World> worlds;
    [HideInInspector]
    public List<int> notClearCounts;
    [HideInInspector]
    public int worldCount = 0;

    public GameObject backgroundStar;
    public GameObject backgroundStars;
    public int backgroundCount = 0;
    public int seed = 42;

    void Awake() {
        if (instance == null) {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    public void initialize() {

        // 배경 별 만들기
        Random.InitState(seed);
        for(int i=0;i<backgroundCount;i++) {
            GameObject _star = Instantiate(backgroundStar);
            _star.transform.SetParent(backgroundStars.transform);
        }


        gameObject.SetActive(true);

        worlds = new List<World>();
        notClearCounts = new List<int>();
        
        foreach (Transform child in transform) {

            World cp = child.gameObject.GetComponent<World>();
            if (cp == null) {
                continue;
            }
            if(cp.worldNumber == 0) {
                continue;
            }

            worldCount++;

            child.localRotation = Quaternion.Euler(0f, 0f, 360f - ((float)(cp.worldNumber - 1) * 360f / (float)transform.childCount));
            while (worlds.Count <= cp.worldNumber) {
                worlds.Add(null);
                notClearCounts.Add(5);
            }
            worlds[cp.worldNumber] = cp;
            cp.initialize();

            //child.rotation = Quaternion.Euler(0f, 0f, 360f - ((float)(cp.worldNumber - 1) * 360f / (float)transform.childCount));
        }
    }

}