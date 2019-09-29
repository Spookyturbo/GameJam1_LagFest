using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLife : MonoBehaviour, ILaggable
{
    public Laggable lag { get; set; }
    Animator animator;
    [SerializeField] Collider2D hitbox;
    [SerializeField] int health = 100;
    int damageQueue = 0;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
    }

    public void FixedLagUpdate() {

    }

    public void LagUpdate() {
        health -= damageQueue;
        damageQueue = 0;
        if(health <= 0)
            Die();
    }

    public void TakeDamage(int damage) {
        damageQueue += damage;
    }

    void Die() {
        animator.SetTrigger("Die");
    }
}
