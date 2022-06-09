using Assert.system;
using Assets.System;
using ClassLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
///    Public Room Menu.
/// </summary>
public class PublicRoomMenu : Miscellaneous
{
    private Color readyState;
    public List<string> listAction;
    public Semaphore s_listAction;
    private Transform container;
    private Text id_room; //id de la room (pour l'instant : 'X')
    private List<Player> listPlayers;
    private ulong nbPlayer = 0;
    private List<PlayerLine> List_of_Player;
    private Semaphore s_List_of_Player;

    [SerializeField] PlayerLine PlayerLigneModel;

    [SerializeField] RoomParameterRepre repre_parameter;

    bool is_ready = false;

    /// <summary>
    /// Start is called before the first frame update <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    void Start()
    {
        listAction = new List<string>();
        s_listAction = new Semaphore(1, 1);
        RoomInfo.Instance.idPartie = Communication.Instance.IdRoom;
        container = GameObject.Find("SubMenus").transform.Find("PublicRoomMenu").transform.Find("Text").transform;
        id_room = container.Find("NumberOfRoom").GetComponent<Text>();

        s_List_of_Player = new Semaphore(1, 1);
        List_of_Player = new List<PlayerLine>();
        listPlayers = new List<Player>();
    }

    void OnEnable()
    {
        OnMenuChange += OnStart;
    }

    void OnDisable()
    {
        if (RoomInfo.Instance.repre_parameter != null)
            RoomInfo.Instance.repre_parameter.IsInititialized = false;
        OnMenuChange -= OnStart;
    }

    private void TableauPlayer(List<Player> listPlayers)
    {
        PlayerLine model = PlayerLigneModel;
        SuppressionTableauPlayer();

        s_List_of_Player.WaitOne();
        List_of_Player.Clear();
        int taille = listPlayers.Count;
        for (int i = 0; i < taille; i++)
        {
            List_of_Player.Add(CreatePlayerLine(model, listPlayers[i].name, listPlayers[i].status));
        }
        s_List_of_Player.Release();
    }

    private void SuppressionTableauPlayer()
    {
        if (List_of_Player != null)
        {
            foreach (PlayerLine player in List_of_Player)
            {
                player.killPlayerLine();
            }
        }
    }

    /// <summary>
    /// OnStart is called when the menu is changed to this one <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void OnStart(string pageName)
    {
        switch (pageName)
        {
            case "PublicRoomMenu":
                is_ready = false;
                /* Commuication Async */
                Communication.Instance.IsInRoom = 1;
                Communication.Instance.LancementConnexion();
                Action listening = () =>
                {
                    Communication.Instance.StartLoopListening(OnPacketReceived);
                };
                Task.Run(listening);
                /* Communication pour que le serveur set le port */
                Packet packet = new Packet();
                packet.IdMessage = Tools.IdMessage.PlayerJoin;
                packet.IdPlayer = Communication.Instance.IdClient;
                packet.IdRoom = Communication.Instance.IdRoom;
                packet.Data = Array.Empty<string>();
                Communication.Instance.SendAsync(packet);

                List_of_Player = new List<PlayerLine>();
                listPlayers = new List<Player>();
                break;
            default:
                /* Ce n'est pas la bonne page */
                /* Stop la reception dans cette class */
                onUnready();
                Communication.Instance.StopListening(OnPacketReceived);
                repre_parameter.IsInititialized = false;

                listPlayers.Clear();
                SuppressionTableauPlayer();
                break;
        }
    }

    /// <summary>
    /// Hide public room menu <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void HideRoom()
    {
        Packet packet = new Packet();
        packet.IdMessage = Tools.IdMessage.PlayerLeave;
        packet.IdPlayer = Communication.Instance.IdClient;
        packet.IdRoom = Communication.Instance.IdRoom;
        packet.Data = Array.Empty<string>();

        Communication.Instance.IsInRoom = 1;
        Communication.Instance.SendAsync(packet);
        Communication.Instance.IsInRoom = 0;

        ChangeMenu("PublicRoomMenu", "RoomSelectionMenu");
    }

    /// <summary>
    /// Change to Room Parameters menu <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void ShowRoomParameters()
    {
        HidePopUpOptions();
        ChangeMenu("PublicRoomMenu", "RoomParametersMenu");
    }

    /// <summary>
    /// Ready to play <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void Ready()
    {
        Packet packet = new Packet();
        packet.IdMessage = Tools.IdMessage.PlayerReady;
        packet.IdPlayer = Communication.Instance.IdClient;
        packet.IdRoom = Communication.Instance.IdRoom;
        packet.Data = Array.Empty<string>();

        is_ready = !is_ready;
        if (is_ready)
            onReady();
        else
            onUnready();

        Communication.Instance.IsInRoom = 1;
        Communication.Instance.SendAsync(packet);

        s_listAction.WaitOne();
        listAction.Add("playerInfo");
        s_listAction.Release();

        int i;
        s_List_of_Player.WaitOne();
        int taille = listPlayers.Count;
        for (i = 0; i < taille; i++)
        {
            if (listPlayers[i].id == packet.IdPlayer)
            {
                listPlayers[i].status = !listPlayers[i].status;
            }
        }
        s_List_of_Player.Release();
    }
    public void onReady()
    {
        ColorUtility.TryParseHtmlString("#90EE90", out readyState);
        Text preparer = Miscellaneous.FindObject(absolute_parent, "preparation").GetComponent<Text>();

        preparer.color = readyState;
        if (OptionsMenu.langue == 0)
            preparer.text = "PRET A JOUER !";
        else if (OptionsMenu.langue == 1)
            preparer.text = "READY TO PLAY!";
        else if (OptionsMenu.langue == 2)
            preparer.text = "SPIELBEREIT!";
    }

