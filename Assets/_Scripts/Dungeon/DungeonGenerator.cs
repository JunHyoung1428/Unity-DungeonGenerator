using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using UnityEngine.UI;
using System.Linq;
using TMPro;

/// <summary>
/// Create Dungeon with Delaunay Triangulation(DT) + Minimum Spanning Tree(MST)
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("UI Componets")]
    [SerializeField] Button buttonGen;
    [SerializeField] Button buttonClear;
    [SerializeField] Slider counterSlider;
    [SerializeField] TextMeshProUGUI text;


    [Header("Prefabs")]
    [SerializeField]  protected GameObject gridObject; // tilemap Grid
    [SerializeField]  Room roomPrefab; // room

    [Header("Room Settings")]
    [SerializeField] int roomLayer;
    [SerializeField]  protected int generateRoomCnt;
    [SerializeField]  protected int selectRoomCnt;

    [Space(10)]
    [SerializeField] protected int minRoomSize;
    [SerializeField] protected int maxRoomSize;

    [Space(10)]
    [SerializeField] protected int smallMinRoomSize;
    [SerializeField] protected int smallMaxRoomSize;
    [SerializeField] protected int overlapOffset;

    //for define pixel Size
    const int PIXEL = 1;
    protected List<Room> rooms;

    private HashSet<Vertex> points = new HashSet<Vertex>();
    private List<(int index, Vector2 pos)> selectedRooms = new List<(int, Vector2)>();
    private HashSet<GameObject> lines = new HashSet<GameObject>();

    private List<Edge> hallwayEdges;

    [SerializeField] int [,] map; // ��

    void Awake()
    {
        buttonGen.onClick.AddListener(GenerateDungeon);
        buttonClear.onClick.AddListener(ClearDungeons);
        counterSlider.onValueChanged.AddListener(counterChanged);
        counterChanged(generateRoomCnt);
    }

    void counterChanged(float value )
    {
        generateRoomCnt = (int)value;
        text.text = generateRoomCnt.ToString();
    }

    public void ClearDungeons()
    {
        foreach ( Transform child in gridObject.transform )
        {
            Destroy(child.gameObject);
        }
    }

    public void GenerateDungeon()
    {
        // ���� �������� �� �� ��Ʈ����, �ڷ�ƾ
        StartCoroutine(SpreadRoomRoutine());

        // �� �� Main ��(���� ũ�� �̻���) ����
        FindMainRooms();

        // �� ������ 2���� �迭�� �߰�
        GenerateMapArray();

        // DT + MST �̿��ؼ� �� ����
        ConnectRooms();
    }

    [Header("SpreadRoom Setting")]
    [SerializeField] protected float waitTime = 3.0f;
    [SerializeField] protected int randomPoint = 5;
    protected virtual IEnumerator SpreadRoomRoutine()
    {
        rooms = new List<Room> ();
        for(int i = 0; i < generateRoomCnt; i++)
        {
            rooms.Add(Instantiate(roomPrefab, GetRandomPointInCircle(randomPoint), Quaternion.identity, gridObject.transform));
            if ( i > selectRoomCnt )
            {
                rooms [i].transform.localScale = GetRandomScale(smallMinRoomSize, smallMaxRoomSize);
                rooms [i].SetSize();
            }
            else
            {
                rooms [i].transform.localScale = GetRandomScale(minRoomSize, maxRoomSize);
                rooms [i].SetSize();
            }
            yield return new WaitForSeconds(0.01f);
        }

        Debug.Log("Wait for Physics Calculate");
        foreach( var room  in rooms )
        {
            room.ActivateRigidBody(waitTime);
        }
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Wait Done...");
        Destroy(rooms [0].gameObject);
        rooms.Remove(rooms [0]);
        yield return null;
    }

    /// <summary>
    /// Test Method
    /// </summary>
    /// <returns></returns>
    IEnumerator SpreadRoomWithLayerRoutine()
    {
        rooms = new List<Room>();

        for(int i= 1; i<= roomLayer; i++)
        {
            for ( int j = 0; j < generateRoomCnt; j++ )
            {
                rooms.Add(Instantiate(roomPrefab, GetRandomPointInCircle(randomPoint), Quaternion.identity, gridObject.transform));
                if ( j > selectRoomCnt )
                {
                    rooms [j].transform.localScale = GetRandomScale(smallMinRoomSize, smallMaxRoomSize);
                }
                else
                {
                    rooms [j].transform.localScale = GetRandomScale(minRoomSize, maxRoomSize);
                }
                rooms [j].SetSize();
                rooms [j].name = $"Room Layer{i}";
                yield return new WaitForSeconds(0.02f);
            }

            Debug.Log("Wait for Physics Calculate");
            foreach ( var room in rooms )
            {
                room.ActivateRigidBody(waitTime);
            }
            yield return new WaitForSeconds(waitTime);
            Debug.Log("Wait Done...");
        }
        yield return null;
    }

    void FindMainRooms()
    {
        // �� ���� ũ��, ����, �ε����� ������ ����Ʈ ����
        List<(float size, int index)> tmpRooms = new List<(float size, int index)>();

        for ( int i = 0; i < rooms.Count; i++ )
        {
            rooms [i].Rb.bodyType = RigidbodyType2D.Kinematic;
           // rooms [i].GetComponent<BoxCollider2D>().isTrigger = true;
            rooms [i].transform.position = new Vector3(RoundPos(rooms [i].transform.position.x, PIXEL), RoundPos(rooms [i].transform.position.y, PIXEL), 1);


            Vector3 scale = rooms [i].transform.localScale;
            float size = scale.x * scale.y; // ���� ũ��(����) ���
            float ratio = scale.x / scale.y; // ���� ���� ���
            if ( ratio > 2f || ratio < 0.5f ) continue; // 1:2 �Ǵ� 2:1 ������ �ʰ��ϴ� ��� �ǳʶٱ�
            tmpRooms.Add((size, i));
        }

        // ���� ũ�⿡ ���� ������������ ����
        var sortedRooms = tmpRooms.OrderByDescending(room => room.size).ToList();

        // ��� ���� �ϴ� ��Ȱ��ȭ
        foreach ( var room in rooms )
        {
            room.gameObject.SetActive(false);
        }

        // ���� ������ �����ϴ� �� ���� �� ó��
        int count = 0;
        selectedRooms = new List<(int, Vector2)>();
        foreach ( var roomInfo in sortedRooms )
        {
            if ( count >= selectRoomCnt ) break; // ���� �� ����
            GameObject room = rooms [roomInfo.index].gameObject;
            SpriteRenderer renderer = room.GetComponent<SpriteRenderer>();
            if ( renderer != null )
            {
                renderer.color = Color.red;
            }
            room.SetActive(true);
            points.Add(new Delaunay.Vertex(( int ) room.transform.position.x, ( int ) room.transform.position.y)); // points ����Ʈ�� �߰�
            selectedRooms.Add((roomInfo.index, new Vector2(( int ) room.transform.position.x, ( int ) room.transform.position.y)));
            count++;
        }

    }

    private void GenerateMapArray()
    {
        // �迭 ũ�� ������ ���� �ּ�/�ִ� ��ǥ �ʱ�ȭ
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        // �ּ�/�ִ� ��ǥ Ž��
        foreach ( var room in rooms )
        {
            Vector3 pos = room.transform.position;
            Vector3 scale = room.transform.localScale;

            minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x - scale.x));
            minY = Mathf.Min(minY, Mathf.FloorToInt(pos.y - scale.y));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(pos.x + scale.x));
            maxY = Mathf.Max(maxY, Mathf.CeilToInt(pos.y + scale.y));
        }

        // �迭 ũ�� ��� �� �ʱ�ȭ
        int width = maxX - minX;
        int height = maxY - minY;
        map = new int [height, width];

        for ( int i = 0; i < height; i++ )
            for ( int j = 0; j < width; j++ ) map [i, j] = -1;

        // �迭�� GameObject ����
        for ( int i = 0; i < rooms.Count; i++ )
        {
            Vector3 pos = rooms [i].transform.position;
            Vector3 scale = rooms [i].transform.localScale;

            // GameObject�� ũ��� ��ġ�� ����Ͽ� �迭�� ����
            for ( int x = ( int ) -scale.x / 2; x < scale.x / 2; x++ )
            {
                for ( int y = ( int ) -scale.y / 2; y < scale.y / 2; y++ )
                {
                    int mapX = Mathf.FloorToInt(pos.x - minX + x);
                    int mapY = Mathf.FloorToInt(pos.y - minY + y);
                    map [mapY, mapX] = i;
                }
            }
        }
    }

    void ConnectRooms()
    {
        var triangles = DelaunayTriangulation.Triangulate(points);

        var graph = new HashSet<Delaunay.Edge>();
        foreach ( var triangle in triangles )
            graph.UnionWith(triangle.edges);

        hallwayEdges = Kruskal.MST(graph);
    }


    /// <summary>
    /// �������� �Ŀ� ��ġ�� ������ ��ȯ�ϱ� ���� ���
    /// </summary>
    /// <param name="n">��ȯ�� ��</param>
    /// <param name="m">�׸��� ���� (2�̸� return���� ¦���� ����)</param>
    /// <returns></returns>
    private int RoundPos( float n, int m )
    {
        return Mathf.FloorToInt(( ( n + m - 1 ) / m )) * m;
    }

    protected Vector3 GetRandomPointInCircle( int rad )
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0;

        if ( u > 1 ) r = 2 - u;
        else r = u;

        return new Vector3(RoundPos(rad * r * Mathf.Cos(t), PIXEL), RoundPos(rad * r * Mathf.Sin(t), PIXEL), 0);
    }
    protected Vector3 GetRandomScale( int minS, int maxS )
    {
        int x = Random.Range(minS, maxS) * 2;
        int y = Random.Range(minS, maxS) * 2;


        return new Vector3(x, y, 1);
    }
}
