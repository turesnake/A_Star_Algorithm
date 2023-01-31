using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar {



public enum VoronoiCellStateType {
    Idle,
    Src, // 起点
    OnSearching,
    Processed,
    IsPath
}



// cell 存在多种状态, 统一记录每种状态的信息, 比如颜色
public class VoronoiCellState
{
    
    public static Dictionary<VoronoiCellStateType,VoronoiCellState> states = new Dictionary<VoronoiCellStateType, VoronoiCellState>()
    {
        {  
            VoronoiCellStateType.Idle, 
            new VoronoiCellState()
            {
                backColor = new Color( 1f, 1f, 1f ), // 无用
                textColor = new Color( 0f, 0f, 0f )
            } 
        },
        {  
            VoronoiCellStateType.Src, 
            new VoronoiCellState()
            {
                backColor = new Color( 0.1f, 0.5f, 0.2f ), // 深绿
                textColor = new Color( 1f, 1f, 1f )
            } 
        },
        {
            VoronoiCellStateType.OnSearching, 
            new VoronoiCellState()
            {
                backColor = new Color( 0.3f, 0.7f, 0.15f ), // 中绿
                textColor = new Color( 1f, 1f, 0f )
            }
        } ,
        {
            VoronoiCellStateType.Processed,
            new VoronoiCellState()
            {
                backColor = new Color( 0.2f, 0.3f, 0.7f ), // 中蓝
                textColor = new Color( 1f, 1f, 0f )
            }
        },
        {
            VoronoiCellStateType.IsPath,
            new VoronoiCellState() 
            {
                //backColor = new Color( 0.8f, 0.1f, 0f ), // 深红
                backColor = new Color( 1f, 0.5f, 0f ), // 亮橙
                textColor = new Color( 1f, 1f, 1f )
            }
        }
    };



    // =========================:
    public Color backColor;
    public Color textColor;

    public VoronoiCellState(){}

}


}
