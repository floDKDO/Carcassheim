using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_banner : MonoBehaviour
{

    public Banner banner;
    private TimerRepre timer;
    void Start()
    {
        timer = banner.timerTour;
        banner.setTimerTour(0, 10);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("t"))
        {
            if (timer.timerRunning())
            {
                timer.pauseTimer();
            }
            else
            {
                timer.startTimer();
            }
        }
        else if (Input.GetKeyUp("r"))
        {
            timer.reset();
        }
    }
}
