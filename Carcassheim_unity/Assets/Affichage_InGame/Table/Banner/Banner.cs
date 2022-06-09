using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Banner : MonoBehaviour
{

    public TimerRepre timerTour = null;
    [SerializeField] private TMP_Text nbPlayerTMP;
    [SerializeField] private TMP_Text nbMeepleTMP;
    [SerializeField] private TMP_Text timerTourTMP;
    [SerializeField] private TMP_Text nbPointsTMP;
    [SerializeField] private DisplaySystem master;



    [SerializeField] private BannerWinCondition win_condition_time;
    [SerializeField] private BannerWinCondition win_condition_tile;
    [SerializeField] private BannerWinCondition win_condition_point;

    private PlayerRepre _player = null;
    int _nb_player = 0;
    // Start is called before the first frame update

    void Awake()
    {
        nbPlayerTMP.text = "0";
        nbMeepleTMP.text = "0";
        nbPointsTMP.text = "0";
    }

    void OnEnable()
    {
        if (_player != null)
        {
            _player.OnMeepleUpdate += meepleUpdated;
            _player.OnScoreUpdate += scoreUpdated;
            nbMeepleTMP.text = _player.NbMeeple.ToString();
            nbPointsTMP.text = _player.Score.ToString();
        }
        if (timerTour != null)
        {
            timerTour.OnSecondPassed += tourUpdated;
        }
        master.OnPlayerDisconnected += playerDisconnected;
    }

    void OnDisable()
    {
        if (_player != null)
        {
            _player.OnMeepleUpdate -= meepleUpdated;
            _player.OnScoreUpdate -= scoreUpdated;
        }
        if (timerTour != null)
        {
            timerTour.OnSecondPassed -= tourUpdated;
        }
        master.OnPlayerDisconnected -= playerDisconnected;
    }

    public void playerDisconnected(PlayerRepre player)
    {
        if (_nb_player > 0)
            _nb_player -= 1;
        nbPlayerTMP.text = _nb_player.ToString();
    }

    public void setPlayer(PlayerRepre player)
    {
        if (_player != null)
        {
            _player.OnMeepleUpdate -= meepleUpdated;
            _player.OnScoreUpdate -= scoreUpdated;
        }
        _player = player;
        if (_player != null)
        {
            _player.OnMeepleUpdate += meepleUpdated;
            _player.OnScoreUpdate += scoreUpdated;
            // Debug.Log("HERE NB PLAYER " + player.NbMeeple + " " + player.Score);
            nbMeepleTMP.text = player.NbMeeple.ToString();
            nbPointsTMP.text = player.Score.ToString();
        }
    }

    public void setPlayerNumber(int number)
    {
        _nb_player = number;
        nbPlayerTMP.text = _nb_player.ToString();
    }

    public void setTimerTour(int min, int sec)
    {
        timerTour.setTime(min, sec);
        timerTour.OnSecondPassed += tourUpdated;
        timerTourTMP.text = timerTour.ToString();
    }

    void scoreUpdated(int old_score, int new_score)
    {
        nbPointsTMP.text = new_score.ToString();
    }

    void meepleUpdated(int meeple)
    {
        nbMeepleTMP.text = meeple.ToString();
    }

    void playerUpdated(int nb_player)
    {
        _nb_player = nb_player;
        nbPlayerTMP.text = nb_player.ToString();
    }

    void tourUpdated()
    {
        timerTourTMP.text = timerTour.ToString();
    }

    public void setWinCondition(WinCondition win, Table table, List<int> param)
    {
        BannerWinCondition win_cond;
        switch (win)
        {
            case WinCondition.WinByTime:
                win_cond = Instantiate<BannerWinCondition>(win_condition_time, transform);
                table.setTileNumber(-1);
                break;
            case WinCondition.WinByPoint:
                win_cond = Instantiate<BannerWinCondition>(win_condition_point, transform);
                table.setTileNumber(-1);
                break;
            default:
                win_cond = Instantiate<BannerWinCondition>(win_condition_tile, transform);
                table.setTileNumber(param[0]);
                break;
        }
        win_cond.setWinParameters(param);
    }
}
