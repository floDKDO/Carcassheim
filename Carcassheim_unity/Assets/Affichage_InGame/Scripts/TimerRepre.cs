using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerRepre : MonoBehaviour
{
    // Start is called before the first frame update
    private float _remainingTime = 0;
    private float _referenceTime = 0;
    private int _min = 0;
    private int _sec = 0;
    private int _refMin = 0;
    private int _refSec = 0;
    private bool running = false;
    private bool finished = false;

    public event System.Action OnSecondPassed;

    public void setTime(int min, int sec)
    {
        _referenceTime = min * 60f + sec + 0.001f;
        _remainingTime = _referenceTime;
        _min = min;
        _sec = sec;
        _refMin = min;
        _refSec = sec;
        finished = false;
    }

    public void setTime(TimerRepre timer)
    {
        setTime(timer._refMin, timer._refSec);
    }

    void Update()
    {
        if (running && !finished)
        {
            if (Time.deltaTime > _remainingTime)
            {
                _remainingTime = 0;
                _min = 0;
                _sec = 0;
                OnSecondPassed?.Invoke();
                // Debug.Log("Running finish : " + ToString());
                finished = true;
            }
            else
            {
                _remainingTime -= Time.deltaTime;
                _min = (int)(_remainingTime / 60);
                int sec = (int)(_remainingTime + 0.99f) % 60;
                if (sec != _sec)
                {
                    _sec = sec;
                    // Debug.Log("Running : " + ToString());
                    OnSecondPassed?.Invoke();
                }
            }
        }
    }

    public void startTimer()
    {
        // Debug.Log("Timer start : " + ToString());
        OnSecondPassed?.Invoke();
        running = true;
    }

    public void pauseTimer()
    {
        // Debug.Log("Timer pause : " + ToString());
        OnSecondPassed?.Invoke();
        running = false;

    }

    public bool timerRunning()
    {
        return running;
    }

    public void reset()
    {
        // Debug.Log("Timer reset : " + ToString());
        _remainingTime = _referenceTime;
        _min = _refMin;
        _sec = _refSec;
        finished = false;
        OnSecondPassed?.Invoke();
    }

    public void resetStop()
    {
        reset();
        running = false;
    }

    override public string ToString()
    {

        return string.Format("{0:00}:{1:00}", _min, _sec);
    }


}
