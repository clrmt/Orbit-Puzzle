using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class FixedAspectRatio : MonoBehaviour {
    public float targetAspect = 16.0f / 9.0f; // 고정할 화면 비율 (예: 16:9)
    private Camera mainCamera;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Start() {
        mainCamera = Camera.main;
        UpdateCameraViewport();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    void Update() {
        // 해상도가 변경될 때마다 카메라의 뷰포트를 업데이트합니다.
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
            UpdateCameraViewport();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void UpdateCameraViewport() {
        // 현재 화면의 비율을 계산합니다.
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // 카메라의 Viewport Rect를 초기화합니다.
        Rect rect = mainCamera.rect;
        
        if (scaleHeight < 1.0f) {
            // 창의 y축이 더 큰 경우 (Letterboxing)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        } else {
            // 창의 x축이 더 큰 경우 (Pillarboxing)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        mainCamera.rect = rect;
    }
}
