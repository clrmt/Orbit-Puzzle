using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBack : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler {

    private Color originalColor;

    void Awake() {
        originalColor = GetComponent<SpriteRenderer>().color;
        originalColor.a = 1.0f;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Color hdrGlowColor = originalColor * Mathf.Pow(2, 1);
        GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Color hdrGlowColor = originalColor * Mathf.Pow(2, 0);
        GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", hdrGlowColor);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            Stage.currentStage.blur();
        }
    }

    public void OnPointerUp(PointerEventData eventData) {

    }
}
