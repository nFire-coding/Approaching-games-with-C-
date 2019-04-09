using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class PowerUp : MonoBehaviour
{
    public GameObject lightEffect;
    public GameObject pickEffect;
    private GameObject effetto;
    private float fireRate = 3.5f;

    void Start()
    {
        effetto = Instantiate(lightEffect, transform.position, transform.rotation);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            PlayerController controller = collision.gameObject.GetComponent<PlayerController>();
            controller.FireRate /= fireRate;
            Instantiate(pickEffect, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        Destroy(effetto);
    }
}
