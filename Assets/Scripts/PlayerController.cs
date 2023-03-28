﻿using Krivodeling.UI.Effects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public float sobriety = 20f;
    public float timeToWithdrawal = 0.0f;
    public float timeToEndWithdrawal = 0.0f;
    public bool doWithdrawalEffects = false;
    public bool doneWithdrawalEffects = false;
    public int WithdrawalCounter = 0;
    public Image slowicon;
    public Image bluricon;
    public HealthBar withdrawlTimer;

    public GameObject lastWall;

    public float drag_grounded;
    public float drag_inair;

    public DetectObs detectVaultObject; //checks for vault object
    public DetectObs detectVaultObstruction; //checks if theres somthing in front of the object e.g walls that will not allow the player to vault
    public DetectObs detectClimbObject; //checks for climb object
    public DetectObs detectClimbObstruction; //checks if theres somthing in front of the object e.g walls that will not allow the player to climb


    public DetectObs DetectWallL; //detects for a wall on the left
    public DetectObs DetectWallR; //detects for a wall on the right

    public Animator cameraAnimator;

    public float WallRunUpForce;
    public float WallRunUpForce_DecreaseRate;

    private float upforce;

    public float WallJumpUpVelocity;
    public float WallJumpForwardVelocity;
    public float drag_wallrun;
    public bool WallRunning;
    public bool WallrunningLeft;
    public bool WallrunningRight;
    private bool canwallrun; // ensure that player can only wallrun once before needing to hit the ground again, can be modified for double wallruns
    //double wallrun
    private float timeToWallRunAgain = 0f;
    
    public bool IsParkour;
    private float t_parkour;
    private float chosenParkourMoveTime;

    private bool CanVault;
    public float VaultTime; //how long the vault takes
    public Transform VaultEndPoint;

    private bool CanClimb;
    public float ClimbTime; //how long the vault takes
    public Transform ClimbEndPoint;

    public GameObject blurScreen;
    private UIBlur blur;



    private RigidbodyFirstPersonController rbfps;
    private Rigidbody rb;
    private Vector3 RecordedMoveToPosition; //the position of the vault end point in world space to move the player to
    private Vector3 RecordedStartPosition; // position of player right before vault
    // Start is called before the first frame update
    void Start()
    {
        slowicon.gameObject.SetActive(false);
        bluricon.gameObject.SetActive(false);
        rbfps = GetComponent<RigidbodyFirstPersonController>();
        rb = GetComponent<Rigidbody>();
        blur = blurScreen.GetComponent<UIBlur>();
        blur.Multiplier = 0;
       
    }

    // Update is called once per frame
    void Update()
    {
        withdrawlTimer.SetMaxHealth(sobriety);
        withdrawlTimer.SetHealth(timeToWithdrawal);
        //COMMENT THIS if we want one wall run per ground touch
        canwallrun = true;


        if (rbfps.Grounded)
        {
            rb.drag = drag_grounded;
            canwallrun = true;
            lastWall = null;
        }
        else
        {
            rb.drag = drag_inair;
        }
        if(WallRunning)
        {
            rb.drag = drag_wallrun;

        }
        //vault
        if (detectVaultObject.Obstruction && !detectVaultObstruction.Obstruction && !CanVault && !IsParkour && !WallRunning
            && (Input.GetKey(KeyCode.Space) || !rbfps.Grounded) && Input.GetAxisRaw("Vertical") > 0f)
        // if detects a vault object and there is no wall in front then player can pressing space or in air and pressing forward
        {
            CanVault = true;
        }

        if (CanVault)
        {
            CanVault = false; // so this is only called once
            rb.isKinematic = true; //ensure physics do not interrupt the vault
            RecordedMoveToPosition = VaultEndPoint.position;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = VaultTime;

            cameraAnimator.CrossFade("Vault",0.1f);
        }

        //climb
        if (detectClimbObject.Obstruction && !detectClimbObstruction.Obstruction && !CanClimb && !IsParkour && !WallRunning
            && (Input.GetKey(KeyCode.Space) || !rbfps.Grounded) && Input.GetAxisRaw("Vertical") > 0f)
        {
            CanClimb = true;
        }

        if (CanClimb)
        {
            CanClimb = false; // so this is only called once
            rb.isKinematic = true; //ensure physics do not interrupt the vault
            RecordedMoveToPosition = ClimbEndPoint.position;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = ClimbTime;

            cameraAnimator.CrossFade("Climb",0.1f);
        }


        //Parkour movement
        if (IsParkour && t_parkour < 1f)
        {
            t_parkour += Time.deltaTime / chosenParkourMoveTime;
            transform.position = Vector3.Lerp(RecordedStartPosition, RecordedMoveToPosition, t_parkour);

            if (t_parkour >= 1f)
            {
                IsParkour = false;
                t_parkour = 0f;
                rb.isKinematic = false;

            }
        }


        //Wallrun
        if (DetectWallL.Obstruction && !rbfps.Grounded && !IsParkour && canwallrun && DetectWallL.Object != lastWall) // if detect wall on the left and is not on the ground and not doing parkour(climb/vault)
        {
            lastWall = DetectWallL.Object;
            WallrunningLeft = true;
            canwallrun = false;
            upforce = WallRunUpForce; //refer to line 186
        }

        if (DetectWallR.Obstruction && !rbfps.Grounded && !IsParkour && canwallrun && DetectWallR.Object != lastWall) // if detect wall on thr right and is not on the ground
        {
            lastWall = DetectWallR.Object;
            WallrunningRight = true;
            canwallrun = false;
            upforce = WallRunUpForce;
        }
        if (WallrunningLeft && !DetectWallL.Obstruction || Input.GetAxisRaw("Vertical") <= 0f || rbfps.relativevelocity.magnitude < 1f) // if there is no wall on the lef tor pressing forward or forward speed < 1 (refer to fpscontroller script)
        {
            WallrunningLeft = false;
            WallrunningRight = false;
        }
        if (WallrunningRight && !DetectWallR.Obstruction || Input.GetAxisRaw("Vertical") <= 0f || rbfps.relativevelocity.magnitude < 1f) // same as above
        {
            WallrunningLeft = false;
            WallrunningRight = false;
        }

        if (WallrunningLeft || WallrunningRight) 
        {
            WallRunning = true;
            rbfps.Wallrunning = true; // this stops the playermovement (refer to fpscontroller script)
        }
        else
        {
            WallRunning = false;
            rbfps.Wallrunning = false;
        }

        if (WallrunningLeft)
        {     
            cameraAnimator.SetBool("WallLeft", true); //Wallrun camera tilt
        }
        else
        {
            cameraAnimator.SetBool("WallLeft", false);
        }
        if (WallrunningRight)
        {           
            cameraAnimator.SetBool("WallRight", true);
        }
        else
        {
            cameraAnimator.SetBool("WallRight", false);
        }

        if (WallRunning)
        {
            
            rb.velocity = new Vector3(rb.velocity.x, upforce ,rb.velocity.z); //set the y velocity while wallrunning
            upforce -= WallRunUpForce_DecreaseRate * Time.deltaTime; //so the player will have a curve like wallrun, upforce from line 136

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = transform.forward * WallJumpForwardVelocity + transform.up * WallJumpUpVelocity; //walljump
                WallrunningLeft = false;
                WallrunningRight = false;
            }
            if(rbfps.Grounded)
            {
                WallrunningLeft = false;
                WallrunningRight = false;
            }
        }

        //Only wallrun once per wall touch
        /*
        if (!canwallrun && !WallrunningLeft && !WallrunningRight)
        {
            timeToWallRunAgain += Time.deltaTime;
            if (timeToWallRunAgain > .05f)
            {
                canwallrun = true;
                timeToWallRunAgain = 0f;
            }
        }
        */


        //Withdrawal
        timeToWithdrawal += Time.deltaTime;

        if (timeToWithdrawal >= sobriety)
        {
            timeToWithdrawal = 0.0f;
            doWithdrawalEffects = true;
            WithdrawalCounter++;
        }

        if (doWithdrawalEffects)
        {
            timeToEndWithdrawal += Time.deltaTime;
            if (timeToEndWithdrawal <= 10.0f)
            {
                //Effects
                if (WithdrawalCounter % 2 == 1 && !doneWithdrawalEffects)
                {
                    this.gameObject.SendMessage("Fatigue", true);
                    doneWithdrawalEffects = true;
                    slowicon.gameObject.SetActive(true);
                }

                if (WithdrawalCounter % 2 == 0 && blur.Multiplier < 1)
                {
                    blur.Multiplier += .005f;
                    bluricon.gameObject.SetActive(true);
                    //doneWithdrawalEffects = true;
                }




            }

            if (timeToEndWithdrawal >= 10.0f || Input.GetKeyDown(KeyCode.F))
            {
                if (WithdrawalCounter % 2 == 1)
                {
                    this.gameObject.SendMessage("Fatigue", false);
                    slowicon.gameObject.SetActive(false);
                }

                if (WithdrawalCounter % 2 == 0)
                {
                    blur.Multiplier = 0;
                    bluricon.gameObject.SetActive(false);
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    sobriety -= 5;
                }
                if (timeToEndWithdrawal >= 10.0f)
                {
                    sobriety += 5;
                }


                timeToEndWithdrawal = 0;
                doneWithdrawalEffects = false;
                doWithdrawalEffects = false;


            }
        }


    }
 

}
