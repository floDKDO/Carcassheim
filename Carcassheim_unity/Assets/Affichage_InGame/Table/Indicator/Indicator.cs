using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Indicator : MonoBehaviour
{
    public int Value { set { _stored_value = value; value_repre.text = _stored_value < 0 ? "<size=200%>\u221E</size>" : value.ToString(); } get { return _stored_value; } }
    private int _stored_value = -1;
    [SerializeField] private TMP_Text value_repre;

    // Start is called before the first frame update
    void Start()
    {
        value_repre.text = _stored_value < 0 ? "<size=200%>\u221E</size>" : _stored_value.ToString();
    }
}
