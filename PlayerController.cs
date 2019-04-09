using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public delegate void PlayerDeathHandler(object source, PlayerDeathArgument e);

//Il giocatore NON implementa doDamage perche' non fa danno a contatto a differenza dei nemici che fanno danno anche se toccati!
public class PlayerController : MonoBehaviour , Health, Shooter
{
    internal event PlayerDeathHandler onPlayerDeath;

    //Velocità massima personaggio
    [SerializeField]
    private float _maxSpeed;
    [SerializeField]
    private float _coinBoost;
    //Oggetto del motore fisico 
    private Rigidbody2D _myRB;
    //Oggetto per la gestione delle animazioni
    private Animator _myAnim;
    //Se il personaggio guarda a destra oppure no
    private bool _facingRight;
    //Soldi del personaggio
    private int _coins;
    private int _deathCounter;

    //Jumping variables, se il personaggio si trova sul terreno
    private bool _grounded = false;
    //Crea un piccolo cerchio sotto al tag "check ground" che permette di capire quando stai intersecando un collider e quindi quando sei a terra oppure no
    //private const float _groundCheckRadius = 0.13f;
    //Consente di ottenere il "ground Layer", oggetto che identifica il layer di un gameObject del gioco
    [SerializeField]
    private string _groundLayer;
    //[SerializeField]
    //private Transform _groundCheck;
    //Altezza massima del salto
    [SerializeField]
    private float _jumpHeight;
    EdgeCollider2D myEdge;

    //Bullet variables
    //Game Object che indica il LUOGO dove nascerà il proiettile sparato dal personaggio
    [SerializeField]
    private Vector3 _gunTip;
    //Tempo tra uno sparo e l'altro
    [SerializeField]
    private float _fireRate;
    //Prossimo momento in cui potrai ri-iniziare a sparare
    private float _nextFire = 0f;
    //Fire rate deve essere disponibile a PowerUp per la modifica dei parametri


    //Variabili per la salute del giocatore e per l' interfaccia della barra degli HP
    public float _maxHealth; //La salute massima del giocatore
    private float _currentHealth; //La salute ( aggiornata ad ogni colpo subito ) attuale del giocatore
    [SerializeField]
    internal const string HP_BAR_NAME = "salute verde";
    public GameObject _deathEffect; //Effetto di morte del giocatore
    public GameObject _vitaPersaEffect; //Effetto di perdita vita del giocatore

    private Factory _factory = null;

