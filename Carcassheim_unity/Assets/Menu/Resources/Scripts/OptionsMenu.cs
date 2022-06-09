using Assets.System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Options menu.
/// </summary>
public class OptionsMenu : Miscellaneous
{
    private Button _btnSon, _btnMusique;
    private AudioSource _soundCtrl, _musicCtrl;
    private Slider _soundSlider, _musicSlider;
    private Text _pourcentSon, _pourcentMusique;
    private float _previousSoundVol = 0.0f;
    private float _previousMusicVol = 0.0f;
    float lastSoundValue = 0;
    float lastMusicValue = 0;
    public Toggle toggle_french, toggle_english, toggle_german;
    private GameObject _btnSonUnselected, _btnMusicUnselected;
    private Transform optionsMenu, OCB, OCS, OCT; // Options Container Buttons
    private GameObject _btnSonUnselectedP, _btnMusicUnselectedP;
    private Button _btnSonP, _btnMusiqueP;
    private Transform panelMenu, PCB; // Options Container Buttons
    private Sprite _spriteMusicON, _spriteMusicOFF, _spriteSoundON, _spriteSoundOFF, _spriteMusicUnselectedON, _spriteMusicUnselectedOFF, _spriteSoundUnselectedON, _spriteSoundUnselectedOFF;
    private Text[] all_textes;
    private List<Text> l_all_textes;
    private List<string> fr_textes, en_textes, de_textes;
    public static int langue = 0;
    private GameObject ready, connect, status;
    //0 FR, 1 EN, 2 DE

    /// <summary>
    /// Start is called before the first frame update <see cref = OptionsMenu"/>
    /// </summary>

