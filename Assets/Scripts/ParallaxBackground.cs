using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ParallaxBackground : MonoBehaviour
{
    private float _scrollingSpeed = 1f;
    private Vector3 _startPos;
    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * _scrollingSpeed * Time.deltaTime);
        if(transform.position.y < - 11.4)
        {
             transform.position = _startPos;
        }
    }
}
    