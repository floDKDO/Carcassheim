using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BannerWinPoint : BannerWinCondition
{
    public TMP_Text point_text;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    override public void setWinParameters(List<int> param)
    {
        point_text.text = param[0].ToString();
    }
}
