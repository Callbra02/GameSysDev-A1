using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino { I, O, T, J, L, S, Z, C}

[Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public Vector2Int[] cells;
    public Tile tile;
}
