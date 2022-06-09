using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
///    Miscellaneous.
/// </summary>
public abstract class Miscellaneous : MonoBehaviour
{
    [SerializeField]
    public GameObject absolute_parent;
    static protected GameObject absolute_parent_ref;
    public static event Action<string> OnMenuChange;
    private static bool s_state = false;
    private static bool s_menuHasChanged = false;
    private static GameObject previousMenu = null;
    private static GameObject nextMenu = null;
    private Color colState;
    public GameObject Pop_up_Options;
    public static bool s_isOpenPanel = false;

    public static bool s_menuChanged = false;

    private static bool clicked = false;

    /// <summary>
    ///    MonoBehaviour Awake is called when the script instance is being loaded.
    ///    Awake is called only once during the lifetime of the script instance.
    ///    Awake is always called before any Start functions.
    ///    Awake is called in the editor before OnEnable.
    ///    If it is overridden, it is called before the subclass' Awake.
    /// </summary>
    void Awake()
    {
        if (absolute_parent == null)
            absolute_parent = absolute_parent_ref;
        Pop_up_Options = Miscellaneous.FindObject(absolute_parent, "SubMenus").transform.Find("Panel Options").gameObject;
        nextMenu = Miscellaneous.FindObject(absolute_parent, "HomeMenu"); // Menu courant au lancement du jeu
    }

    /// <summary>
    ///     Create a room line <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "roomline_model">The roomline model.</param>
    /// <param name = "id">The identifier.</param>
    /// <param name = "host">The host.</param>
    /// <param name = "nb_player">The number of players.</param>
    /// <param name = "nb_player_max">The max number of players.</param>
    /// <param name = "endgame">The endgame.</param>
    /// <returns>The room line.</returns>
    static protected RoomLine CreateRoomLine(RoomLine roomline_model, string id, string host, string nb_player, string nb_player_max, string endgame)
    {
        RoomLine rm = Instantiate<RoomLine>(roomline_model, roomline_model.parent_area);
        rm.Id = ulong.Parse(id);
        rm.Host = host;
        rm.NbPlayer = int.Parse(nb_player);
        rm.NbPlayerMax = int.Parse(nb_player_max);
        //rm.Victory = int.Parse(endgame);
        rm.victory_text.text = endgame;
        rm.model = null;
        rm.EnableOnList();
        return rm;
    }

    /// <summary>
    ///     Create a room line <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "playerline_model">The roomline model.</param>
    /// <param name = "playerName">The name.</param>
    /// <param name = "status">The status.</param>
    /// <returns>The player line.</returns>
    static protected PlayerLine CreatePlayerLine(PlayerLine playerline_model, string playerName, bool status)
    {
        PlayerLine pl = Instantiate<PlayerLine>(playerline_model, playerline_model.parent_area);
        pl.Player_name = playerName;
        pl.Player_status = status;
        pl.EnableOnList();
        return pl;
    }

