using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.system;


public enum WinCondition
{
    WinByTime,
    WinByPoint,
    WinByTile
};

public struct PlayerInitParam
{
    public PlayerInitParam(int id_player, int nb_meeple, string player_name)
    {
        this.id_player = id_player;
        this.nb_meeple = nb_meeple;
        this.player_name = player_name;
    }

    public int id_player;
    public int nb_meeple;
    public string player_name;
};

public struct MeepleInitParam
{
    public MeepleInitParam(int id_meeple, int nb_meeple)
    {
        this.id_meeple = id_meeple;
        this.nb_meeple = nb_meeple;
    }
    public int id_meeple;
    public int nb_meeple;
};

public struct TileInitParam
{
    public TileInitParam(int id_tile, bool tile_flags)
    {
        this.id_tile = id_tile;
        this.tile_flags = tile_flags;
    }
    public int id_tile;
    public bool tile_flags;
};

public struct MeeplePosParam
{
    public MeeplePosParam(int id_tile, PositionRepre pos_tile, int id_meeple)
    {
        this.id_tile = id_tile;
        this.id_meeple = id_meeple;
        this.pos_tile = pos_tile;
    }

    public int id_meeple;

    public int id_tile;

    public PositionRepre pos_tile;
}

public struct TurnPlayParam
{
    public TurnPlayParam(int id_tile, PositionRepre tile_pos, int id_meeple, int slot_pos)
    {
        this.id_tile = id_tile;
        this.tile_pos = tile_pos;
        this.id_meeple = id_meeple;
        this.slot_pos = slot_pos;
    }

    public int id_tile;
    public PositionRepre tile_pos;
    public int id_meeple;
    public int slot_pos;
}

public abstract class CarcasheimBack : MonoBehaviour
{
    /// <summary>
    /// donne une tuile a un joueur, qu'il place sur le plateau et pose un meeple dessus
    /// </summary>
    /// <param name="play">la tuile a jouer, dans quelle position, le type de meeple l'id du slot sur lequel mettre le meeple</param>
    public abstract void sendTile(TurnPlayParam play);

    /// <summary>
    /// recupere la derniere tuile jouee
    /// </summary>
    /// <param name="play">l'id de la tuile, sa position, l'id du slot sur lequel le meeple a ete place, le type du meeple</param>
    public abstract void getTile(out TurnPlayParam play);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="meeples"></param>
    public abstract void askMeeplesInit(List<MeepleInitParam> meeples);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    public abstract int askTilesInit(List<TileInitParam> tiles);

    /// <summary>
    /// remplie une liste avec comme informations l'id des joueurs, leur nom et de combien de meeples ils disposent pour cette partie
    /// </summary>
    /// <param name="players">la liste</param>
    public abstract void askPlayersInit(List<PlayerInitParam> players);

    /// <summary>
    /// calcul les positions ou une tuile est posable sur le plateau
    /// </summary>
    /// <param name="tile_id">l'id de la tuile</param>
    /// <param name="positions">liste des positions ou la tuile est posable</param>
    public abstract void getTilePossibilities(int tile_id, List<PositionRepre> positions);

    /// <summary>
    /// remplie et trie une liste avec les ids des joueurs en fonction de l'ordre dans lequel il joue
    /// </summary>
    /// <param name="player_ids">la liste contenant les ids ordonnes des joueurs</param>
    public abstract void askPlayerOrder(List<int> player_ids);

    /// <summary>
    /// retourne le joueur qui doit etre le prochain a jouer
    /// </summary>
    /// <returns>l'id du prochain joueur</returns>
    public abstract int getNextPlayer();

    /// <summary>
    /// getter du joueur utilisant ce client
    /// </summary>
    /// <returns>l'id du joueur</returns>
    public abstract int getMyPlayer();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="players_scores"></param>
    /// <param name="zones"></param>
    public abstract void askScores(List<PlayerScoreParam> players_scores, List<Zone> zones);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerScores"></param>
    /// <param name="zones"></param>
    public abstract void askFinalScore(List<PlayerScoreParam> playerScores, List<Zone> zones);

    /// <summary>
    /// le temps maximal qu'un joueur peut prendre pour un tour, en minute + seconde
    /// </summary>
    /// <param name="min">les minutes</param>
    /// <param name="sec">les secondes</param>
    public abstract void askTimerTour(out int min, out int sec);

    /// <summary>
    /// renvoie soit la tuile demarrant le jeu
    /// </summary>
    /// <returns>l'id de la tuile</returns>
    public abstract int askIdTileInitial();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public abstract void sendAction(DisplaySystemAction action);

    /// <summary>
    /// renvoie les conditions qui arrete une partie
    /// </summary>
    /// <param name="win_cond">le type de la condition de victoire</param>
    /// <param name="parameters">les parametres de la victoire</param>
    public abstract void askWinCondition(ref WinCondition win_cond, List<int> parameters);

    /// <summary>
    /// calcul les slots ou un joueur peut poser un meeple
    /// </summary>
    /// <param name="mp">les informations du tour du joueur</param>
    /// <param name="slot_pos">liste d'id des slots ou le meeple est posable</param>
    public abstract void askMeeplePosition(MeeplePosParam mp, List<int> slot_pos);

    public abstract void askMeepleRetired(List<System.Tuple<int, int, ulong>> positions);
}