using System;
using UnityEngine;

[Serializable]
public class HexagonCell
{
    public int id;

    public Vector3 position;

    public HexagonCellConfig config;

    public HexagonCell(int id, HexagonCellConfig config)
    {
        this.id = id;
        this.config = config;
    }
}
