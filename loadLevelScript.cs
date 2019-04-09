using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadLevelScript : MonoBehaviour {
    //Permette di caricare una scena in base all'indice indicato nello scene builder
    //Il primo pulsante chiamerà sempre la scena numero 1 che sarà la scena di selezione dei livelli!
    public void loadByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex); 
        //Per integrare con game manager passare scene index e fare la load su di esso
    }
}
