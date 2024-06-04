using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using UnityEngine.UI;

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
                rooms [i].SetSize(GetRandomScale(smallMinRoomSize, smallMaxRoomSize));
            }
            else
            {
                rooms [i].SetSize(GetRandomScale(minRoomSize, maxRoomSize));
            }
            rooms [i].FillTile();
            yield return null;
        }

        Debug.Log("Wait for Physics Calculate");
        foreach( var room  in rooms )
        {
            room.ActivateRigidBody(waitTime);
        }
        Debug.Log("Wait Done...");

        yield return null;
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