    /// <summary>
    ///     Find a GameObject <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "parent">The parent of the object to find.</param>
    /// <param name = "name">The name of the object to find.</param>
    /// <returns>The GameObject.</returns>
    public static GameObject FindObject(GameObject parent, string name)
    {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    ///     Hide pop up options <see cref = "Miscellaneous"/> class.
    /// </summary>
    public void HidePopUpOptions()
    {
        SetPanelOpen(false);
        Pop_up_Options.SetActive(GetPanelOpen());
    }

    public void SwitchPopUpOption()
    {
        SetPanelOpen(!GetPanelOpen());
        Pop_up_Options.SetActive(GetPanelOpen());
    }

    /// <summary>
    ///     Set the opening or closing of the panel <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "b">if set to <c>true</c> [b] is open panel else is close panel.</param>
    public void SetPanelOpen(bool b) => s_isOpenPanel = b;

    /// <summary>
    ///     Gets the panel open <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <returns>The opened panel boolean.</returns>
    public bool GetPanelOpen()
    {
        return s_isOpenPanel;
    }

    /// <summary>
    ///     Gets the state <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <returns>The state boolean.</returns>
    public bool GetState()
    {
        return s_state;
    }

    /// <summary>
    ///     Set the state to true or false <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "b">if set to <c>true</c> [b] the state is true else it is false.</param>
    public void SetState(bool b) => s_state = b;

    /// <summary>
    ///     Change the display when conected <see cref = "Miscellaneous"/> class.
    /// </summary>
    public void Connected()
    {
        ColorUtility.TryParseHtmlString("#90EE90", out colState);
        Button tmpStat = Miscellaneous.FindObject(absolute_parent, "ShowStat").GetComponent<Button>();
        Button tmpJouer = Miscellaneous.FindObject(absolute_parent, "ShowRoomSelection").GetComponent<Button>();
        Button tmpConnection = Miscellaneous.FindObject(absolute_parent, "ShowConnection").GetComponent<Button>();
        Miscellaneous.FindObject(absolute_parent, "Etat de connexion").GetComponent<Text>().color = colState;
        Miscellaneous.FindObject(absolute_parent, "Etat de connexion").GetComponent<Text>().text = "Connecte";
        tmpConnection.gameObject.SetActive(false);
        tmpJouer.interactable = tmpStat.interactable = true;
        tmpJouer.GetComponentInChildren<Text>().color = tmpStat.GetComponentInChildren<Text>().color = Color.white;
        // Remonte les boutons après la connexion 
        // ! (NE PAS CHANGER)
        Transform buttons = Miscellaneous.FindObject(absolute_parent, "Buttons").transform;
        for (int i = 1; i < buttons.childCount - 1; i++)
            buttons.GetChild(i).transform.position += new Vector3(0, 150 - i * 10, 0);
        // Probleme de hover quand connecté sur home menu par défaut suite à l'intégration (sémaphore ??)
        // PATCH provisoire (A CHANGER):
        GameObject TridentGo = GameObject.Find("Other").transform.Find("Trident").gameObject;
        if (TridentGo.activeSelf == true)
            TridentGo.SetActive(false);
    }

    /// <summary>
    ///     Hover with trident <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "c">The component.</param>
    /// <param name = "GoT">The game object (current button).</param>
    public void tridentHover(Component c, GameObject GoT)
    {
        if (!(c.transform.parent.name == "ForgottenPwdUser" || c.transform.parent.name == "CGU"))
        {
            GoT.SetActive(true);
            GameObject curBtn = c.gameObject;
            float width = curBtn.GetComponent<RectTransform>().rect.width;
            float height = curBtn.GetComponent<RectTransform>().rect.height;
            GameObject TF = GoT.transform.Find("TridentFront").gameObject;
            GameObject TB = GoT.transform.Find("TridentBack").gameObject;
            TF.transform.position = curBtn.transform.position + new Vector3(width / 2 + 90, 0, 0);
            TB.transform.position = curBtn.transform.position - new Vector3(width / 2 + 20, 0, 0);
        }
    }

    /// <summary>
    ///     Set the menu to the boolean value <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "b">if set to <c>true</c> [b] the menu changed else it is not changed.</param>
    public void SetMenuChanged(bool b) => s_menuChanged = b;
    /// <summary>
    ///     Gets the boolean if menu has changed <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <returns>The boolean value of menu has changed.</returns>
    public bool HasMenuChanged()
    {
        return s_menuHasChanged;
    }

    /// <summary>
    ///     Gets the previous menu GameObject <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <returns>The previous menu GameObject.</returns>
    public GameObject GetPreviousMenu()
    {
        return previousMenu;
    }

    /// <summary>
    ///     Gets the current menu GameObject <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <returns>The current menu GameObject.</returns>
    public GameObject GetCurrentMenu()
    {
        return nextMenu;
    }

    /// <summary>
    ///     Get the first active child and return it (as GameObject) <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "FAGO">The parent GameObject.</param>
    /// <returns>The first active child.</returns>
    public GameObject FirstActiveChild(GameObject FAGO)
    {
        foreach (Transform child in FAGO.transform)
            if (child.gameObject.activeSelf)
                return child.gameObject;
        return null;
    }

    /// <summary>
    ///     Change the Menu <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "close">The previous menu.</param>
    /// <param name = "goTo">The next menu.</param>
    public void ChangeMenu(string close, string goTo)
    {
        s_menuHasChanged = true;
        previousMenu = Miscellaneous.FindObject(absolute_parent, close).gameObject;
        nextMenu = Miscellaneous.FindObject(absolute_parent, goTo).gameObject;
        OnMenuChange?.Invoke(goTo);
        previousMenu.SetActive(false);
        nextMenu.SetActive(true);
    }

    /// <summary>
    ///     Remove the last space of a string <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "mot">The string to remove the last space.</param>
    /// <returns>The string without the last space.</returns>
    public string RemoveLastSpace(string mot) // Inputfield
    {
        string modif = mot.TrimEnd(); //Not for passwords -> "" = char
        return (mot.Length > 1) ? modif : mot;
    }

    /// <summary>
    ///     Clear the InputField <see cref = "Miscellaneous"/> class.
    /// </summary>
    /// <param name = "inputfield">The InputField to clear.</param>
    /// <returns>The cleared InputField.</returns>
    public static InputField Clear(InputField inputfield)
    {
        inputfield.Select();
        inputfield.text = "";
        return inputfield;
    }
}