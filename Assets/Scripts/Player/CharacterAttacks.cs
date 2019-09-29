using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttacks : MonoBehaviour, ILaggable
{   
    Rigidbody2D rb;
    Animator animator;
    public Laggable lag {get;set;}
    [SerializeField] Collider2D sideHitbox;
    [SerializeField] Collider2D upHitbox;
    [SerializeField] Collider2D downHitbox;

    bool swordAttack = false;
    float attackVerticality = 0;
    private float verticalAttackLimit = 0.5f;
    private const float attackCooldown = 0.5f;
    private float attackOffCd = 0;

    [SerializeField] Transform projectileSpawnLocation;

    [SerializeField] GameObject lagBlastPrefab;
    int lagBlastPower = 5;
    bool projectileAttack = false;

    //In percentage
    float cpuChannelSpeed = 0.25f; //Takes 3 seconds to full chanel
    bool empowerSelf = false;

    bool reclaimCpu = false;

    void Start() {
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        attackVerticality = Input.GetAxisRaw("Vertical");
        if(Input.GetButtonDown("SwordAttack") && attackOffCd <= 0)
            swordAttack = true;

        if(Input.GetButtonDown("Reclaim"))
            reclaimCpu = true;

        if(Input.GetButton("EmpowerSelf")) {
            empowerSelf = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.freezeRotation = true;
        }
        else {
            if(Input.GetButtonUp("EmpowerSelf"))
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            empowerSelf = false;
        }

        if(Input.GetButtonDown("Projectile")) {
            projectileAttack = true;
        }

        animator.SetBool("Empowering", empowerSelf);

        //Need to be able to empower/reclaim cpu if you get frozen, can't be in lag functions
        //Empowerement
        if(empowerSelf) {
            if(attackVerticality < - verticalAttackLimit) {
                EmpowerCpu(false);
            }
            else {
                EmpowerCpu(true);
            }
        }

        if(reclaimCpu) {
            lag.UnlagAll();
            reclaimCpu = false;
        }
    }

    public void FixedLagUpdate() {

    }

    public void LagUpdate() {
        //Sword attack
        if(swordAttack && attackOffCd <= 0) {
            animator.SetTrigger("Attack");
            attackOffCd = attackCooldown;
            if(attackVerticality > verticalAttackLimit) {
                Attack(upHitbox);
            }
            else if(attackVerticality < -verticalAttackLimit) {
                Attack(downHitbox);
            }
            else {
                Attack(sideHitbox);
            }
            swordAttack = false;
        }
        
        attackOffCd -= Time.deltaTime;

        if(projectileAttack) {
            if(attackVerticality > verticalAttackLimit) {
                AccelerateBlast();
            }
            else if(attackVerticality < -verticalAttackLimit) {
                LagBlast();
            }
            else {
                FpsLaser();
            }
            projectileAttack = false;
        }
    }

    //Channel usable CPU into self to increase processing speed or decrease it
    //Channel speed is not affected by how much lag you currently have (Maybe it should?)
    void EmpowerCpu(bool increase) {
        float maxCpuUse = Mathf.Min(lag.maxCpu * 0.75f - lag.deltaCpu, lag.usableCpu);
        if(lag.usableCpu > 0 && lag.deltaCpu < lag.maxCpu * 0.75) {
                float changeInCpu = Mathf.Min(lag.maxCpu * cpuChannelSpeed * Time.deltaTime, maxCpuUse);
                if(!increase) changeInCpu = -changeInCpu;
                lag.Lag(lag, -changeInCpu);
        }
    }

    //Hits harder and moves faster higher deltaCpu.
    void FpsLaser() {

    }

    //Slows enemies it hits
    void LagBlast() {
        if(lag.usableCpu > lagBlastPower) {
            GameObject lagBlast = Instantiate(lagBlastPrefab, projectileSpawnLocation.position, lagBlastPrefab.transform.rotation) as GameObject;
            LagBlast blast = lagBlast.GetComponent<LagBlast>();
            blast.SetLag(lagBlastPower);
            blast.SetSource(lag);
        }
    }

    //Accelerates enemies it hits
    void AccelerateBlast() {

    }

    void Attack(Collider2D attackBox) {
        Collider2D[] cols = Physics2D.OverlapBoxAll(attackBox.bounds.center, attackBox.bounds.extents, attackBox.transform.rotation.z, LayerMask.GetMask("Hitbox"));
        if(cols.Length > 0) {
            Vector2 knockbackForce = (transform.position - cols[0].transform.position).normalized;
            rb.AddForce(knockbackForce * 1000 * Mathf.Clamp(lag.TimeScale, 1, 2));
            foreach(Collider2D col in cols) {
                col.GetComponentInParent<CharacterLife>().TakeDamage(20);
            }
        }
    }
}
