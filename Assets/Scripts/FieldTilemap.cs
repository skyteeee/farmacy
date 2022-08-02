using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldTilemap : MonoBehaviour
{

    GridLayout gridLayout;
    Tilemap tilemap;
    public GameObject tileSelectorPrefab;
    private Vector3Int playerPos;
    private GameObject selector;

    void Start()
    {
        gridLayout = GetComponentInParent<GridLayout>();
        tilemap = GetComponent<Tilemap>();
    }

   
    void OnTriggerStay2D(Collider2D collision)
    {
        var pos1 = gridLayout.WorldToCell(collision.bounds.center);
        

        if (tilemap.HasTile(pos1))
        {
            var tile1 = tilemap.GetTile<Tile>(pos1);
            Debug.Log($"{tile1.name}: {pos1.x}, {pos1.y}");

            if (selector == null)
            {
                selector = Instantiate(tileSelectorPrefab, tilemap.GetCellCenterWorld(pos1), Quaternion.identity);
                playerPos = pos1;
            }

            if (playerPos != pos1)
            {
                playerPos = pos1;                
                selector.transform.Translate(-selector.transform.position + tilemap.GetCellCenterWorld(pos1));
            }

        } else
        {
            DestroySelector();
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        DestroySelector();
    }

    private void DestroySelector ()
    {
        if (selector != null)
        {
            Destroy(selector);
            selector = null;
        }
    }

}
