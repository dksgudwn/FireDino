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
        //���� �̺�Ʈ
        string strWeather = AuthManager.Instance.GetWeather();
        Console.WriteLine(strWeather);
        switch (strWeather)
        {
            case "Night":
                cam.backgroundColor = Color.black;
                Debug.Log("��");
                break;
            case "Afternoon":
                cam.backgroundColor = Color.white;
                Debug.Log("��");
                break;

                default: Debug.Log("����������������������������������������");
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
