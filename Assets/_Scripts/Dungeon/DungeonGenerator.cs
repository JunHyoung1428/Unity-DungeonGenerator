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
    [SerializeField]  LineRenderer lineRenderer; //for Visualize 


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

    [SerializeField] int [,] map; // 맵

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
    }

    public void GenerateDungeon()
    {
        // 물리 연산으로 각 방 퍼트리기, 코루틴
        StartCoroutine(SpreadRoomRoutine());

        // 방 중 Main 룸(일정 크기 이상의) 고르기
        //FindMainRooms();

        // 룸 정보를 2차원 배열에 추가
        //GenerateMapArray();

        // DT + MST 이용해서 룸 연결 (그래프)
       // ConnectRooms();

        // 복도 그려서 연결

        // 얻어진 데이터를 Tilemap에 다시 그려냄 -> 데이터는 DungeonManager에 넘겨줘서 이후 작업들은 거기서 제어할것
    }

    [Header("SpreadRoom Setting")]
    [SerializeField] protected float waitTime = 3.0f;
    [SerializeField] protected int randomPoint = 5;

    /// <summary>
    /// Step.1 RandomScale의 공간 생성후,  Unity에서 Rigidbody 의 충돌 연산의 무작위성을 이용해 공간 배치
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
    /// 무작위로 나뉘어진 공간에서, 일정 비율&크기 이상의 공간을 "방"으로 선정
    /// </summary>
    IEnumerator FindMainRooms()
    {
        Debug.Log("Start Find Main Rooms");
        // 각 방의 크기, 비율, 인덱스를 저장할 리스트 생성
        List<(float size, int index)> tmpRooms = new List<(float size, int index)>();

        for ( int i = 0; i < rooms.Count; i++ )
        {
            // rooms [i].GetComponent<BoxCollider2D>().isTrigger = true;
            rooms [i].transform.position = new Vector3(RoundPos(rooms [i].transform.position.x, PIXEL), RoundPos(rooms [i].transform.position.y, PIXEL), 1); //정수 단위로 변환
            Debug.Log($"Translate to int : {rooms [i].transform.position}");

            Vector3 scale = rooms [i].transform.localScale;
            float size = scale.x * scale.y; // 방의 크기(넓이) 계산
            float ratio = scale.x / scale.y; // 방의 비율 계산
            Debug.Log($"Calculate Size & ratiod :  {size} ,  {ratio}");

            if ( ratio > 2f || ratio < 0.5f ) continue; // 1:2 또는 2:1 비율을 초과하는 경우 건너뛰기, (제외하는 과정)


            tmpRooms.Add((size, i));
        }

        // 방의 크기에 따라 내림차순으로 정렬
        var sortedRooms = tmpRooms.OrderByDescending(room => room.size).ToList();

        // 모든 방을 일단 비활성화
        foreach ( var room in rooms )
        {
            room.gameObject.SetActive(false);
        }

        // 비율 조건을 만족하는 방 선택 및 처리
        int count = 0;
        selectedRooms = new List<(int, Vector2)>();
        foreach ( var roomInfo in sortedRooms )
        {
            if ( count >= selectRoomCnt ) break; // 선택 후 종료
            GameObject room = rooms [roomInfo.index].gameObject;
            // SpriteRenderer renderer = room.GetComponent<SpriteRenderer>(); <- TilemapReneder 로 교체?
           /*if ( renderer != null )
            {
               renderer.color = Color.red;
            }*/
            room.SetActive(true);
            yield return new WaitForSeconds(0.01f);
            points.Add(new Vertex(( int ) room.transform.position.x, ( int ) room.transform.position.y)); // points 리스트에 추가
            selectedRooms.Add((roomInfo.index, new Vector2(( int ) room.transform.position.x, ( int ) room.transform.position.y)));
            count++;
        }

        yield return null;
        GenerateMapArray();
    }



    private void GenerateMapArray()
    {
        // 배열 크기 결정을 위한 최소/최대 좌표 초기화
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        // 최소/최대 좌표 탐색
        foreach ( var room in rooms )
        {
            Vector3 pos = room.transform.position;
            Vector3 scale = room.transform.localScale;

            minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x - scale.x));
            minY = Mathf.Min(minY, Mathf.FloorToInt(pos.y - scale.y));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(pos.x + scale.x));
            maxY = Mathf.Max(maxY, Mathf.CeilToInt(pos.y + scale.y));
        }

        // 배열 크기 계산 및 초기화
        int width = maxX - minX;
        int height = maxY - minY;
        map = new int [height, width];

        for ( int i = 0; i < height; i++ )
            for ( int j = 0; j < width; j++ ) map [i, j] = -1;

        // 배열에 GameObject 저장
        for ( int i = 0; i < rooms.Count; i++ )
        {
            Vector3 pos = rooms [i].transform.position;
            Vector3 scale = rooms [i].transform.localScale;

            // GameObject의 크기와 위치를 고려하여 배열에 저장
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
            DrawLine(edge);
        }

        StartCoroutine(GenerateHall(hallwayEdges));
        yield return null;
    }


    IEnumerator GenerateHall(IEnumerable <Edge> tree)
    {
        


        yield return null;
    }


  

    void DrawLine(Edge edge)
    {
        LineRenderer line = Instantiate(lineRenderer,transform);
        Vector3 startPos = new Vector3(edge.a.x, edge.a.y, 0);
        Vector3 endPos = new Vector3(edge.b.x, edge.b.y, 0);

        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }

    /// <summary>
    /// 물리연산 후에 위치를 정수로 변환하기 위해 사용
    /// </summary>
    /// <param name="n">변환할 값</param>
    /// <param name="m">그리드 간격 (2이면 return값이 짝수만 나옴)</param>
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
