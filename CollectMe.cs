using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectMe : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            addCoin(playerController, 1);
            Instantiate(Resources.Load<GameObject>("CoinCollectEffect") , this.transform.localPosition, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }

    public void addCoin(PlayerController playerCoins, int coin)
    {
        playerCoins.addCoin(coin);
    }

}
