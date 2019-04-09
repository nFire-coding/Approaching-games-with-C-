using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void NextSceneRequiredHandler(object source, SceneArguments e);

public class MyGameManager : MonoBehaviour
{
    private int _currentScene = 0;
    private event NextSceneRequiredHandler _onNextSceneRequirment;

    private Factory _factory = new Factory();

    internal int CurrentScene
    {
        get
        {
            return _currentScene;
        }

        set
        {
            _currentScene = value;
        }
    }

    public Factory GetFactory()
    {
        return _factory;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        InitializeDelegateForSceneRequest(ref _onNextSceneRequirment);
    }

    internal void InitializeDelegateForSceneRequest(ref NextSceneRequiredHandler x)
    {
        x += new NextSceneRequiredHandler(LoadRequiredScene);
    }

    private void Start()
    {
        GameObject.Find("ScriptObject").GetComponent<ClickOnButton>().OnClick += new ClickOnButtonHandler(ButtonClicked);
    }

    private void LoadRequiredScene(object source, SceneArguments s)
    {
        StartCoroutine(LoadWithDelay(s));
    }

    private IEnumerator LoadWithDelay(SceneArguments s)
    {
        yield return new WaitForSeconds(2);
        Debug.Log("loading new scene");
        if (CurrentScene == 4)
        {
            Debug.Log("fine gioco");
        }
        SceneManager.LoadScene(s.GetSceneNumber() + 1);
        CurrentScene = s.GetSceneNumber() + 1;
    }

    private void ButtonClicked(object source, ButtonArguments e)
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("Clickable");
        Debug.Log(buttons.Length);
        SwitchButtonsStatus(buttons); //disable

        if(e.GetButton().Contains("Validation"))
        {
            Debug.Log("validating");
            ValidateKey();
        }
        else if(e.GetButton().Contains("Exit"))
        {
            Debug.Log("quitting");
            Application.Quit();
        }
        else if (e.GetButton().Contains("Start"))
        {
            Debug.Log("starting");
            StartCoroutine(LoadWithDelay(new SceneArguments(CurrentScene)));
        }
        else if (e.GetButton().Contains("Load"))
        {
            Debug.Log("loading");
            try
            {
                Debug.Log("scena da caricare: "+PersistenceManager.Load());
                StartCoroutine(LoadWithDelay(new SceneArguments(PersistenceManager.Load() -1)));
            }
            catch
            {
                Debug.Log("can't find any saved data");
                SwitchButtonsStatus(buttons); //enable
            }
        }

    }

    private void SwitchButtonsStatus(GameObject[] buttons)
    {
        foreach (GameObject b in buttons)
        {
            if (b.GetComponent<Button>() != null)
            {
                b.GetComponent<Button>().enabled = !b.GetComponent<Button>().enabled;
            }
            if (b.GetComponent<InputField>() != null)
            {
                b.GetComponent<InputField>().enabled = !b.GetComponent<InputField>().enabled;
            }
        }
    }

    private void ValidateKey()
    {
        int cod = 700;
        int checksum = 0;
        //reperisco input di testo da input field
        InputField campoDiTesto = InputField.FindObjectOfType<InputField>();
        //Button exitButton = GameObject.Find("Exit button").GetComponent<Button>();
        Text resultValidation = GameObject.Find("Result validation").GetComponent<Text>();
        Debug.Log(resultValidation.text); //Denota test OK!!
        
        foreach (byte b in Encoding.ASCII.GetBytes(campoDiTesto.text))
        {
            checksum += b;
        }
        resultValidation.enabled = true;
        if (cod == checksum)
        {
            resultValidation.text = "Validazione completata con successo";
            //exitButton.enabled = false;
            StartCoroutine(LoadWithDelay(new SceneArguments(CurrentScene)));
        }
        else
        {
            resultValidation.text = "Chiave Non Valida";
            GameObject[] buttons = GameObject.FindGameObjectsWithTag("Clickable");
            Debug.Log(buttons.Length);
            SwitchButtonsStatus(buttons); //enable
        }
    }

    //private IEnumerator DelayAndLoadMenu(Button b)
    //{
       
    //}
}

public class ButtonArguments : EventArgs
{
    private string _button;

    public ButtonArguments(string Text)
    {
        _button = Text;
    }

    public string GetButton()
    {
        return _button;
    }
}

public class SceneArguments : EventArgs
{
    private int _scene;

    public SceneArguments(int n)
    {
        _scene = n;
    }

    public int GetSceneNumber()
    {
        return _scene;
    }
}
