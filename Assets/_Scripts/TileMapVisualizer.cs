using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTileMap, wallTileMap;
    [SerializeField]
    private TileBase floorTile, wallTop; // can become an array to select from random tiles

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTileMap, floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tileMap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tileMap, tile, position);
        }
    }

    private void PaintSingleTile(Tilemap tileMap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tileMap.WorldToCell((Vector3Int)position);
        tileMap.SetTile(tilePosition, tile);
    }

    internal void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingleTile(wallTileMap, wallTop, position);
    }

    public void Clear()
    {
        floorTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();
    }
}
