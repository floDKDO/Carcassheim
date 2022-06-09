using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class Notification : MonoBehaviour
{
    [SerializeField] private bool notification;
    [SerializeField] private string message;
    [SerializeField] private TMP_Text messageBox;

    [SerializeField] DisplaySystem system;

    // Start is called before the first frame update
    void Start()
    {
        notification = false;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (notification)
        {
            gameObject.SetActive(true);

            Thread.Sleep(3000);
            gameObject.SetActive(false);
            notification = false;
        }
        
    }

    void OnEnable()
    {
        system.OnPlayerDisconnected += activateNotifier;   
    }


    void OnDisable()
    {
        system.OnPlayerDisconnected -= activateNotifier;   
    }

    string getMessageFromServer()
    {
        return "";
    }

    void setMessage(string m)
    {
        message = m;
        messageBox.text = message;
    }

    void activateNotifier(PlayerRepre player)
    {
        notification = true;
        string message = "Player " + player.Name + " is disconnected";
        setMessage(message);
    }
}
