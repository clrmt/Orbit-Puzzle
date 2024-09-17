using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Star : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    private Stage stage;
    private CircleCollider2D cc;
    private SpriteRenderer sr;
    
    private Color currentColor;
    private Color originalColor;
    private Color disabledColor;
    private bool isMouseOver = false;

    private float initialOrthographicSize;
    private Vector3 initialScale;

    [HideInInspector]
    public bool needUpdate = false;

    private float hoverProgress_prev = 0.0f;
    private float hoverProgress = 0.0f;

    public void initialize(Stage _s) {
        stage = _s;
        cc = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        initialOrthographicSize = Camera.main.orthographicSize; // 모든 인스턴스에서 중복, 하지만 편의상 놔두기
        initialScale = transform.localScale;

        disabledColor = new Color(0.01f, 0.01f, 0.01f, 0.1f);
        needUpdate = true;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            if (Stage.focused == 0) {
                stage.focus();
            }
            if (Stage.focused == 2) {
                // 재생, 현재는 안 씀
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData) {
        isMouseOver = true;
        needUpdate = true;
        if(Stage.focused == 0 && GameDataManager.instance.getStageState(stage.world.worldNumber, stage.stageNumber) != 0) {
            AudioManager.instance.playNote(stage.pitch + 30);
        }
    }
    public void OnPointerExit(PointerEventData eventData) {
        isMouseOver = false;
        needUpdate = true;
    }

    void Update() {

        if (isMouseOver && Stage.focused == 0) {
            hoverProgress += 0.1f;
            if(hoverProgress > 1.0f) {
                hoverProgress = 1.0f;
            }
        } else {
            hoverProgress -= 0.1f;
            if( hoverProgress < 0.0f) {
                hoverProgress = 0.0f;
            }
        }

        if(hoverProgress != hoverProgress_prev || needUpdate || Stage.focused == 1) {
            needUpdate = false;
            hoverProgress_prev = hoverProgress;

            int stageState = GameDataManager.instance.getStageState(stage.world.worldNumber, stage.stageNumber);
            if (stageState == -1) { // 가능
                if (Piece.currentlyDragging != null) { // 드래그중이라면
                    //Color cColor = new Color(
                    //    Random.Range(0.5f, 1f), // R (0.5f = 7F in hex, 1f = FF in hex)
                    //    Random.Range(0.5f, 1f), // G (0.5f = 7F in hex, 1f = FF in hex)
                    //    Random.Range(0.5f, 1f)  // B (0.5f = 7F in hex, 1f = FF in hex)
                    //);

                    //Color hdrGlowColor = cColor * Mathf.Pow(2, Random.Range(0.5f, 2.0f));
                    Color hdrGlowColor = (new Color(1.0f, 0.1f, 0.1f)) * Mathf.Pow(2, UnityEngine.Random.Range(0.0f, 0.7f));
                    GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
                    needUpdate = true; // 사실 Stage랑 중복
                } else {
                    GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", originalColor * 0.8f * Mathf.Pow(2.0f, hoverProgress));
                }
            } else if(stageState == 0) { // 불가
                GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", disabledColor);
            } else if(stageState == 1) { // 클
                if (Piece.currentlyDragging != null) { // 드래그중이라면
                    //Color cColor = new Color(
                    //    Random.Range(0.5f, 1f), // R (0.5f = 7F in hex, 1f = FF in hex)
                    //    Random.Range(0.5f, 1f), // G (0.5f = 7F in hex, 1f = FF in hex)
                    //    Random.Range(0.5f, 1f)  // B (0.5f = 7F in hex, 1f = FF in hex)
                    //);

                    //Color hdrGlowColor = cColor * Mathf.Pow(2, Random.Range(0.5f, 2.0f));
                    Color hdrGlowColor = (new Color(1.0f, 0.1f, 0.1f)) * Mathf.Pow(2, UnityEngine.Random.Range(0.3f, 1.0f));
                    GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
                    needUpdate = true; // 사실 Stage랑 중복
                } else {
                    GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", originalColor * Mathf.Pow(2.0f, 1.0f)); 
                }
                
            }
            
            float cameraMultiplier = Camera.main.orthographicSize / initialOrthographicSize;
            float hoverMultiplier = hoverProgress * 0.5f + 1.0f;
            transform.localScale = initialScale * cameraMultiplier * hoverMultiplier;
            if (Stage.focused == 0) { // 포커스되지 않을 경우
                
            } else if(Stage.focused == 1){
                needUpdate = true;
            } else if(Stage.focused == 2) {

            }
            
        }

    }

    public void enableCollider() {
        cc.enabled = true;
    }
    public void disableCollider() {
        cc.enabled = false;
    }
    
}
 