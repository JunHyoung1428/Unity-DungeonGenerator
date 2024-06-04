using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigidBody;

    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase tile;

    [Header("Room Size")]
    [SerializeField] int sizeX;
    [SerializeField] int sizeY;


    Vector3 size;
    public void SetSize( )
    {
        size = transform.localScale;
        sizeX = (int)size.x;
        sizeY = (int)size.y;
        tilemap.size = new Vector3Int(sizeX, sizeY);
    }

    public void FillTile()
    {
        //transform.localScale = Vector3.one;
        transform.position = new Vector3((int)transform.position.x, (int)transform.position.y, transform.position.z);

        Vector3Int pos = tilemap.LocalToCell(transform.position);
        Vector3Int startPos = tilemap.LocalToCell( new Vector3Int((pos.x - sizeX/2 ), (pos.y- sizeY/2)));
        Vector3Int endPos = tilemap.LocalToCell(new Vector3Int(( pos.x + sizeX / 2 ), ( pos.y + sizeY / 2 )));
        Debug.Log($"TilemapSize {tilemap.size}");
        Debug.Log($"Pos : {pos} / StartPos {startPos}, EndPos{endPos}");
        TilemapGenerator.Instance.FillTile(startPos, endPos, tile);

        //tilemap.BoxFill(startPos, tile, startPos.x, startPos.y, endPos.x, endPos.y);
        //tilemap.FloodFill(tilemap.LocalToCell(transform.position), tile);
    }

    public void ActivateRigidBody( float waitTime )
    {
        StartCoroutine(ActiveRigidBodyRoutine(waitTime));
    }

    IEnumerator ActiveRigidBodyRoutine( float waitTime )
    {
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        rigidBody.gravityScale = 0f;
        yield return new WaitForSeconds(waitTime);
        Destroy(rigidBody);

        //FillTile();
        //Destroy(gameObject);
    }

}
