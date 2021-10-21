using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puck : Player
{
    public Puck(string type, string color, float scale, int health){
        Type = type;
        Color = color;
        Scale = scale;
        Health = health;
    }
    public override void Attack()
    {
        Debug.Log("Puck is moved to attack");
    }
} //
