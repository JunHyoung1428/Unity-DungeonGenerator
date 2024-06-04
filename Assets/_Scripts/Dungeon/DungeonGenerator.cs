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
        // ���� �������� �� �� ��Ʈ����, �ڷ�ƾ
        StartCoroutine(SpreadRoomRoutine());


        // �� �� Main ��(���� ũ�� �̻���) ����

        // �� ������ 2���� �迭�� �߰�

        // DT + MST �̿��ؼ� �� ����
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
    /// �������� �Ŀ� ��ġ�� ������ ��ȯ�ϱ� ���� ���
    /// </summary>
    /// <param name="n">��ȯ�� ��</param>
    /// <param name="m">�׸��� ���� (2�̸� return���� ¦���� ����)</param>
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
