using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLine : MonoBehaviour
{
    [SerializeField] public Text player_name;
    [SerializeField] public Image player_status;

    private string _player_name;
    public string Player_name
    {
        get => _player_name;
        set
        {
            player_name.text = value;
        }
    }

    private bool _player_status;
    public bool Player_status
    {
        get => _player_status;
        set
        {
            Color c = Color.red;
            if (value)
            {
                c = Color.green;
            }

            player_status.color = c;
        }
    }


    [SerializeField] public PlayerLine model;

    [SerializeField]
    public RectTransform parent_area;

    static int player_created_index = 0;

    bool incremented = false;
    bool destroyed = false;

    [SerializeField] Image background;
    [SerializeField] List<Color> background_color;

    // Start is called before the first frame update
    void Start()
    {
        background.color = background_color[player_created_index % 2];
        player_created_index += 1;
        incremented = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void EnableOnList()
    {
        if (this != model)
        {
            Vector2 dim = parent_area.sizeDelta;
            dim.y += 100;
            parent_area.sizeDelta = dim;
        }
    }
    void OnDisable()
    {
        if (this != model)
        {
            Vector2 dim = parent_area.sizeDelta;
            dim.y -= 100;
            parent_area.sizeDelta = dim;
            Destroy(gameObject);
        }
        destroyed = true;
    }

    public void killPlayerLine()
    {
        if (!destroyed)
        {
            destroyed = true;
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (this != model && incremented)
        {
            incremented = false;
            player_created_index -= 1;
        }
    }
}
