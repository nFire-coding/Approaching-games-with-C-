using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AumentaSalute : MonoBehaviour {

    public GameObject lightEffect;
    public GameObject pickEffect;
    private GameObject effetto;
    void Start()
    {
        effetto = Instantiate(lightEffect, transform.position, transform.rotation);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            PlayerController controller = collision.gameObject.GetComponent<PlayerController>();
            //Se la salute non è già al massimo viene aumentata, sennò viene lanciato l'effetto e basta.
            Debug.Log("salute prima: " + controller.CurrentHealth);
            if (controller.CurrentHealth != 3)
            {
                controller.CurrentHealth += 1;
                GameObject.Find("Level").GetComponent<BoardManager>().playerHpBar.transform.GetChild(2).transform.localScale += new Vector3(1 / controller.MaxHealth, 0, 0);
            }
            Debug.Log("salute dopo: " + controller.CurrentHealth);
            Instantiate(pickEffect, transform.position, transform.rotation);
            Destroy(effetto);
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        Destroy(effetto);
    }
}
