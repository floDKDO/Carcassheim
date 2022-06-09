using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.System;
using ClassLibrary;

public struct RoomParametersStruct
{
    public RoomParametersStruct(bool room_policy, Tools.Mode wm, int timer_tour, int timer_win, int point_win, int tile_win, bool river_on, bool abbaye_on)
    {
        this.wm = wm;
        this.timer_win = timer_win;
        this.tile_win = tile_win;
        this.point_win = point_win;
        this.room_policy = room_policy;
        this.timer_tour = timer_tour;
        this.river_on = river_on;
        this.abbaye_on = abbaye_on;
    }
    public bool room_policy;
    public Tools.Mode wm;
    public int timer_tour;
    public int timer_win;
    public int point_win;
    public int tile_win;
    public bool river_on;
    public bool abbaye_on;
}

public class RoomParameterRepre : MonoBehaviour
{
    [SerializeField] Text id_room;

    [SerializeField] Dropdown time_tour_slider;
    [SerializeField] Toggle room_extension_rivier;
    [SerializeField] Toggle room_extension_abbaye;
    [SerializeField] Toggle room_access_private;
    [SerializeField] Toggle room_access_public;
    [SerializeField] Dropdown room_winmode_drop;

    [SerializeField] Dropdown room_win_timer;
    [SerializeField] Dropdown room_win_tile;
    [SerializeField] Dropdown room_win_point;


    [SerializeField] List<Selectable> menu_activate;

    Tools.Mode winmode = Tools.Mode.Default;
    int timer_winmode = 20, def_timer_winmode = 20;
    int tile_winmode = 70, def_tile_winmode = 70;
    int point_winmode = 100, def_point_winmode = 100;

    bool room_policy_public = true, def_room_policy_public = true;
    public bool? default_room_policy = null;
    bool riverOn = false, abbayeOn = false;

    public int? default_player_number = null;
    int timer_tour = 60, def_timer_tour = 60;

    RoomParametersStruct change_planned;
    bool changed = false;

    private bool _initialize = false;
    public bool to_initialize = false;
    public bool IsInititialized { set { _initialize = value; if (!_initialize) { default_player_number = null; default_room_policy = null; } to_initialize = false; } get => _initialize; }

    [SerializeField] List<GameObject> wincond_parameters;

    // Start is called before the first frame update
    void Start()
    {
        RoomInfo room = RoomInfo.Instance;
        initRoom(false);
        room.repre_parameter = this;
        setParameters(room.isPrivate, room.mode, room.timerJoueur, room.timerPartie, room.scoreMax, room.idTileInit, room.riverOn, room.abbayeOn);
    }

    public void initRoom(bool isinit = true)
    {
        if (IsInititialized)
        {
            Debug.LogWarning("Shouldn't have reinitialize");
            return;
        }
        RoomInfo room = RoomInfo.Instance;
        id_room.text = Communication.Instance.IdRoom.ToString();
        bool interactif = Communication.Instance.IdClient == room.idModerateur;
        foreach (Selectable selectable in menu_activate)
        {
            selectable.interactable = interactif;
        }
        if (interactif && default_room_policy != null && default_player_number != null)
        {
            room.setDefault((bool)default_room_policy, (int)default_player_number);

            default_room_policy = null;
            default_player_number = null;
        }
        if (!isinit && IsInititialized)
            IsInititialized = isinit;
    }

    public void OnRoomWinChange(int index)
    {
        switch (index)
        {
            case 1:
                winmode = Tools.Mode.Time;
                break;
            case 2:
                winmode = Tools.Mode.Point;
                break;
            default:
                winmode = Tools.Mode.Default;
                break;
        }
        for (int i = 0; i < wincond_parameters.Count; i++)
        {
            wincond_parameters[i].SetActive(i == index);
        }
        RoomInfo.Instance.mode = winmode;
    }

    public void OnRoomPolicyChange(int i)
    {
        RoomInfo.Instance.isPrivate = i != 0;
    }

