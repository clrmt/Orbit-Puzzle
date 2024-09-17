using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour {

    private float x = 6.0f;
    private float xSpeed = 0.0f;
    private float y;
    private float ySpeed = 1.0f;
    private float yAcc = 0.0f;
    private float rotation = 0.0f;
    private float rotationSpeed = 0.0f;
    void Start() {
        Vector3 pos = transform.position;
        if(Random.Range(0.0f, 1.0f) < 0.5f) {
            pos.x = 5.849f - Random.Range(0.0f, 5.0f);
        } else {
            pos.x = 5.849f + Random.Range(0.0f, 5.0f);
        }
        
        transform.position = pos;
        xSpeed = Random.Range(-0.01f, 0.01f);

        rotationSpeed = Random.Range(-10.0f, 10.0f);
        ySpeed = Random.Range(0.0f, 0.001f);

        yAcc = 0.0003f;
        y = 1.5f;
    }

    // Update is called once per frame
    void Update() {
        y -= ySpeed;
        ySpeed += yAcc;

        Vector3 pos = transform.position;
        pos.y = y;

        x += xSpeed;
        pos.x = x;

        transform.position = pos;



        rotation += rotationSpeed;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotation);

        if(y < -10.0f) {
            Destroy(gameObject);
        }
    }
}
