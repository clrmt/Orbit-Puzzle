using UnityEngine;
using UnityEngine.UI;

public class FadeInEffect : MonoBehaviour {
    public Image fadeImage; // 페이드인에 사용할 이미지
    public float fadeDuration = 2.0f; // 페이드인 지속 시간

    private float elapsedTime = 0f;
    [HideInInspector]
    public static bool isFading = true;

    private void Start() {
        // 초기 알파값을 1로 설정하여 이미지가 완전히 불투명하게 설정
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
