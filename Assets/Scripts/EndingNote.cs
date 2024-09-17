using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingNote : MonoBehaviour {
    private SpriteRenderer spriteRenderer = null;
    private Color originalColor;

    private float alpha = 0.0f;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalColor.a = alpha; // 일단은 없어도 됨
    }
    void Update() {
        if(alpha > 0.0f) {
            alpha += 0.00005f;
            if(alpha > 1.0f) {
                alpha = 1.0f;
            }
        }

        Color cColor = new Color(
                Random.Range(0.5f, 1f), // R (0.5f = 7F in hex, 1f = FF in hex)
                Random.Range(0.5f, 1f), // G (0.5f = 7F in hex, 1f = FF in hex)
                Random.Range(0.5f, 1f),  // B (0.5f = 7F in hex, 1f = FF in hex)
                alpha
            );

        Color hdrGlowColor = cColor * Mathf.Pow(2, Random.Range(0.5f, 2.0f));
        spriteRenderer.material.SetColor("_GlowColor", hdrGlowColor);
    }
    public void enable() {
        alpha = 0.004f;
    }
    public void disable() {
        alpha = 0.0f;
    }
}
