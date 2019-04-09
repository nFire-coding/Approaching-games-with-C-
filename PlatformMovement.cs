using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    //Posizione iniziale piattaforma
    private Vector3 posA;
    //Posizione finale (movimento)
    private Vector3 posB;
    //Alternativamente posA o posB in base a dove mi trovo
    private Vector3 nextPos;

    private Vector3 offset;
    //Riferimento ad una posizione data dall'engine di unity(dragged)
    public Transform childTransform;
    //Oggetto vuoto la cui posizione è posB
    public Transform transformB;
    public float speed;
  


	void Start ()
    {
        //Variabili prese (drag) unity
        posB = transformB.localPosition;
        posA = childTransform.localPosition;
		nextPos = posB;
	}
	
	
	void Update ()
    {
        Move();
	}

    private void Move()
    {
        childTransform.localPosition = Vector3.MoveTowards(childTransform.localPosition, nextPos, speed*Time.deltaTime);
        if(Vector3.Distance(childTransform.localPosition, nextPos) <= 0)
            nextPos = nextPos != posA ? posA : posB;
    }
    


}
