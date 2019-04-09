using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public delegate void EnemyDeathHandler(object source, EnemyDeathArgument e);

public class EnemyController : MonoBehaviour, Health , DoDamage
{
    internal event EnemyDeathHandler onEnemyDeath;
    internal int enemyNumber;

    //Quando non trovo piu il "borderEndLayer" il personaggio cambia direzione. Nel nostro caso useremo GROUND
    [SerializeField]
    internal string _borderEndLayer;
    //Velocita' di movimento del nemico e variabili per accedere al rigidBody fisico e alle coordinate Transform
    [SerializeField]
    internal float _speed;
    private Rigidbody2D _rB;
    private Transform _myTransform;
    //Ampiezza (bordo) dello sprite, serve per identificare quando il personaggio ha il bordo appena fuori da una piattaforma (per farlo girare)
    private float _width;
    private bool _facingRight = false; //Il personaggio inizia guardando a destra
    //Effetto invocato quando si viene colpiti da proiettile
    public GameObject hitByBulletEffect;

    //Variabili per la salute: salute massima, saute attuale e oggetto canvas che rappresenta la barra verde della salute (e il coin drop)
    [SerializeField]
    internal float _enemyMaxHealth;
    internal float _currentHealth;
    [SerializeField]
    internal GameObject _deathEffect;
    [SerializeField]
    internal GameObject _soulEffect;
    [SerializeField]
    internal GameObject _salute;
    [SerializeField]
    private const string HP_BAR_NAME = "salute verde";

    //Variabili per la gestione del danno:
    [SerializeField]
    internal float damage;
    //Quanto spesso un nemico puo' danneggiarci
    [SerializeField]
    internal float damageRate;
    //Quando avverrà il prossimo danneggiamento nemico
    [SerializeField]
    internal float nextDamage;
    //Quanto veniamo sparati indietro quando un nemico ci danneggia
    [SerializeField]
    internal float pushBackForce;

    protected Factory _factory = null;

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


    //get per la salute
    public bool FacingRight
    {
        get { return _facingRight; }
    }
    //internal -> Permette di essere vista dal package (enemyShooter controller estende tale classe, deve vederla!)
    //virtual -> Rende possibile l'estensione
    internal virtual void Start()
    {
        //Ottengo i riferimenti agli oggetti
        _myTransform = this.transform;
        _rB = this.GetComponent<Rigidbody2D>();
        _width = this.GetComponent<SpriteRenderer>().bounds.extents.x;
        _currentHealth = _enemyMaxHealth;
        nextDamage = 0f; //Posso ricevere danno appena nasce il nemico
    }


    void FixedUpdate()
    {
        //Origine dei vettori down e horizontal. La linea viene generata di default al centro dell'oggetto, viene quindi spostata DI FRONTE al personaggio
        //Per farlo si usa la grandezza dello sprite come nel disegno di sotto NB myTransform.right è un versore che punta a destra e vale uno

        /* **********                                       Dopo la sottrazione il punto "x"        **********
           *   <-   *                                       Di spawn della linea viene spostato     *   <-   *
           *        *                                                                               *        *
           *********x   <-- Line cast starting position                                             x*********
          <--width-->                                                                               ^-- La linea viene spawnata nel punto corretto ora
         */
        Vector2 lineCastPost = _myTransform.position - (_myTransform.right * _width);
        //Vettore verticale che interseca il terreno(verso il basso) indica quando un corpo si trova a contatto su una superficie
        Vector2 down = new Vector2(0, -0.5f);
        //Vettore orizzontale che interseca il terreno, indica quando un corpo puo' trovarsi a contatto con una superficie orizzontale
        Vector2 horizontal = new Vector2(-0.05f, 0);
        //Il vettore orizzontale viene invertito se il peronaggio non 
        if (!this.FacingRight)
        {
            horizontal = -horizontal;
        }
        Debug.DrawLine(lineCastPost, lineCastPost + down);
        Debug.DrawLine(lineCastPost, lineCastPost + horizontal);
        //La funzione Linecast disegna una riga nella posizione indicata da linecastpos. Prende in ingresso inizio, fine e il bordo da controllare
        //Ritorna un boolean che indica se la linea interseca il layer indicato come terzo parametro
        bool isGrounded = Physics2D.Linecast(lineCastPost, lineCastPost + down, LayerMask.GetMask(_borderEndLayer));
        bool horizontalGrounded = Physics2D.Linecast(lineCastPost, lineCastPost + horizontal, LayerMask.GetMask(_borderEndLayer));

        if (!isGrounded || horizontalGrounded)
        { //Se non sono a terra oppure sto per cadere allora ruoto tutto di 180 gradi e riprendo a muovermi
            Vector3 currentRotation = _myTransform.eulerAngles;
            currentRotation.y += 180;
            _facingRight = !_facingRight;
            //Quando sono nel bordo mi giro di 180°
            _myTransform.eulerAngles = currentRotation;


        }
        //Mi salvo la velocita' del corpo
        Vector2 myVelocity = _rB.velocity;
        //Lo sprite che abbiamo e' girato verso sinistra, il "-" serve per evitare che i nemici camminino all'indietro automaticamente
        myVelocity.x = -_myTransform.right.x * _speed;
        _rB.velocity = myVelocity;

    }

