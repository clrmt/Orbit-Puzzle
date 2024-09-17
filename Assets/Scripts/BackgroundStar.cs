using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundStar : MonoBehaviour {

    public Sprite BackgroundStarSprite1;
    public Sprite BackgroundStarSprite2;

    private Image targetImage; // 위치를 설정할 이미지 컴포넌트
    private Canvas canvas; // 부모 캔버스

    private float alpha = 1.0f;
    private float alphaMultiplier = 0.99f;
    private Image imageCP;

    void Start () {
        // Image 컴포넌트를 가져옵니다.
        targetImage = GetComponent<Image>();

        // 부모 Canvas를 가져옵니다.
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null && targetImage != null) {
            // Canvas의 RectTransform을 가져옵니다.
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

            // 이미지의 RectTransform을 가져옵니다.
            RectTransform imageRectTransform = targetImage.GetComponent<RectTransform>();

            // 랜덤 위치를 계산합니다.
            Vector2 randomPosition = GetRandomPosition(canvasRectTransform, imageRectTransform);

            // 이미지의 위치를 설정합니다.
            imageRectTransform.anchoredPosition = randomPosition;
        } else {
            Debug.LogError("Canvas 또는 Image를 찾을 수 없습니다.");
        }

        // 랜덤 알파값 설정
        imageCP = GetComponent<Image>();
        alpha = Mathf.Pow(Random.Range(0.4f, 0.8f), 2.2f);

        Color _color = imageCP.color;
        _color.a = alpha * alphaMultiplier;
        imageCP.color = _color;

        // 랜덤 스케일 설정
        float randomScale = Random.Range(0.2f, 0.4f);
        transform.localScale = new Vector3(randomScale, randomScale, 1.0f);

        // 랜덤 스프라이트 설정
        if (Random.Range(0.0f, 1.0f) < 0.05f) {
            imageCP.sprite = BackgroundStarSprite2;
        } else {
            imageCP.sprite = BackgroundStarSprite1;
        }
    }

    // Canvas 안에서 Image의 랜덤 위치를 반환하는 메서드
    Vector2 GetRandomPosition(RectTransform canvasRect, RectTransform imageRect) {
        // Canvas의 크기
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Image의 크기
        float imageWidth = imageRect.rect.width;
        float imageHeight = imageRect.rect.height;

        // 랜덤 X, Y 좌표를 계산 (화면 안에서 이미지가 완전히 들어가도록)
        float randomX = Random.Range(-canvasWidth / 2 + imageWidth / 2, canvasWidth / 2 - imageWidth / 2);
        float randomY = Random.Range(-canvasHeight / 2 + imageHeight / 2, canvasHeight / 2 - imageHeight / 2);

        return new Vector2(randomX, randomY);
    }

    void Update() {
        if (Stage.focused == 1) {
            if(Stage.previousFocused == 0) {
                if (alphaMultiplier > 0.0f) {
                    alphaMultiplier -= 0.012f;
                    Color _color = imageCP.color;
                    _color.a = alpha * alphaMultiplier;
                    imageCP.color = _color;
                }
            } else {
                if (alphaMultiplier < 1.0f) {
                    alphaMultiplier += 0.012f;
                    Color _color = imageCP.color;
                    _color.a = alpha * alphaMultiplier;
                    imageCP.color = _color;
                }
            }
        }

    }

}
