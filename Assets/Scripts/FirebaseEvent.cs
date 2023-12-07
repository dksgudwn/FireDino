using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseEvent : MonoBehaviour
{
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        //劾松 戚坤闘
        string strWeather = AuthManager.Instance.GetWeather();
        Console.WriteLine(strWeather);
        switch (strWeather)
        {
            case "Night":
                cam.backgroundColor = Color.black;
                Debug.Log("鴻");
                break;
            case "Afternoon":
                cam.backgroundColor = Color.white;
                Debug.Log("碍");
                break;

                default: Debug.Log("しししししししししししししししししししし");
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