    public void addDamage(float damage)
    {
        Debug.Log("ho subito danno");
        _currentHealth -= damage;
        //recupero SOLO LA BARRA VERDE  e la decremento
        this._salute.transform.localScale -= new Vector3(damage / _enemyMaxHealth, 0, 0);
        //Debug.Log("Nemico colpito " + damage + " " + _currentHealth + " " + this._salute.transform.localScale);
        if (_currentHealth <= 0)
        {
            Instantiate(_deathEffect, transform.position, transform.rotation);
            Instantiate(_soulEffect, new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), transform.rotation);
            _factory.SpawnItem("Coin", transform.position);
            if (onEnemyDeath != null)
            {
                onEnemyDeath(this, new EnemyDeathArgument("enemy morto", enemyNumber));
            }
            this._salute = null;
            Destroy(this._salute);
            Destroy(this.gameObject);
        }
    }

    //Se un nemico viene colpito da un proiettile viene istanziato l'effetto corrispondente
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
            Instantiate(hitByBulletEffect, transform.position, transform.rotation);

        //Il danno da collisione infligge danno al giocatore !
        if (collision.tag == "Player" && nextDamage < Time.time)
        {
            //Restituisce un riferimento alla salute attuale del giocatore
            Health playerHP = collision.gameObject.GetComponent<PlayerController>();
            doDamage(playerHP, damage);
            //offset per evitare di prendere danno di continuo
            nextDamage = Time.time + damageRate;
            pushBack(collision.transform);
        }
    }

    //Danno viene calcolato sia ad inizio collisione che in caso si resti attaccati al giocatore
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player" && nextDamage < Time.time)
        {
            Health playerHP = other.gameObject.GetComponent<PlayerController>();
            doDamage(playerHP, damage);
            //offset per evitare di prendere danno di continuo
            nextDamage = Time.time + damageRate;
            pushBack(other.transform);
        }
    }

    private void pushBack(Transform pushedObject)
    {
        //Direzione di dove voglio lanciare via il personaggio(DA AGGIUSTARE SE VOGLIO SPOSTARE ANCHE ORZZONTALMENTE AGGIUNGENDO 1 COMPONENTE)
        //normalized -> Rende il vettore tra 0 e 1 (diventa un versore praticamente)
        Vector2 pushDirection = new Vector2(0, pushedObject.position.y - transform.position.y).normalized;
        //Consente di aggiustare la forza di rimbalzo
        pushDirection *= pushBackForce;
        //Ottengo il riferimento del mio corpo da sparare indietro, in questo modo lo script è generalizzato e puo' essere usato
        //ovunque per spostare qualsiasi altra cosa!
        Rigidbody2D pushRB = pushedObject.gameObject.GetComponent<Rigidbody2D>();
        pushRB.velocity = Vector2.zero;
        pushRB.AddForce(pushDirection, ForceMode2D.Impulse);
    }

    public void doDamage(Health hp, float dmg)
    {
        hp.addDamage(dmg);
    }

}

