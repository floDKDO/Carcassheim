using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class candle_manager : MonoBehaviour
{
    [SerializeField] private Animator myAnimator;
    private bool candle_is_lit = false;

    public void lightCandle()
    {
        myAnimator.SetTrigger("OnActivation");
        candle_is_lit = true;
    }

    public void shutCandle()
    {
        myAnimator.SetTrigger("OnDeactivation");
        candle_is_lit = false;
    }

    public void swicthCandle()
    {
        if (candle_is_lit)
        {
            shutCandle();
        }
        else
        {
            lightCandle();
        }
    }

}
