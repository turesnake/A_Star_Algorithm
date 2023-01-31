using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AStar {


/*
    图中的节点

*/


public class GNode
{
    public Vector3 pos;

    // 将周围8个格子的 节点当作自己的邻居;
    public List<GNode> neighbors = new List<GNode>();

    // F = G + H
    public float G {set;get;} // 出发点 到 本节点 的 cost
    public float H {set;get;} // 本节点 到 目的地 的 理想化的 cost (实际付出会大于 h 值)
    public float F => G + H;

    public GNode previous = null; // 最短路径的 前一个节点;

    public GNode( Vector3 pos_ ) 
    {
        pos = pos_;
        G = 1f; // 暂定为 1.0 
    }

}


}