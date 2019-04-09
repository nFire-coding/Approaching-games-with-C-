using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooterController : EnemyController, Shooter
{
    public Vector3 gunTip;
    //public GameObject bullet;
    [SerializeField]
    internal float waitToFire;

    override internal void Start ()
    {
        base.Start();
        Shoot();
    }

    public void Shoot()
    {
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        //tempo tra ogni attacco
        yield return new WaitForSeconds(waitToFire);
        //spawn proiettile nella posizione di gunTip
        /*Vector3 angoli = gunTip.transform.rotation.eulerAngles;
        angoli.x += 180;
        */
        Vector3 rot = this.transform.rotation.eulerAngles;
        rot = new Vector3(rot.x, rot.y + 180, rot.z);

        _factory.SpawnBullet(this, FacingRight ? this.transform.position + gunTip : this.transform.position - gunTip, !FacingRight);
        //Debug.Log("setted bullet tag: " + shootedBullet.tag);

        StartCoroutine(Attack());
    }
}
