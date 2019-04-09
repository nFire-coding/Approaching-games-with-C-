using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Library
{
    public static void doDamage(Health hp, float dmg)
    {
        hp.addDamage(dmg);
    }

}