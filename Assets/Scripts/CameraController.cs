using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraController : MonoBehaviour {

    private Vector3 startPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 endPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private float startSize = 5.0f;
    private float endSize = 5.0f;

    private float elapsedTime = 0.0f;
    private float totalTime = 0.0f;
    private Action callback = null;

    void Update() {
        if (elapsedTime < totalTime) {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / totalTime;
            t = Mathf.Clamp01(t);
            //float lerpT = 1.0f - Mathf.Exp(-5 * t);
            // float lerpT = t * t * (3.0f - 2.0f * t); // 갑자기 가버림
            //float lerpT = Mathf.SmoothStep(0.0f, 1.0f, t);
            float lerpT = 0.5f * (1.0f - Mathf.Cos(t * Mathf.PI));

            transform.position = Vector3.Lerp(startPosition, endPosition, lerpT);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, endSize, lerpT);

            if (elapsedTime >= totalTime) {
                callback?.Invoke();
                callback = null;
            }
        }
        GL.Clear(true, true, Camera.main.backgroundColor);
    }

    // 카메라 이동을 멈춥니다.
    public void stop() {
        elapsedTime = totalTime + 0.1f;
    }
    // 카메라 이동을 처음부터 취소합니다.
    public void cancel() {
        transform.position = startPosition;
        Camera.main.orthographicSize = startSize;
        stop();
    }
    public void moveToPosition(Vector3 _target, float _totalTime, float _size, Action _callback = null) {
        startPosition = transform.position;
        endPosition = _target;

        startSize = Camera.main.orthographicSize;
        endSize = _size;

        elapsedTime = 0.0f;
        totalTime = _totalTime;
        callback = _callback;

        if (_totalTime == 0.0f) {
            transform.position = _target;
            callback?.Invoke();
            callback = null;
        }
    }

}
