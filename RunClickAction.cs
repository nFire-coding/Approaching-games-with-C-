using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunClickAction : MonoBehaviour
{
	public void FindAndRunScript()
    {
        GameObject.Find("ScriptObject").GetComponent<ClickOnButton>().Click(gameObject.name);
    }
}
