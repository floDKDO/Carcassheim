using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using Assets.system;
using System;

public class FalseBack : CarcasheimBack
{
    public int tile_number;


    public List<PlayerInitParam> my_players;
    public List<List<PositionRepre>> my_positions;
    public List<TurnPlayParam> my_plays;

    public List<List<TileInitParam>> my_tiles;
    public List<List<MeepleInitParam>> my_meeples;
    public List<List<PlayerScoreParam>> my_scores;

    public WinCondition my_win_condition;
    public List<int> win_param;

    public int my_timer_min;
    public int my_timer_sec;

    public int init_tile;
    public int player_index;
    public int my_player;


    public int scenario;

    public int num_turn;
    public int nb_turn;
    public bool first = true;

    [SerializeField] private DisplaySystem system_display;

    public int my_scenario;

    void Awake()
    {
        my_players = new List<PlayerInitParam>();


        my_scores = new List<List<PlayerScoreParam>>();
        my_meeples = new List<List<MeepleInitParam>>();
        my_tiles = new List<List<TileInitParam>>();
        my_positions = new List<List<PositionRepre>>();
        my_plays = new List<TurnPlayParam>();
        win_param = new List<int>();

        num_turn = 0;

        read_scenario();
    }



    void read_init(XmlReader reader)
    {
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "wincondition":
                            read_wincond(reader, win_param);
                            break;
                        case "players":
                            state = 0;
                            break;
                        case "tile":
                            state = 1;
                            break;
                        case "player":
                            if (state == 0)
                                read_player(reader);
                            break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 1: init_tile = int.Parse(reader.Value); break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "init";
                    state = -1;
                    break;
            }
        }
    }

    void read_turn(XmlReader reader)
    {
        int L = my_positions.Count;
        my_positions.Add(new List<PositionRepre>());
        my_meeples.Add(new List<MeepleInitParam>());
        my_tiles.Add(new List<TileInitParam>());
        my_scores.Add(new List<PlayerScoreParam>());

        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "positions": state = 0; break;
                        case "tiles": state = 1; break;
                        case "meeples": state = 2; break;
                        case "scores": state = 3; break;
                        case "play": read_play(reader); break;

                        case "position":
                            if (state == 0)
                                read_position_list(reader, my_positions[L]);
                            break;
                        case "tile":
                            if (state == 1)
                                read_tile(reader, my_tiles[L]);
                            break;
                        case "meeple":
                            if (state == 2)
                                read_meeple(reader, my_meeples[L]);
                            break;
                        case "score":
                            if (state == 3)
                                read_score(reader, my_scores[L]);
                            break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "turn";
                    state = -1;
                    break;
            }
        }
        nb_turn += 1;
    }

    void read_player(XmlReader reader)
    {
        PlayerInitParam player = new PlayerInitParam();
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "name": state = 0; break;
                        case "id": state = 1; break;
                        case "nb_meeple": state = 2; break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0: player.player_name = reader.Value; break;
                        case 1: player.id_player = int.Parse(reader.Value); break;
                        case 2: player.nb_meeple = int.Parse(reader.Value); break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "player";
                    state = -1;
                    break;
            }
        }
        my_players.Add(player);
    }

    void read_position_list(XmlReader reader, List<PositionRepre> list)
    {
        PositionRepre pos = new PositionRepre(0, 0, 0);
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "x": state = 0; break;
                        case "y": state = 1; break;
                        case "r": state = 2; break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0: pos = new PositionRepre(int.Parse(reader.Value), pos.Y, pos.Rotation); break;
                        case 1: pos = new PositionRepre(pos.X, int.Parse(reader.Value), pos.Rotation); break;
                        case 2: pos = new PositionRepre(pos.X, pos.Y, int.Parse(reader.Value)); break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "position";
                    state = -1;
                    break;
            }
        }
        if (pos.Rotation % 2 == 1)
            list.Add(new PositionRepre(pos.X, pos.Y, 0));
        if ((pos.Rotation / 2) % 2 == 1)
            list.Add(new PositionRepre(pos.X, pos.Y, 1));
        if ((pos.Rotation / 4) % 2 == 1)
            list.Add(new PositionRepre(pos.X, pos.Y, 2));
        if ((pos.Rotation / 8) % 2 == 1)
            list.Add(new PositionRepre(pos.X, pos.Y, 3));
    }

    PositionRepre read_position(XmlReader reader)
    {
        PositionRepre pos = new PositionRepre(0, 0, 0);
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "x": state = 0; break;
                        case "y": state = 1; break;
                        case "r": state = 2; break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0: pos = new PositionRepre(int.Parse(reader.Value), pos.Y, pos.Rotation); break;
                        case 1: pos = new PositionRepre(pos.X, int.Parse(reader.Value), pos.Rotation); break;
                        case 2: pos = new PositionRepre(pos.X, pos.Y, int.Parse(reader.Value)); break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "position";
                    state = -1;
                    break;
            }
        }
        return pos;
    }

    void read_wincond(XmlReader reader, List<int> param)
    {
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "cond": state = 0; break;
                        case "param": state = 1; break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0:
                            switch (int.Parse(reader.Value))
                            {
                                case 0:
                                    my_win_condition = WinCondition.WinByPoint;
                                    break;
                                case 1:
                                    my_win_condition = WinCondition.WinByTile;
                                    break;
                                default:
                                    my_win_condition = WinCondition.WinByTime;
                                    break;
                            }
                            break;
                        case 1:
                            param.Add(int.Parse(reader.Value));
                            break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "wincondition";
                    state = -1;
                    break;
            }
        }
    }
    void read_tile(XmlReader reader, List<TileInitParam> tiles)
    {
        TileInitParam tile = new TileInitParam();
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "id": state = 0; break;
                        case "flag": state = 1; break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0: tile.id_tile = int.Parse(reader.Value); break;
                        case 1: tile.tile_flags = int.Parse(reader.Value) != 0; break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "tile";
                    state = -1;
                    break;
            }
        }
        tiles.Add(tile);
    }

    void read_meeple(XmlReader reader, List<MeepleInitParam> meeples)
    {
        MeepleInitParam meeple = new MeepleInitParam();
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "id": state = 0; break;
                        case "ammount": state = 1; break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0: meeple.id_meeple = int.Parse(reader.Value); break;
                        case 1: meeple.nb_meeple = int.Parse(reader.Value); break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "meeple";
                    state = -1;
                    break;
            }
        }
        meeples.Add(meeple);
    }

    void read_score(XmlReader reader, List<PlayerScoreParam> scores)
    {
        PlayerScoreParam score = new PlayerScoreParam();
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "player": state = 0; break;
                        case "value": state = 1; break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0: score.id_player = ulong.Parse(reader.Value); break;
                        case 1: score.points_gagnes = int.Parse(reader.Value); break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "score";
                    state = -1;
                    break;
            }
        }
        scores.Add(score);
    }

    void read_play(XmlReader reader)
    {
        TurnPlayParam play = new TurnPlayParam();
        bool finished = false;
        int state = 0;
        while (!finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "tile": state = 0; break;
                        case "meeple": state = 1; break;
                        case "slot": state = 2; break;
                        case "position": play.tile_pos = read_position(reader); break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0:
                            play.id_tile = int.Parse(reader.Value);
                            break;
                        case 1:
                            play.id_meeple = int.Parse(reader.Value);
                            break;
                        case 2:
                            play.slot_pos = int.Parse(reader.Value);
                            break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    finished = reader.Name == "play";
                    state = -1;
                    break;
            }
        }
        my_plays.Add(play);
    }
    void read_scenario()
    {
        bool finished = false;
        using (XmlReader reader = XmlReader.Create(Application.streamingAssetsPath + "/scenario" + scenario.ToString() + ".xml"))
        {
            while (!finished && reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "scenario": break;
                            case "init":
                                read_init(reader);
                                break;
                            case "turn":
                                read_turn(reader);
                                break;
                            default:
                                Debug.Log("Tried to enter unknown state : " + reader.Name);
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        finished = reader.Name == "scenario";
                        break;
                }
            }
        }
    }

    void Start()
    {
        system_display.gameBegin();
    }

    // Update is called once per frame
    void Update()
    {

    }

    override public void sendTile(TurnPlayParam play)
    {
        if (num_turn + 1 >= nb_turn)
        {
            system_display.setNextState(DisplaySystemState.endOfGame);
        }
        else
        {
            system_display.setNextState(DisplaySystemState.scoreChange);
        }
    }

    override public void getTile(out TurnPlayParam play)
    {
        play = my_plays[num_turn];
    }

    override public void askMeeplesInit(List<MeepleInitParam> meeples)
    {
        meeples.AddRange(my_meeples[num_turn]);
    }

    override public int askTilesInit(List<TileInitParam> tiles)
    {
        tiles.AddRange(my_tiles[num_turn]);
        int tot = 0;
        foreach (TileInitParam param in tiles)
        {
            if (param.tile_flags)
                tot++;
        }
        return tot;
    }

    override public void askPlayersInit(List<PlayerInitParam> players)
    {
        players.AddRange(my_players);
    }

    override public void getTilePossibilities(int tile_id, List<PositionRepre> positions)
    {
        positions.AddRange(my_positions[num_turn]);
    }

    override public void askPlayerOrder(List<int> player_ids)
    {
    }

    override public void askScores(List<PlayerScoreParam> players_scores, List<Zone> zones)
    {
        players_scores.AddRange(my_scores[num_turn]);
        num_turn += 1;
        if (num_turn < nb_turn)
            system_display.setNextState(DisplaySystemState.turnStart);
        else
            system_display.setNextState(DisplaySystemState.endOfGame);
    }

    override public int askIdTileInitial()
    {
        return init_tile;
    }

    override public int getNextPlayer()
    {
        return my_players[num_turn % my_players.Count].id_player;
    }

    override public int getMyPlayer()
    {
        return my_player;
    }

    override public void askTimerTour(out int min, out int sec)
    {
        min = my_timer_min;
        sec = my_timer_sec;
    }

    override public void sendAction(DisplaySystemAction action)
    {
    }

    override public void askWinCondition(ref WinCondition win_cond, List<int> parameters)
    {
        win_cond = my_win_condition;
        parameters.AddRange(win_param);
    }

    override public void askFinalScore(List<PlayerScoreParam> playerScores, List<Zone> zones)
    {
        //TODO Pareil que askScore
    }

    override public void askMeeplePosition(MeeplePosParam mp, List<int> slot_pos)
    {

    }
    public override void askMeepleRetired(List<Tuple<int, int, ulong>> positions)
    {
    }
}