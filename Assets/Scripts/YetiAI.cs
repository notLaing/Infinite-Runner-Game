﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YetiAI : MonoBehaviour
{
    
    public Rigidbody rb;
    public SphereCollider hitBall;
    public GameObject player;
    public GameObject ground;
    public GameObject JumpReticle;
    public GameObject Shockwave;
    public GameObject Snowball;
    public GameObject Lane;
    public Vector3 jumpPoint = new Vector3();
    public Vector3 lanePoint = new Vector3();
    public Vector3 snowballPos = new Vector3();
    public bool JumpLanded = false;
    public bool laneSet = false;

    private BoxCollider groundHitBox;
    private Rigidbody playerBody;
    private float speed = 100f;
    private float forwardAccel = 5.5f; // 5.5f
    private float moveDistance;
    private float launchTimeDelay = 3f;
    private float attackTimeDelay = 5f;
    private float landCooldown = 2f;
    private float laneX = 0f;
    private Vector3 distanceToPlayer = new Vector3();
    private bool launching = false;
    private bool attacking = false;
    private bool jumpReticleMade = false;
    private bool hasJumped = false;
    private bool shockSpawned = false;
    
    

    private float jumpCountdown;
    private float snowballCountdown;
    private float jumpChance = .3f;
    private float snowballChance = .5f;

    //private Animation anim;




    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerBody = player.GetComponent<Rigidbody>();
        hitBall = GetComponent<SphereCollider>();
        snowballCountdown = Random.Range(-3f, 3f) + 8;
        jumpCountdown = Random.Range(-2f, 2f) + 5;
        
    }

    // Update is called once per frame
    void Update()
    {
        //print(rb.position);
        RunAI();

        StandStill();
        UpdateReticle();
       
        

    }
    private void FixedUpdate()
    {
        moveDistance = Mathf.Abs(rb.position.x - playerBody.position.x);

        distanceToPlayer = new Vector3(Mathf.Abs(rb.position.x - playerBody.position.x), Mathf.Abs(rb.position.y - playerBody.position.y), Mathf.Abs(rb.position.z - playerBody.position.z));
        //print(distanceToPlayer);

        if (moveDistance >= 1)
        {
            if (!hasJumped)
            {
                if (rb.position.x - playerBody.position.x < 0) rb.AddForce(moveDistance * speed * Time.deltaTime, 0f, forwardAccel);
                else rb.AddForce(-moveDistance * speed * Time.deltaTime, 0f, forwardAccel);
            }
        }
        else
        {
            if (!hasJumped)
            {
                rb.AddForce(0f, 0f, forwardAccel);
                rb.velocity = new Vector3((float)(rb.velocity.x * .99), rb.velocity.y, rb.velocity.z);
            }

        }
        if (distanceToPlayer.z <= 5f && !hasJumped && !player.GetComponent<PlayerController>().hit && !JumpLanded)
        {
            rb.velocity = playerBody.velocity - new Vector3(2f, 0f, 2f);
            forwardAccel = 0f;
        }
        if (rb.velocity.z >= 45 || (Mathf.Abs(rb.position.z - playerBody.position.z) <= 2) || rb.position.z > playerBody.position.z) forwardAccel = 0f;
        else if ((Mathf.Abs(rb.position.z - playerBody.position.z) >= 30)) rb.velocity = playerBody.velocity;
        else forwardAccel = 5.5f;
        if (Input.GetKeyDown(KeyCode.Y)) print(rb.velocity);
    }
    private void JumpAttack()
    {

        launchTimeDelay -= Time.deltaTime;
        if (launchTimeDelay > 0f)
        {
            if (!jumpReticleMade)
            {
                Instantiate(JumpReticle, jumpPoint, Quaternion.identity);
                jumpReticleMade = true;

            }
        }
        else
        {

            Vector3 vector = CalculateTrajectoryVelocity(transform.position, jumpPoint, 2.5f);
            if (playerBody.velocity.z < 24.75) rb.velocity = new Vector3(vector.x, vector.y, vector.z + playerBody.velocity.z + 5.125f);
            if (playerBody.velocity.z >= 24.75 && playerBody.velocity.z < 35) rb.velocity = new Vector3(vector.x, vector.y, vector.z + playerBody.velocity.z + (35f - playerBody.velocity.z));
            if (playerBody.velocity.z >= 35) rb.velocity = new Vector3(vector.x, vector.y, vector.z + playerBody.velocity.z);


            forwardAccel = 0f;
            jumpReticleMade = false;
            launchTimeDelay = 3;
            hasJumped = true;
            shockSpawned = false;
            launching = false;
        }




    }

    private void StandStill()
    {

        if (JumpLanded)
        { 
            landCooldown -= Time.deltaTime;
            if (landCooldown > 0f)
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
            }
            else
            {
                JumpLanded = false;
            }

        }
        if (!JumpLanded)
        {
            landCooldown = 2.0f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {

         if (other.gameObject.CompareTag("Obstacle"))
        {
            other.gameObject.SetActive(false);
        }
        
    }
    private void OnCollisionEnter(Collision other)
    {
        if (hasJumped && !shockSpawned)
        {
            Instantiate(Shockwave);
            Shockwave.transform.position = this.transform.position;
            shockSpawned = true;
            JumpLanded = true;
            hasJumped = false;
        }
    }
    private void UpdateReticle()
    {
        if (!JumpLanded)
        {
            if (!hasJumped) {
                jumpPoint = new Vector3(playerBody.position.x, 13.2f, playerBody.position.z + 20);
            }
            else
            {
                jumpPoint = new Vector3(jumpPoint.x, jumpPoint.y, playerBody.position.z + 20);
            }
        }

    }
    private void SummonSnowball()
    {
        if (!laneSet)
        {
            laneX = Random.Range(playerBody.transform.position.x - 20f, playerBody.transform.position.x + 20f);
            lanePoint = new Vector3(laneX, 13.1f, playerBody.position.z + 75f);
            Instantiate(Lane, lanePoint, Quaternion.identity);
            laneSet = true;
            
        }
        attackTimeDelay -= Time.deltaTime;
        if (attackTimeDelay > 0f)
        {
            //Snowball lane!

            lanePoint = new Vector3(laneX, 13.1f, playerBody.position.z + 110f);
        }
        else
        {
            // Send that snowball!
            snowballPos = new Vector3(lanePoint.x, lanePoint.y + 5f, playerBody.position.z - 20f);
            Instantiate(Snowball, snowballPos, Quaternion.identity);
            attackTimeDelay = 5f;
            laneSet = false;
            attacking = false;
        }
    }
    Vector3 CalculateTrajectoryVelocity(Vector3 origin, Vector3 target, float t)
    {
        float vx = (target.x - origin.x) / t;
        float vz = (target.z - origin.z) / t;
        float vy = ((target.y - origin.y) - 0.5f * Physics.gravity.y * t * t) / t;
        return new Vector3(vx, vy, vz);
    }

    private void RunAI()
    {
        if (jumpCountdown <= 0 && !launching)
        {
            float randChance = Random.Range(0f, 1f);
            
            if (randChance <= jumpChance )
            {
                jumpCountdown = Random.Range(-2f, 2f) + 5;
                launching = true;
                
            }
            else jumpCountdown = Random.Range(-2f, 2f) + 5;
        }
        if (snowballCountdown <= 0 && !attacking)
        {
            
            float randChance = Random.Range(0f, 1f);
            if (randChance <= snowballChance)
            {
                snowballCountdown = Random.Range(-3f, 3f) + 8;
                
                attacking = true;
                
            }

            else snowballCountdown = Random.Range(-3f, 3f) + 8;
        }
        if (!attacking && !launching)
        {
            jumpCountdown -= Time.deltaTime;
            snowballCountdown -= Time.deltaTime;
            
        }
        if (attacking) SummonSnowball();
        if (launching) JumpAttack();
        
    }
    

}
