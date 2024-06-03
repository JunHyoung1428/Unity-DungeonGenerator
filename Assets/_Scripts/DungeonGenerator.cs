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

    [Header("Genrator Settings")]
    [SerializeField] Vector2Int mapSize;
    [SerializeField] int maximumDepth;
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
        GenerateRoom(root, 0);

        if(showLoad)
            GenerateLoad(root, 0);

        isGened = true;
    }

    void ClearDungeon()
    {
        foreach ( Transform child in transform )
        {
            Destroy(child.gameObject);
        }
    }

    [Space(10)]
    [Header("Line")]
    [SerializeField] LineRenderer map;
    [SerializeField] LineRenderer line;
    [SerializeField] LineRenderer room;


    [Space(10)]
    [SerializeField] bool showSpace;
    [SerializeField] bool showLoad;

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
        if ( n == maximumDepth )
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
            if(showSpace)
                DrawLine(new Vector2(tree.nodeRect.x + split, tree.nodeRect.y), new Vector2(tree.nodeRect.x + split, tree.nodeRect.y + tree.nodeRect.height));
        }
        else
        {
            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, tree.nodeRect.width, split));
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y + split, tree.nodeRect.width, tree.nodeRect.height - split));
            if(showSpace)
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


    RectInt GenerateRoom(Node tree, int n)
    {
        RectInt rect;
        if ( n == maximumDepth ) //�ش� ��尡 ��������� �� ����
        {
            rect = tree.nodeRect;
            int width = Random.Range(rect.width / 2, rect.width - 1);
            //���� ���� �ּ� ũ��� ����� ���α����� ����, �ִ� ũ��� ���α��̺��� 1 �۰� ������ �� �� ���� ���� ������ ���� �����ش�.
            int height = Random.Range(rect.height / 2, rect.height - 1);
            //���̵� ���� ����.
            int x = rect.x + Random.Range(1, rect.width - width);
            //���� x��ǥ�̴�. ���� 0�� �ȴٸ� �پ� �ִ� ��� �������� ������,�ּڰ��� 1�� ���ְ�, �ִ��� ���� ����� ���ο��� ���� ���α��̸� �� �� ���̴�.
            int y = rect.y + Random.Range(1, rect.height - height);
            //y��ǥ�� ���� ����.
            rect = new RectInt(x, y, width, height);
            DrawRoom(rect);
        }
        else
        {
            tree.leftNode.roomRect = GenerateRoom(tree.leftNode, n + 1);
            tree.rightNode.roomRect = GenerateRoom(tree.rightNode, n + 1);
            rect = tree.leftNode.roomRect;
        }
        return rect;
    }

    void DrawRoom(RectInt rect)
    {
        LineRenderer lineRenderer = Instantiate(room,transform);
        lineRenderer.SetPosition(0, new Vector2(rect.x, rect.y) - mapSize / 2); //���� �ϴ�
        lineRenderer.SetPosition(1, new Vector2(rect.x + rect.width, rect.y) - mapSize / 2); //���� �ϴ�
        lineRenderer.SetPosition(2, new Vector2(rect.x + rect.width, rect.y + rect.height) - mapSize / 2);//���� ���
        lineRenderer.SetPosition(3, new Vector2(rect.x, rect.y + rect.height) - mapSize / 2); //���� ��
    }

    private void GenerateLoad( Node tree, int n )
    {
        if ( n == maximumDepth ) 
            return;
        Vector2Int leftNodeCenter = tree.leftNode.center;
        Vector2Int rightNodeCenter = tree.rightNode.center;

        DrawLine(new Vector2(leftNodeCenter.x, leftNodeCenter.y), new Vector2(rightNodeCenter.x, leftNodeCenter.y));
        //���� ������ leftnode�� ���缭 ���� ������ ��������.
        DrawLine(new Vector2(rightNodeCenter.x, leftNodeCenter.y), new Vector2(rightNodeCenter.x, rightNodeCenter.y));
        //���� ������ rightnode�� ���缭 ���� ������ ��������.
        GenerateLoad(tree.leftNode, n + 1); //�ڽ� ���鵵 Ž��
        GenerateLoad(tree.rightNode, n + 1);
    }

}

public class Node
{
    public Node leftNode;
    public Node rightNode;
    public Node parNode;

    public RectInt nodeRect; //�и��� ������ rect����
    public RectInt roomRect; // '' room ����

    public Vector2Int center
    {
        get
        {
            return new Vector2Int(roomRect.x + roomRect.width / 2, roomRect.y + roomRect.height / 2);
        }
    } //�� ��� ��

    public Node( RectInt rect )
    {
        this.nodeRect = rect;
    }
}