    void Start()
    {
        // INITIALISATION
        panelMenu = GameObject.Find("SubMenus").transform.Find("Panel Options").transform;
        PCB = panelMenu.Find("Buttons").transform;
        _btnMusiqueP = PCB.Find("SwitchMusic").GetComponent<Button>();
        _btnMusicUnselectedP = _btnMusiqueP.transform.GetChild(0).gameObject;
        _btnSonP = PCB.Find("SwitchSound").GetComponent<Button>();
        _btnSonUnselectedP = _btnSonP.transform.GetChild(0).gameObject;
        optionsMenu = GameObject.Find("SubMenus").transform.Find("OptionsMenu").transform;
        OCB = optionsMenu.Find("Buttons").transform;
        OCS = optionsMenu.Find("Sliders").transform;
        OCT = optionsMenu.Find("Text").transform;
        _soundSlider = OCS.Find("Slider Son").GetComponent<Slider>();
        _musicSlider = OCS.Find("Slider Musique").GetComponent<Slider>();
        _pourcentSon = OCT.Find("Pourcent Son").GetComponent<Text>();
        _pourcentMusique = OCT.Find("Pourcent Musique").GetComponent<Text>();
        _btnMusique = OCB.Find("SwitchMusic").GetComponent<Button>();
        _btnMusicUnselected = _btnMusique.transform.GetChild(0).gameObject;
        _btnSon = OCB.Find("SwitchSound").GetComponent<Button>();
        _btnSonUnselected = _btnSon.transform.GetChild(0).gameObject;
        _soundCtrl = GameObject.Find("SoundController").GetComponent<AudioSource>();
        _musicCtrl = GameObject.Find("MusicController").GetComponent<AudioSource>();
        string pathBlue = "Miscellaneous/UI/Buttons/Moss_Blue/";
        string pathWhite = "Miscellaneous/UI/Buttons/Rock_white/";
        _spriteMusicON = Resources.Load<Sprite>(pathBlue + "button_moss_blue13");
        _spriteMusicOFF = Resources.Load<Sprite>(pathBlue + "button_moss_blue14");
        _spriteSoundON = Resources.Load<Sprite>(pathBlue + "button_moss_blue30");
        _spriteSoundOFF = Resources.Load<Sprite>(pathBlue + "button_moss_blue28");
        _spriteMusicUnselectedON = Resources.Load<Sprite>(pathWhite + "button_white13");
        _spriteMusicUnselectedOFF = Resources.Load<Sprite>(pathWhite + "button_white14");
        _spriteSoundUnselectedON = Resources.Load<Sprite>(pathWhite + "button_white30");
        _spriteSoundUnselectedOFF = Resources.Load<Sprite>(pathWhite + "button_white28");
        // Debug.Log(_spriteMusicON.name + _spriteMusicOFF.name + _spriteSoundON.name + _spriteSoundOFF.name);
        // Debug.Log(_spriteMusicUnselectedON.name + _spriteMusicUnselectedOFF.name + _spriteSoundUnselectedON.name + _spriteSoundUnselectedOFF.name);
        DefaultMusicSound();
        //Subscribe to the Slider event
        _soundSlider.onValueChanged.AddListener(SoundSliderCallBack);
        lastSoundValue = _soundSlider.value;
        _musicSlider.onValueChanged.AddListener(MusicSliderCallBack);
        lastMusicValue = _musicSlider.value;
        //tableaux contenant tous les textes du menu
        all_textes = Resources.FindObjectsOfTypeAll<Text>();
        //on enleve de ce tableau les Text avec le tag no_trad (mots qu'on a pas besoin de traduire) et modif (les textes modifies par le script) et les textes vides
        all_textes = Array.FindAll(all_textes, i => i.tag != "no_trad" && i.tag != "modif" && !string.IsNullOrEmpty(i.text)).ToArray();
        //on convertit le tableau en liste (pour faciliter les oprations)
        l_all_textes = all_textes.ToList<Text>();
        /*foreach (Text a in l_all_textes)
			Debug.Log(a.text);*/
        //liste contenant tous les textes en franais
        fr_textes = new List<string>();
        foreach (Text a in l_all_textes)
            fr_textes.Add(a.text);
        //liste contenant tous les textes en anglais (tres sensible  l'ordre des gameobject)
        en_textes = new List<string> { "STATUS", "Room ID -> X", "OPTIONS", "Extensions", "Year ...", "Join by ID", "CREDITS", "CONNECT", "River", "Enter your password ...", "Time per round", "Enter your Username ...", "PUBLIC ROOM", "CREATE", "JOIN ROOM", "ID to enter", "QUIT", "Abbey", "The room is created !", "Enter your password ...", "Email / Username", "Accept ", "Enter your Email/Pseudo ...", "Confirm your password ...", "CREATE AN ACCOUNT", "month ...", "STATUS", "Day ...", "Endgame", "Enter an ID ...", "PLAY LOCAL", "ID", "Max players", "Password", "Enter your Email ...", "CREATE AN ACCOUNT", "Public", "HELP", "STATS", "Option A", "MULTIPLAYER", "Show password", "JOIN ROOM MENU", "Show password", "Private", "Create your account", "Confirm password", "Room settings", "JOIN BY ID", "Ending condition", "Options", "Public", "GCU", "Email", "Create your room", "Forgot your username/password?", "Username", "Room ", "Number of players ", "Password", "Private", "LOG IN", "Date of birth", "Hosts", "READY", "Players", "Tile", "ID", "Players", "Hosts", "Endgame", "Max players" };
        //liste contenant tous les textes en allemand (tres sensible  l'ordre des gameobject)
        de_textes = new List<string> { "STATUS", "Raum-ID -> X", "OPTIONEN", "Erweiterungen", "Jahr ...", "Beitritt nach ID", "CREDITS", "VERBINDEN", "Fluss", "Geben Sie Ihr Passwort ein ...", "Zeit pro Runde", "Geben Sie Ihren Benutzernamen ein ...", "FFENTLICHER RAUM", "ANLEGEN", "RAUM ANMELDEN", "ID zum Betreten", "BEENDEN", "Abtei", "Der Raum wird erstellt!", "Geben Sie Ihr Passwort ein ...", "E-Mail / Benutzername", "Akzeptieren ", "Geben Sie Ihre Email/Pseudo ein ...", "Besttigen Sie Ihr Passwort ...", "EIN KONTO ERSTELLEN", "Monat ...", "STATUS", "Tag ...", "Endgame", "Eine ID eingeben ...", "LOCAL SPIELEN", "KENNUNG", "Max Spieler", "Kennwort", "Geben Sie Ihre E-Mail ein ...", "EIN KONTO ERSTELLEN", "fentlich", "HILFE", "STATISTIKEN", "Option A", "MEHRSPIELER", "Passwort anzeigen", "ZIMMERMEN BETRETEN", "Passwort anzeigen", "Privat", "Erstellen Sie Ihr Konto", "Besttigen Sie Ihr Passwort", "Raum-Einstellungen", "JOIN BY ID", "Bedingung zum Beenden", "Optionen", "ffentlich", "GCU", "E-Mail", "Erstellen Sie Ihren Raum", "Haben Sie Ihren Benutzernamen/Passwort vergessen?", "Benutzername", "Raum ", "Anzahl der Spieler ", "Kennwort", "Privat", "ANMELDEN", "Geburtsdatum", "Gastgeber", "READY", "Spieler", "Kachel", "ID", "Spieler", "Gastgeber", "Endspiel", "Maximale Spieler" };
        //les 3 textes qui se modifient
        //NON PRET
        ready = GameObject.Find("SubMenus").transform.Find("PublicRoomMenu").transform.Find("Text").transform.Find("preparation").gameObject;
        //Deconnecte
        connect = GameObject.Find("SubMenus").transform.Find("HomeMenu").transform.Find("Text").transform.Find("Etat de connexion").gameObject;
        //Connectez-vous
        status = GameObject.Find("SubMenus").transform.Find("ConnectionMenu").transform.Find("Text").transform.Find("Instructions").gameObject;
    }

