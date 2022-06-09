using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BannerWinTime : BannerWinCondition
{
    [SerializeField] TimerRepre timer;
    [SerializeField] TMP_Text timer_text;


    // Start is called before the first frame update
    void Start()
    {

    }

    void OnEnable()
    {
        timer.OnSecondPassed += secondPassed;
        timer_text.text = timer.ToString();
    }

    void OnDisable()
    {
        timer.OnSecondPassed -= secondPassed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void secondPassed()
    {
        timer_text.text = timer.ToString();
    }

    public override void setWinParameters(List<int> parameters)
    {
        timer.setTime(parameters[0], parameters[1]);
        timer_text.text = timer.ToString();
        timer.startTimer();
    }
}
