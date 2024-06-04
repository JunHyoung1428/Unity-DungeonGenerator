using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    static TilemapGenerator instance;
    public static TilemapGenerator Instance
    {
        get { return instance; }  }

    [SerializeField] Tilemap baseMap;


    private void Awake()
    {
        CreateInstance();
        baseMap = GetComponent<Tilemap>();
    }
    
    void CreateInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }


    public void FillTile(Vector3Int startPos, Vector3Int endPos, TileBase tile )
    {
        for(int x =  startPos.x; x < endPos.x; x++)
        {
            for(int y =  startPos.y; y < endPos.y; y++)
            {
                baseMap.SetTile(new Vector3Int(x, y), tile);
            }
        }
    }
}