    public void onUnready()
    {
        ColorUtility.TryParseHtmlString("#FFA500", out readyState);
        Text preparer = Miscellaneous.FindObject(absolute_parent, "preparation").GetComponent<Text>();

        preparer.color = readyState;
        if (OptionsMenu.langue == 0)
            preparer.text = "NON PRET";
        else if (OptionsMenu.langue == 1)
            preparer.text = "NOT READY";
        else if (OptionsMenu.langue == 2)
            preparer.text = "NICHT BEREIT";
    }

    /// <summary>
    /// Pacjet received <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="packet">Packet.</param>
    public void OnPacketReceived(object sender, Packet packet)
    {
        if (packet.IdMessage == Tools.IdMessage.StartGame)
        {
            if (packet.Error == Tools.Errors.None)
            {
                RoomInfo.Instance.idTileInit = int.Parse(packet.Data[0]);
                s_listAction.WaitOne();
                listAction.Add("loadScene");
                s_listAction.Release();
            }
        }
        else if (packet.IdMessage == Tools.IdMessage.PlayerJoin)
        {
            ulong id = Communication.Instance.IdClient;
            string name = Communication.Instance.Name;
            bool status = false;

            if (packet.IdPlayer == Communication.Instance.IdClient)
            {
                Packet packet1 = new Packet();
                packet1.IdMessage = Tools.IdMessage.RoomSettingsGet;
                packet1.IdPlayer = Communication.Instance.IdClient;
                packet1.IdRoom = Communication.Instance.IdRoom;
                packet1.Data = Array.Empty<string>();

                Communication.Instance.SendAsync(packet1);
            }
            else
            {
                id = packet.IdPlayer;
                name = packet.Data[0];
                status = false;
            }

            if (packet.Data.Length > 1)
            {
                int i, taille = packet.Data.Length;
                for (i = 0; i < taille; i += 3)
                {
                    id = ulong.Parse(packet.Data[i]);
                    name = packet.Data[i + 1];

                    switch (packet.Data[i + 2])
                    {
                        case "true":
                            status = true;
                            break;
                        default:
                            status = false;
                            break;
                    }

                    s_List_of_Player.WaitOne();
                    listPlayers.Add(new Player(id, name, status));
                    s_List_of_Player.Release();
                }
            }
            else
            {
                s_List_of_Player.WaitOne();
                listPlayers.Add(new Player(id, name, status));
                s_List_of_Player.Release();
            }

            s_listAction.WaitOne();
            listAction.Add("playerInfo");
            s_listAction.Release();
        }
        else if (packet.IdMessage == Tools.IdMessage.PlayerLeave)
        {
            s_listAction.WaitOne();
            listAction.Add("playerInfo");
            s_listAction.Release();

            int i;
            s_List_of_Player.WaitOne();
            int taille = listPlayers.Count;
            for (i = 0; i < taille; i++)
            {
                if (listPlayers[i].id == packet.IdPlayer)
                {
                    listPlayers.RemoveAt(i);
                    break;
                }
            }
            s_List_of_Player.Release();
        }
        else if (packet.IdMessage == Tools.IdMessage.RoomSettingsGet)
        {
            if (packet.Error == Tools.Errors.None)
            {
                string[] res = new string[packet.Data.Length];
                Array.Copy(packet.Data, res, packet.Data.Length);
                RoomInfo.Instance.SetValues(packet.Data);
            }
        }

        else if (packet.IdMessage == Tools.IdMessage.PlayerReady)
        {
            if (packet.IdPlayer != Communication.Instance.IdClient)
            {
                s_listAction.WaitOne();
                listAction.Add("playerInfo");
                s_listAction.Release();

                int i;
                s_List_of_Player.WaitOne();
                int taille = listPlayers.Count;
                for (i = 0; i < taille; i++)
                {
                    if (listPlayers[i].id == packet.IdPlayer)
                    {
                        listPlayers[i].status = !listPlayers[i].status;
                    }
                }
                s_List_of_Player.Release();
            }
        }
        else if (packet.IdMessage == Tools.IdMessage.PlayerKick)
        {
            s_listAction.WaitOne();
            listAction.Add("kickplayer");
            s_listAction.Release();
        }
    }

    /// <summary>
    /// Loads the Scene in the background as the current Scene runs <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("InGame_VG");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Update every frame <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    void Update()
    {
        s_listAction.WaitOne();
        int taille = listAction.Count;
        s_listAction.Release();

        if (taille > 0)
        {
            s_listAction.WaitOne();
            string choixAction = listAction[0];
            listAction.Clear();
            s_listAction.Release();

            switch (choixAction)
            {
                case "loadScene":
                    StartCoroutine(LoadYourAsyncScene());
                    gameObject.SetActive(false);
                    break;
                case "playerInfo":
                    /* Update l'affichage */
                    TableauPlayer(listPlayers);
                    break;
                case "kickplayer":
                    HideRoom();
                    break;
            }
        }
    }
}