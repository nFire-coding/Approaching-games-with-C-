using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager
{

    internal static void scriptController(GameObject oggetto, string scriptName, bool active)
    {
        //La classe da cui derivano tutti gli script in unity è MonoBehaviour, si ottiene quindi la lista di tutti gli script di un oggetto
        MonoBehaviour[] c = oggetto.GetComponents<MonoBehaviour>();
        //Si cerca solamente lo script da disattivare in base alla stringa passata in ingresso e si interrompe lo script
        foreach (MonoBehaviour script in c)
        {
            if (script.GetType().ToString() == scriptName)
                script.enabled = active;
        }
    }
    //Ritorna la rotazione corretta in base alla direzione verso cui è rivolto un oggetto
    internal static Quaternion getRotation(bool facingRight)
    {
        return Quaternion.Euler(new Vector3(0, 0, facingRight ? 0 : 180f));
    }

    public static GameObject getTagObj(string tag, string name)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
        {
            if (go.name.Equals(name))
            {
                return go;
            }
        }
        throw new ArgumentException(tag + " " + name + " was't found");
    }
}
