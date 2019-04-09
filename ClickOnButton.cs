using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ClickOnButtonHandler(object source, ButtonArguments e);

public class ClickOnButton : MonoBehaviour
{
    internal event ClickOnButtonHandler OnClick;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Click(string name)
    {
        if (OnClick != null)
        {
            Debug.Log("button clicked");
            OnClick(this, new ButtonArguments(name));
        }
    }
}
