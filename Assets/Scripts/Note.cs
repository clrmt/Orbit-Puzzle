using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour {
    public bool played = false;
    private bool colored = false;
    private SpriteRenderer spriteRenderer = null;
    private Color originalColor;

    public int pitch = 0;
    public float x = 0.0f;
    public float y = 0.0f;

    void Awake() {
        initialize();
    }
    
    private void initialize() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void Play() {
        played = true;
        AudioManager.instance.playNote(pitch);
    }

    void Update() {
        if (played) {
            colored = true;
            
            Color cColor = new Color(
                Random.Range(0.5f, 1f), // R (0.5f = 7F in hex, 1f = FF in hex)
                Random.Range(0.5f, 1f), // G (0.5f = 7F in hex, 1f = FF in hex)
                Random.Range(0.5f, 1f)  // B (0.5f = 7F in hex, 1f = FF in hex)
            );
            
            Color hdrGlowColor = cColor * Mathf.Pow(2, Random.Range(0.5f, 2.0f));
            spriteRenderer.material.SetColor("_GlowColor", hdrGlowColor);
        } else {
            if (colored) {
                spriteRenderer.material.SetColor("_GlowColor", new Color(1f, 1f, 1f));
                colored = false;
            }
        }
    }
}
