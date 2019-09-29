using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LagBlast : MonoBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float lagAmount = 5;
    //Affected by lag
    [SerializeField] float lifeTime = 4;
    public Laggable lag { get; set; }
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update() {
        //Can't kill objects in the lag functions as it messes up the laggable update loop
        lifeTime -= Time.deltaTime * lag.TimeScale; 
        if(lifeTime <= 0)
            Die();
    }

    void FixedUpdate() {
        rb.velocity = new Vector2(speed * lag.TimeScale, 0);
    }

    //Hit stuff
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Hitbox")) {
            Laggable toLag = other.gameObject.GetComponentInParent<Laggable>();
            if(lag != null) {
                lag.Lag(toLag, lagAmount);
            }
            Die();
        }
    }

    //Set amount to lag what it hits
    public void SetLag(int amount) {
        lagAmount = amount;
    }

    public void SetSource(Laggable src) {
        lag = src;
    }

    //Remove from screen
    void Die() {
        Destroy(gameObject);
    }
}
