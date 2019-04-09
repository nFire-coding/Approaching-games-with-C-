using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class ObstacleDamage : MonoBehaviour, DoDamage
{
    //Variabili per la gestione del danno:
    [SerializeField]
    private float damage;
    //Quanto spesso un nemico puo' danneggiarci
    [SerializeField]
    private float damageRate;
    //Quando avverrà il prossimo danneggiamento nemico
    [SerializeField]
    private float nextDamage;


    //Se un nemico viene colpito da un proiettile viene istanziato l'effetto corrispondente
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Il danno da collisione infligge danno al giocatore !
        if (collision.tag == "Player" && nextDamage < Time.time)
        {
            //Restituisce un riferimento alla salute attuale del giocatore
            Health playerHP = collision.gameObject.GetComponent<PlayerController>();
            doDamage(playerHP, damage);
            //offset per evitare di prendere danno di continuo
            nextDamage = Time.time + damageRate;
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
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Il danno da collisione infligge danno al giocatore !
        if (collision.tag == "Player" && nextDamage < Time.time)
        {
            //Restituisce un riferimento alla salute attuale del giocatore
            Health playerHP = collision.gameObject.GetComponent<PlayerController>();
            doDamage(playerHP, damage);
            //offset per evitare di prendere danno di continuo
            nextDamage = Time.time + damageRate;
        }
    }


    public void doDamage(Health hp, float dmg)
    {
        hp.addDamage(dmg);
    }
}
