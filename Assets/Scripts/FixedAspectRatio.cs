using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class FixedAspectRatio : MonoBehaviour {
    public float targetAspect = 16.0f / 9.0f; // ������ ȭ�� ���� (��: 16:9)
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
        // �ػ󵵰� ����� ������ ī�޶��� ����Ʈ�� ������Ʈ�մϴ�.
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
            UpdateCameraViewport();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void UpdateCameraViewport() {
        // ���� ȭ���� ������ ����մϴ�.
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // ī�޶��� Viewport Rect�� �ʱ�ȭ�մϴ�.
        Rect rect = mainCamera.rect;
        
        if (scaleHeight < 1.0f) {
            // â�� y���� �� ū ��� (Letterboxing)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        } else {
            // â�� x���� �� ū ��� (Pillarboxing)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        mainCamera.rect = rect;
    }
}
