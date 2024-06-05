using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DungeonShapeTester : DungeonGenerator
{

    [SerializeField] List<Room> roomShapes;
    [SerializeField] List<int> genCounts;

    [SerializeField] Dictionary<Room, int> roomDic;

    protected override IEnumerator SpreadRoomRoutine()
    {
        base.rooms = new List<Room>();
        for ( int i = 0; i < roomShapes.Count; i++ )
        {
            for ( int j = 0; j < genCounts [i]; j++ )
            {
                base.rooms.Add(roomShapes [i]);
            }
        }
        Shuffle(base.rooms);

        List<Room> rooms = new List<Room>();    
        foreach ( Room room in base.rooms )
        {
            Vector3 position = GetRandomPointInCircle(randomPoint);
            Quaternion rotation;

            if ( room.type == 1 )
            {
                int randomAngle = Random.Range(0, 4) * 90; // 0, 90, 180, 270 
                rotation = Quaternion.Euler(0, 0, randomAngle);
            }
            else
            {
                rotation = Quaternion.identity;
            }

            rooms.Add(Instantiate(room, position, rotation, gridObject.transform));
            yield return new WaitForSeconds(0.01f);
        }

        Debug.Log("Wait for Physics Calculate");
        foreach ( var room in rooms )
        {
            room.ActivateRigidBody(waitTime);
        }
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Wait Done...");
        Destroy(base.rooms [0].gameObject);
        base.rooms.Remove(base.rooms [0]);
        yield return null;
    }


    private void Shuffle<T>( List<T> list )
    {
        for ( int i = 0; i < list.Count; i++ )
        {
            T temp = list [i];
            int randomIndex = Random.Range(i, list.Count);
            list [i] = list [randomIndex];
            list [randomIndex] = temp;
        }
    }

}