    private AudioSource shootEffect;
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
    }

    public float CurrentHealth
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }

    private void Start()
    {
        _myRB = this.GetComponent<Rigidbody2D>();
        _myAnim = this.GetComponent<Animator>();
        _currentHealth = _maxHealth;
        Debug.Log("current health:" + _currentHealth);
        shootEffect = GameObject.Find("Shoot Sound").GetComponent<AudioSource>();
        _facingRight = true;
    }

    private void Update()
    {
        //Se sono a terra e sto premendo un salto per saltare -> Non sono più a terra e aggiungo la forza verticale (salto)
        if (_grounded && Input.GetAxis("Jump") > 0 ||  _grounded  && Input.GetKey(KeyCode.UpArrow) || _grounded && Input.GetKey(KeyCode.W))
        {
            _grounded = false;
            _myAnim.SetBool("onGround", _grounded);
            _myRB.AddForce(new Vector2(0, JumpHeight), ForceMode2D.Impulse);
        }

        //Se l'utente preme il tasto di fuoco (Macro Fire1)
        if (Input.GetAxisRaw("Fire1") > 0 || Input.GetKey(KeyCode.LeftShift))
            Shoot();

    }

    private void FixedUpdate()
    {
        //Ad ogni frame del gioco si crea un piccolo cerchio che intercetta altri colliders e indica quando siamo a terra
        //Indicata la posizione, il raggio del cerchio e il riferimento al Layer lo disegna e fa un controllo che ritorna true se interseco un oggetto
        //Il cui layer corrisponde a "ground". Si setta quindi il parametro dell'animator a true/false che triggera l'animazione del personaggio
        //grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask(groundLayer));
        _grounded = ((Physics2D.Raycast(this.transform.position + new Vector3(0.02f, 0, 0), Vector2.down, 0.3f, LayerMask.GetMask(GroundLayer))) ||
            (Physics2D.Raycast(this.transform.position - new Vector3(0.02f, 0, 0), Vector2.down, 0.3f, LayerMask.GetMask(GroundLayer))));
        Debug.DrawRay(this.transform.position - new Vector3(0.02f, 0, 0), Vector2.down, Color.black, 0.3f);
        Debug.DrawRay(this.transform.position + new Vector3(0.02f, 0, 0), Vector2.down, Color.black, 0.3f);
        _myAnim.SetBool("onGround", _grounded);
        //Oltre al controllo "sono a terra" viene controllata di continuo la velocità verticale (serve per le animazioni del salto!)
        _myAnim.SetFloat("verticalSpeed", _myRB.velocity.y);

        //Transform trans = this.transform;
        //Debug.DrawLine(trans.position, trans.position - trans.right * this.GetComponent<SpriteRenderer>().bounds.extents.x);

        // GetAxis restituisce un valore tra -1 e 1 riguardante la pressione del tasto o del joystick.
        //Movimento orizzontale del personaggio, alla perssione 
        float move = Input.GetAxis("Horizontal");
        _myAnim.SetFloat("Speed", Mathf.Abs(move));
        _myRB.velocity = new Vector2(move * MaxSpeed, _myRB.velocity.y);
        //Gira il personaggio e il suo sprite nel caso in cui mi stia spostando nella direzione OPPOSTA a come viene dato l'input dall'utente (salvato in move)
        if ((move > 0 && !_facingRight) || (move < 0 && _facingRight))
            Flip();
    }

    private void Flip()
    {
        //Giro il personaggio
        _facingRight = !_facingRight;
        //Usiamo scale perchè usando rotation si potrebbe andare in overflow
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        //this.transform.GetChild(0).localScale *= -1; 
    }

    //Consente di sparare con il personaggio
    public void Shoot() //so bad public
    {
        //Time.time e' tempo attuale (a partire da quando è stato creato l'oggetto)
        if (Time.time > _nextFire)
        {
            _nextFire = Time.time + FireRate;
            //Crea un'istanza del bullet e fa partire la funzione awake()
            _factory.SpawnBullet(this, _facingRight ? this.transform.position + GunTip : this.transform.position - GunTip, !_facingRight);
            //Debug.Log("setted bullet tag: " + shootedBullet.tag);
            shootEffect.Play();

        }
    }

    public void addDamage(float damage)
    {
        Debug.Log("damage: " +damage);
        Debug.Log("current health:" +CurrentHealth);
        GameObject hpBar = GameObject.Find("Level").GetComponent<BoardManager>().playerHpBar;
        //evito che la barra vada in negativo e poi sottraggo alla barra un tot di salute
        if (CurrentHealth - damage <= 0)
        {
            CurrentHealth = 0;
            hpBar.transform.GetChild(2).transform.localScale = new Vector3(0, 0, 0);
        }
        else
        { 
            //Ogni chiamata addDamage sul giocatore infligge un danno alla salute attuale e riduce la salute del personaggio
            CurrentHealth -= damage;
            hpBar.transform.GetChild(2).transform.localScale -= new Vector3(damage / MaxHealth, 0, 0);
        }
        //Se la salute e' negativa il giocatore muore e vengono instanziati gli effetti di morte 
        if (CurrentHealth <= 0)
        {
            Instantiate(_deathEffect, transform.position, transform.rotation);
            Instantiate(_vitaPersaEffect, transform.position, transform.rotation);
            //Dopo gli effetti il giocatore viene riportato alla posizione originale 
            if (onPlayerDeath != null)
            {
                onPlayerDeath(this, new PlayerDeathArgument("player morto", _deathCounter+1));
                Debug.Log("Death sent to board manager");
            }
            Destroy(this.gameObject);
        }
    }

    public void addCoin(int howMany)
    {
        _coins += howMany;
        Debug.Log("total coin: " + _coins);
        MaxSpeed += CoinBoost;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Se sono su una piattforma ne divento figlio -> Il movimento diventa relativo e non assoluto
        if (collision.gameObject.tag == "Mobile Floor")
            transform.parent = collision.transform;
    }

    //Se salto da una piattforma ne divento figlio -> Il movimento diventa relativo e non assoluto
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Mobile Floor")
            transform.parent = null;
    }

    public int Coins
    {
        get { return _coins; }
        set { _coins = value; }
    }

    public int Deaths
    {
        get { return _deathCounter; }
        set { _deathCounter = value; }
    }

    public float MaxSpeed
    {
        get
        {
            return _maxSpeed;
        }

        set
        {
            _maxSpeed = value;
        }
    }

    public float CoinBoost
    {
        get
        {
            return _coinBoost;
        }

        set
        {
            _coinBoost = value;
        }
    }

    public string GroundLayer
    {
        get
        {
            return _groundLayer;
        }

        set
        {
            _groundLayer = value;
        }
    }
    /*
    public Transform GroundCheck
    {
        get
        {
            return _groundCheck;
        }

        set
        {
            _groundCheck = value;
        }
    }
    */
    public float JumpHeight
    {
        get
        {
            return _jumpHeight;
        }

        set
        {
            _jumpHeight = value;
        }
    }

    public Vector3 GunTip
    {
        get
        {
            return _gunTip;
        }

        set
        {
            _gunTip = value;
        }
    }

    public float FireRate
    {
        get
        {
            return _fireRate;
        }

        set
        {
            _fireRate = value;
        }
    }

    public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }

        set
        {
            _maxHealth = value;
        }
    }
}
