using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private Vector2 velocity;
    [SerializeField]
    private float smoothTimeY;
    [SerializeField]
    private float smoothTimeX;
    public GameObject player; //changed by bulky
    //public bool bounds;
    //public Vector3 minCameraPosition;
    //public Vector3 maxCameraPosition;
    void Start()
    {
        //changed by bulky
        //player = GameObject.FindGameObjectsWithTag("Player"); //Cerca la tag corrispondente al giocatore purtroppo ritorna solo un array
    }

    private void FixedUpdate()
    {
        if (player != null)
            follow(player);
        else
            Debug.Log("probably the player has died because you are trying to access it but it is null, pls wait for respawn");
    }

    void follow(GameObject player)
    {
        float posX = Mathf.SmoothDamp(this.transform.position.x, player.transform.position.x, ref velocity.x, smoothTimeX);
        float posY = Mathf.SmoothDamp(this.transform.position.y, player.transform.position.y, ref velocity.y, smoothTimeY);
        this.transform.position = new Vector3(posX, posY, this.transform.position.z);
    }
}