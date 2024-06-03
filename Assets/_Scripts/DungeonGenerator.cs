using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] Tilemap tilemap;
    [SerializeField] Button buttonGen;


    [SerializeField] Vector2Int mapSize;
    [SerializeField] int maximumDetph;
    [SerializeField] float maximumDivideRate;
    [SerializeField] float minimumDivideRate;

    void Awake()
    {
        buttonGen.onClick.AddListener(GenerateDungeon);
    }

    bool isGened;
    void GenerateDungeon()
    {
        if(isGened)
            ClearDungeon();

        Node root = new Node(new RectInt(0, 0, mapSize.x, mapSize.y));
        DrawMap(0, 0);
        Divide(root, 0);
        isGened = true;
    }

    void ClearDungeon()
    {
        foreach ( Transform child in transform )
        {
            Destroy(child.gameObject);
        }
    }


    [SerializeField] LineRenderer map;
    [SerializeField] LineRenderer line;
    [SerializeField] LineRenderer room;

    //Use BSP Algorithm

    void DrawMap(int x, int y)
    {
        LineRenderer lineRenderer = Instantiate(map,transform);
        lineRenderer.SetPosition(0, new Vector2(x, y) - mapSize / 2); //좌측 하단
        lineRenderer.SetPosition(1, new Vector2(x + mapSize.x, y) - mapSize / 2); //우측 하단
        lineRenderer.SetPosition(2, new Vector2(x + mapSize.x, y + mapSize.y) - mapSize / 2);//우측 상단
        lineRenderer.SetPosition(3, new Vector2(x, y + mapSize.y) - mapSize / 2); //좌측 상단
    }

    void Divide(Node tree, int n)
    {
        if ( n == maximumDetph )
            return;


        int maxLength = Mathf.Max(tree.nodeRect.width, tree.nodeRect.height);
        //가로와 세로중 더 긴것을 구한후, 가로가 길다면 위 좌, 우로 세로가 더 길다면 위, 아래로 나눠주게 될 것이다.
        int split = Mathf.RoundToInt(Random.Range(maxLength * minimumDivideRate, maxLength * maximumDivideRate));
        //나올 수 있는 최대 길이와 최소 길이중에서 랜덤으로 한 값을 선택
        if ( tree.nodeRect.width >= tree.nodeRect.height ) //가로가 더 길었던 경우에는 좌 우로 나누게 될 것이며, 이 경우에는 세로 길이는 변하지 않는다.
        {
            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, split, tree.nodeRect.height)); 
            //위치는 좌측 하단 기준이므로 변하지 않으며, 가로 길이는 위에서 구한 랜덤값을 넣어준다.
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x + split, tree.nodeRect.y, tree.nodeRect.width - split, tree.nodeRect.height));
            //위치는 좌측 하단에서 오른쪽으로 가로 길이만큼 이동한 위치이며, 가로 길이는 기존 가로길이에서 새로 구한 가로값을 뺀 나머지 부분이 된다.
            DrawLine(new Vector2(tree.nodeRect.x + split, tree.nodeRect.y), new Vector2(tree.nodeRect.x + split, tree.nodeRect.y + tree.nodeRect.height));
        }
        else
        {
            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, tree.nodeRect.width, split));
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y + split, tree.nodeRect.width, tree.nodeRect.height - split));
            DrawLine(new Vector2(tree.nodeRect.x, tree.nodeRect.y + split), new Vector2(tree.nodeRect.x + tree.nodeRect.width, tree.nodeRect.y + split));
        }
        tree.leftNode.parNode = tree; 
        tree.rightNode.parNode = tree;
        Divide(tree.leftNode, n + 1); 
        Divide(tree.rightNode, n + 1);

    }

    void DrawLine(Vector2 start, Vector2 end )
    {
        LineRenderer lineRenderer = Instantiate(line, transform);
        lineRenderer.SetPosition(0, start - mapSize / 2);
        lineRenderer.SetPosition(1, end - mapSize / 2);
    }

}

public class Node
{
    public Node leftNode;
    public Node rightNode;
    public Node parNode;
    public RectInt nodeRect; //분리된 공간의 rect정보
    public Node( RectInt rect )
    {
        this.nodeRect = rect;
    }
}
