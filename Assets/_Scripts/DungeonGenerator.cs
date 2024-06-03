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
        lineRenderer.SetPosition(0, new Vector2(x, y) - mapSize / 2); //���� �ϴ�
        lineRenderer.SetPosition(1, new Vector2(x + mapSize.x, y) - mapSize / 2); //���� �ϴ�
        lineRenderer.SetPosition(2, new Vector2(x + mapSize.x, y + mapSize.y) - mapSize / 2);//���� ���
        lineRenderer.SetPosition(3, new Vector2(x, y + mapSize.y) - mapSize / 2); //���� ���
    }

    void Divide(Node tree, int n)
    {
        if ( n == maximumDetph )
            return;


        int maxLength = Mathf.Max(tree.nodeRect.width, tree.nodeRect.height);
        //���ο� ������ �� ����� ������, ���ΰ� ��ٸ� �� ��, ��� ���ΰ� �� ��ٸ� ��, �Ʒ��� �����ְ� �� ���̴�.
        int split = Mathf.RoundToInt(Random.Range(maxLength * minimumDivideRate, maxLength * maximumDivideRate));
        //���� �� �ִ� �ִ� ���̿� �ּ� �����߿��� �������� �� ���� ����
        if ( tree.nodeRect.width >= tree.nodeRect.height ) //���ΰ� �� ����� ��쿡�� �� ��� ������ �� ���̸�, �� ��쿡�� ���� ���̴� ������ �ʴ´�.
        {
            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, split, tree.nodeRect.height)); 
            //��ġ�� ���� �ϴ� �����̹Ƿ� ������ ������, ���� ���̴� ������ ���� �������� �־��ش�.
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x + split, tree.nodeRect.y, tree.nodeRect.width - split, tree.nodeRect.height));
            //��ġ�� ���� �ϴܿ��� ���������� ���� ���̸�ŭ �̵��� ��ġ�̸�, ���� ���̴� ���� ���α��̿��� ���� ���� ���ΰ��� �� ������ �κ��� �ȴ�.
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
    public RectInt nodeRect; //�и��� ������ rect����
    public Node( RectInt rect )
    {
        this.nodeRect = rect;
    }
}
