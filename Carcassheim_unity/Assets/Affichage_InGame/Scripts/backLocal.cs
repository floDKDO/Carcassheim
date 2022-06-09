using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.system;
using System;
using TMPro;
public class backLocal : CarcasheimBack
{
    private const string XML_PATH = "config_back.xml";
    private Dictionary<ulong, Tuile> dicoTuile;

    private Plateau _plateau;
    private List<PlayerInitParam> players = new List<PlayerInitParam>();
    private List<int> saved_players_score = new List<int>();

    private int index_player = 0; // joueur en jeu
    private int nb_player = 2; // à remplir via field

    public int NbPlayer { get => nb_player; set { nb_player = value + 2; } }

    [SerializeField] private TMP_Text error_msg;

    private WinCondition my_wincond = WinCondition.WinByTile;
    [SerializeField] private GameObject fen_point;
    [SerializeField] private GameObject fen_tile;
    [SerializeField] private GameObject fen_time;

    public int win_time_sec { set; get; } = 0;
    public int win_time_min { set; get; } = 10;

    private long _timeLapse = 0;
    private int _turnMaxMin = 99999;
    private int _turnMaxSec;

    long time_start_of_game = 0;

    bool _hasGameStarted = false;
    public int win_tile_nb { set; get; } = 70;
    int nb_tile_drawn = 0;
    public int win_point_nb { set; get; } = 60;

    private int nb_meeple = 10;


    private int compteur_de_tour = 0;
    private int last_generated_tile_tour = -1;

    private TurnPlayParam act_turn_play;

    private ulong tile_init_normal = 20;

    private ulong tile_init_river = 24;

    private ulong tile_final_river = 35;

    public bool river_on { set; get; } = false;

    private List<ulong> tiles_for_river;
    List<PlayerScoreParam> gains = new List<PlayerScoreParam>();
    List<Zone> zones = new List<Zone>();

    private List<Position> possibilities_tile_act_turn = new List<Position>();
    private List<ulong> tile_drawn = new List<ulong>();

    [SerializeField] private DisplaySystem system_display;

    [SerializeField] GameObject panel_param;

    [SerializeField] List<GameObject> win_params;
    List<System.Tuple<int, int, ulong>> meeple_positions = new List<Tuple<int, int, ulong>>();

    bool turn_on = false;

    void Start()
    {
        dicoTuile = LireXML2.Read(XML_PATH);
        _plateau = new Plateau(dicoTuile);
        //gameStart();
    }

    public void begin_pressed()
    {
        // Debug.Log("COucou hibou");
    }

    public void setWinCondition(int win_cond)
    {
        switch (my_wincond)
        {
            case WinCondition.WinByTime:
                win_params[1].SetActive(false);
                break;
            case WinCondition.WinByPoint:
                win_params[2].SetActive(false);
                break;
            default:
                win_params[0].SetActive(false);
                break;
        }
        win_params[win_cond].SetActive(true);
        switch (win_cond)
        {
            case 2:
                my_wincond = WinCondition.WinByPoint;
                break;
            case 1:
                my_wincond = WinCondition.WinByTime;
                break;
            default:
                my_wincond = WinCondition.WinByTile;
                break;
        }
    }

    bool validate_start()
    {
        bool valid = true;
        switch (my_wincond)
        {
            case WinCondition.WinByPoint:
                valid = win_point_nb > 0;
                break;
            case WinCondition.WinByTime:
                valid = win_time_min > 0 && win_time_sec >= 0;
                break;
            case WinCondition.WinByTile:
                valid = win_tile_nb > 10;
                break;
        }
        valid = valid && nb_player >= 2;

        return valid;
    }

    void generatePlayers()
    {
        for (int i = 0; i < nb_player; i++)
        {
            players.Add(new PlayerInitParam(i, nb_meeple, "Joueur " + (i + 1).ToString()));
            saved_players_score.Add(0);
        }
    }

    public void gameStart()
    {
        if (validate_start())
        {
            _hasGameStarted = true;
            _timeLapse = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            askTimerTour(out _turnMaxMin, out _turnMaxSec);

            panel_param.SetActive(false);
            _plateau.Poser1ereTuile((ulong)askIdTileInitial());

            switch (my_wincond)
            {
                case WinCondition.WinByTime:
                    time_start_of_game = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    break;
            }

            if (river_on)
            {
                tiles_for_river = new List<ulong>();
                foreach (Tuile tile in dicoTuile.Values)
                {
                    if (tile.isARiver() && tile.Id != tile_init_river && tile.Id != tile_final_river)
                    {
                        tiles_for_river.Add(tile.Id);
                    }
                }
            }

            generatePlayers();
            system_display.setNextState(DisplaySystemState.gameStart);
        }
        else
        {
            error_msg.text = "Paramètres invalides";
        }
    }

