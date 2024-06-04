using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Create Dungeon with Delaunay Triangulation(DT) + Minimum Spanning Tree(MST)
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("UI Componets")]
    [SerializeField] Button buttonGen;


    [Header("Prefabs")]
    [SerializeField]  GameObject gridObject; // tilemap Grid
    [SerializeField]  Room roomPrefab; // room

    [Header("Room Settings")]
    [SerializeField]  int generateRoomCnt;
    [SerializeField]  int selectRoomCnt;

    [Space(10)]
    [SerializeField]  int minRoomSize;
    [SerializeField]  int maxRoomSize;

    [Space(10)]
    [SerializeField]  int smallMinRoomSize;
    [SerializeField]  int smallMaxRoomSize;
    [SerializeField]  int overlapOffset;

    //for define pixel Size
    const int PIXEL = 1;
    List<Room> rooms;

    private HashSet<Vertex> points = new HashSet<Vertex>();
    private List<(int index, Vector2 pos)> selectedRooms = new List<(int, Vector2)>();

    void Awake()
    {
        buttonGen.onClick.AddListener(GenerateDungeon);
    }


    public void GenerateDungeon()
    {
        // 물리 연산으로 각 방 퍼트리기, 코루틴
        StartCoroutine(SpreadRoomRoutine());

        // 방 중 Main 룸(일정 크기 이상의) 고르기

        // 룸 정보를 2차원 배열에 추가

        // DT + MST 이용해서 룸 연결
    }

    [Header("SpreadRoom Setting")]
    [SerializeField] float waitTime = 3.0f;
    [SerializeField] int randomPoint = 5;
     IEnumerator SpreadRoomRoutine()
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
            yield return new WaitForSeconds(0.05f);
        }

        Debug.Log("Wait for Physics Calculate");
        foreach( var room  in rooms )
        {
            room.ActivateRigidBody(waitTime);
        }
        Debug.Log("Wait Done...");

        yield return null;
    }
   
    void FindMainRooms()
    {
        // 각 방의 크기, 비율, 인덱스를 저장할 리스트 생성
        List<(float size, int index)> tmpRooms = new List<(float size, int index)>();

        for ( int i = 0; i < rooms.Count; i++ )
        {
            rooms [i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            rooms [i].GetComponent<BoxCollider2D>().isTrigger = true;
            rooms [i].transform.position = new Vector3(RoundPos(rooms [i].transform.position.x, PIXEL), RoundPos(rooms [i].transform.position.y, PIXEL), 1);


            Vector3 scale = rooms [i].transform.localScale;
            float size = scale.x * scale.y; // 방의 크기(넓이) 계산
            float ratio = scale.x / scale.y; // 방의 비율 계산
            if ( ratio > 2f || ratio < 0.5f ) continue; // 1:2 또는 2:1 비율을 초과하는 경우 건너뛰기
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
            SpriteRenderer renderer = room.GetComponent<SpriteRenderer>();
            if ( renderer != null )
            {
                renderer.color = Color.red;
            }
            room.SetActive(true);
            points.Add(new Delaunay.Vertex(( int ) room.transform.position.x, ( int ) room.transform.position.y)); // points 리스트에 추가
            selectedRooms.Add((roomInfo.index, new Vector2(( int ) room.transform.position.x, ( int ) room.transform.position.y)));
            count++;
        }

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

    private Vector3 GetRandomPointInCircle( int rad )
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0;

        if ( u > 1 ) r = 2 - u;
        else r = u;

        return new Vector3(RoundPos(rad * r * Mathf.Cos(t), PIXEL), RoundPos(rad * r * Mathf.Sin(t), PIXEL), 0);
    }
    private Vector3 GetRandomScale( int minS, int maxS )
    {
        int x = Random.Range(minS, maxS) * 2;
        int y = Random.Range(minS, maxS) * 2;


        return new Vector3(x, y, 1);
    }
}
