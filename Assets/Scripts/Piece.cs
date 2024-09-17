using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler {

    [HideInInspector]
    public bool isReady = false;

    public static Piece currentlyMouseOver = null;
    public static Piece currentlyDragging = null;

    [HideInInspector]
    public Stage stage;

    [HideInInspector]
    public Piece parent = null;
    public bool isRoot;
    public int length;
    public int velocity;
    public int angle;
    private float inheritAngle = 0.0f;

    private bool isMouseOver = false;
    private Vector3 offset;
    private Camera mainCamera;

    private Material material;
    private Color originalColor;
    private Color disabledColor;

    private GameObject oHead;
    [HideInInspector]
    public SpriteRenderer oHeadSpriteRenderer;
    private GameObject oBody;
    private List<GameObject> oSpine;
    private GameObject oFoot;
    [HideInInspector]
    public SpriteRenderer oFootSpriteRenderer;
    private List<GameObject> oToe;
    private List<SpriteRenderer> aSpriteRenderer;

    public GameObject bodyPrefab;
    public GameObject toePrefab;
    public GameObject nailPrefab;
    public GameObject notePrefab;

    private float footZRotate = 0.0f;

    public int index = -1; // ���� ������ �� �� ��° �� ��Ʈ���� ��������
    [HideInInspector]
    public bool played = false; // ����� ���� �ִ� piece�� ���� ������
    [HideInInspector]
    public int visit = 1; // Piece ��ȸ�� �� �ߺ� �湮���� �ʵ���

    [HideInInspector]
    public float x = 0.0f; // foot�� ���� ��ġ���ƴ� ����ġ(��Ȯ�� �䱸)
    [HideInInspector]
    public float y = 0.0f; // foot�� ���� ��ġ���ƴ� ����ġ(��Ȯ�� �䱸)

    private int hitGlow = -1;

    public bool draggingGlow = false;

    private Vector3 initialPosition;

    public void Play() {
        if (hitGlow <= 0) {
            Color hdrGlowColor = originalColor * Mathf.Pow(2f, 1.4f);
            oFootSpriteRenderer.material.SetColor("_GlowColor", hdrGlowColor);
            //foreach (GameObject cObj in oToe) {
            //    cObj.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
            //}
        }
        hitGlow = 5;
        played = true;
    }
    public void Stop() {
        hitGlow = -1;
        oFootSpriteRenderer.material.SetColor("_GlowColor", originalColor);
        //foreach (GameObject cObj in oToe) {
        //    cObj.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", originalColor);
        //}
        played = false;
    }

    public GameObject trailNote() { // ���� ���� �� ����� ������ ���� �����
        GameObject oTmp;
        oTmp = Instantiate(notePrefab, oFoot.transform.position, Quaternion.identity);
        oTmp.transform.SetParent(stage.notesObj.transform);
        oTmp.transform.localScale = new Vector3(0.125f, 0.125f, 0.125f);
        return oTmp;
    }

    public void initialize(Stage _stage) {

        stage = _stage;

        initialPosition = transform.position;

        if (isRoot) {
            length = 0;
            velocity = 0;
            angle = 0;
        }

        played = false;

        oHead = transform.Find("Head").gameObject;
        oHeadSpriteRenderer = oHead.GetComponent<SpriteRenderer>();
        oBody = transform.Find("Body").gameObject;
        oFoot = transform.Find("Foot").gameObject;
        oFootSpriteRenderer = oFoot.GetComponent<SpriteRenderer>();

        aSpriteRenderer = new List<SpriteRenderer>();
        aSpriteRenderer.Add(oHeadSpriteRenderer);
        aSpriteRenderer.Add(oFootSpriteRenderer);

        GameObject oTmp;

        // ����
        const float _BodyOffset = 0.2f;
        const float _BodyInterval = 0.2f;

        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (float)angle);

        oSpine = new List<GameObject>();

        // ���̿� ���� body�� �κ� ����
        for (int i = 1; i < length; i++) {
            oTmp = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
            oTmp.transform.SetParent(oBody.transform);
            oTmp.transform.localPosition = new Vector3(_BodyOffset + (float)i * _BodyInterval, 0, 0);
            oTmp.transform.localRotation = Quaternion.Euler(0, 0, 0);
            oTmp.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            aSpriteRenderer.Add(oTmp.GetComponent<SpriteRenderer>());
            oSpine.Add(oTmp);
        }

        // ���� �� ����
        oFoot.transform.localPosition = new Vector3(_BodyOffset + (float)length * _BodyInterval, 0, 0);
        oFoot.transform.localRotation = Quaternion.Euler(0, 0, 0);
        oFoot.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        oToe = new List<GameObject>();

        /*
        // �߿� ... �߰��� ... ����
        for (int i = 0; i < Math.Abs(velocity); i++) {
            oTmp = Instantiate(toePrefab, Vector3.zero, Quaternion.identity);
            //0717 oTmp.transform.SetParent(oFoot.transform);
            oTmp.transform.SetParent(oHead.transform);
            oTmp.transform.localPosition = new Vector3(0f, 0f, 0f);
            oTmp.transform.localRotation = Quaternion.Euler(0, 0, 360.0f / (float)velocity * (float)i);
            oTmp.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            aSpriteRenderer.Add(oTmp.GetComponent<SpriteRenderer>());
            oToe.Add(oTmp);
        }
        */

        // �߿� �߰��� ����(12����)
        if (velocity > 12 || velocity < -12) {
            Debug.LogError("Piece�� velocity�� 12�� �Ѿ�ϴ�.");
            return;
        }

        if (!isRoot) {
            for (int i = 0; i < 12; i++) {

                bool _isToe = false;
                // �־���� ������ üũ
                for (int j = 0; j < Math.Abs(velocity); j++) {
                    if ((int)Mathf.Round((float)j / (float)Math.Abs(velocity) * 12.0f) == i) {
                        _isToe = true;
                        break;
                    }
                }
                if (_isToe) {
                    oTmp = Instantiate(toePrefab, Vector3.zero, Quaternion.identity);
                } else {
                    oTmp = Instantiate(nailPrefab, Vector3.zero, Quaternion.identity);
                }
                //0717 oTmp.transform.SetParent(oFoot.transform);
                oTmp.transform.SetParent(oHead.transform);
                oTmp.transform.localPosition = new Vector3(0f, 0f, 0f);
                oTmp.transform.localRotation = Quaternion.Euler(0, 0, 360.0f / 12.0f * (float)i);
                oTmp.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                aSpriteRenderer.Add(oTmp.GetComponent<SpriteRenderer>());
                oToe.Add(oTmp);
            }
        }



        mainCamera = Camera.main;

        originalColor = oFootSpriteRenderer.color; // Foot Color�� ����...
        disabledColor = originalColor;
        disabledColor.a = 0.1f;

        // �ϴ� ���� alpha 0����
        foreach (SpriteRenderer _sr in aSpriteRenderer) {
            _sr.color = disabledColor;
        }

        if (isRoot) {
            oHead.GetComponent<SpriteRenderer>().enabled = false;
            oFootSpriteRenderer.enabled = false;
            oFoot.GetComponent<CircleCollider2D>().enabled = false;
            gameObject.transform.localPosition = new Vector3(.0f, .0f, .0f);
            oFoot.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        }

    }

    public void enable() {
        isReady = true;
        foreach (SpriteRenderer _sr in aSpriteRenderer) {
            _sr.color = originalColor;
        }
    }
    public void disable() {
        isReady = false;
        foreach (SpriteRenderer _sr in aSpriteRenderer) {
            _sr.color = disabledColor;
        }
    }

    public void updateTransform() {
        if (parent != null) {
            this.gameObject.transform.position = parent.oFoot.transform.position;

            // 0~1 float
            float additionalAngle = (float)(stage.frame * velocity) / (float)WorldDataManager.getCurrentStageData().totalFrame;

            // 0~360 float
            inheritAngle = additionalAngle * 360.0f + parent.inheritAngle;
            float cAngle = (float)angle + inheritAngle;
            this.gameObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, cAngle);
            oHead.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, cAngle);

            // ���� x, y ���(transform�� ����� �Ź� �޶��� �� ������ �̰����� ��ü)
            x = parent.x + (float)length * Mathf.Cos(cAngle);
            y = parent.y + (float)length * Mathf.Sin(cAngle);
        }
        foreach (Piece cp in stage.pieces) {
            if (cp.parent == this) {
                cp.updateTransform();
            }
        }
    }

    void Update() {
        if (oFoot != null && stage == Stage.currentStage) {
            if (velocity > 0) {
                if (stage.playing && isReady) {
                    //footZRotate += 0.2f * (float)velocity;
                    //if (footZRotate >= 360.0f) {
                    //footZRotate -= 360.0f;
                    //}
                } else {
                    footZRotate += 0.4f;
                    if (footZRotate >= 360.0f) {
                        footZRotate -= 360.0f;
                    }
                }
                oHead.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, footZRotate);
            } else if (velocity < 0) {
                if (stage.playing && isReady) {
                    //footZRotate += 0.2f * (float)velocity;
                    //if (footZRotate < 0.0f) {
                    //footZRotate += 360.0f;
                    //}
                } else {
                    footZRotate -= 0.4f;
                    if (footZRotate < 0.0f) {
                        footZRotate += 360.0f;
                    }
                }
                oHead.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, footZRotate);
            }

            // ���� ���콺������ ������Ʈ�� �����鼭 ���� ���콺������ ���¶��
            if (isMouseOver && isReady && !currentlyMouseOver && Stage.focused == 2) {
                currentlyMouseOver = this;

                // �������� ��� order 3
                // �̰��� order 4
                foreach (Piece _pi in stage.pieces) {
                    if (_pi.isRoot) {
                        continue;
                    }
                    _pi.oFootSpriteRenderer.sortingOrder = 3;
                }
                oFootSpriteRenderer.sortingOrder = 4;

                Color hdrGlowColor = originalColor * Mathf.Pow(2, 1);
                oFootSpriteRenderer.material.SetColor("_GlowColor", hdrGlowColor);
                foreach (GameObject cObj in oToe) {
                    cObj.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
                }

                transform.SetAsLastSibling();
            }
            
            if ((stage.playing || !isMouseOver) && currentlyMouseOver == this) {
                //isMouseOver = false;
                if (currentlyDragging == null) {
                    currentlyMouseOver = null;
                    oFootSpriteRenderer.material.SetColor("_GlowColor", originalColor);
                    foreach (GameObject cObj in oToe) {
                        cObj.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", originalColor);
                    }
                }
            }

            // ��ǥ ���� ��
            if (hitGlow > 0) {
                hitGlow--;
                if (hitGlow == 0) {
                    oFootSpriteRenderer.material.SetColor("_GlowColor", originalColor);
                    foreach (GameObject cObj in oToe) {
                        cObj.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", originalColor);
                    }
                }
            }

            if (draggingGlow) {
                if(Piece.currentlyDragging != null) {
                    //Color cColor = new Color(
                    //    UnityEngine.Random.Range(0.5f, 1f), // R (0.5f = 7F in hex, 1f = FF in hex)
                    //    UnityEngine.Random.Range(0.5f, 1f), // G (0.5f = 7F in hex, 1f = FF in hex)
                    //    UnityEngine.Random.Range(0.5f, 1f)  // B (0.5f = 7F in hex, 1f = FF in hex)
                    //);

                    //Color hdrGlowColor = cColor * Mathf.Pow(2, UnityEngine.Random.Range(0.5f, 2.0f));
                    Color hdrGlowColor = (new Color(1.0f, 0.1f, 0.1f)) * Mathf.Pow(2, UnityEngine.Random.Range(0.0f, 0.7f));
                    oFootSpriteRenderer.material.SetColor("_GlowColor", hdrGlowColor);
                } else {
                    draggingGlow = false;
                    oFootSpriteRenderer.material.SetColor("_GlowColor", originalColor);
                }
            }
        }
    }

    // ���콺�� ������Ʈ�� ���� �� ȣ��˴ϴ�.
    public void OnPointerEnter(PointerEventData eventData) {
        if (oFoot != null && !isRoot/* && !stage.playing*/) {
            isMouseOver = true;
        }
        // �߰����� ������ ���⿡ �ۼ��մϴ�.
    }

    // ���콺�� ������Ʈ���� ���� �� ȣ��˴ϴ�.
    public void OnPointerExit(PointerEventData eventData) {
        if (oFoot != null && !isRoot) {
            isMouseOver = false;
        }
        // �߰����� ������ ���⿡ �ۼ��մϴ�.
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left && Stage.focused == 2 && !stage.playing) {
            if (currentlyMouseOver == this) {
                currentlyDragging = this;
                AudioManager.instance.playDragSound();
                if (stage.glittering) {
                    stage.resetNotes();
                }
                stage.enableDraggingGlow();
                stage.star.needUpdate = true;

                //oFoot.GetComponent<CircleCollider2D>().radius = 30.0f; // �ӽ������� �̰��� ��� ������ ũ��
                // ������Ʈ�� ���콺 ������ ������ ���
                Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                offset = transform.position - new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, transform.position.z);

                // �巡���� �߰��� ���ϰ�
                //Color hdrGlowColor = originalColor * Mathf.Pow(2f, 2f);
                Color hdrGlowColor = (new Color(1.0f, 0.1f, 0.1f)) * Mathf.Pow(2f, 1.0f);
                foreach (GameObject cObj in oToe) {
                    cObj.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
                }

            }
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            if (currentlyDragging == this) {
                // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
                Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 newPosition = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, transform.position.z) + offset;



                Piece closestPiece = null;
                float closestDistance = 100.0f;

                float cDistance = 0.0f;

                foreach (Piece cp in stage.pieces) {

                    // �ڱ� �ڽ��� �θ�� ���� �� ����.
                    if (cp == this) {
                        continue;
                    }

                    // ���콺 ���� ���������� ����, recursive parent glitch ����
                    bool recursive = false;
                    Piece _parent = cp.parent;
                    while (_parent != null) {
                        if (_parent == this) {
                            recursive = true;
                            break;
                        }
                        _parent = _parent.parent;
                    }
                    if (recursive) {
                        continue;
                    }

                    cDistance = 0.0f;
                    cDistance += (cp.oFoot.transform.position.x - newPosition.x) * (cp.oFoot.transform.position.x - newPosition.x);
                    cDistance += (cp.oFoot.transform.position.y - newPosition.y) * (cp.oFoot.transform.position.y - newPosition.y);
                    cDistance += (cp.oFoot.transform.position.z - newPosition.z) * (cp.oFoot.transform.position.z - newPosition.z);
                    if (cDistance < 0.0007f) {
                        if (cDistance < closestDistance) {
                            closestDistance = cDistance;
                            closestPiece = cp;
                        }
                    }
                }

                if (closestPiece != null) {
                    parent = closestPiece;
                } else {
                    parent = null;
                    transform.position = newPosition;
                    transform.position += ClampPositionToScreen(oFoot.transform.position) - oFoot.transform.position;
                }
                updateTransform();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            drop();
        }
    }

    public void drop() {
        if (currentlyDragging == this) {
            currentlyDragging = null;
            //oFoot.GetComponent<CircleCollider2D>().radius = 0.3f; // �ӽ������� �̰��� ��� ������ ũ��
            if(parent != null) {
                AudioManager.instance.playDropSound();
            }

            // �߰��� �״��
            Color hdrGlowColor = originalColor * Mathf.Pow(2f, 1f);
            foreach (GameObject cObj in oToe) {
                cObj.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
            }
        }
    }

    public void reset() {
        transform.position = initialPosition;
        parent = null;
    }

    // ��ũ���� �����ڸ��� �ִٸ� ��������
    private Vector3 ClampPositionToScreen(Vector3 position) {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);

        // ȭ���� �����ڸ� 10% ��� ����
        float minViewportX = 0.1f;
        float maxViewportX = 0.9f;
        float minViewportY = 0.05f;
        float maxViewportY = 0.95f;

        // ����Ʈ ��ǥ�� ��� ���� ����
        viewportPosition.x = Mathf.Clamp(viewportPosition.x, minViewportX, maxViewportX);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, minViewportY, maxViewportY);

        // ���ѵ� ����Ʈ ��ǥ�� ���� ��ǥ�� ��ȯ
        return mainCamera.ViewportToWorldPoint(viewportPosition);
    }

    public void enableEditingGraphic() {
        if (isRoot) {
            return;
        }
        //oHeadSpriteRenderer.enabled = true;
    }

    public void disableEditingGraphic() {
        if (isRoot) {
            return;
        }
        //oHeadSpriteRenderer.enabled = false;
    }

}
