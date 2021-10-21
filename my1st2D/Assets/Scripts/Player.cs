using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{
    string _type = "puck";
    public string Type
    {
        get {
            return _type;
        }
        set {
            _type = value;
        }
    }
    private string _color = "blue";
    public string Color
    {
        get {
            return _color;
        }
        set {
            _color = value;
        }
    }
    private float _scale = 1.0f;
    public float Scale
    {
        get {
            return _scale;
        }
        set {
            _scale = value;
        }
    }
    private int _health = 100;

    public int Health
    {
        get {
            return _health;
        }
        set {
            _health = value;
        }
    }

    public Player() {}
    public Player(string type, string color, float scale, int health){
        Type = type;
        Color = color;
        Scale = scale;
        Health = health;

    }

    public void Info(){
        Debug.Log("Type: " + Type);
        Debug.Log("Color: " + Color); 
        Debug.Log("Scaled: " + Scale);
        Debug.Log("Health: " + Health);
    }

    public virtual void Attack(){
        Debug.Log("Player is attacking with fire");
    }

    public void SetHealth(int health) {
        //this.health = health;
        Health = health;
    }

    public int GetHealth() {
        return Health;
    }

    
}
