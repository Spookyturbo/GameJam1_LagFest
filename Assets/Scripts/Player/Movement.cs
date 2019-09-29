using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour, ILaggable
{
    Animator animator;
    public Laggable lag { get; set; }
    [SerializeField] CharacterController2D controller;
    
    bool jump = false;
    float horizontalMove = 0f;


    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        jump = Input.GetButton("Jump");

        //Horizontal movement
        horizontalMove = Input.GetAxisRaw("Horizontal");
    }

    public void FixedLagUpdate() {
        controller.Move(horizontalMove * lag.TimeScale);
        controller.Jump(jump);
    }

    public void LagUpdate() {
        
    }
}