    /// <summary>
    /// Fonction toggle qui permet de traduire les textes en fonction de la langue choisie <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    /// <param name = "curT">The current toggle.</param>
    public void ToggleValueChangedOM(Toggle curT)
    {
        if (curT.name == "Toggle French")
        {
            //francais
            langue = 0;
            //on met a jour les textes en franais
            for (int i = 0; i < l_all_textes.Count; i++)
                l_all_textes[i].text = fr_textes[i];
            //on actualise les textes
            all_textes = l_all_textes.ToArray();
            //on gere les textes particuliers qui sont modifies par le script
            if (ready.GetComponent<Text>().text == "NOT READY" || ready.GetComponent<Text>().text == "NICHT BEREIT")
                ready.GetComponent<Text>().text = "NON PRET";
            else if (ready.GetComponent<Text>().text == "READY TO PLAY!" || ready.GetComponent<Text>().text == "SPIELBEREIT!")
                ready.GetComponent<Text>().text = "PRET A JOUER !";
            if (connect.GetComponent<Text>().text == "Disconnected" || connect.GetComponent<Text>().text == "Offline")
                connect.GetComponent<Text>().text = "Deconnecte";
            else if (connect.GetComponent<Text>().text == "Connected" || connect.GetComponent<Text>().text == "Verbunden")
                connect.GetComponent<Text>().text = "Connecte";
            if (status.GetComponent<Text>().text == "Log in" || status.GetComponent<Text>().text == "Loggen Sie sich ein")
                status.GetComponent<Text>().text = "Connectez-vous";
            else if (status.GetComponent<Text>().text == "Re-enter your login and password!" || status.GetComponent<Text>().text == "Geben Sie Ihren Login und Ihr Passwort erneut ein!")
                status.GetComponent<Text>().text = "Ressaisissez votre login et votre mot de passe !";
        }
        else if (curT.name == "Toggle English")
        {
            //anglais
            langue = 1;
            //on met a jour les textes en anglais
            for (int i = 0; i < l_all_textes.Count; i++)
                l_all_textes[i].text = en_textes[i];
            //on actualise les textes
            all_textes = l_all_textes.ToArray();
            //on gere les textes particuliers qui sont modifies par le script
            if (ready.GetComponent<Text>().text == "NON PRET" || ready.GetComponent<Text>().text == "NICHT BEREIT")
                ready.GetComponent<Text>().text = "NOT READY";
            else if (ready.GetComponent<Text>().text == "PRET A JOUER !" || ready.GetComponent<Text>().text == "SPIELBEREIT!")
                ready.GetComponent<Text>().text = "READY TO PLAY!";
            if (connect.GetComponent<Text>().text == "Deconnecte" || connect.GetComponent<Text>().text == "Offline")
                connect.GetComponent<Text>().text = "Disconnected";
            else if (connect.GetComponent<Text>().text == "Connecte" || connect.GetComponent<Text>().text == "Verbunden")
                connect.GetComponent<Text>().text = "Connected";
            if (status.GetComponent<Text>().text == "Connectez-vous" || status.GetComponent<Text>().text == "Loggen Sie sich ein")
                status.GetComponent<Text>().text = "Log in";
            else if (status.GetComponent<Text>().text == "Ressaisissez votre login et votre mot de passe !" || status.GetComponent<Text>().text == "Geben Sie Ihren Login und Ihr Passwort erneut ein!")
                status.GetComponent<Text>().text = "Re-enter your login and password!";
        }
        else if (curT.name == "Toggle German")
        {
            //allemand
            langue = 2;
            //on met a jour les textes en allemand
            for (int i = 0; i < l_all_textes.Count; i++)
                l_all_textes[i].text = de_textes[i];
            //on actualise les textes
            all_textes = l_all_textes.ToArray();
            //on gere les textes particuliers qui sont modifies par le script
            if (ready.GetComponent<Text>().text == "NON PRET" || ready.GetComponent<Text>().text == "NOT READY")
                ready.GetComponent<Text>().text = "NICHT BEREIT";
            else if (ready.GetComponent<Text>().text == "PRET A JOUER !" || ready.GetComponent<Text>().text == "READY TO PLAY!")
                ready.GetComponent<Text>().text = "SPIELBEREIT!";
            if (connect.GetComponent<Text>().text == "Deconnecte" || connect.GetComponent<Text>().text == "Disconnected")
                connect.GetComponent<Text>().text = "Offline";
            else if (connect.GetComponent<Text>().text == "Connecte" || connect.GetComponent<Text>().text == "Connected")
                connect.GetComponent<Text>().text = "Verbunden";
            if (status.GetComponent<Text>().text == "Connectez-vous" || status.GetComponent<Text>().text == "Log in")
                status.GetComponent<Text>().text = "Loggen Sie sich ein";
            else if (status.GetComponent<Text>().text == "Ressaisissez votre login et votre mot de passe !" || status.GetComponent<Text>().text == "Re-enter your login and password!")
                status.GetComponent<Text>().text = "Geben Sie Ihren Login und Ihr Passwort erneut ein!";
        }
    }

