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

    public int index = -1; // 음악 생성할 때 몇 번째 곡 스트링을 가져올지
    [HideInInspector]
    public bool played = false; // 재생한 적이 있는 piece의 개수 세려고
    [HideInInspector]
    public int visit = 1; // Piece 순회할 때 중복 방문하지 않도록

    [HideInInspector]
    public float x = 0.0f; // foot의 실제 수치가아닌 계산수치(정확도 요구)
    [HideInInspector]
    public float y = 0.0f; // foot의 실제 수치가아닌 계산수치(정확도 요구)

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

    public GameObject trailNote() { // 음악 생성 및 디버깅 목적의 자취 남기기
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

        // 설정
        const float _BodyOffset = 0.2f;
        const float _BodyInterval = 0.2f;

        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (float)angle);

        oSpine = new List<GameObject>();

        // 길이에 따른 body의 부분 생성
        for (int i = 1; i < length; i++) {
            oTmp = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
            oTmp.transform.SetParent(oBody.transform);
            oTmp.transform.localPosition = new Vector3(_BodyOffset + (float)i * _BodyInterval, 0, 0);
            oTmp.transform.localRotation = Quaternion.Euler(0, 0, 0);
            oTmp.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            aSpriteRenderer.Add(oTmp.GetComponent<SpriteRenderer>());
            oSpine.Add(oTmp);
        }

        // 끝에 발 생성
        oFoot.transform.localPosition = new Vector3(_BodyOffset + (float)length * _BodyInterval, 0, 0);
        oFoot.transform.localRotation = Quaternion.Euler(0, 0, 0);
        oFoot.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        oToe = new List<GameObject>();

        /*
        // 발에 ... 발가락 ... 생성
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

        // 발에 발가락 생성(12고정)
        if (velocity > 12 || velocity < -12) {
            Debug.LogError("Piece의 velocity가 12를 넘어섭니다.");
            return;
        }

        if (!isRoot) {
            for (int i = 0; i < 12; i++) {

                bool _isToe = false;
                // 있어야할 곳인지 체크
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

        originalColor = oFootSpriteRenderer.color; // Foot Color를 기준...
        disabledColor = originalColor;
        disabledColor.a = 0.1f;

        // 일단 전부 alpha 0으로
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

            // 내부 x, y 계산(transform의 계산은 매번 달라질 수 있으니 이것으로 대체)
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

            // 현재 마우스오버한 오브젝트가 없으면서 현재 마우스오버된 상태라면
            if (isMouseOver && isReady && !currentlyMouseOver && Stage.focused == 2) {
                currentlyMouseOver = this;

                // 나머지를 모두 order 3
                // 이것을 order 4
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

            // 음표 쳤을 때
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

    // 마우스가 오브젝트에 들어올 때 호출됩니다.
    public void OnPointerEnter(PointerEventData eventData) {
        if (oFoot != null && !isRoot/* && !stage.playing*/) {
            isMouseOver = true;
        }
        // 추가적인 동작을 여기에 작성합니다.
    }

    // 마우스가 오브젝트에서 나갈 때 호출됩니다.
    public void OnPointerExit(PointerEventData eventData) {
        if (oFoot != null && !isRoot) {
            isMouseOver = false;
        }
        // 추가적인 동작을 여기에 작성합니다.
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

                //oFoot.GetComponent<CircleCollider2D>().radius = 30.0f; // 임시적으로 이것의 잡는 범위를 크게
                // 오브젝트와 마우스 사이의 오프셋 계산
                Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                offset = transform.position - new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, transform.position.z);

                // 드래그중 발가락 진하게
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
                // 마우스 위치를 월드 좌표로 변환
                Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 newPosition = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, transform.position.z) + offset;



                Piece closestPiece = null;
                float closestDistance = 100.0f;

                float cDistance = 0.0f;

                foreach (Piece cp in stage.pieces) {

                    // 자기 자신은 부모로 삼을 수 없다.
                    if (cp == this) {
                        continue;
                    }

                    // 마우스 과한 움직임으로 인해, recursive parent glitch 방지
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
            //oFoot.GetComponent<CircleCollider2D>().radius = 0.3f; // 임시적으로 이것의 잡는 범위를 크게
            if(parent != null) {
                AudioManager.instance.playDropSound();
            }

            // 발가락 그대로
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

    // 스크린의 가장자리에 있다면 안쪽으로
    private Vector3 ClampPositionToScreen(Vector3 position) {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);

        // 화면의 가장자리 10% 경계 설정
        float minViewportX = 0.1f;
        float maxViewportX = 0.9f;
        float minViewportY = 0.05f;
        float maxViewportY = 0.95f;

        // 뷰포트 좌표를 경계 내로 제한
        viewportPosition.x = Mathf.Clamp(viewportPosition.x, minViewportX, maxViewportX);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, minViewportY, maxViewportY);

        // 제한된 뷰포트 좌표를 월드 좌표로 변환
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
