using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerRepre
{

    public Color color;

    public event Action<int, int> OnScoreUpdate;
    public event Action<int> OnMeepleUpdate;


    public int Id { get => _id; private set => _id = value; }
    public string Name { get => _name; private set => _name = value; }

    public bool is_my_player;
    public int Score
    {
        get => _score;
        set
        {
            if (value < 0)
                value = 0;
            OnScoreUpdate?.Invoke(_score, value);
            _score = value;
        }
    }
    public int NbMeeple
    {
        get => _nb_meeple;
        set
        {
            if (value < 0)
                value = 0;
            OnMeepleUpdate?.Invoke(value);
            _nb_meeple = value;
        }
    }

    static private int id_gen = 0;
    private int _id;
    private int _score;
    private int _nb_meeple;
    private string _name;


    public PlayerRepre()
    {
        color = new Color(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f));

        _id = id_gen++;
        _nb_meeple = 0;
        _score = 0;
        _name = "Zorglub";
    }

    public PlayerRepre(string name, int id, Color col)
    {
        Id = id;
        Name = name;
        color = col;
        _score = 0;
        _nb_meeple = 0;
    }
    public PlayerRepre(PlayerInitParam param, Color col)
    {
        Id = param.id_player;
        Name = param.player_name;
        _nb_meeple = param.nb_meeple;
        color = col;
        _score = 0;
    }
}
