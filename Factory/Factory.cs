using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Assets.Scripts;
using System;
//Il pattern flyweight sarà implementato su due livelli. Il primo livello è uno scheletro del bullet e nemico
//GENERICI. Attualmente il game manager chiede di generare un nemico, per prima cosa la factory ne crea uno
//generico ( il suo scheletro appunto ) e poi lo completa e lo rende disponibile sul terreno di gioco.
//Alla seconda richiesta NON viene ri-creato lo scheletro (essendo in comune a tutti) ma usa quello già fatto.
//Se ri-chiedessi un nemico già settato al 100% verrebbre utilizzata un'istanza già usata!
public class Factory : MonoBehaviour
{
    //La prima volta che un oggetto viene creato esso viene manutenuto in memoria (pattern fly weight)
    private GameObject _bullet = null;
    private GameObject _enemy = null;
    private GameObject _hpBar = null;
    private GameObject _player = null;
    private GameObject _item = null;

    public GameObject SpawnBullet(Shooter shooter, Vector3 position, bool facingRight = true) //facingRight è opzionale
    {
        if (_bullet == null)
            _bullet = this.CreateBullet();

        GameObject spawnedBullet = Instantiate(_bullet, position, Quaternion.Euler(new Vector3(0, 0, !facingRight ? 0 : 180f)));

        if (shooter.GetType().ToString().Contains("Player"))
        {
            spawnedBullet.GetComponent<Bullet_Controller>()._target = "Enemy";
            spawnedBullet.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("MainPlayerSprites/spr")[34];
            spawnedBullet.GetComponent<Bullet_Controller>()._timeToLive = 2;
            spawnedBullet.tag = "BulletPlayer";
        }
        else if (shooter.GetType().ToString().Contains("Enemy"))
        {
            spawnedBullet.GetComponent<Bullet_Controller>()._target = "Player";
            spawnedBullet.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("MainPlayerSprites/spr")[44];
            spawnedBullet.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 1); //rosso
            spawnedBullet.GetComponent<Bullet_Controller>()._timeToLive = 4;
            spawnedBullet.tag = "BulletEnemy";
        }
        else if (shooter.GetType().ToString().Contains("Manager"))
        {
            spawnedBullet.GetComponent<Bullet_Controller>()._target = "Player";
            spawnedBullet.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("mmm-mmx4meteor1")[5];
            spawnedBullet.GetComponent<Bullet_Controller>()._timeToLive = 10;
            spawnedBullet.tag = "BulletEnvironment";

            //modifiche smart per farlo funzionare senza modificare il controller because I'm lazy
            spawnedBullet.GetComponent<Bullet_Controller>()._bulletSpeed = 200;
            Destroy(spawnedBullet.GetComponent<TrailRenderer>());
            spawnedBullet.GetComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.6f);
            spawnedBullet.GetComponent<BoxCollider2D>().offset = new Vector2(0, -0.2f);
        }
        else
        {
            //ha ricevuto null perchè non è riconosciuto chi abbia sparato
        }
        spawnedBullet.SetActive(true);
        return (spawnedBullet);
    }

    private GameObject CreateBullet()
    {
        SpriteRenderer spriteRenderer;
        BoxCollider2D boxCollider2D;
        Rigidbody2D rigidBody2D;
        TrailRenderer trailRenderer;
        Bullet_Controller bulletController;

        GameObject bullet = new GameObject();
        bullet.SetActive(false); //just to be sure that this idiot object will not mess up with something LOL
        bullet.name = "bullet"; //useless but still good
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 0f);

        bulletController = bullet.AddComponent<Bullet_Controller>();
        bulletController._bulletSpeed = 180;
        bulletController._weaponDamage = 1;
        bulletController._wallHitEffect = Resources.Load<GameObject>("BulletEffect");

        boxCollider2D = bullet.AddComponent<BoxCollider2D>();
        boxCollider2D.size = new Vector2(0.3f, 0.1f);
        boxCollider2D.offset = new Vector2(0.07f, 0f);
        boxCollider2D.isTrigger = true;

        rigidBody2D = bullet.AddComponent<Rigidbody2D>();
        rigidBody2D.gravityScale = 0;

        //la trail appena sparata si comporta in modo strano
        trailRenderer = bullet.AddComponent<TrailRenderer>();
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.material = Resources.Load<Material>("Flame02");
        trailRenderer.time = 0.3f;
        trailRenderer.autodestruct = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.4f);
        curve.AddKey(0.5f, 0f);
        trailRenderer.widthCurve = curve;

        spriteRenderer = bullet.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 10;

        return bullet;
    }

    public GameObject SpawnEnemy(string enemyType, Vector3 position)
    {
        if (_enemy == null)
            _enemy = this.CreateEnemy();

        GameObject spawnedEnemy = Instantiate(_enemy, position, Quaternion.Euler(new Vector3(-1, 0, 0))); //nemici rivolti a sinistra by default

        if (_hpBar == null)
            _hpBar = this.LoadHP_Bar();
        GameObject enemyHpBar = Instantiate(_hpBar, spawnedEnemy.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));
        enemyHpBar.transform.SetParent(spawnedEnemy.transform);

        if (enemyType.Equals("Walker"))
        {
            enemyHpBar.transform.localPosition += new Vector3(enemyHpBar.transform.localPosition.x, enemyHpBar.transform.localPosition.y+0.2f, enemyHpBar.transform.localPosition.x);
            EnemyController enemyController = spawnedEnemy.AddComponent<EnemyController>();

            spawnedEnemy.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("mmx4__mettool_d2")[0];

            spawnedEnemy.GetComponent<CapsuleCollider2D>().offset = new Vector2(0.005f, 0.025f);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().size = new Vector2(0.26f, 0.26f);

            spawnedEnemy.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("walkerAnimator");

            enemyController._borderEndLayer = "Ground";
            enemyController._speed = 1.5f;
            enemyController.hitByBulletEffect = Resources.Load<GameObject>("Enemy Hit");
            enemyController._enemyMaxHealth = 2;
            enemyController._soulEffect = Resources.Load<GameObject>("Skull death effect");
            enemyController._deathEffect = Resources.Load<GameObject>("Schizzo sangue");
            enemyController._salute = enemyHpBar.transform.GetChild(2).gameObject;
            enemyController.damage = 1f;
            enemyController.nextDamage = 0.1f;
            enemyController.damageRate = 1.5f;
            enemyController.pushBackForce = 3;
        }
        else if(enemyType.Equals("Rolling"))
        {
            enemyHpBar.transform.localPosition += new Vector3(enemyHpBar.transform.localPosition.x, enemyHpBar.transform.localPosition.y + 0.3f, enemyHpBar.transform.localPosition.x);
            EnemyController enemyController = spawnedEnemy.AddComponent<EnemyController>();

            spawnedEnemy.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("rmx4__spiky_mk-ii")[0];

            spawnedEnemy.GetComponent<CapsuleCollider2D>().offset = new Vector2(0, 0);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().size = new Vector2(0.45f, 0.45f);

            spawnedEnemy.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("rollingAnimator");

            enemyController._borderEndLayer = "Ground";
            enemyController._speed = 1f;
            enemyController.hitByBulletEffect = Resources.Load<GameObject>("Enemy Hit");
            enemyController._enemyMaxHealth = 2;
            enemyController._soulEffect = Resources.Load<GameObject>("Skull death effect");
            enemyController._deathEffect = Resources.Load<GameObject>("Schizzo sangue");
            enemyController._salute = enemyHpBar.transform.GetChild(2).gameObject;
            enemyController.damage = 1f;
            enemyController.nextDamage = 0.1f;
            enemyController.damageRate = 1.5f;
            enemyController.pushBackForce = 3;
        }
        else if (enemyType.Equals("Turret"))
        {
            enemyHpBar.transform.localPosition += new Vector3(enemyHpBar.transform.localPosition.x, enemyHpBar.transform.localPosition.y + 0.5f, enemyHpBar.transform.localPosition.x);
            EnemyShooterController enemyController = spawnedEnemy.AddComponent<EnemyShooterController>();

            spawnedEnemy.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            spawnedEnemy.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("vulcan")[0];

            spawnedEnemy.GetComponent<CapsuleCollider2D>().offset = new Vector2(-0.01f, 0);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().size = new Vector2(0.54f, 0.58f);

            spawnedEnemy.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("turretAnimator");

            enemyController._borderEndLayer = "Ground";
            enemyController._speed = 0;
            enemyController.hitByBulletEffect = Resources.Load<GameObject>("Enemy Hit");
            enemyController._enemyMaxHealth = 2;
            enemyController._soulEffect = Resources.Load<GameObject>("Skull death effect");
            enemyController._deathEffect = Resources.Load<GameObject>("Schizzo sangue");
            enemyController._salute = enemyHpBar.transform.GetChild(2).gameObject;
            enemyController.damage = 1f;
            enemyController.nextDamage = 0.1f;
            enemyController.damageRate = 1.5f;
            enemyController.pushBackForce = 3;
            enemyController.waitToFire = 2;
            enemyController.gunTip = new Vector3(0.32f, -0.06f, 0);
        }
        else if (enemyType.Equals("Rock"))
        {
            enemyHpBar.transform.localPosition += new Vector3(enemyHpBar.transform.localPosition.x, enemyHpBar.transform.localPosition.y + 0.3f, enemyHpBar.transform.localPosition.x);
            EnemyController enemyController = spawnedEnemy.AddComponent<EnemyController>();

            spawnedEnemy.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            spawnedEnemy.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("cragger")[0];

            spawnedEnemy.GetComponent<CapsuleCollider2D>().offset = new Vector2(0, 0.025f);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().size = new Vector2(0.5f, 0.6f);

            spawnedEnemy.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("rockAnimator");

            enemyController._borderEndLayer = "Ground";
            enemyController._speed = 2f;
            enemyController.hitByBulletEffect = Resources.Load<GameObject>("Enemy Hit");
            enemyController._enemyMaxHealth = 30;
            enemyController._soulEffect = Resources.Load<GameObject>("Skull death effect");
            enemyController._deathEffect = Resources.Load<GameObject>("Schizzo sangue");
            enemyController._salute = enemyHpBar.transform.GetChild(2).gameObject;
            enemyController.damage = 1f;
            enemyController.nextDamage = 0.1f;
            enemyController.damageRate = 1.5f;
            enemyController.pushBackForce = 3;
        }
        else if(enemyType.Equals("Spider"))
        {
            enemyHpBar.transform.localPosition += new Vector3(enemyHpBar.transform.localPosition.x, enemyHpBar.transform.localPosition.y + 0.2f, enemyHpBar.transform.localPosition.x);
            EnemyController enemyController = spawnedEnemy.AddComponent<EnemyController>();

            spawnedEnemy.transform.localScale = new Vector3(1, 1, 1);

            spawnedEnemy.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("volcanoSpiderball")[14];

            spawnedEnemy.GetComponent<CapsuleCollider2D>().offset = new Vector2(0, 0);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().size = new Vector2(0.275f, 0.15f);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().direction = CapsuleDirection2D.Horizontal;

            spawnedEnemy.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("VolcanoSpiderball_16");

            enemyController._borderEndLayer = "Ground";
            enemyController._speed = 0;
            enemyController.hitByBulletEffect = Resources.Load<GameObject>("Enemy Hit");
            enemyController._enemyMaxHealth = 2;
            enemyController._soulEffect = Resources.Load<GameObject>("Skull death effect");
            enemyController._deathEffect = Resources.Load<GameObject>("Schizzo sangue");
            enemyController._salute = enemyHpBar.transform.GetChild(2).gameObject;
            enemyController.damage = 1f;
            enemyController.nextDamage = 0.1f;
            enemyController.damageRate = 1.5f;
            enemyController.pushBackForce = 3;
        }
        else if (enemyType.Equals("Machine"))
        {
            enemyHpBar.transform.localPosition += new Vector3(enemyHpBar.transform.localPosition.x, enemyHpBar.transform.localPosition.y + 0.7f, enemyHpBar.transform.localPosition.x);
            EnemyShooterController enemyController = spawnedEnemy.AddComponent<EnemyShooterController>();

            spawnedEnemy.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            spawnedEnemy.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("boss")[24];

            spawnedEnemy.GetComponent<CapsuleCollider2D>().offset = new Vector2(0, 0);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().size = new Vector2(1, 1.15f);
            spawnedEnemy.GetComponent<CapsuleCollider2D>().direction = CapsuleDirection2D.Vertical;

            spawnedEnemy.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("boss_24");

            enemyController._borderEndLayer = "Ground";
            enemyController._speed = 2;
            enemyController.hitByBulletEffect = Resources.Load<GameObject>("Enemy Hit");
            enemyController._enemyMaxHealth = 40;
            enemyController._soulEffect = Resources.Load<GameObject>("Skull death effect");
            enemyController._deathEffect = Resources.Load<GameObject>("Schizzo sangue");
            enemyController._salute = enemyHpBar.transform.GetChild(2).gameObject;
            enemyController.damage = 1f;
            enemyController.nextDamage = 0.1f;
            enemyController.damageRate = 1.5f;
            enemyController.pushBackForce = 3;
            enemyController.waitToFire = 0.5f;
            enemyController.gunTip = new Vector3(0.6f, 0, 0);
        }
        else //Asking something that doesn't exists
            return null;

        spawnedEnemy.SetActive(true);
        return spawnedEnemy;
    }

    private GameObject CreateEnemy()
    {
        SpriteRenderer spriteRenderer;
        //CapsuleCollider2D capsuleCollider2D; //dipende interamente dal tipo di nemico quindi nnt settaggi!
        Rigidbody2D rigidBody2D;
        //Animator animator; //dipende interamente dal tipo di nemico quindi nnt settaggi!
        //EnemyController enemyController;

        GameObject enemy = new GameObject();
        enemy.SetActive(false);

        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Shootable");

        spriteRenderer = enemy.AddComponent<SpriteRenderer>();
        enemy.AddComponent<CapsuleCollider2D>();
        rigidBody2D = enemy.AddComponent<Rigidbody2D>();
        enemy.AddComponent<Animator>();
       

        spriteRenderer.sortingOrder = 10;

        rigidBody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidBody2D.angularDrag = 0f; //no fucking idea what this should be, but someone (probably dano) setted it at 0 in the prefab, even if it is 0.05 by defautl, so...

        return enemy;
    }

    public GameObject SpawnPlayer(Vector3 position, int deaths)
    {
        Debug.Log("spawningPlayer");
        if (_player == null)
            _player = this.CreatePlayer();

        Debug.Log("createdPlayer");
        GameObject spawnedPlayer = Instantiate(_player, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        spawnedPlayer.GetComponent<PlayerController>().Deaths = deaths;
        Debug.Log("spawnedPlayer");

        spawnedPlayer.name = "Player";
        spawnedPlayer.SetActive(true);
        return spawnedPlayer;
    }

    private GameObject CreatePlayer()
    {
        Debug.Log("creatingPlayer");
        SpriteRenderer spriteRenderer;
        CapsuleCollider2D capsuleCollider2D;
        Rigidbody2D rigidBody2D;
        Animator animator;
        PlayerController playerController;

        GameObject player = new GameObject();
        player.SetActive(false);

        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Shootable");

        spriteRenderer = player.AddComponent<SpriteRenderer>();
        capsuleCollider2D = player.AddComponent<CapsuleCollider2D>();
        rigidBody2D = player.AddComponent<Rigidbody2D>();
        animator = player.AddComponent<Animator>();
        playerController = player.AddComponent<PlayerController>();

        spriteRenderer.sortingOrder = 10;
        spriteRenderer.sprite = Resources.LoadAll<Sprite>("MainPlayerSprites/spr")[0];

        rigidBody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidBody2D.angularDrag = 0f; //no fucking idea what this should be, but someone (probably dano) setted it at 0 in the prefab, even if it is 0.05 by defautl, so...

        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("MainPlayerSprites/Player");

        capsuleCollider2D.offset = new Vector2(-0.025f, 0);
        capsuleCollider2D.size = new Vector2(0.33f, 0.44f);
        capsuleCollider2D.sharedMaterial = Resources.Load<PhysicsMaterial2D>("MainPlayerSprites/Ground");

        playerController.GroundLayer = "Ground";
        //it feels like this way is old like shit but I'm too idiot to change player and too lazy too create this shit runtime, so...
        //GameObject checkGround = Resources.Load<GameObject>("MainPlayerSprites/checkGround");
        //checkGround = Instantiate(checkGround);
        //checkGround.transform.SetParent(player.transform);
        //playerController.GroundCheck = checkGround.transform;
        //
        playerController.MaxSpeed = 2;
        playerController.CoinBoost = 0.075f;
        playerController.MaxHealth = 3;
        playerController._vitaPersaEffect = Resources.Load<GameObject>("MainPlayerSprites/Vita persa effect");
        playerController._deathEffect = Resources.Load<GameObject>("MainPlayerSprites/PlayerDeathParticles");
        playerController.FireRate = 0.3f;
        playerController.JumpHeight = 1f;
        playerController.GunTip = new Vector3(0.2f, 0, 0);
        player.name = ("PlayerPrefab");

        return player;
    }

    public GameObject SpawnItem(string type, Vector3 position)
    {
        if (_item == null)
            _item = this.CreateItem();

        GameObject spawnedItem = Instantiate(_item, position, Quaternion.Euler(new Vector3(0, 0, 0)));

        if (type.Equals("Coin"))
        {
            spawnedItem.tag = "Coin";

            spawnedItem.transform.localScale = new Vector3(0.4f, 0.4f, 0);

            spawnedItem.AddComponent<CollectMe>();

            spawnedItem.GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("Gems")[4];

            spawnedItem.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.28f);
        }
        else if (type.Equals("FireRateBoost"))
        {
            spawnedItem.tag = "FireRateBoost";

            spawnedItem.transform.localScale = new Vector3(0.03f, 0.03f, 0);

            PowerUp powerUp = spawnedItem.AddComponent<PowerUp>();
            powerUp.pickEffect = Resources.Load<GameObject>("boostEffect");
            powerUp.lightEffect = Resources.Load<GameObject>("lightEffect");

            spawnedItem.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("boost");

            spawnedItem.GetComponent<BoxCollider2D>().size = new Vector2(8f, 7.26f);
        }
        else if (type.Equals("Health"))
        {
            spawnedItem.tag = "Health";

            spawnedItem.transform.localScale = new Vector3(0.01f, 0.01f, 0);

            AumentaSalute aumentaSalute = spawnedItem.AddComponent<AumentaSalute>();
            aumentaSalute.pickEffect = Resources.Load<GameObject>("pickUpHearth");
            aumentaSalute.lightEffect = Resources.Load<GameObject>("healthAura");

            spawnedItem.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cuore");

            spawnedItem.GetComponent<BoxCollider2D>().size = new Vector2(11f, 9f);
            spawnedItem.GetComponent<BoxCollider2D>().offset = new Vector2(0, -0.5f);
        }
        else //Asking something that doesn't exists
            return null;
        spawnedItem.SetActive(true);
        return spawnedItem;
    }

    private GameObject CreateItem()
    {
        Debug.Log("creatingItem");

        GameObject item = new GameObject();
        item.SetActive(false);
        item.tag = "Item";

        item.AddComponent<SpriteRenderer>();
        BoxCollider2D boxCollider = item.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;

        return item;
    }

    public GameObject GetHP_Bar()
    {
        if (_hpBar == null)
            _hpBar = this.LoadHP_Bar();
        return Instantiate(_hpBar);
    }

    private GameObject LoadHP_Bar()
    {
        GameObject hpBar = Resources.Load<GameObject>("Enemy Hp Bar");

        return hpBar;
    }
}