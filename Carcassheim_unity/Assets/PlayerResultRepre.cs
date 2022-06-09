using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResultRepre : MonoBehaviour
{
    private int _rank = 0;
    private string _player_name = "";

    private int _score = 0;

    public int Height = 110;

    [SerializeField] TMPro.TMP_Text rank_repre;
    [SerializeField] TMPro.TMP_Text name_repre;
    [SerializeField] TMPro.TMP_Text score_repre;

    public int Rank { set { _rank = value; rank_repre.text = _rank.ToString(); } get => _rank; }
    public string PlayerName { set { _player_name = value; name_repre.text = _player_name; } get => _player_name; }

    public int Score { set { _score = value; score_repre.text = _score.ToString(); } get => _score; }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
