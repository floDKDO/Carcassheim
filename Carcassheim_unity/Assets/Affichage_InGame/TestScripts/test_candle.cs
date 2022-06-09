using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_candle : MonoBehaviour
{
    [SerializeField] private candle_manager activated;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            activated.swicthCandle();
        }
    }
}
