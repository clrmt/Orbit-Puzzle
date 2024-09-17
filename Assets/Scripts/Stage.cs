using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour {

    [HideInInspector]
    public bool cleared = false;
    [HideInInspector]
    public bool playing = false;
    [HideInInspector]
    public bool glittering = false;

    public static int previousFocused = 0;
    public static int focused = 0; // 0: not focused, 1: focusing/blurring, 2: focused

    public int stageNumber;
    [HideInInspector]
    public Star star;
    [HideInInspector]
    public World world;

    [HideInInspector]
    public List<Piece> pieces;
    [HideInInspector]
    public GameObject piecesObj;
    [HideInInspector]
    public Piece rootPiece;

    [HideInInspector]
    public int frame; // ����ÿ� ������ ī����
    private int noteIndex; // �� ��° ��Ʈ���� �����߳�
    [HideInInspector]
    public GameObject notesObj;
    private List<Note> notes;
    private int notesPlayed = 0;
    public GameObject notePrefab;

    [HideInInspector]
    public static Stage currentStage = null;

    private int playTarget; // �� ���� Piece�� hit�ؾ� Ŭ���� ������ ���������� �ʱ�ȭ
    private int playCount;

    public int pitch = 40;

    [HideInInspector]
    public bool clearing = false;

    public void initialize(World _w) {
        world = _w;

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        //focused = 0;
        pieces = new List<Piece>();
        star = transform.Find("Star").gameObject.GetComponent<Star>();
        piecesObj = transform.Find("Pieces").gameObject;
        piecesObj.SetActive(false);
        notesObj = transform.Find("Notes").gameObject;
        notes = new List<Note>();

        star.initialize(this);
        foreach (Transform child in transform.Find("Pieces")) {
            Piece cp = child.gameObject.GetComponent<Piece>();
            if (cp == null) {
                continue;
            }

            pieces.Add(cp);
            if (cp.isRoot) {
                rootPiece = cp;
            }
            cp.initialize(this);
            if(cp.index >= 0) { // ���� ����� ���̴� piece�� ���
                playTarget++; // �� ���� piece�� ����Ǿ�� Ŭ���� ������ ����
            }
        }

    }

    void Update() {

        if (playing) {
            frame++;
            rootPiece.updateTransform();

            StageData cStageData = WorldDataManager.getCurrentStageData();

            if (frame >= WorldDataManager.getCurrentStageData().totalFrame) {
                // �Ϸ�����

                if (WorldDataManager.generating) { // ���� ��Ʈ ��ġ �� ������
                    WorldDataManager.saveStageData();
                    stop(1);
                } else {
                    if(world.worldNumber == 7 && stageNumber == 5) {
                        stop(1);
                        AudioManager.instance.playMusic();
                        UIManager.instance.enableEnding();
                        GameDataManager.instance.clearCurrentStage();
                        world.updateConstellationAlphaBase();

                        GameManager.instance.UnlockAchievement("ACHIEVEMENT_CLEAR");
                    } else if (notesPlayed != cStageData.notes.Count || GameDataManager.instance.getStageState(world.worldNumber, stageNumber) == 1) { // ��� ������ ��� �Ǵ� Ŭ����� ��Ȳ
                        stop(1);
                    } else {
                        clear(); 
                    }
                }
            } else {
                if (WorldDataManager.generating) {
                    while (noteIndex < cStageData.notes.Count) {
                        //Debug.Log("fra: " + frame.ToString() + " " + cStageData.notes[noteIndex].frame.ToString() + " " + frame.ToString());
                        if (cStageData.notes[noteIndex].frame > frame) {
                            break;
                        }

                        foreach (Piece _p in pieces) {
                            if (_p.index == cStageData.notes[noteIndex].pieceIndex) {
                                GameObject _obj = _p.trailNote(); // �����ֱ��
                                AudioManager.instance.playNote(cStageData.notes[noteIndex].pitch);
                                cStageData.notes[noteIndex].x = _p.x;
                                cStageData.notes[noteIndex].y = _p.y;
                                cStageData.notes[noteIndex].positionX = _obj.transform.localPosition.x;
                                cStageData.notes[noteIndex].positionY = _obj.transform.localPosition.y;
                                //Debug.Log(_obj.transform.localPosition);
                                break;
                            }
                        }
                        noteIndex++;
                    }
                } else {
                    foreach (Piece _p in pieces) {
                        // �� ��Ʈ�� hit�ϴ°�?
                        foreach (Note _n in notes) {
                            if (!_n.played) {
                                if (_p.x == _n.x && _p.y == _n.y) {
                                    _n.Play();
                                    if (!_p.played) {
                                        playCount++;
                                        if(playCount == playTarget) {
                                            // todo
                                            // �������� �����Ǹ�
                                            if(GameDataManager.instance.getStageState(world.worldNumber, stageNumber) == -1) {
                                                UIManager.instance.enableSkip();
                                            }
                                        }
                                    }
                                    _p.Play();
                                    notesPlayed++;
                                }
                            }
                        }
                    }

                }
            }

        } else if(Piece.currentlyDragging) {

            // Ŭ���� ������ ���¶�� Star ��¦��¦
            if (GameDataManager.instance.getStageState(world.worldNumber, stageNumber) == -1) {
                star.needUpdate = true;
            }

        }

    }

    IEnumerator playFocusSound() {
        // ù ��° ���� ����
        AudioManager.instance.playNote(pitch + 30);
        yield return new WaitForSeconds(0.04f); // 0.1�� ���

        // �� ��° ���� ����
        AudioManager.instance.playNote(pitch + 34);
        yield return new WaitForSeconds(0.08f); // 0.1�� ���

        // �� ��° ���� ����
        AudioManager.instance.playNote(pitch + 37);

    }
    public void focus() {
        if (focused > 0) { return; }
        if(GameDataManager.instance.getStageState(world.worldNumber, stageNumber) == 0) {
            return;
        }
        focused = 1;

        StartCoroutine(playFocusSound());

        piecesObj.SetActive(true);
        currentStage = this;
        World.currentWorld = world;

        star.disableCollider();
        star.needUpdate = true;

        Vector3 pos = new Vector3(transform.position.x, transform.position.y, Camera.main.gameObject.transform.position.z);
        Camera.main.gameObject.GetComponent<CameraController>().moveToPosition(pos, 2.0f, 0.3f, focusEnd);

        notes = new List<Note>();
        // notes �����
        if (!WorldDataManager.generating) {
            StageData cStageData = WorldDataManager.getCurrentStageData();
            foreach (NoteData cNote in cStageData.notes) {
                GameObject oTmp = Instantiate(notePrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                oTmp.transform.SetParent(notesObj.transform, false);
                Vector3 _pos = new Vector3(cNote.positionX, cNote.positionY, 0.0f);

                oTmp.transform.localPosition = _pos;
                oTmp.transform.localScale = new Vector3(0.125f, 0.125f, 0.125f);

                Note _n = oTmp.GetComponent<Note>();
                _n.pitch = cNote.pitch;
                _n.x = cNote.x;
                _n.y = cNote.y;

                notes.Add(_n);
            }

        }

    }

    public void focusEnd() {
        focused = 2;
        previousFocused = 2;
        UIManager.instance.focusStage();
        foreach (Piece _p in pieces) {
            _p.enable();
        }
    }

    // ��ŵ ������ ��� ��ŵ
    public void skip() {
        if(focused != 2) {
            return;
        }
        StageData cStageData = WorldDataManager.getCurrentStageData();
        notesPlayed = cStageData.notes.Count;
        foreach (Note _n in notes) {
            _n.played = true;
        }
        frame = 0;
        rootPiece.updateTransform();

        clear();
    }
    


    // �� �������� ���� Ŭ����
    public void clear() {
        clearing = true;

        playing = false;
        star.needUpdate = true;
        UIManager.instance.blurStage();
        GameDataManager.instance.clearCurrentStage();
        world.updateConstellationAlphaBase();
        foreach (Piece cp in currentStage.pieces) {
            cp.disable();
            cp.Stop();
            cp.enableEditingGraphic();
        }

        StartCoroutine(clear2());
    }
    IEnumerator clear2() {
        yield return new WaitForSeconds(1.4f);
        blur(1);
    }

    public void blur() {
        blur(0);
    }

    public void blur(int mode) {
        if (focused != 2) {
            return;
        }

        StageData cStageData = WorldDataManager.getCurrentStageData();
        if (notesPlayed < cStageData.notes.Count || playing) {
            stop();
        }
        focused = 1;

        star.needUpdate = true;

        UIManager.instance.blurStage();
        foreach (Piece _p in pieces) {
            _p.disable();
        }

        Vector3 pos = new Vector3(0.0f, 0.0f, Camera.main.gameObject.transform.position.z);
        if (mode == 1) {
            Camera.main.gameObject.GetComponent<CameraController>().moveToPosition(pos, 3.0f, 5.0f, blurEnd);
        } else {
            Camera.main.gameObject.GetComponent<CameraController>().moveToPosition(pos, 2.0f, 5.0f, blurEnd);
        }
    }

    public void blurEnd() {
        clearing = false;

        focused = 0;
        previousFocused = 0;

        StageData cStageData = WorldDataManager.getCurrentStageData();
        if (notesPlayed == cStageData.notes.Count) {
            stop();
        }
        foreach (Piece _p in pieces) {
            _p.disable();
        }

        currentStage = null;
        World.currentWorld = null;

        star.enableCollider();
        piecesObj.SetActive(false);

        // Note ����
        foreach (Note _n in notes) {
            Destroy(_n.gameObject);
        }
    }

    public void play() {
        if (focused < 2) { // Stage�� ���� ��Ŀ���� �� �Ǿ��ٸ�
            return;
        }
        if (playing) { // ������̶��...
            stop();
        }

        // ���� ������̶��
        AudioManager.instance.stopMusic();
        UIManager.instance.disableEnding();

        noteIndex = 0;
        notesPlayed = 0; // ��Ʈ ��� ����
        playCount = 0; // piece ��� ���� ��
        frame = 0;
        playing = true;
        if (glittering) {
            resetNotes();
        }
        glittering = true;
        UIManager.instance.playStageMusic();

        // ��Ʈ�������� ã�� �� ���� �ֵ���... disable
        foreach (Piece cp in currentStage.pieces) {
            cp.visit = 0;
            cp.disableEditingGraphic();
        }
        rootPiece.visit = 1;
        bool done = false;
        while (!done) {
            done = true;
            foreach (Piece cp in currentStage.pieces) {
                if (cp.visit == 0 && cp.parent?.visit == 1) {
                    done = false;
                    cp.visit = 1;
                }
            }
        }
        foreach (Piece cp in currentStage.pieces) {
            if (cp.visit == 0) {
                cp.disable();
            }
        }

    }
    
    public void stop(int mode) {
        playing = false;
        frame = 0;
        noteIndex = 0;
        rootPiece.updateTransform();
        UIManager.instance.stopStageMusic();

        if(world.worldNumber == 7 && stageNumber == 5) {
            AudioManager.instance.stopMusic();
            UIManager.instance.disableEnding();
        }

        // ���� �巡�� ���� ��
        foreach (Piece cp in currentStage.pieces) {
            cp.enable();
            cp.Stop();
            cp.enableEditingGraphic();
        }

        if(mode == 1) {
            // pass
        } else {
            if (glittering) {
                glittering = false;
                resetNotes();
            }
        }
    }
    public void stop() {
        stop(0);
    }

    public void reset() {
        stop();
        foreach (Piece _p in pieces) {
            _p.reset();
        }
    }

    public void resetNotes() {
        foreach (Note _n in notes) {
            _n.played = false;
        }
    }

    public void enableDraggingGlow() {

        // �巡�� ������ �ֵ� ��¦��¦
        foreach (Piece cp in currentStage.pieces) {
            cp.visit = 0;
        }
        rootPiece.visit = 1;
        bool done = false;
        while (!done) {
            done = true;
            foreach (Piece cp in currentStage.pieces) {
                if (cp != Piece.currentlyDragging && cp.visit == 0 && cp.parent?.visit == 1) {
                    done = false;
                    cp.visit = 1;
                }
            }
        }
        foreach (Piece cp in currentStage.pieces) {
            if (cp.visit == 1) {
                if (Piece.currentlyDragging != cp) {
                    cp.draggingGlow = true;
                }
            }
        }

    }

}