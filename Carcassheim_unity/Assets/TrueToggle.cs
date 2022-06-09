using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrueToggle : MonoBehaviour
{

    [SerializeField] List<Toggle> toggles;

    public UnityEvent<int> onChanged;
    int last_connected = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].SetIsOnWithoutNotify(i == 0);
            toggles[i].onValueChanged.AddListener(onChange);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onChange(bool state)
    {
        if (state)
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                if (i == last_connected)
                    continue;
                else if (toggles[i].isOn)
                {
                    last_connected = i;
                }
            }
            onChanged?.Invoke(last_connected);
        }
    }
    public void test_int(int i)
    {
        Debug.Log(i);
    }
}
