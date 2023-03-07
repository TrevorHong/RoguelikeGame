using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;


    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        floor = CreateSimpleRooms(roomList);

        tileMapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tileMapVisualizer);
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        /**
         * https://youtu.be/pWZg1oChtnc?list=PLcRSafycjWFenI87z7uZHFv6cUG2Tzu9v&t=586
         * Interesting comment he made about decorating the rooms procedurally
         */
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }

        return floor;
    }
}
