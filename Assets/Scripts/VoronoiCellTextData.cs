using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar {


// 单个 ui 元素的 配置参数:
public class VoronoiCellTextData
{
    
    // =====================:
    static public VoronoiCellTextData ui_G_data = new VoronoiCellTextData( 
        new Vector3( -0.4f, 0.1f, 0.4f ),
        0.9f,
        0.07f
    );

    static public VoronoiCellTextData ui_H_data = new VoronoiCellTextData( 
        new Vector3( 0.4f, 0.1f, 0.4f ),
        0.9f,
        0.07f
    );

    static public VoronoiCellTextData ui_F_data = new VoronoiCellTextData( 
        new Vector3( 0f, 0.1f, -0.4f ),
        1.2f,
        0.07f
    );



    // =====================:
    // 以 {0,0,0} 为中心,在 [-1,1] 区间内的理想偏移值
    public Vector3 posOffset;
    public float localScale;
    public float fontScale;


    // =====================:
    public VoronoiCellTextData( Vector3 posOffset_, float localScale_, float fontScale_ )
    {
        posOffset = posOffset_;
        localScale = localScale_;
        fontScale = fontScale_;
    }

    public Vector3 GetPos( Vector3 parentPos_, float entSideHalfLength_ )
    {
        float innScale = 0.6f;
        return parentPos_ + posOffset * entSideHalfLength_ * innScale;
    }
}


}
