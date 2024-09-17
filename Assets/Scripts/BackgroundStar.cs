using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundStar : MonoBehaviour {

    public Sprite BackgroundStarSprite1;
    public Sprite BackgroundStarSprite2;

    private Image targetImage; // ��ġ�� ������ �̹��� ������Ʈ
    private Canvas canvas; // �θ� ĵ����

    private float alpha = 1.0f;
    private float alphaMultiplier = 0.99f;
    private Image imageCP;

    void Start () {
        // Image ������Ʈ�� �����ɴϴ�.
        targetImage = GetComponent<Image>();

        // �θ� Canvas�� �����ɴϴ�.
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null && targetImage != null) {
            // Canvas�� RectTransform�� �����ɴϴ�.
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

            // �̹����� RectTransform�� �����ɴϴ�.
            RectTransform imageRectTransform = targetImage.GetComponent<RectTransform>();

            // ���� ��ġ�� ����մϴ�.
            Vector2 randomPosition = GetRandomPosition(canvasRectTransform, imageRectTransform);

            // �̹����� ��ġ�� �����մϴ�.
            imageRectTransform.anchoredPosition = randomPosition;
        } else {
            Debug.LogError("Canvas �Ǵ� Image�� ã�� �� �����ϴ�.");
        }

        // ���� ���İ� ����
        imageCP = GetComponent<Image>();
        alpha = Mathf.Pow(Random.Range(0.4f, 0.8f), 2.2f);

        Color _color = imageCP.color;
        _color.a = alpha * alphaMultiplier;
        imageCP.color = _color;

        // ���� ������ ����
        float randomScale = Random.Range(0.2f, 0.4f);
        transform.localScale = new Vector3(randomScale, randomScale, 1.0f);

        // ���� ��������Ʈ ����
        if (Random.Range(0.0f, 1.0f) < 0.05f) {
            imageCP.sprite = BackgroundStarSprite2;
        } else {
            imageCP.sprite = BackgroundStarSprite1;
        }
    }

    // Canvas �ȿ��� Image�� ���� ��ġ�� ��ȯ�ϴ� �޼���
    Vector2 GetRandomPosition(RectTransform canvasRect, RectTransform imageRect) {
        // Canvas�� ũ��
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Image�� ũ��
        float imageWidth = imageRect.rect.width;
        float imageHeight = imageRect.rect.height;

        // ���� X, Y ��ǥ�� ��� (ȭ�� �ȿ��� �̹����� ������ ������)
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
