using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar {



public enum VoronoiCellStateType {
    Idle,
    OnSearching,
    Processed,
    IsPath
}



// cell 存在多种状态, 统一记录每种状态的信息, 比如颜色
public class VoronoiCellState
{
    
    public static VoronoiCellState idleState = new VoronoiCellState(
        new Color( 1f, 1f, 1f ),
        new Color( 0f, 0f, 0f )
    );

    public static VoronoiCellState onSearchingState = new VoronoiCellState(
        new Color( 0.1f, 0.7f, 0.1f ), // 中绿
        new Color( 0f, 0f, 0f )
    );

    public static VoronoiCellState processedState = new VoronoiCellState(
        new Color( 0.1f, 0.1f, 0.6f ), // 中蓝
        new Color( 1f, 1f, 1f )
    );

    public static VoronoiCellState isPathState = new VoronoiCellState(
        new Color( 0.8f, 0.1f, 0.1f ), // 红
        new Color( 1f, 1f, 1f )
    );

    // =========================:
    public Color backColor;
    public Color textColor;

    public VoronoiCellState( Color backColor_, Color textColor_ )
    {
        backColor = backColor_;
        textColor = textColor_;
    }
}


}