    public void OnRoomExtensionChangeRiviere(bool state)
    {
        RoomInfo.Instance.riverOn = state;
        RoomInfo.Instance.nbTuile += 11;
    }
    public void OnRoomExtensionChangeAbbaye(bool state)
    {
        RoomInfo.Instance.abbayeOn = state;
    }

    public void OnRoomTimerTurnChange()
    {
        int timer_tour = 30;
        if (time_tour_slider.value > 0)
            timer_tour = (time_tour_slider.value + 1) * 30;
        RoomInfo.Instance.timerJoueur = timer_tour;
    }

    public void OnRoomTimerGameChange(int val)
    {
        if (winmode != Tools.Mode.Time)
            return;
        int nb = 1800;
        switch (val)
        {
            case 1:
                nb = 3600;
                break;
        }
        RoomInfo.Instance.timerPartie = nb;
    }

    public void OnRoomNbTileChange(int val)
    {
        if (winmode != Tools.Mode.Default)
            return;
        int nb = (val + 1) * 30;
        RoomInfo.Instance.nbTuile = nb;
    }

    public void OnRoomNbPointChange(int val)
    {
        if (winmode != Tools.Mode.Point)
            return;
        int nb = (val + 1) * 50;
        RoomInfo.Instance.scoreMax = nb;
    }

    public void addParameters(bool room_policy, Tools.Mode wm, int timer_tour, int timer_win, int point_win, int tile_win, bool river_on, bool abbaye_on)
    {
        changed = true;
        change_planned = new RoomParametersStruct(room_policy, wm, timer_tour, timer_win, point_win, tile_win, river_on, abbaye_on);
    }

    public void setParameters(RoomParametersStruct param)
    {
        changed = false;
        setParameters(param.room_policy, param.wm, param.timer_tour, param.timer_win, param.point_win, param.tile_win, param.river_on, param.abbaye_on);
    }

    public void setParameters(bool room_policy, Tools.Mode wm, int timer_tour, int timer_win, int point_win, int tile_win, bool river_on, bool abbaye_on)
    {
        winmode = wm;
        timer_winmode = timer_win;
        tile_winmode = tile_win;
        point_winmode = point_win;
        room_policy_public = room_policy;
        this.timer_tour = timer_tour;
        riverOn = river_on;
        abbayeOn = abbaye_on;

        int index_win = 0;
        switch (winmode)
        {
            case Tools.Mode.Time:
                index_win = 1;
                break;
            case Tools.Mode.Default:
                index_win = 0;
                break;
            case Tools.Mode.Point:
                index_win = 2;
                break;
        }
        room_winmode_drop.SetValueWithoutNotify(index_win);
        for (int i = 0; i < wincond_parameters.Count; i++)
        {
            wincond_parameters[i].SetActive(i == index_win);
        }


        int time_value = 0;
        if (timer_tour > 20)
            time_value = timer_tour / 30 - 1;
        time_tour_slider.SetValueWithoutNotify(time_value);



        room_access_public.SetIsOnWithoutNotify(!room_policy);
        room_access_private.SetIsOnWithoutNotify(room_policy);

        room_extension_abbaye.SetIsOnWithoutNotify(abbaye_on);
        room_extension_rivier.SetIsOnWithoutNotify(river_on);

        room_win_tile.SetValueWithoutNotify(tile_win / 30 - 1);
        room_win_point.SetValueWithoutNotify(point_win / 50 - 1);
        if (timer_win == 3600)
            room_win_timer.SetValueWithoutNotify(1);
        else
            room_win_timer.SetValueWithoutNotify(0);
    }

    void OnDisable()
    {
        changed = false;
        setParameters(def_room_policy_public, Tools.Mode.Default, def_timer_tour, def_tile_winmode, def_point_winmode, def_tile_winmode, false, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (to_initialize)
        {
            initRoom();
            to_initialize = false;
        }
        if (changed)
            setParameters(change_planned);
    }
}