    void newTurn()
    {
        system_display.setNextState(DisplaySystemState.turnStart);
        index_player = (index_player + 1) % players.Count;
        compteur_de_tour += 1;
        _timeLapse = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }


    override public void sendTile(TurnPlayParam play)
    {
        bool end = false;
        bool score_changed = false;
        // Debug.Log("Am i looking likethis " + play.id_tile + " " + play.tile_pos + " " + play.id_meeple + " " + play.slot_pos);
        // Debug.Log("Meeple at " + play.id_meeple + " " + play.slot_pos);
        bool tuile_valide = false;
        bool meeple_valide = false;
        ulong player_act = (ulong)players[index_player].id_player;
        // Debug.Log(_plateau.PlacementLegal((ulong)play.id_tile, play.tile_pos.X, play.tile_pos.Y, play.tile_pos.Rotation));
        if (play.id_tile != -1 && _plateau.PlacementLegal((ulong)play.id_tile, play.tile_pos.X, play.tile_pos.Y, play.tile_pos.Rotation))
        {
            _plateau.PoserTuileFantome((ulong)play.id_tile, play.tile_pos.X, play.tile_pos.Y, play.tile_pos.Rotation);
            tuile_valide = true;
        }
        //Debug.Log(play.id_meeple != -1 && _plateau.PionPosable(play.tile_pos.X, play.tile_pos.Y, (ulong)play.slot_pos, player_act, (ulong)play.id_meeple));
        if (play.id_meeple != -1 && tuile_valide && _plateau.PionPosable(play.tile_pos.X, play.tile_pos.Y, (ulong)play.slot_pos, player_act, (ulong)play.id_meeple))
        {
            _plateau.PoserPion(player_act, play.tile_pos.X, play.tile_pos.Y, (ulong)play.slot_pos);
            players[index_player] = new PlayerInitParam(players[index_player].id_player, players[index_player].nb_meeple - 1, players[index_player].player_name);
            meeple_valide = true;
        }
        if (tuile_valide)
        {
            _plateau.ValiderTour();

            gains.Clear();
            zones.Clear();
            score_changed = _plateau.VerifZoneFermeeTuile(play.tile_pos.X, play.tile_pos.Y, gains, zones);
            if (score_changed)
            {
                for (int i = 0; i < gains.Count; i++)
                {
                    saved_players_score[(int)gains[i].id_player] += gains[i].points_gagnes;
                }

                Dictionary<ulong, int> dico = _plateau.RemoveAllPawnInTile(play.tile_pos.X, play.tile_pos.Y, meeple_positions);
                foreach (ulong id_player in dico.Keys)
                {
                    var joueur = players[(int)id_player];
                    int meeplesRendus = dico[id_player];
                    var temp = new PlayerInitParam(joueur.id_player, joueur.nb_meeple + meeplesRendus, joueur.player_name);
                    players[(int)id_player] = temp;
                    joueur = temp;
                }
            }
        }
        switch (my_wincond)
        {
            case WinCondition.WinByTime:
                end = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - time_start_of_game) > win_time_min * 60 + win_time_sec;
                break;
            case WinCondition.WinByPoint:
                for (int i = 0; i < players.Count; i++)
                {
                    end = end || (saved_players_score[i] >= win_point_nb);
                }
                break;
            case WinCondition.WinByTile:
                end = _plateau.GetTuiles.Length >= win_tile_nb;
                break;
        }
        act_turn_play = new TurnPlayParam(tuile_valide ? play.id_tile : -1, tuile_valide ? play.tile_pos : null, meeple_valide ? play.id_meeple : -1, meeple_valide ? play.slot_pos : -1);

