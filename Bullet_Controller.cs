using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Text.RegularExpressions;


public class Bullet_Controller : MonoBehaviour, DoDamage
{
    [SerializeField]
    internal float _weaponDamage;
    [SerializeField]
    internal GameObject _wallHitEffect;
    [SerializeField]
    internal float _bulletSpeed;
    internal float _timeToLive; //dipende da chi l'ha sparato, impostato in FindShootedBy

    //public GameObject explosionEffect;

    internal String _target;


    void Awake() //funzione che viene lanciata PRIMA che venga creato l'oggetto (costruttore)
    {
        //inizialzzazioni in caso di mancato inserimento da Unity
        if (_weaponDamage == 0)
            _weaponDamage = 1;
        if (_bulletSpeed == 0)
            _bulletSpeed = 200;

        //Aggiunge una forza al proiettile che potra andare a sinistra o a destra in base a localRotation(posizione personaggio)
        if(this.gameObject.tag.Contains("Environment"))
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * -_bulletSpeed);
        else
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.right * _bulletSpeed);

    }

    void Start()
    {
        Destroy(gameObject, _timeToLive);
    }

    internal virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Shootable"))
        {
            impactWithTarget(collision.tag, _target, collision.gameObject.GetComponent<Health>());
        }

        //Se colpisce il terreno il proiettile genera l'effetto e poi sparisce
        else if (collision.gameObject.tag.Contains("Floor"))
        {
            impactWithEnvironment();
        }
    }

    //Per evitare compenetrazioni anche nella onTriggerStay
    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    public void doDamage(Health hp, float dmg)
    {
        Library.doDamage(hp, dmg);
    }

    private void impactWithTarget(String tagHitted, String tagToHit, Health hp)
    {
        // Debug.Log("target colpito" + this.name + " " + _weaponDamage + " " + tagHitted + " " + hp);
        //Azzero la velocità del proiettile
        removeForce();
        Instantiate(_wallHitEffect, transform.position, transform.rotation);
        Debug.Log("effetto instanziato da: " + this.name + " " + tagHitted);
        Destroy(this.gameObject);
        if (tagHitted == tagToHit)
        {
            Debug.Log("colpito nemico da colpire");
            doDamage(hp, _weaponDamage);
        }
    }
    private void impactWithEnvironment()
    {
        Instantiate(_wallHitEffect, transform.position, transform.rotation);
        Destroy(this.gameObject);
    }

    public void removeForce()
    {
        //Quando il proiettile colpisce si ferma!
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }
}
