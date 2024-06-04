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
        // �� ���� ũ��, ����, �ε����� ������ ����Ʈ ����
        List<(float size, int index)> tmpRooms = new List<(float size, int index)>();

        for ( int i = 0; i < rooms.Count; i++ )
        {
            rooms [i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            rooms [i].GetComponent<BoxCollider2D>().isTrigger = true;
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
