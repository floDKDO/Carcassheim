using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
///    Input/Output Manager.
/// </summary>
public class IOManager : Miscellaneous, IPointerEnterHandler
{
    private OptionsMenu _option;
    private AccountMenu _acc;
    private HomeMenu _home;
    private ConnectionMenu _co;
    private CreditsMenu _cred;
    private StatistiquesMenu _stat;
    private RoomSelectionMenu _sroom;
    private JoinByIdMenu _jid;
    private PublicRoomMenu _proom;
    private RoomParameters _rparam;
    private CreateRoomMenu _croom;
    private RoomIsCreated _rcreated;
    private Color _previousColor, FCcolor, colHover;
    private Text _btnText;
    static private GameObject previousGo, nextGo, TridentGo;
    private Texture2D _cursorTexture;
    private CursorMode _cursorMode = CursorMode.Auto;
    private Vector2 _cursorHotspot = new Vector2(10, 5);
    private EventSystem eventSystem;
    private bool boolSelectionChange = true;
    private bool cooldown = false;
    private InputField IF = null;
    private static bool boolPC = true;
    [SerializeField]
    GameObject loading_screen;

    /// <summary>
    /// Start is called before the first frame update <see cref = "IOManager"/> class.
    /// </summary>
    void Start()
    {
        absolute_parent_ref = absolute_parent;
#if !(UNITY_IOS || UNITY_ANDROID)
        boolPC = true;
#endif
        absolute_parent_ref = null;
        //Cursor Texture :
        _cursorTexture = Resources.Load("Miscellaneous/Cursors/BlueCursor") as Texture2D; // Texture Type = Cursor
        Cursor.SetCursor(_cursorTexture, _cursorHotspot, _cursorMode);
        //Fetch the current EventSystem. Make sure your Scene has one.
        eventSystem = EventSystem.current;
        if (GameObject.Find("InputFieldEndEdit") != null)
            nextGo = FirstActiveChild(GameObject.Find("InputFieldEndEdit"));
        else
            nextGo = FirstActiveChild(GameObject.Find("Buttons"));
        eventSystem.SetSelectedGameObject(nextGo);
        ColorUtility.TryParseHtmlString("#1e90ff", out colHover);
        ColorUtility.TryParseHtmlString("#FFA500", out FCcolor);
        TridentGo = GameObject.Find("Other").transform.Find("Trident").gameObject;
        changeHover();
        // Cherche chaque menu -> liste chaque boutons par menu -> assignation de la fonction respectivement
    }

