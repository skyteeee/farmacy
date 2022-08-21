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
    public CropType carriedItem = CropType.NONE;
    public GameObject carriedItemPrefab;
    private GameObject carriedItemObj;
    private Sprite[] cropSprites;

    private static Dictionary<CropType, int> cropSpritesIdxs = new Dictionary<CropType, int>()
    {
        {CropType.Carrot, 0 },
        {CropType.Tomato, 1 },
        {CropType.Strawberry, 2 },
        {CropType.Pumpkin, 3 },
        {CropType.Corn, 4 },
        {CropType.Potato, 5 },
        {CropType.Watermelon, 6 },
        {CropType.Radish, 7 },
        {CropType.Lettuce, 8 },
        {CropType.Wheat, 9 },
        {CropType.Eggplant, 10 },
    };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        cropSprites = Resources.LoadAll<Sprite>("Sprites/item_carry");
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
        if (tilemapScript != null)
        {

            if (currentTool != null && carriedItem == CropType.NONE)
            {

                tilemapScript.ApplyToolToTile(currentTool);

            }

            if (currentTool == null && carriedItem == CropType.NONE)
            {
                carriedItem = tilemapScript.TryPickup();
                if (carriedItem != CropType.NONE)
                {
                    carriedItemObj = Instantiate(carriedItemPrefab, new Vector3(transform.position.x + 0.1f, transform.position.y + 0.14f, transform.position.z), Quaternion.identity);
                    carriedItemObj.transform.SetParent(transform);
                    carriedItemObj.GetComponent<SpriteRenderer>().sprite = cropSprites[cropSpritesIdxs[carriedItem]];
                }
            }
            else
            {
                if (carriedItem != CropType.NONE)
                {
                    if (tilemapScript.TryPutIntoStorage(carriedItem))
                    {
                        Destroy(carriedItemObj);
                        carriedItemObj = null;
                        carriedItem = CropType.NONE;
                    }
                }
            }
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

    void OnNoTool()
    {
        currentTool = null;
    }



}



