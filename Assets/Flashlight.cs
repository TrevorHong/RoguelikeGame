using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class Flashlight : MonoBehaviour
{
    UnityEngine.Rendering.Universal.Light2D flashlight;

    void Start()
    {
        flashlight = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        { 
        flashlight.enabled = !flashlight.enabled;
        }
    }
}
