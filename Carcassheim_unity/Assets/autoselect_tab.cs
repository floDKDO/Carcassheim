using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class autoselect_tab : MonoBehaviour
{
    [SerializeField] List<InputField> inputs;

    [SerializeField] List<Button> event_on_validate;

    private int index_field = -1;

    [SerializeField] bool tab_enabled = true;
    [SerializeField] bool enter_enabled = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (tab_enabled && Input.GetKeyUp(KeyCode.Tab) && inputs.Count > 0)
        {
            index_field = (index_field + 1) % inputs.Count;
            inputs[index_field].Select();
            inputs[index_field].ActivateInputField();
        }
        if (enter_enabled && Input.GetKeyUp(KeyCode.Return) && index_field != -1)
        {
            foreach (Button bt in event_on_validate)
            {
                bt.onClick?.Invoke();
            }
        }
    }


    void OnEnable()
    {
        index_field = 0;
        if (tab_enabled && inputs.Count > 0)
        {
            inputs[index_field].Select();
            inputs[index_field].ActivateInputField();
        }
    }

    void OnDisable()
    {
        index_field = -1;
    }

}
