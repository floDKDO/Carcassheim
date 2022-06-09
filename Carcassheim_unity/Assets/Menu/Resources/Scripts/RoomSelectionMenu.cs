using UnityEngine;
using UnityEngine.UI;
using Assets.System;
using System;
using ClassLibrary;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///    Room Selection Menu.
/// </summary>
public class RoomSelectionMenu : Miscellaneous
{
    private Transform roomSelectMenu, panelRooms;
    //il faut qu'on recoive le nombre de room
    /// <summary>
    ///     number of room 
    /// </summary>
    private static int nombreRoom = 5;
    /// <summary>
    ///     list that contains all game rooms
    /// </summary>
    List<RoomLine> List_of_Rooms = new List<RoomLine>();
    public List<string> listAction;
    public List<bool> RoomChangeAction = new List<bool>();
    public Semaphore s_listAction;
    private bool isVisible = false;
    private DateTime lastTime;

    [SerializeField] RoomLine roomline_model;

    /// <summary>
    ///     Start is called before the first frame update <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    void Start()
    {
        roomSelectMenu = Miscellaneous.FindObject(absolute_parent, "RoomSelectionMenu").transform;
        panelRooms = Miscellaneous.FindObject(absolute_parent, "PanelRooms").transform;
        listAction = new List<string>();
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
    /// Take the number of room and create the room lines <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    private void TableauRoom()
    {
        RoomLine model = roomline_model;
        if (List_of_Rooms != null)
        {
            foreach (RoomLine room in List_of_Rooms)
            {
                room.killRoomLine();
            }
        }

        List_of_Rooms.Clear();
        nombreRoom = 0;
        s_listAction.WaitOne();
        int taille = listAction.Count;
        s_listAction.Release();
        s_listAction.WaitOne();
        for (int i = 0; i < taille; i += 5)
        {
            List_of_Rooms.Add(CreateRoomLine(model, listAction[i], listAction[i + 1], listAction[i + 2], listAction[i + 3], listAction[i + 4]));
            nombreRoom++;
        }

        listAction.Clear();
        s_listAction.Release();
    }

    /// <summary>
    ///     OnStart is called when the menu is changed to this one <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    /// <param name = "pageName">Page name.</param>
    public void OnStart(string pageName)
    {
        // Debug.Log(pageName);
        switch (pageName)
        {
            case "RoomSelectionMenu":
                /* Commuication Async */
                Communication.Instance.StartListening(OnPacketReceived);
                LoadRoomInfo();
                isVisible = true;
                lastTime = DateTime.Now;
                break;
            default:
                RoomChangeAction.Clear();
                /* Ce n'est pas la bonne page */
                /* Stop la reception dans cette class */
                Communication.Instance.StopListening(OnPacketReceived);
                isVisible = false;
                break;
        }
    }

    /// <summary>
    /// Change to home menu <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    public void HideRoomSelection()
    {
        HidePopUpOptions();
        ChangeMenu("RoomSelectionMenu", "HomeMenu");
    }

    /// <summary>
    /// Change to Join By Id Menu <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    public void ShowJoinById()
    {
        HidePopUpOptions();
        ChangeMenu("RoomSelectionMenu", "JoinByIdMenu");
    }

    /// <summary>
    /// Change to public room menu <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    public void ShowJoinPublicRoom()
    {
        HidePopUpOptions();
        ChangeMenu("RoomSelectionMenu", "PublicRoomMenu");
    }

    /// <summary>
    /// Change to create room menu <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    public void ShowCreateRoom()
    {
        HidePopUpOptions();
        ChangeMenu("RoomSelectionMenu", "CreateRoomMenu");
    }

    /// <summary>
    /// Get the room line <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    public RoomLine GetRoomLine(int index)
    {
        if (List_of_Rooms.Count <= index || index < 0)
            return null;
        return List_of_Rooms[index];
    }

    /// <summary>
    /// Get the room info from the server <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    public void LoadRoomInfo()
    {
        Packet packet = new Packet();
        packet.IdMessage = Tools.IdMessage.RoomList;
        packet.IdPlayer = Communication.Instance.IdClient;
        packet.Data = Array.Empty<string>();
        Communication.Instance.IsInRoom = 0;

        Communication.Instance.SendAsync(packet);
    }

    /// <summary>
    /// OnPacketReceived is called when a packet is received <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    /// <param name = "sender">Sender.</param>
    /// <param name = "packet">Packet.</param>
    public void OnPacketReceived(object sender, Packet packet)
    {
        if (packet.IdMessage == Tools.IdMessage.RoomList)
        {
            if (packet.Error == Tools.Errors.None)
            {
                s_listAction.WaitOne();
                listAction.AddRange(packet.Data);
                s_listAction.Release();
            }
        }
        else if (packet.IdMessage == Tools.IdMessage.RoomAskPort)
        {
            bool res = false;
            if (packet.Error == Tools.Errors.None)
            {
                res = true;
                Communication.Instance.PortRoom = int.Parse(packet.Data[0]);
            }

            s_listAction.WaitOne();
            RoomChangeAction.Add(res);
            s_listAction.Release();
        }
    }

    /// <summary>
    /// Update is called once per frame <see cref = "RoomSelectionMenu"/> class.
    /// </summary>
    void Update()
    {
        if (isVisible)
        {
            TimeSpan diffTemps = DateTime.Now - lastTime;
            if (diffTemps > TimeSpan.FromSeconds(10))
            {
                lastTime = DateTime.Now;
                LoadRoomInfo();
            }
        }

        s_listAction.WaitOne();
        int taille = listAction.Count;
        s_listAction.Release();
        if ((taille > 0) && (taille % 5 == 0))
        {
            TableauRoom();
        }

        s_listAction.WaitOne();
        taille = RoomChangeAction.Count;
        s_listAction.Release();
        bool res = false;
        if (taille > 0)
        {
            for (int i = 0; i < taille; i++)
            {
                s_listAction.WaitOne();
                res = (RoomChangeAction[i]);
                s_listAction.Release();
            }

            if (res)
            {
                ChangeMenu("RoomSelectionMenu", "PublicRoomMenu");
            }

            s_listAction.WaitOne();
            RoomChangeAction.Clear();
            s_listAction.Release();
        }
    }
}