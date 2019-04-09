using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follow_enemy : MonoBehaviour {
    public GameObject enemy;
    public float altezzaBarraSalute;
	void Start () {
        this.transform.position = new Vector2(enemy.transform.position.x, enemy.transform.position.y + altezzaBarraSalute);
	}
	void Update () {
        this.transform.position = new Vector2(enemy.transform.position.x, enemy.transform.position.y + altezzaBarraSalute);
    }
}