    /// <summary>
    /// Update is called once per frame <see cref = "IOManager"/> class.
    /// </summary>
    public void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) // résout probleme souris/clavier avec GetKey 
        {
            /* lockMouse(true); */
            previousGo = nextGo;
            nextGo = eventSystem.currentSelectedGameObject;
            changeHover();
        }

        if (((nextGo != null && nextGo.GetComponent<InputField>()) || (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponent<InputField>()) && GameObject.Find("InputFieldEndEdit")))
        {
            if (TridentGo.activeSelf == true) // Desactive car aucune selection
                TridentGo.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
            {
                if (nextGo.name == "InputFieldEndEdit")
                    nextGo = eventSystem.currentSelectedGameObject;
                if (nextGo.transform.GetSiblingIndex() > 0)
                    nextGo = nextGo.transform.parent.GetChild(nextGo.transform.GetSiblingIndex() - 1).gameObject;
                else
                    nextGo = nextGo.transform.parent.GetChild(GameObject.Find("InputFieldEndEdit").transform.childCount - 1).gameObject;
                eventSystem.SetSelectedGameObject(nextGo);
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (nextGo.name == "InputFieldEndEdit")
                    nextGo = eventSystem.currentSelectedGameObject;
                if (nextGo.transform.GetSiblingIndex() < GameObject.Find("InputFieldEndEdit").transform.childCount - 1)
                    nextGo = nextGo.transform.parent.GetChild(nextGo.transform.GetSiblingIndex() + 1).gameObject;
                else
                    nextGo = nextGo.transform.parent.GetChild(0).gameObject;
                eventSystem.SetSelectedGameObject(nextGo);
            }
        }
    }

    /// <summary>
    /// Lock the mouse cursor <see cref = "IOManager"/> class.
    /// </summary>
    /// <param name = "b">True pour bloquer, False pour débloquer</param>
    private void lockMouse(bool b)
    {
        Cursor.lockState = b ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !b;
    }

    /// <summary>
    /// Reset the mouse clic cooldown <see cref = "IOManager"/> class.
    /// </summary>
    private void ResetCooldown()
    { // EVITE SPAM CLIC
        cooldown = false;
    }

    /// <summary>
    ///     Show pop up options <see cref = "IOManager"/> class.
    /// </summary>
    public void ShowPopUpOptions()
    {
        var wp = Miscellaneous.FindObject(absolute_parent, "WheelPlayer").GetComponent<UnityEngine.Video.VideoPlayer>();
        bool b = !GetPanelOpen();
        if (b) wp.Play();
        else wp.Stop();
        wp.isLooping = b;
        SetPanelOpen(b);
        Pop_up_Options.SetActive(GetPanelOpen());
    }

    /// <summary>
    /// Detect the hover of the mouse cursor <see cref = "IOManager"/> class.
    /// </summary>
    /// <param name = "eventData"type="PointerEventData">Données de l'évènement</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //IF = eventData.pointerCurrentRaycast.gameObject.GetComponent<InputField>(); // PATCH INPUTFIELD 
        //if (!IF)
        //{
        nextGo = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
        selectionChange();
        //}
    }

    /*     
    /// <summary>
    /// Detect the key name <see cref = "IOManager"/> class.
    /// </summary>
    void OnGUI() // TROP LENT (a gardé pour détecter une touche quelconque)
    {
        if(Input.anyKeyDown &&  Event.current.isKey)
                switch(Event.current.keyCode.ToString()){
                case "UpArrow" : case "DownArrow" : case "LeftArrow" : case "RightArrow" :
                break;
                }
    }  
    */

    /// <summary>
    /// Change the text color <see cref = "IOManager"/> class.
    /// </summary>
    /// <param name = "c">Couleur</param>
    /// <param name = "s">Taille de la police</param>
    /// <param name = "go">GameObject</param>
    public void textColor(Color c, int s, GameObject go)
    {
        _btnText = go.GetComponentInChildren<Text>();
        if (_btnText.color != c)
            _btnText.fontSize += s;
        _btnText.color = c;
    }
    /// <summary>
    /// Change the button color when the mouse cursor is on it <see cref = "IOManager"/> class.
    /// </summary>
    public void selectionChange()
    {
        // Aparté : (Les inpufield : le "Text" doit avoir du raycast pour fonctionner donc à ne pas désactiver)
        // nextGo.GetComponent<Button>() est testé d'abord donc si false la partie gauche du ET non testé donc pas d'erreur
        bool btn = nextGo.GetComponent<Button>() && nextGo.GetComponent<Button>().interactable;
        bool slider = nextGo.transform.GetChild(0).name == "Handle";
        bool inputfd = nextGo.transform.parent.name == "InputField" || nextGo.transform.parent.name == "InputFieldEndEdit";
        // Si nextGo != currentSelected ET (selection de : slider ou bouton ou toggle)
        if (nextGo != eventSystem.currentSelectedGameObject && (slider || btn || inputfd || nextGo.GetComponent<Toggle>()))
        {
            previousGo = eventSystem.currentSelectedGameObject;
            eventSystem.SetSelectedGameObject(nextGo);
            boolSelectionChange = true;
        }
        else
            boolSelectionChange = false;
        changeHover();
    }

    /// <summary>
    /// Change the image color or transparency <see cref = "IOManager"/> class.
    /// </summary>  
    /// <param name = "go">GameObject</param>
    /// <param name = "r">Rouge</param>
    /// <param name = "g">Vert</param>
    /// <param name = "b">Bleu</param>
    /// <param name = "f">Alpha</param>
    /// <param name = "changeColor">True pour changer la couleur, False pour ne pas la changer</param>
    public void colorImage(GameObject go, byte r, byte g, byte b, byte f, bool changeColor)
    {
        Image image = go.transform.GetChild(0).gameObject.GetComponent<Image>();
        if (changeColor == false) // transparence 
            image.color = new Color(image.color.r, image.color.g, image.color.b, (float)f / 255);
        else
            image.color = new Color32(r, g, b, f); // transparence et couleur
    }

    /// <summary>
    /// Reset the hover of the previous GameObject <see cref = "IOManager"/> class.
    /// </summary>
    public void resetHoverPreviousGo()
    {
        if (previousGo != null && boolPC == true)
        {
            //GameObject.Find("Trident").SetActive(false);
            Component previousTarget = previousGo.transform.GetChild(0).GetComponent<Component>();
            bool FC = previousTarget.transform.parent.name == "ForgottenPwdUser" || previousTarget.transform.parent.name == "CGU";
            switch (previousTarget.name)
            { // previousGO
                case "RawImage": // GIF : A changer (mettre autre chose que dezoom)
                    previousGo.GetComponentInChildren<RawImage>().rectTransform.sizeDelta = new Vector2(50, 50);
                    break;
                case "Unselected": // IMAGE
                    colorImage(previousGo, 0, 0, 0, 255, false);
                    break;
                case "Text": // BOUTON
                    _previousColor = FC ? FCcolor : new Color(1, 1, 1, 1); // COULEUR PAR DEFAUT (RESET COLOR)
                    textColor(_previousColor, -3, previousGo);
                    break;
                case "Background": // TOGGLE
                    colorImage(previousGo, 0, 0, 0, 255, false); // (à changer)
                    break;
                case "Handle": // SLIDER
                    colorImage(previousGo, 255, 255, 255, 255, true); // COULEUR PAR DEFAUT (RESET COLOR)
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Change the hover of the next GameObject <see cref = "IOManager"/> class.
    /// </summary>
    public void changeHover()
    {
        if ((boolSelectionChange && boolPC) == true)
        {
            if (TridentGo.activeSelf == true) // TRIDENT
                TridentGo.SetActive(false);
            resetHoverPreviousGo();
            Component nextTarget = nextGo.transform.GetChild(0).GetComponent<Component>();
            switch (nextTarget.name)
            {
                case "RawImage": // GIF : A changer (mettre autre chose que zoom)
                    nextGo.GetComponentInChildren<RawImage>().rectTransform.sizeDelta = new Vector2(70, 70);
                    break;
                case "Unselected": // IMAGE
                    colorImage(nextGo, 0, 0, 0, 0, false);
                    break;
                case "Text": // BOUTON
                    tridentHover(nextTarget, TridentGo); // TRIDENT
                    textColor(colHover, 3, nextGo);
                    break;
                case "Background": // TOGGLE
                    colorImage(nextGo, 0, 0, 0, 125, false); // semi transparent (à changer)
                    break;
                case "Handle": // SLIDER
                    colorImage(nextGo, 47, 79, 79, 255, true); // équivalent à #FF4500
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Change  the selected button (to the new menu) <see cref = "IOManager"/> class.
    /// </summary>
    public void NewMenuSelectButton()
    {
        if (HasMenuChanged() == true)
        {
            string previousMenu = GetPreviousMenu().name.Remove(GetPreviousMenu().name.Length - 4);
            if (GameObject.Find("InputFieldEndEdit") != null)
                nextGo = FirstActiveChild(GameObject.Find("InputFieldEndEdit"));
            else
            {
                foreach (Transform child in GameObject.Find("Buttons").transform)
                {
                    if (GameObject.Find("InputFieldEndEdit") != null)
                        nextGo = FirstActiveChild(GameObject.Find("InputFieldEndEdit"));
                    else
                        nextGo = FirstActiveChild(GameObject.Find("Buttons"));
                    if (child.name.Contains(previousMenu) && child.gameObject.activeSelf)
                    {
                        nextGo = child.gameObject;
                        break;
                    }
                }
            }

            SetMenuChanged(false);
            selectionChange();
        }
    }

    /// <summary>
    /// Call the function <see cref = "IOManager"/> class.
    /// </summary>
    /// <param name = "methode">Nom de la méthode</param>
    /// <param name = "tog">Toggle</param>
    /// <param name = "inp">InputField</param>
    public void MethodCall(string methode, Toggle tog, InputField inp)
    {
        GameObject.Find("SoundController").GetComponent<AudioSource>().Play();
        if (inp == null)
            gameObject.SendMessage(methode, tog);
        else
            gameObject.SendMessage(methode, inp);
        if (tog == null || inp == null)
            NewMenuSelectButton();
    }

    /// <summary>
    /// Deactivate the trident and activate the loading creen <see cref = "IOManager"/> class.
    /// </summary>
    void OnDisable()
    {
        TridentGo?.SetActive(false);
        loading_screen?.SetActive(true);
    }
}