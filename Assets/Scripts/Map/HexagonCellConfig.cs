/// <summary>
using System;

[Serializable]
/// 地块的基本配置
/// </summary>
public struct HexagonCellConfig
{
    public Enum.HexagonType type;

    public bool isReachable;
}
