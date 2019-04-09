using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoardManager : MonoBehaviour, Shooter
{
    private event NextSceneRequiredHandler _onNextSceneRequirment;

    [SerializeField]
    private enum EnemyType
    {
        Walker,
        Turret,
        Rock,
        Rolling,
        Spider,
        Machine
    }

    [SerializeField]
    private enum ItemType
    {
        FireRateBoost,
        Health,
        Coin
    }

    [Serializable]
    private struct Enemy
    {
        //why public?
        public EnemyType type;
        public Vector3 position;
        public GameObject parent;
    }

    [Serializable]
    private struct Item
    {
        //why public?
        public ItemType type;
        public Vector3 position;
        public GameObject parent;
    }

    [SerializeField]
    private Vector3 _spawnPlayer;
    [SerializeField]
    private Enemy[] _enemies;
    [SerializeField]
    private Item[] _items;
    [SerializeField]
    private Vector3[] _environmentBulletsSpawns;

    private GameObject _player;
    public GameObject playerHpBar;
    private Factory _factory;
    private List<GameObject> _enemiesAlive = new List<GameObject>();
    private List<int> _enemiesDeadId = new List<int>();

    private void Awake()
    {
        try
        {
            _factory = GameObject.Find("Game").GetComponent<MyGameManager>().GetFactory();
        }
        catch
        {
            ///////////////////
            //ONLY FOR DEBUG PURPOSE!!!
            if (_factory == null)
            {
                Debug.Log("Using debug factory");
                _factory = new Factory();
            }
            //////////////////
        }
        try
        {
            GameObject.Find("Game").GetComponent<MyGameManager>().InitializeDelegateForSceneRequest(ref _onNextSceneRequirment);
        }
        catch
        {
            ///////////////////
            //ONLY FOR DEBUG PURPOSE!!!
            Debug.Log("While debbugging a single level the method to load the next doesn't work, you have to test the whole game to make it work");
            //////////////////
        }
    }

    private void Start()
    {

        for (int i = 0; i < _enemies.Length; i++)
        {
            GameObject instantiatedEnemy;
            if (_enemies[i].parent != null)
            {
                instantiatedEnemy = _factory.SpawnEnemy(_enemies[i].type.ToString(), new Vector3 (_enemies[i].parent.transform.position.x + _enemies[i].position.x, _enemies[i].parent.transform.position.y + _enemies[i].position.y, 0));
                Transform child = instantiatedEnemy.transform;
                Transform parent = _enemies[i].parent.transform;
                child.SetParent(parent);
            }
            else
            {
                instantiatedEnemy = _factory.SpawnEnemy(_enemies[i].type.ToString(), _enemies[i].position);
            }
            instantiatedEnemy.GetComponent<EnemyController>().enemyNumber = i;
            instantiatedEnemy.GetComponent<EnemyController>().onEnemyDeath += new EnemyDeathHandler(EnemyDied);
            _enemiesAlive.Add(instantiatedEnemy);
            //tutti i nemici spawnano rivolti a sinistra
        }

        for (int i = 0; i < _items.Length; i++)
        {
            GameObject instantiatedItem;
            if (_items[i].parent != null)
            {
                instantiatedItem = _factory.SpawnItem(_items[i].type.ToString(), new Vector3(_items[i].parent.transform.position.x + _items[i].position.x, _items[i].parent.transform.position.y + _items[i].position.y, 0));
                Transform child = instantiatedItem.transform;
                Transform parent = _items[i].parent.transform;
                child.SetParent(parent);
            }
            else
            {
                instantiatedItem = _factory.SpawnItem(_items[i].type.ToString(), _items[i].position);
            }
        }

        StartCoroutine(StartShooting());

        _player = _factory.SpawnPlayer(_spawnPlayer, 0); //spawna con 0 morti
        _player.GetComponent<PlayerController>().onPlayerDeath += new PlayerDeathHandler(PlayerDied);

        GameObject camera = GameObject.Find("Main Camera");
        camera.GetComponent<CameraController>().player = _player;

        playerHpBar = _factory.GetHP_Bar();
        playerHpBar.transform.SetParent(camera.transform);
        playerHpBar.transform.localPosition = new Vector3(0, 2.3f, 10);
        playerHpBar.transform.localScale = new Vector3(5, 1.5f, 0);
    }

    private IEnumerator StartShooting()
    {
        Shoot();
        System.Random rnd = new System.Random();
        yield return new WaitForSeconds(0.75f + 0.25f*rnd.Next(0,5));
        StartCoroutine(StartShooting());
    }

    public void Shoot()
    {
        foreach (Vector3 v in _environmentBulletsSpawns)
        {
            _factory.SpawnBullet(this, v);
        }
    }

    private void RespawnEnemy()
    {
        foreach (int id in _enemiesDeadId)
        {
            GameObject instantiatedEnemy;
            if (_enemies[id].parent != null)
            {
                instantiatedEnemy = _factory.SpawnEnemy(_enemies[id].type.ToString(), new Vector3(_enemies[id].parent.transform.position.x + _enemies[id].position.x, _enemies[id].parent.transform.position.y + _enemies[id].position.y, 0));
                Transform child = instantiatedEnemy.transform;
                Transform parent = _enemies[id].parent.transform;
                child.SetParent(parent);
            }
            else
            {
                instantiatedEnemy = _factory.SpawnEnemy(_enemies[id].type.ToString(), _enemies[id].position);
            }
            instantiatedEnemy.GetComponent<EnemyController>().enemyNumber = id;
            instantiatedEnemy.GetComponent<EnemyController>().onEnemyDeath += new EnemyDeathHandler(EnemyDied);
            _enemiesAlive[id] = instantiatedEnemy;
        }
    }

    private void PlayerDied(object source, PlayerDeathArgument e)
    {
        Debug.Log(e.GetInfo() + e.GetDeaths());
        //ripristina hp nemici vivi
        foreach (GameObject g in _enemiesAlive)
        {
            if (g != null)
            {
                Debug.Log("curo: " + g.GetComponent<EnemyController>().enemyNumber);
                g.GetComponent<EnemyController>()._currentHealth = g.GetComponent<EnemyController>()._enemyMaxHealth;
                g.GetComponent<EnemyController>()._salute.transform.localScale = new Vector3(1, 0.7375f, 0); //ripristino barra hp secondo prefab fatto da dano
            }
        }
        CleanItem();
        RebuildHiddenWalls();
        for (int i = 0; i < _items.Length; i++)
        {
            GameObject instantiatedItem;
            if (_items[i].parent != null)
            {
                instantiatedItem = _factory.SpawnItem(_items[i].type.ToString(), new Vector3(_items[i].parent.transform.position.x + _items[i].position.x, _items[i].parent.transform.position.y + _items[i].position.y, 0));
                Transform child = instantiatedItem.transform;
                Transform parent = _items[i].parent.transform;
                child.SetParent(parent);
            }
            else
            {
                instantiatedItem = _factory.SpawnItem(_items[i].type.ToString(), _items[i].position);
            }
        }
        RespawnEnemy();
        _enemiesDeadId = new List<int>();

        if(GameObject.Find("Player") != null) //this should prevent the bug on level 2 and must be tested
            StartCoroutine(WaitRespawn(e));
    }

    private IEnumerator WaitRespawn(PlayerDeathArgument e)
    {
        yield return new WaitForSeconds(1);
        playerHpBar.transform.GetChild(2).transform.localScale = new Vector3(1, 0.7375f, 0);
        _player = _factory.SpawnPlayer(_spawnPlayer, e.GetDeaths());
        _player.GetComponent<PlayerController>().onPlayerDeath += new PlayerDeathHandler(PlayerDied);
        GameObject.Find("Main Camera").GetComponent<CameraController>().player = _player;
    }

    private void CleanItem()
    {
        foreach(GameObject c in GameObject.FindGameObjectsWithTag("Coin"))
        {
            Destroy(c);
        }

        foreach (GameObject b in GameObject.FindGameObjectsWithTag("FireRateBoost"))
        {
            Destroy(b);
        }

        foreach (GameObject h in GameObject.FindGameObjectsWithTag("Health"))
        {
            Destroy(h);
        }
    }

    private void RebuildHiddenWalls()
    {
        foreach (GameObject c in GameObject.FindGameObjectsWithTag("Hidden Floor"))
        {
            try
            {
                c.GetComponent<Collider2D>().enabled = true;
            }
            catch
            {
                Debug.Log("catch edge collider (elemento floor)");
            }
            try
            {
                c.GetComponent<SpriteRenderer>().enabled = true;
            }
            catch
            {
                Debug.Log("catch sprite (elemento platform)");
            }
        }
    }


    private void EnemyDied(object source, EnemyDeathArgument e)
    {
        Debug.Log(e.GetInfo() + e.GetNumber());
        _enemiesDeadId.Add(e.GetNumber());
        _enemiesAlive[e.GetNumber()] = null;
        Debug.Log("\nrimosso nemico" + e.GetNumber() + "\n");
        if (_enemiesDeadId.Count == _enemies.Length)
        {
            Debug.Log("gg");
            if (_onNextSceneRequirment != null)
            {
                _onNextSceneRequirment(this, new SceneArguments(GameObject.Find("Game").GetComponent<MyGameManager>().CurrentScene));
            }
        }
    }

    //Mostra in alto a sinistra il numero dei coins attualmente raccolti dal giocatore
    
    void OnGUI()
    {
       
        if (_player != null && _enemiesDeadId.Count != _enemies.Length)
        {
            GUI.skin.font = Resources.Load<Font>("customFont");
            GUI.contentColor = Color.black;
            GUI.skin.label.fontSize = Screen.width/40;
            GUI.Label(new Rect(Screen.width/50, Screen.height/50, Screen.width /4, Screen.height /8), "Coins Collected: " + _player.GetComponent<PlayerController>().Coins);
            GUI.Label(new Rect(Screen.width/50, Screen.height/10, Screen.width /4, Screen.height /8), "Deaths: " + _player.GetComponent<PlayerController>().Deaths);
        }
        if (_enemiesDeadId.Count == _enemies.Length)
        {
            GUI.skin.font = Resources.Load<Font>("customFont");
            GUI.contentColor = Color.yellow;
            GUI.skin.GetStyle("Label").alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = Screen.width/25;
            GUI.Label(new Rect(0, -Screen.height/5f, Screen.width, Screen.height), "Livello completato!!");
        }
    }
    
}

public class PlayerDeathArgument : EventArgs
{
    private string _eventInfo;
    private int _deaths;

    public PlayerDeathArgument(string Text, int deaths)
    {
        _eventInfo = Text;
        _deaths = deaths;
    }

    public string GetInfo()
    {
        return _eventInfo;
    }

    public int GetDeaths()
    {
        return _deaths;
    }
}

public class EnemyDeathArgument : EventArgs
{
    private string _EventInfo;
    private int _num;

    public EnemyDeathArgument(string Text, int num)
    {
        _EventInfo = Text;
        _num = num;
    }

    public string GetInfo()
    {
        return _EventInfo;
    }

    public int GetNumber()
    {
        return _num;
    }
}
