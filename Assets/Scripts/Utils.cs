using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

namespace AStar {


// 单个 ui 元素的 配置参数:
public static class Utils
{
    
    // x 在区间[t1,t2] 中, 求区间[s1,s2] 中同比例的点的值
    public static float Remap( float t1, float t2, float s1, float s2, float x )
    {
        return ((x - t1) / (t2 - t1) * (s2 - s1) + s1);
    }


    public static Vector3 Vector2f_2_Vector3( Vector2f a_ )
    {
        return new Vector3( a_.x, 0f, a_.y );
    }


    public static float CalcDistance( VoronoiCell a_, VoronoiCell b_ )
    {
        return (a_.position - b_.position).magnitude;
    }
}


}