        if (score_changed)
            system_display.setNextState(DisplaySystemState.scoreChange);
        if (end)
        {
            system_display.setNextState(DisplaySystemState.endOfGame);
        }
        else
        {
            newTurn();
        }

    }

    public void Update()
    {
        if (!_hasGameStarted)
            return;

        long timeLapse = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _timeLapse;

        if (turn_on && timeLapse > _turnMaxMin * 60 + _turnMaxSec)
        {
            turn_on = false;

            system_display.setNextState(DisplaySystemState.idleState);
        }
    }

    override public void getTile(out TurnPlayParam play)
    {
        play = act_turn_play;
    }

    override public void askMeeplesInit(List<MeepleInitParam> meeples)
    {
        if (players[index_player].nb_meeple > 0 && !river_on)
            meeples.Add(new MeepleInitParam(0, players[index_player].nb_meeple));
        river_on = river_on && tile_final_river != ulong.MaxValue;
    }

    private void generateTile()
    {
        if (last_generated_tile_tour >= compteur_de_tour)
        {
            Debug.Log("Shouldn't generate new tile");
        }
        else
        {
            //int[] tiles = new int[] { 19, 21, 18, 11, 17, 1, 1 };
            int[] tiles = new int[] { 23, 14, 10, 10, 12, 11, 11, 15 };
            possibilities_tile_act_turn.Clear();
            tile_drawn.Clear();
            do
            {
                int index;
                if (compteur_de_tour >= tiles.Length)
                    index = UnityEngine.Random.Range(0, 24);
                else
                    index = tiles[compteur_de_tour];
                tile_drawn.Add((ulong)index);
                possibilities_tile_act_turn.AddRange(_plateau.PositionsPlacementPossible(tile_drawn[tile_drawn.Count - 1]));
                nb_tile_drawn += 1;
            } while (possibilities_tile_act_turn.Count <= 0);// && nb_tile_drawn < win_tile_nb);
            last_generated_tile_tour = compteur_de_tour;
        }
    }

    int drawRiver()
    {
        possibilities_tile_act_turn.Clear();
        ulong result = 0;
        if (tiles_for_river.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, tiles_for_river.Count);
            result = tiles_for_river[index];
            tiles_for_river.RemoveAt(index);
        }
        else
        {
            result = tile_final_river;
            tile_final_river = ulong.MaxValue;

        }
        possibilities_tile_act_turn.AddRange(_plateau.PositionsPlacementPossible(result));
        return (int)result;
    }

    public override void askMeepleRetired(List<Tuple<int, int, ulong>> positions)
    {
        system_display.meepleGoback(meeple_positions);
        meeple_positions.Clear();
    }

    override public int askTilesInit(List<TileInitParam> tiles)
    {
        if (river_on)
        {
            tiles.Add(new TileInitParam(drawRiver(), true));
        }
        else
        {
            generateTile();

            for (int i = 0; i < tile_drawn.Count; i++)
            {
                tiles.Add(new TileInitParam((int)tile_drawn[i], i + 1 == tile_drawn.Count));
            }
        }
        return 1;
    }

    override public void askPlayersInit(List<PlayerInitParam> players)
    {
        players.AddRange(this.players);
    }

    override public void getTilePossibilities(int tile_id, List<PositionRepre> positions)
    {
        foreach (Position pos in possibilities_tile_act_turn)
        {
            //Debug.Log("POSITION POSSIBLE : "+pos.ToString());
            positions.Add(new PositionRepre(pos.X, pos.Y, pos.ROT));
        }
    }

    override public void askPlayerOrder(List<int> player_ids)
    {

    }

    override public void askScores(List<PlayerScoreParam> players_scores, List<Zone> zones)
    {
        for (int i = 0; i < players.Count; i++)
        {
            ulong id_p = (ulong)players[i].id_player;
            int score_p = saved_players_score[i];
            players_scores.Add(new PlayerScoreParam(id_p, score_p));
        }
    }

    override public int askIdTileInitial()
    {
        return (int)(river_on ? tile_init_river : tile_init_normal);
    }

    override public int getNextPlayer()
    {
        return players[index_player].id_player;
    }

    override public int getMyPlayer()
    {
        return -1;
    }

    override public void askTimerTour(out int min, out int sec)
    {
        min = 0;
        sec = 10;
    }

    override public void sendAction(DisplaySystemAction action)
    {

    }

    override public void askWinCondition(ref WinCondition win_cond, List<int> parameters)
    {
        win_cond = my_wincond;
        switch (win_cond)
        {
            case WinCondition.WinByTime:
                parameters.Add(win_time_min);
                parameters.Add(win_time_sec);
                break;
            case WinCondition.WinByPoint:
                parameters.Add(win_point_nb);
                break;
            case WinCondition.WinByTile:
                parameters.Add(win_tile_nb + (river_on ? tiles_for_river.Count + 1 : 0));
                break;
        }
    }

    override public void askFinalScore(List<PlayerScoreParam> playerScores, List<Zone> zones)
    {
        //TODO Pareil que askScore
    }

    override public void askMeeplePosition(MeeplePosParam mp, List<int> slot_pos)
    {
        _plateau.PoserTuileFantome((ulong)mp.id_tile, mp.pos_tile.X, mp.pos_tile.Y, mp.pos_tile.Rotation);
        // Debug.Log("ROTATION   " + mp.pos_tile);
        slot_pos.AddRange(_plateau.EmplacementPionPossible(mp.pos_tile.X, mp.pos_tile.Y, (ulong)players[index_player].id_player/*, (ulong)mp.id_meeple*/));
    }
}
