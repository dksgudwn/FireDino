using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseEvent : MonoBehaviour
{
    public GameObject particleSnow;
    public GameObject particleRain;

    // Start is called before the first frame update
    void Start()
    {
        //���� �̺�Ʈ
        string strWeather = AuthManager.Instance.GetWeather();
        Console.WriteLine(strWeather);
        switch (strWeather)
        {
            case "Snow":
                particleSnow.SetActive(true);
                Debug.Log("Snow");
                break;
            case "Rain":
                particleRain.SetActive(true);
                Debug.Log("Rain");
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
