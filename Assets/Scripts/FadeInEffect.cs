using UnityEngine;
using UnityEngine.UI;

public class FadeInEffect : MonoBehaviour {
    public Image fadeImage; // ���̵��ο� ����� �̹���
    public float fadeDuration = 2.0f; // ���̵��� ���� �ð�

    private float elapsedTime = 0f;
    [HideInInspector]
    public static bool isFading = true;

    private void Start() {
        // �ʱ� ���İ��� 1�� �����Ͽ� �̹����� ������ �������ϰ� ����
        if (fadeImage != null) {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }
    }

    private void Update() {
        if (isFading && fadeImage != null) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1.0f - elapsedTime / fadeDuration);
            Color color = fadeImage.color;
            color.a = Mathf.Sqrt(alpha);
            fadeImage.color = color;

            if (elapsedTime >= fadeDuration) {
                isFading = false;
                fadeImage.gameObject.SetActive(false);
            }
        }
    }
}
