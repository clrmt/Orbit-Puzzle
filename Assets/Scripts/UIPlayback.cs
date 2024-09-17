using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPlayback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    public void OnPointerEnter(PointerEventData eventData) {
        UIManager.instance.playbackPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.instance.playbackPointerExit();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            UIManager.instance.playbackPointerDown();
        }
    }
    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            //UIManager.instance.playbackPointerUp();
        }
    }
}