    //---------------------------- Music/Sound Begin ----------------------------//
    /// <summary>
    /// Volume of the music/sound <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    /// <param name = "ads">The ads.</param>
    /// <param name = "text">The text.</param>
    /// <param name = "sb">The sb.</param>
    public void Volume(AudioSource ads, Text txt, Slider sb)
    {
        ads.volume = sb.value;
        txt.text = Mathf.RoundToInt(sb.value * 100) + "%";
    }

    /// <summary>
    /// Display Volume of the music/sound <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    /// <param name = "b">The boolean music or sound.</param>
    /// <param name = "value">The value.</param>
    public void DisplayVolume(int b, float value)
    {
        if (value > 0)
            if (b == 0)
            {
                _btnSon.GetComponent<Image>().sprite = _spriteSoundON;
                _btnSonUnselected.GetComponent<Image>().sprite = _spriteSoundUnselectedON;
                _btnSonP.GetComponent<Image>().sprite = _spriteSoundON;
                _btnSonUnselectedP.GetComponent<Image>().sprite = _spriteSoundUnselectedON;
            }
            else
            {
                _btnMusique.GetComponent<Image>().sprite = _spriteMusicON;
                _btnMusicUnselected.GetComponent<Image>().sprite = _spriteMusicUnselectedON;
                _btnMusiqueP.GetComponent<Image>().sprite = _spriteMusicON;
                _btnMusicUnselectedP.GetComponent<Image>().sprite = _spriteMusicUnselectedON;
            }
        else if (b == 0)
        {
            _btnSon.GetComponent<Image>().sprite = _spriteSoundOFF;
            _btnSonUnselected.GetComponent<Image>().sprite = _spriteSoundUnselectedOFF;
            _btnSonP.GetComponent<Image>().sprite = _spriteSoundOFF;
            _btnSonUnselectedP.GetComponent<Image>().sprite = _spriteSoundUnselectedOFF;
        }
        else
        {
            _btnMusique.GetComponent<Image>().sprite = _spriteMusicOFF;
            _btnMusicUnselected.GetComponent<Image>().sprite = _spriteMusicUnselectedOFF;
            _btnMusiqueP.GetComponent<Image>().sprite = _spriteMusicOFF;
            _btnMusicUnselectedP.GetComponent<Image>().sprite = _spriteMusicUnselectedOFF;
        }

        if (b == 0)
            Volume(_soundCtrl, _pourcentSon, _soundSlider);
        else
            Volume(_musicCtrl, _pourcentMusique, _musicSlider);
    }

