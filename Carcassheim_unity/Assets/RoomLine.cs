using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.System;
using ClassLibrary;
using System;

public class RoomLine : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Text id_text;
    [SerializeField] Text modo_text;
    [SerializeField] public Text victory_text;
    [SerializeField] Text joueur_present;
    [SerializeField] Text joueur_max;

    private ulong _id;
    public ulong Id
    {
        get => _id;
        set
        {
            _id = value;
            id_text.text = _id.ToString();
        }
    }

    private string _host;
    public string Host
    {
        get => _host;
        set
        {
            _host = value;
            modo_text.text = _host.ToString();
        }
    }
    private int _nbplayer;
    public int NbPlayer
    {
        get => _nbplayer;
        set
        {
            _nbplayer = value;
            joueur_present.text = _nbplayer.ToString();
        }
    }
    private int _nbplayermax;
    public int NbPlayerMax
    {
        get => _nbplayermax;
        set
        {
            _nbplayermax = value;
            joueur_max.text = _nbplayermax.ToString();
        }
    }

    private int _victory;
    public int Victory
    {
        get => _victory;
        set
        {
            _victory = value;
            string res;
            switch (_victory)
            {
                case 1:
                    res = "Temps";
                    break;
                case 2:
                    res = "Point";
                    break;
                default:
                    res = "Tuile";
                    break;
            }
            victory_text.text = res;
        }
    }


    [SerializeField] public RoomLine model;

    [SerializeField]
    public RectTransform parent_area;

    static int room_created_index = 0;

    bool incremented = false;
    bool destroyed = false;

    [SerializeField] Image background;
    [SerializeField] List<Color> background_color;

    // Start is called before the first frame update
    void Start()
    {
        background.color = background_color[room_created_index % 2];
        room_created_index += 1;
        incremented = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void EnableOnList()
    {
        if (this != model)
        {
            Vector2 dim = parent_area.sizeDelta;
            dim.y += 100;
            parent_area.sizeDelta = dim;
        }
    }
    void OnDisable()
    {
        if (this != model)
        {
            Vector2 dim = parent_area.sizeDelta;
            dim.y -= 100;
            parent_area.sizeDelta = dim;
            Destroy(gameObject);
        }
        destroyed = true;
    }

    public void killRoomLine()
    {
        if (!destroyed)
        {
            destroyed = true;
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (this != model && incremented)
        {
            incremented = false;
            room_created_index -= 1;
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            Packet packet = new Packet();
            packet.IdMessage = Tools.IdMessage.RoomAskPort;
            packet.IdPlayer = Communication.Instance.IdClient;
            packet.IdRoom = (int)Id;
            packet.Data = Array.Empty<string>();

            Communication.Instance.IdRoom = packet.IdRoom;
            Communication.Instance.IsInRoom = 0;
            Communication.Instance.SendAsync(packet);
        }
    }
}
