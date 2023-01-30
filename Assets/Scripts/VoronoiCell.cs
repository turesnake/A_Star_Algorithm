using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;


namespace AStar {


/*
    Voronoi 的单个 cell, 是对 Site 的封装

*/


public class VoronoiCell
{

    public int idx;
    
    public Vector3 pos;

    // 将周围8个格子的 节点当作自己的邻居;
    public List<VoronoiCell> neighbors = new List<VoronoiCell>();

    // F = G + H
    public float G {set;get;} // 出发点 到 本节点 的 cost
    public float H {set;get;} // 本节点 到 目的地 的 理想化的 cost (实际付出会大于 h 值)
    public float F => G + H;

    // 最短路径的 前一个节点;
    VoronoiCell _previous;
    public VoronoiCell previous 
    {
        get { return _previous; }
        set 
        { 
            _previous = value; 

            // 实际上, 每次寻路后, 所有 cell 都会设置自己的 previous...

            // if( value == null )
            // {
            //     ShowBaseCell();
            // }
            // else 
            // {
            //     ShowCell( Color.red );
            // }
        }
    }


    // -------
    AStarBrain2 brain;
    public Site site;

    public float pNoise; // [0f,1f]


    MeshRenderer    mRenderer; // newQuad
    MeshFilter      mFilter; // newQuad 
    MeshCollider    mCollider; // newQuad 

    static Transform parentTF = null;

    Color baseColor;


    public VoronoiCell( AStarBrain2 brain_, Site site_ )
    {
        brain = brain_;
        site = site_;
        pos = brain.Vector2f_2_Vector3(site.Coord);
        idx = site.SiteIndex;
        //neighbors = site.NeighborSites();

        
        CalcPerlinNoise();

        G = pNoise * 100f; // tmp 
        //Debug.Log( "G = " + G );

        GetSiteVertics();
    }

    void CalcPerlinNoise()
    {
        float x = pos.x / brain.worldMapRadius;
        float z = pos.z / brain.worldMapRadius;

        float sclae1 = 3f;
        float sclae2 = 7f;
        float sclae3 = 11f;

        float p1 = 1f - Mathf.PerlinNoise( x * sclae1, z * sclae1 );
        float p2 = 1f - Mathf.PerlinNoise( x * sclae2, z * sclae2 );
        float p3 = 1f - Mathf.PerlinNoise( x * sclae3, z * sclae3 );
        p3 = p3 * p3;
        float w1 = 0.3f;
        float w2 = 0.0f;
        float w3 = 0.8f;

        pNoise = w3*p3 + w2*p2 + w1*p1;
        //pNoise = Mathf.PerlinNoise( pos.x, pos.z );
        //Debug.Log( "pNoise: " + pNoise );
    }

    
    void GetSiteVertics() 
    {

        if( parentTF == null )
        {
            var go = new GameObject("_Cells_");
            parentTF = go.transform;
        }


        // 准备数据:
        var edges =  site.Edges;

        var newQuad = Object.Instantiate( brain.showQuad.gameObject, Vector3.zero, Quaternion.identity, parentTF );
        newQuad.name = "cell_" + idx;
        bool compRet1 = newQuad.TryGetComponent( out mRenderer );
        bool compRet2 = newQuad.TryGetComponent( out mFilter );
        bool compRet3 = newQuad.TryGetComponent( out mCollider );
        bool compRet4 = newQuad.TryGetComponent( out VoronoiCellComp cellComp );
        Debug.Assert( compRet1 && compRet2 && compRet3 && compRet4 );
        var material = mRenderer.material; // todo: 暂不关心它的释放问题

        cellComp.cell = this;

        // ----
        List<Vector3> verticesA = new List<Vector3>(){ pos }; // 找出 testSite 的一圈顶点

        foreach( var e in site.Edges ) 
        {            
            if (e.ClippedEnds == null) continue;
            Vector3 ll = brain.Vector2f_2_Vector3(e.ClippedEnds[LR.LEFT]);
            Vector3 rr = brain.Vector2f_2_Vector3(e.ClippedEnds[LR.RIGHT]);


            ll = Vector3.Lerp( ll, pos, 0.1f );
            rr = Vector3.Lerp( rr, pos, 0.1f );
            ll.y = ll.y + 0.01f;
            rr.y = rr.y + 0.01f;


            if( e.LeftSite.CompareTo( site ) == 0 )
            {   
                verticesA.Add( ll );
                verticesA.Add( rr );
            }
            else 
            {
                verticesA.Add( rr );
                verticesA.Add( ll );
            }
        }

        //---: 
        var mesh = new Mesh { name = "Procedural Mesh " + site.SiteIndex };

        mesh.vertices = verticesA.ToArray();

        //---:
        List<int> idxs = new List<int>();
        for( int i=1; i<verticesA.Count-1; i+=2 ) // 从第二元素开始,一次遍历两个
        {
            idxs.Add( 0 );
            idxs.Add( i );
            idxs.Add( i+1 );
        }
        mesh.triangles = idxs.ToArray();

        //---:
        Vector3[] normals = new Vector3[ verticesA.Count ];
        System.Array.Fill( normals, new Vector3( 0f, 1f, 0f ) );
        mesh.normals = normals;

        // 绑定:
        mFilter.mesh = mesh;
        mCollider.sharedMesh = mesh;


        // todo: 根据 weight 设置颜色:
        baseColor = new Color( pNoise, pNoise, pNoise, 1f );
        //var color = new Color( 0.8f, 0.6f, 0.2f, 1f );
        //material.SetColor( "_BaseColor", color );
        ShowBaseCell();
    }

    public void ShowBaseCell()
    {
        mRenderer.material.SetColor( "_BaseColor", baseColor );
    }


    public void ShowCell( Color color_ )
    {
        mRenderer.material.SetColor( "_BaseColor", color_ );
    }
    



}


}