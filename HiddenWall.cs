using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenWall : MonoBehaviour {
    private SpriteRenderer sprite;
    public GameObject rockBoomEffect;
	

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("BulletPlayer"))
        {
            StartCoroutine(Wait());
        }
        
        
    }

    private IEnumerator Wait()
    {
        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
        {
            Instantiate(rockBoomEffect, sprite.transform.position, sprite.transform.rotation);
            yield return new WaitForSeconds(0.02f);
            sprite.enabled = false;
        }
        GetComponent<EdgeCollider2D>().enabled = false;
    }
}
