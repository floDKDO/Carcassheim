using Assets.System;
using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///    Create Room Menu.
/// </summary>
public class CreateRoomMenu : Miscellaneous
{
    public List<bool> listAction;
    public Semaphore s_listAction;
    [SerializeField] private RoomParameterRepre repre_parameter;
    [SerializeField] private Dropdown nb_player;
    [SerializeField] private Toggle room_private;

    /// <summary>
    /// Start is called before the first frame update <see cref = "CreateRoomMenu"/> class.
    /// </summary>
    private void Start()
    {
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
    /// OnStart is called when the menu is changed to this one <see cref = "CreateRoomMenu"/> class.
    /// </summary>
    /// <param name = "pageName">Page name.</param>
    public void OnStart(string pageName)
    {
        switch (pageName)
        {
            case "CreateRoomMenu":
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
    /// Hide create room menu <see cref = "CreateRoomMenu"/> class.
    /// </summary>
    public void HideCreateRoom()
    {
        HidePopUpOptions();
        ChangeMenu("CreateRoomMenu", "RoomSelectionMenu");
    }

    /// <summary>
    /// Create room <see cref = "CreateRoomMenu"/> class.
    /// </summary>
    public void ShowRoomIsCreated()
    {
        HidePopUpOptions();
        Packet packet = new Packet();
        packet.IdMessage = Tools.IdMessage.RoomCreate;
        packet.IdPlayer = Communication.Instance.IdClient;
        packet.Data = Array.Empty<string>();
        repre_parameter.default_room_policy = !room_private.isOn;
        repre_parameter.default_player_number = nb_player.value + 2;
        Communication.Instance.SendAsync(packet);
    }

    /// <summary>
    /// Toggle value <see cref = "CreateRoomMenu"/> class.
    /// </summary>
    /// <param name = "curT">Current toggle.</param>
    public void ToggleValueChangedCRM(Toggle curT)
    {
    }

    /// <summary>
    /// OnPacketReceived is called when a packet is received <see cref = "CreateRoomMenu"/> class.
    /// </summary>
    /// <param name = "sender">Sender.</param>
    /// <param name = "packet">Packet.</param>
    public void OnPacketReceived(object sender, Packet packet)
    {
        bool res = false;
        if (packet.IdMessage == Tools.IdMessage.RoomCreate)
        {
            if (packet.Error == Tools.Errors.None)
            {
                res = true;
                Communication.Instance.IdRoom = packet.IdRoom;
                Communication.Instance.PortRoom = int.Parse(packet.Data[0]);
                Communication.Instance.IsInRoom = 1;
            }

            s_listAction.WaitOne();
            listAction.Add(res);
            s_listAction.Release();
        }
    }

    /// <summary>
    /// Update is called once per frame <see cref = "CreateRoomMenu"/> class.
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
                ChangeMenu("CreateRoomMenu", "PublicRoomMenu");
            }

            s_listAction.WaitOne();
            listAction.Clear();
            s_listAction.Release();
        }
    }
}