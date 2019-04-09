using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class PersistenceManager
{
    public static void Save(int sceneNumber)
    {
        //Creo un oggetto in grado di generare un file binario
        BinaryFormatter bf = new BinaryFormatter();
        //Unity mette a disposizione un folder nascosto alla quale l'utente NON puo' eccedere per modificare i dati salvati! Si chiama persistentDataPath!) NB funziona in ogni S.O
        FileStream fileBinario = File.Open(Application.persistentDataPath + "/savedLevelInformation.dat", FileMode.OpenOrCreate);
        //Non potendo salvare le informazioni di oggetti monoBehaviour occorre creare delle classi CLEAN vuote in cui inserire le informazioni da salvare come farò qui sotto.

        bf.Serialize(fileBinario, sceneNumber);
        fileBinario.Close();
    }
    //Analocamente la load fa uso dello stesso file e recupera la classe Serializzabile appena salvata sul nostro file binario di informazioni!
    public static int Load()
    {
        if (File.Exists(Application.persistentDataPath + "/savedLevelInformation.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/savedLevelInformation.dat");
            int sceneNumber = (int)bf.Deserialize(file);

            return sceneNumber;
        }
        throw new Exception();

    }
}
