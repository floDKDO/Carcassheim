using UnityEngine;
using UnityEngine.UI;
using Assets.System;
using ClassLibrary;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

/// <summary>
///    Join By Id Menu.
/// </summary>
public class JoinByIdMenu : Miscellaneous
{
    private Transform idMenu, IMCI;
    private InputField idCM;
    public List<bool> listAction;
    public Semaphore s_listAction;
    /// <summary>
    /// Start is called before the first frame update <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    void Start()
    {
        idMenu = GameObject.Find("SubMenus").transform.Find("JoinByIdMenu").transform;
        IMCI = idMenu.Find("InputField").transform.Find("InputFieldEndEdit").transform;
        idCM = IMCI.GetChild(0).GetComponent<InputField>();
        listAction = new List<bool>();
        s_listAction = new Semaphore(1, 1);
    }


    void OnEnable()
    {
        OnMenuChange += OnStart;
    }

    void OnDisable()
    {
        OnMenuChange -= OnStart;
    }

    /// <summary>
    /// OnStart is called when the menu is changed to this one <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    public void OnStart(string pageName)
    {
        switch (pageName)
        {
            case "JoinByIdMenu":
                /* Commuication Async */
                Communication.Instance.StartListening(OnPacketReceived);
                break;
            default:
                /* Ce n'est pas la bonne page */
                /* Stop la reception dans cette class */
                Communication.Instance.StopListening(OnPacketReceived);
                break;
        }
    }

    /// <summary>
    /// Input Field End Edit <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    public void InputFieldEndEdit(InputField inp)
    {
        // Debug.Log("Input submitted" + " : " + inp.text);
    }

    /// <summary>
    /// Hide JoinByIdMenu <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    public void HideJoinById()
    {
        HidePopUpOptions();
        ChangeMenu("JoinByIdMenu", "RoomSelectionMenu");
    }

    /// <summary>
    /// Communication Async public room <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    public void ShowJoinPublicRoom2()
    {
        HidePopUpOptions();
        InputFieldEndEdit(idCM);
        Packet packet = new Packet();
        packet.IdMessage = Tools.IdMessage.RoomAskPort;
        packet.IdPlayer = Communication.Instance.IdClient;
        packet.IdRoom = int.Parse(RemoveLastSpace(idCM.text));
        packet.Data = Array.Empty<string>();
        Communication.Instance.IdRoom = packet.IdRoom;
        Communication.Instance.IsInRoom = 0;
        Communication.Instance.SendAsync(packet);
    }

    /// <summary>
    /// OnPacketReceived is called when a packet is received <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    public void OnPacketReceived(object sender, Packet packet)
    {
        bool res = false;
        if (packet.IdMessage == Tools.IdMessage.RoomAskPort)
        {
            if (packet.Error == Tools.Errors.None)
            {
                res = true;
                Communication.Instance.PortRoom = int.Parse(packet.Data[0]);
            }

            s_listAction.WaitOne();
            listAction.Add(res);
            s_listAction.Release();
        }
    }

    /// <summary>
    /// Update is called once per frame <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    private void Update()
    {
        s_listAction.WaitOne();
        int taille = listAction.Count;
        s_listAction.Release();
        bool res = false;
        if (taille > 0)
        {
            for (int i = 0; i < taille; i++)
            {
                s_listAction.WaitOne();
                res = (listAction[i]);
                s_listAction.Release();
            }

            if (res)
            {
                ChangeMenu("JoinByIdMenu", "PublicRoomMenu");
            }

            s_listAction.WaitOne();
            listAction.Clear();
            s_listAction.Release();
        }
    }

    /// <summary>
    /// Clear all input field <see cref = "JoinByIdMenu"/> class.
    /// </summary>
    public void ClearAll(string arg)
    {
        idCM = Clear(idCM);
    }
}