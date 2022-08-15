using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlls : MonoBehaviour
{

    Vector2 movementValue = Vector2.zero;
    Rigidbody2D rb;
    Animator animator;
    public float moveSpeed = 1f;
    public PlayerTool currentTool = new PlayerTool(PlayerToolClass.MOWER);
    public FieldTilemap tilemapScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (movementValue != Vector2.zero)
        {
            rb.MovePosition(rb.position + movementValue * Time.fixedDeltaTime * moveSpeed);

        }

        SwitchAnimationsIfNeeded();

    }

    void OnMove(InputValue movementValue)
    {
        this.movementValue = movementValue.Get<Vector2>();
    }


    void SwitchAnimationsIfNeeded()
    {
        if (movementValue.x > 0)
        {
            animator.SetBool("facingRight", true);
        }
        if (movementValue.x < 0)
        {
            animator.SetBool("facingRight", false);
        }

        animator.SetBool("isMoving", movementValue != Vector2.zero);

    }

    void OnUseTool ()
    {
        if (currentTool != null)
        {

            tilemapScript.ApplyToolToTile(currentTool);


        }
    }

    void OnMower ()
    {
        currentTool = new PlayerTool(PlayerToolClass.MOWER);
    }

    void OnPlough()
    {
        currentTool = new PlayerTool(PlayerToolClass.PLOUGH);
    }

    void OnWater()
    {
        currentTool = new PlayerTool(PlayerToolClass.WATER);
    }

    void OnSeed()
    {
        currentTool = new PlayerTool(PlayerToolClass.SEED, CropType.Tomato);
    }

    void OnPick()
    {
        currentTool = new PlayerTool(PlayerToolClass.PICK);
    }



}



