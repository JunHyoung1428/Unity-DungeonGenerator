using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEditor;

/// <summary>
/// Create Dungeon with Delaunay Triangulation(DT) + Minimum Spanning Tree(MST)
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("UI Componets")]
    [SerializeField] Button buttonGen;
    [SerializeField] Button buttonClear;
    [SerializeField] Slider counterSlider;
    [SerializeField] Slider loadRatioSlider;
    [SerializeField] TextMeshProUGUI roomCounttext;
    [SerializeField] TextMeshProUGUI loadRatioText;


    [Header("Prefabs")]
    [SerializeField]  protected GameObject gridObject; // tilemap Grid
    [SerializeField]  Room roomPrefab; // room
    [SerializeField]  LineRenderer graphRenderer; //for Visualize 

    [SerializeField]  GameObject doorPrefab; // for test
    [SerializeField]  List<LineRenderer> corridors;

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
    [SerializeField] protected int overlapOffset;     // ���� ������ ������ �� offSet

    [Space(10)]
    [Range(0,100)]
    [SerializeField] int addingLoadRatio =30;

    //for define pixel Size
    const int PIXEL = 1;
    protected List<Room> rooms;

    private HashSet<Vertex> points = new HashSet<Vertex>();
    private List<(int index, Vector2 pos)> selectedRooms = new List<(int, Vector2)>();
    private HashSet<GameObject> lines = new HashSet<GameObject>();

    private List<Edge> hallwayEdges;

    [SerializeField] int [,] map; // ��
    int minX = int.MaxValue, minY = int.MaxValue;
    int maxX = int.MinValue, maxY = int.MinValue;

    void Awake()
    {
        buttonGen.onClick.AddListener(GenerateDungeon);
        buttonClear.onClick.AddListener(ClearDungeons);
        counterSlider.onValueChanged.AddListener(counterChanged);
        loadRatioSlider.onValueChanged.AddListener(ratioChanged);
        counterChanged(generateRoomCnt);
    }

    void counterChanged(float value )
    {
        generateRoomCnt = (int)value;
        selectRoomCnt = (int) (value * 0.8f);
        roomCounttext.text = generateRoomCnt.ToString();
    }

    void ratioChanged(float value )
    {
        addingLoadRatio = (int)value;
        loadRatioText.text= addingLoadRatio.ToString();

        foreach ( Transform child in transform )
        {
            Destroy(child.gameObject);
        }
        StartCoroutine(ConnectRooms());
    }

    public void ClearDungeons()
    {
        foreach ( Transform child in gridObject.transform )
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in transform )
        {
            Destroy(child.gameObject);
        }

        rooms.Clear();
        points.Clear();
        selectedRooms.Clear();
    }

    public void GenerateDungeon()
    {
        // ���� �������� �� �� ��Ʈ����, �ڷ�ƾ
        StartCoroutine(SpreadRoomRoutine());

        // �� �� Main ��(���� ũ�� �̻���) ����
        //FindMainRooms(); V

        // �� ������ 2���� �迭�� �߰�
        //GenerateMapArray(); V

        // DT + MST �̿��ؼ� �� ���� (�׷���)
        // ConnectRooms(); V

        // ���� �׷��� ���� 

        // �ʹ� �� ������ ���? �߰��� �߰������� �� ���� ...

        // ����� �����͸� Tilemap�� �ٽ� �׷��� -> �����ʹ� DungeonManager�� �Ѱ��༭ ���� �۾����� �ű⼭ �����Ұ�
    }

    [Header("SpreadRoom Setting")]
    [SerializeField] protected float waitTime = 3.0f;
    [SerializeField] protected int randomPoint = 5;

    /// <summary>
    /// Step.1 RandomScale�� ���� ������,  Unity���� Rigidbody �� �浹 ������ ���������� �̿��� ���� ��ġ
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator SpreadRoomRoutine()
    {
        rooms = new List<Room> ();
        for(int i = 0; i < generateRoomCnt; i++)
        {
            rooms.Add(Instantiate(roomPrefab, GetRandomPointInCircle(randomPoint), Quaternion.identity, gridObject.transform));
            if ( i > selectRoomCnt )
            {
                rooms [i].transform.localScale = GetRandomScale(smallMinRoomSize, smallMaxRoomSize);
            }
            else
            {
                rooms [i].transform.localScale = GetRandomScale(minRoomSize, maxRoomSize);
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

        yield return null;
        StartCoroutine(FindMainRooms());
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

    /// <summary>
    /// �������� �������� ��������, ���� ����&ũ�� �̻��� ������ "��"���� ����
    /// </summary>
    IEnumerator FindMainRooms()
    {
        Debug.Log("Start Find Main Rooms");
        // �� ���� ũ��, ����, �ε����� ������ ����Ʈ ����
        List<(float size, int index)> tmpRooms = new List<(float size, int index)>();

        for ( int i = 0; i < rooms.Count; i++ )
        {
            // rooms [i].GetComponent<BoxCollider2D>().isTrigger = true;
            rooms [i].transform.position = new Vector3(RoundPos(rooms [i].transform.position.x, PIXEL), RoundPos(rooms [i].transform.position.y, PIXEL), 1); //���� ������ ��ȯ
            //Debug.Log($"Translate to int : {rooms [i].transform.position}");
            
            Vector3 scale = rooms [i].transform.localScale;
            float size = scale.x * scale.y; // ���� ũ��(����) ���
            float ratio = scale.x / scale.y; // ���� ���� ���
            // Debug.Log($"Calculate Size & ratiod :  {size} ,  {ratio}");

            if ( ratio > 2f || ratio < 0.5f ) continue; // 1:2 �Ǵ� 2:1 ������ �ʰ��ϴ� ��� �ǳʶٱ�, (�����ϴ� ����)


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
            // SpriteRenderer renderer = room.GetComponent<SpriteRenderer>(); <- TilemapReneder �� ��ü?
           /*if ( renderer != null )
            {
               renderer.color = Color.red;
            }*/
            room.SetActive(true);
            yield return new WaitForSeconds(0.01f);
            points.Add(new Vertex(( int ) room.transform.position.x, ( int ) room.transform.position.y)); // points ����Ʈ�� �߰�
            selectedRooms.Add((roomInfo.index, new Vector2(( int ) room.transform.position.x, ( int ) room.transform.position.y)));
            count++;
        }

        yield return null;
        GenerateMapArray();
    }

        

    private void GenerateMapArray()
    {
        // �迭 ũ�� ������ ���� �ּ�/�ִ� ��ǥ �ʱ�ȭ
        minX = int.MaxValue;
        minY = int.MaxValue;
        maxX = int.MinValue;
        maxY = int.MinValue;

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

        Debug.Log($"map Size : {width}, {height}");

        StartCoroutine(ConnectRooms());
    }

    IEnumerator ConnectRooms()
    {
        var triangles = DelaunayTriangulation.Triangulate(points);

        var graph = new HashSet<Edge>();
        foreach ( var triangle in triangles )
            graph.UnionWith(triangle.edges);

        hallwayEdges = Kruskal.MST(graph , addingLoadRatio);
        foreach(var edge in hallwayEdges )
        {
            yield return new WaitForSeconds(0.01f);
            DrawLine(graphRenderer ,edge);
        }

        StartCoroutine(GenerateHallways(hallwayEdges));
        yield return null;
    }

    GameObject corriderParent;

    IEnumerator GenerateHallways(IEnumerable <Edge> tree)
    {
        Debug.Log("Hallways Start");
        // edge ��ȸ�ؼ� �� �� ������ ����, ���� �����ϴ� �޼���

        foreach ( var edge in tree )
        {
            //���������� ���⿡
            GenerateCorrider(edge);
        }

        // ����? �� ������ �߰�?

        yield return null;
    }
    
    // �� ���� ( �ӽ� )
    // ���� �� �߾ӿ� �����Ǵµ�, �� ������ ���� �޾Ƽ� ���� �����Ұ�
    // �Ǵ� ������ ���� �����ϰ� �� �糡�� ���� ����?
    void GenerateDoor(Edge edge)
    {
        Vector3 startPos = new Vector3(edge.a.x, edge.a.y);
        Vector3 endPos = new Vector3(edge.b.x, edge.b.y);

        if(startPos == endPos )
        {
            Instantiate(doorPrefab, startPos, Quaternion.identity, transform);
            return;
        }
        else
        {
            Instantiate(doorPrefab, startPos, Quaternion.identity, transform);
            Instantiate(doorPrefab, endPos, Quaternion.identity, transform);
        }
    }

    // x,y ��ǥ �˻��ؼ� ��������, ��������, �� ����, �� ���� �� Shape ���� �ؼ� ���� ����
    void GenerateCorrider(Edge edge)
    {
        Vertex start = edge.a;
        Vertex end = edge.b;

        Vector2Int size1 = new Vector2Int((int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.x, (int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.y);
        Vector2Int size2 = new Vector2Int((int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.x, (int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.y);
       
        bool isHorizontalOverlap = Mathf.Abs(start.x - end.x) < (( size1.x + size2.x) / 2f - overlapOffset);
        bool isVerticalOverlap = Mathf.Abs(start.y - end.y) < (( size1.y + size2.y) / 2f - overlapOffset);


        if (isVerticalOverlap) // ������ ��� ,offset ��ŭ�� �����ؼ� �����������
        {
            int startY = Mathf.Min(start.y + size1.y / 2, end.y + size2.y / 2) + Mathf.Max(start.y - size1.y / 2, end.y - size2.y / 2);
            startY = startY / 2;
            int startX = Mathf.Min(start.x + size1.x / 2, end.x + size2.x / 2);
            int endX = Mathf.Max(start.x - size1.x / 2, end.x - size2.x / 2);

            Vertex newA = new Vertex(startX, startY);
            Vertex newB = new Vertex(endX, startY);
            Edge newEdge = new Edge(newA, newB);
            DrawLine(corridors [0], newEdge);
            GenerateDoor(newEdge);
        }
        else if ( isHorizontalOverlap ) // ������ ���
        {
            int startX = Mathf.Min(start.x + size1.x / 2, end.x + size2.x / 2) + Mathf.Max(start.x - size1.x / 2, end.x - size2.x / 2);
            startX = startX / 2;
            int startY = Mathf.Min(start.y + size1.y / 2, end.y + size2.y / 2);
            int endY = Mathf.Max(start.y - size1.y / 2, end.y - size2.y / 2);

            Vertex newA = new Vertex(startX, startY);
            Vertex newB = new Vertex(startX, endY);
            Edge newEdge = new Edge(newA, newB);
            DrawLine(corridors [0], newEdge);
            GenerateDoor(newEdge);
        }
        else //offset ����� ��� �� , �� ���̴� ���� ����
        {

            // Vertex�� �߰���
            // int mapCenterX = map.GetLength(0) / 2;
            // int mapCenterY = map.GetLength(1) / 2;
            int mapCenterX = 0;
            int mapCenterY = 0;

            Debug.Log($"Map Center {mapCenterX} , {mapCenterY}");


            // start�� end ������ �߰��� ���
            int midX = (start.x + end.x) / 2;
            int midY = (start.y + end.y) / 2;

            // -> �߰����� ��ü ���� �߽����� ���� ��� ��и����� �ľ�

            bool isRight = midX >= mapCenterX;
            bool isAbove = midY >= mapCenterY;

            if ( isRight && isAbove ) // 1��и�
            {
                int startX = start.x + size1.x / 2;
                int endY = end.y - size2.y / 2;

                Vertex verticalA = new Vertex(startX, start.y);
                Vertex verticalB = new Vertex(startX, endY);
                Edge verticalEdge = new Edge(verticalA, verticalB);
                DrawLine(corridors [1], verticalEdge);

                Vertex horizontalA = new Vertex(startX, endY);
                Vertex horizontalB = new Vertex(end.x - size2.x / 2, endY);
                Edge horizontalEdge = new Edge(horizontalA, horizontalB);
                DrawLine(corridors [1], horizontalEdge);
            }
            else if ( isRight && !isAbove ) // 4��и�
            {
                int startX = start.x + size1.x / 2;
                int endY = end.y + size2.y / 2;

                Vertex verticalA = new Vertex(startX, start.y);
                Vertex verticalB = new Vertex(startX, endY);
                Edge verticalEdge = new Edge(verticalA, verticalB);
                DrawLine(corridors [4], verticalEdge);

                Vertex horizontalA = new Vertex(startX, endY);
                Vertex horizontalB = new Vertex(end.x - size2.x / 2, endY);
                Edge horizontalEdge = new Edge(horizontalA, horizontalB);
                DrawLine(corridors [4], horizontalEdge);
            }
            else if ( !isRight && isAbove ) // 2��и�
            {
                int endX = end.x + size2.x / 2;
                int startY = start.y + size1.y / 2;

                Vertex horizontalA = new Vertex(start.x, end.y - size2.y / 2);
                Vertex horizontalB = new Vertex(endX, end.y - size2.y / 2);
                Edge horizontalEdge = new Edge(horizontalA, horizontalB);
                DrawLine(corridors [2], horizontalEdge);

                Vertex verticalA = new Vertex(start.x, startY);
                Vertex verticalB = new Vertex(start.x, end.y - size2.y / 2);
                Edge verticalEdge = new Edge(verticalA, verticalB);
                DrawLine(corridors [2], verticalEdge);

                GenerateDoor(new Edge(horizontalB, verticalA));
            }
            else if ( !isRight && !isAbove ) // 3��и�
            {
                int endX = end.x + size2.x / 2;
                int startY = start.y - size1.y / 2;

                Vertex horizontalA = new Vertex(start.x, end.y + size2.y / 2);
                Vertex horizontalB = new Vertex(endX, end.y + size2.y / 2);
                Edge horizontalEdge = new Edge(horizontalA, horizontalB);
                DrawLine(corridors [3], horizontalEdge);


                Vertex verticalA = new Vertex(start.x, startY);
                Vertex verticalB = new Vertex(start.x, end.y + size2.y / 2);
                Edge verticalEdge = new Edge(verticalA, verticalB);
                DrawLine(corridors [3], verticalEdge);

                GenerateDoor(new Edge(horizontalB, verticalA));
            }

            // Case : 2, 3 ��и�
            //  step 1: ���� �׸���
            //  step 2: ���� �׸���

            // Case : 1, 4 ��и�
            //  step 1: ���� �׸���
            //  step 2: ���� �׸���
        }

    }

  

    void DrawLine(LineRenderer line , Edge edge)
    {
        LineRenderer newLine = Instantiate(line,transform);
        Vector3 startPos = new Vector3(edge.a.x, edge.a.y, 0);
        Vector3 endPos = new Vector3(edge.b.x, edge.b.y, 0);

        newLine.SetPosition(0, startPos);
        newLine.SetPosition(1, endPos);
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
