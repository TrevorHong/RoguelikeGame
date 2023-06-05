using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

/// <summary>
/// Class that combines procedural generation algorithms, room creation,
/// and corrdior connection logic to generate a dungeon layout based on
/// predefined parameters
/// </summary>
public class RoomsFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;

    // value to determine whether dungeon should have rectangular rooms or not
    [SerializeField]
    private bool randomWalkRooms = false;

    private DungeonData dungeonData;

    [SerializeField]
    private bool generateInEditor = false;

    public UnityEvent OnFinishedRoomGeneration;

    private void Awake()
    {
        // generator game object must have dungeonData script
        dungeonData = FindObjectOfType<DungeonData>();
        if (dungeonData == null)
            dungeonData = gameObject.AddComponent<DungeonData>();
    }

    protected override void RunProceduralGeneration()
    {
        if (!generateInEditor)
        {
            Debug.Log("RWRWERWE");
            dungeonData.Reset();
        }
        CreateRooms();
        OnFinishedRoomGeneration?.Invoke();
    }

    /// <summary>
    /// Generates and creates rooms within the dungeon.
    /// </summary>
    private void CreateRooms()
    {
        // generate room boundaries using Binary Space Partitioning algorithm
        List<BoundsInt> roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        // create rooms randomly or in a rectangular shape
        if (randomWalkRooms)
            floor = CreateRoomsRandomly(roomsList);
        else
            floor = CreateRectangularRooms(roomsList);

        List<Vector2Int> roomCenters = new List<Vector2Int>();

        // iterate through each rooms and add their centers to roomCenters List
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));

            Debug.Log(room);
        }

        // connect rooms with corridors
        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tileMapVisualizer.PaintFloorTiles(floor); // place floor tile sprites
        //tileMapVisualizer.paintCorridorFloorTiles(corridors); // place test corridor floor tile sprites
        WallGenerator.CreateWalls(floor, tileMapVisualizer); // generate walls with colliders
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roomCenters"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }

        return corridors;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentRoomCenter"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        var thickness = 1;
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
            for (int i = 0; i < thickness; i++)
            {
                corridor.Add(position + Vector2Int.right * i);
                corridor.Add(position + Vector2Int.left * i);
            }

        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
            for (int i = 0; i < thickness; i++)
            {
                corridor.Add(position + Vector2Int.up * i);
                corridor.Add(position + Vector2Int.down * i);
            }
        }
        if (!generateInEditor)
            dungeonData.Path.UnionWith(corridor);

        return corridor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentRoomCenter"></param>
    /// <param name="roomCenters"></param>
    /// <returns></returns>
    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }

        return closest;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roomsList"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset)
                    && position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }

        return floor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roomsList"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> CreateRectangularRooms(List<BoundsInt> roomsList)
    {
        /*
         * https://youtu.be/pWZg1oChtnc?list=PLcRSafycjWFenI87z7uZHFv6cUG2Tzu9v&t=586
         * Interesting comment he made about decorating the rooms procedurally
         * - which I did eheheheh
         */
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            // to be added to 
            HashSet<Vector2Int> tempRoomTiles = new();

            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                    tempRoomTiles.Add(position);
                }
            }

            if (!generateInEditor)
                dungeonData.Rooms.Add(new Room(room.center, tempRoomTiles));
        }

        return floor;
    }
}
