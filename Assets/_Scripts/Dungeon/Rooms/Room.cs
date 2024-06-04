using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase tile;


    public void SetSize(Vector3 vector)
    {
        tilemap.size = new Vector3Int((int)vector.x, ( int ) vector.y);
    }

    public void FillTile()
    {
        for ( int x = 0; x < tilemap.size.x; x++ )
        {
            for ( int y = 0; y < tilemap.size.y; y++ )
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    public void ActivateRigidBody( float waitTime)
    {
        StartCoroutine(ActiveRigidBodyRoutine(waitTime));
    }

    IEnumerator ActiveRigidBodyRoutine(float waitTime )
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        yield return new WaitForSeconds(waitTime);
        Destroy(rb);
    }

}