    /// <summary>
    /// Default muic/sound <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void DefaultMusicSound()
    {
        _soundSlider.maxValue = _musicSlider.maxValue = 1;
        _soundCtrl.volume = _musicCtrl.volume = _soundSlider.value = _musicSlider.value = 0.4f;
        Volume(_soundCtrl, _pourcentSon, _soundSlider);
        Volume(_musicCtrl, _pourcentMusique, _musicSlider);
    }

    /// <summary>
    /// Will be called when sound Scrollbar changes <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    /// <param name = "value">The value.</param>
    public void SoundSliderCallBack(float value)
    {
        DisplayVolume(0, value);
    }

    /// <summary>
    /// Will be called when music Scrollbar changes <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    /// <param name = "value">The value.</param>
    public void MusicSliderCallBack(float value)
    {
        DisplayVolume(1, value);
    }

    /// <summary>
    /// Switch the sound <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void SwitchSound()
    {
        if (_soundSlider.value != 0)
        {
            _previousSoundVol = _soundCtrl.volume;
            _soundSlider.value = 0.0f;
            DisplayVolume(0, _soundSlider.value);
        }
        else
        {
            _soundSlider.value = _previousSoundVol;
            DisplayVolume(0, _previousSoundVol);
        }
    }

    /// <summary>
    /// Switch the music <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void SwitchMusic()
    {
        if (_musicSlider.value != 0)
        {
            _previousMusicVol = _musicCtrl.volume;
            _musicSlider.value = 0.0f;
            DisplayVolume(1, _musicSlider.value);
        }
        else
        {
            _musicSlider.value = _previousMusicVol;
            DisplayVolume(1, _previousMusicVol);
        }
    }

    // -------------- Music/Sound End -----------------------//
    /// <summary>
    /// Hide the option menu <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void HideOptions()
    {
        ChangeMenu("OptionsMenu", "HomeMenu");
    }

    /// <summary>
    /// Fullscreen <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void FullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    /// <summary>
    /// Help Link <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void Help()
    {
        Application.OpenURL("https://drive.google.com/file/d/1D_QoQDojo3wwI90usxupeO7MYHxK6Eyt/view?usp=sharing");
    }

    /// <summary>
    /// Change to credit menu <see cref = "PublicRoomMenu"/> class.
    /// </summary>
    public void ShowCredits()
    {
        ChangeMenu("OptionsMenu", "CreditsMenu");
    }
}