using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class WorldDataManager {

    //public static bool debug = true; // 아직 안 씀
    public static bool generating = false;

    public static List<WorldData> worldData;

    public static void saveStageData() {
        string json = JsonConvert.SerializeObject(worldData, Formatting.Indented);
        File.WriteAllText(Application.dataPath + "/stageData.json", json);
    }
    public static void loadStageData() {
        string path = Application.dataPath + "/stageData.json";
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);
            worldData = JsonConvert.DeserializeObject<List<WorldData>>(json);
        } else {
            worldData = new List<WorldData>();
            //Application.Quit();
            Debug.Log("stageData.json 파일이 존재하지 않습니다.");
        }
    }

    public static StageData getCurrentStageData() {
        return worldData[World.currentWorld.worldNumber].stageData[Stage.currentStage.stageNumber];
    }

    public static void initialize(bool gen) {

        loadStageData();

        generating = gen;
        if (!generating) {
            return;
        }

        int transpose = 0;
        // 타겟 월드, 스테이지
        int worldNumber = 1;
        int stageNumber = 1;



        transpose = 7;
        // 프레임을 몇 배율로 키우기 : 클 수록 더 느림
        float frameMultiplier = 18.0f;
        List<string> debugString = new List<string>();

        debugString.Add(@"
2

1C4
1
1C4
1
1C4
1D4
1E4
1
1E4
1D4
1E4
1F4
1G4
1
1
1
1C5
1C5
1G4
1G4
1E4
1E4
1C4
1C4
1G4
1F4
1E4
1D4
1C4
1
1
1

");

        while (worldData.Count <= worldNumber) {
            worldData.Add(new WorldData());

        }
        while (worldData[worldNumber].stageData.Count <= stageNumber) {
            worldData[worldNumber].stageData.Add(new StageData());
        }

        int totalFrameRounded = 0;
        float totalFrame = 0.0f;

        int maxFrame = 0; // 혹시나 pitchOffset으로 인해 넘어갈 수 있어서?

        List<NoteData> noteList = new List<NoteData>();

        int pieceIndex = -1;
        foreach (string ss in debugString) {
            pieceIndex++;

            totalFrame = 0.0f;

            string[] lines = ss.Split(new[] { '\n' }, StringSplitOptions.None);
            foreach (string line_ in lines) { // 각 노트마다

                string line = line_.Trim();
                if (line.Length == 0) {
                    continue;
                }

                int pitch = 0;
                float length = 0.0f;
                int pitchOffset = 0;

                // 첫 번째는 무조건 숫자(길이)
                if (!Char.IsDigit(line[0])) {
                    continue;
                }

                int pitchStart = 0;
                while (true) {
                    if (pitchStart >= line.Length || (!Char.IsDigit(line[pitchStart]) && line[pitchStart] != '.')) { // 숫자가 아닐 때가 되면
                        length = float.Parse(line.Substring(0, pitchStart));
                        break;
                    }
                    pitchStart++;
                }

                while (pitchStart < line.Length && (line[pitchStart] == ' ' || line[pitchStart] == ',')) { // 혹시나 나중에 빈 칸이나 comma 넣을 수도
                    pitchStart++;
                }

                int i = pitchStart;

                if (i < line.Length) {

                    if (line[i] == 'A') {
                        pitch = 21;
                    } else if (line[i] == 'B') {
                        pitch = 23;
                    } else if (line[i] == 'C') {
                        pitch = 12;
                    } else if (line[i] == 'D') {
                        pitch = 14;
                    } else if (line[i] == 'E') {
                        pitch = 16;
                    } else if (line[i] == 'F') {
                        pitch = 17;
                    } else if (line[i] == 'G') {
                        pitch = 19;
                    }

                    i++;
                    if (line[i] == 'b') { // 플랫이나 샵이 없음
                        pitch--;
                        i++;
                    } else if (line[i] == '#') {
                        pitch++;
                        i++;
                    }

                    pitch += ((int)(line[i] - '0')) * 12;

                    i++;
                    pitchOffset = 0;
                    while (i < line.Length) {
                        if (line[i] == '+') {
                            while (i < line.Length) {
                                if (Char.IsDigit(line[i])) {
                                    pitchOffset *= 10;
                                    pitchOffset += line[i] - '0';
                                }
                                i++;
                            }
                        } else if (line[i] == '-') {
                            while (i < line.Length) {
                                if (Char.IsDigit(line[i])) {
                                    pitchOffset *= 10;
                                    pitchOffset += line[i] - '0';
                                }
                                i++;
                            }
                            pitchOffset = -pitchOffset;
                        } else {
                            i++;
                        }
                    }

                }

                int currentFrame = (int)Math.Round(totalFrame * frameMultiplier) + pitchOffset; // 이 노트의 실제 프레임 정수값
                maxFrame = Math.Max(maxFrame, currentFrame);
                if(pitch > 0) {
                    noteList.Add(new NoteData(currentFrame, pitch+transpose, pieceIndex));
                }
                totalFrame += length;

            }

        }

        totalFrameRounded = maxFrame + 20;
        //Debug.Log(totalFrameRounded);

        noteList = noteList.OrderBy(item => item.frame).ToList();

        StageData cStageData = new StageData();

        cStageData.rawNote = debugString;
        cStageData.notes = noteList;
        cStageData.transpose = 0; // no used
        cStageData.totalFrame = totalFrameRounded;

        //List<PieceRaw> pieces = new List<PieceRaw>();
        //pieces.Add(new PieceRaw());

        worldData[worldNumber].stageData[stageNumber] = cStageData;
        saveStageData();

    }

}

[System.Serializable]
public class WorldData {
    public List<StageData> stageData;

    public WorldData() {
        stageData = new List<StageData>();
    }
}

[System.Serializable]
public class StageData {
    public List<string> rawNote;
    public List<NoteData> notes;
    //public List<PieceRaw> pieces;
    public int transpose = 0;
    public int totalFrame = 0;
}

[System.Serializable]
public class NoteData {

    public int frame; // 몇 번째 프레임에서 연주
    public int pitch;
    public int pieceIndex;
    public float x;
    public float y;
    public float positionX;
    public float positionY;

    public NoteData(int _frame, int _pitch, int _pieceIndex) {
        frame = _frame;
        pitch = _pitch;
        pieceIndex = _pieceIndex;
    }

}

[System.Serializable]
public class PieceRaw {
    public int length;
    public int velocity;
    public int angle;
    public float x;
    public float y;

    public PieceRaw(int _length, int _velocity, int _angle, float _x, float _y) {
        length = _length;
        velocity = _velocity;
        angle = _angle;
        x = _x;
        y = _y;
    }
}
//public class NoteComparer : IComparer<Note> {
//    public int Compare(Note a, Note b) {
//        if (a.y == b.y) {
//            return b.x.CompareTo(a.x); // x 내림차순
//        }
//        return a.y.CompareTo(b.y); // y 오름차순
//    }
//}