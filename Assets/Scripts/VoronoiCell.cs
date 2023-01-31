using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

//using UnityEngine.UI;

using TMPro;


namespace AStar {


/*
    Voronoi 的单个 cell, 是对 Site 的封装

*/


public class VoronoiCell
{

    public int idx;
    
    public Vector3 position;

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
        set { _previous = value; }
    }


    // -------
    AStarBrain2 brain;
    public Site site;

    public float pNoise; // [0f,1f]

    Transform worldCanvasTF;


    MeshRenderer    mRenderer; // newQuad
    MeshFilter      mFilter; // newQuad 
    MeshCollider    mCollider; // newQuad 


    TextMeshProUGUI text_G, text_H, text_F;



    static Transform parentTF = null;


    Color baseColor;


    public VoronoiCell( AStarBrain2 brain_, Site site_, bool isBorder_ )
    {
        brain = brain_;
        site = site_;
        position = Utils.Vector2f_2_Vector3(site.Coord);
        idx = site.SiteIndex;
        
        CalcPerlinNoise(); 

        G = (1-pNoise) * 10f; // tmp 
        //Debug.Log( "G = " + G );
        //G = 1f;

        GetSiteVertics();

        CreateTexts();
    }

    public bool Walkable()
    {
        return (pNoise > 0.5f);
        //return true;
    }


    void CalcPerlinNoise()
    {
        float x = position.x / brain.mapSideLength;
        float z = position.z / brain.mapSideLength;

        float sclae1 = brain.perlinScale;
        float p1 = 1f - Mathf.PerlinNoise( x * sclae1, z * sclae1 );

        // 疯狂的 smooth 来让曲线在 [0f,1f] 区间无限接近 0f 和 1f, 两级分明
        for( int i=0; i<13; i++ )
        {
            p1 = Mathf.SmoothStep( 0f, 1f, p1 );
        }

        //p1 = Remap( 0f, 1f, -0.5f, 1.5f, p1 );
        p1 = Mathf.Max( p1, 0f );
        pNoise = p1;
    }

    // 3个 ui text 元素
    void CreateTexts() 
    {
        worldCanvasTF = brain.baseText.transform.parent;
        
        text_G = CreateSingleText( VoronoiCellTextData.ui_G_data );
        text_H = CreateSingleText( VoronoiCellTextData.ui_H_data );
        text_F = CreateSingleText( VoronoiCellTextData.ui_F_data );
    }


    TextMeshProUGUI CreateSingleText( VoronoiCellTextData textData_ )
    {
        Vector3 _pos = textData_.GetPos( position, brain.entSideHalfLength );

        var go = Object.Instantiate( brain.baseText.gameObject, _pos, Quaternion.Euler( 90f,0f,0f ), worldCanvasTF );
        go.transform.localScale *= textData_.localScale;

        bool ret1 = go.TryGetComponent( out TextMeshProUGUI textComp );
        Debug.Assert( ret1 );

        textComp.fontSize *= textData_.fontScale * brain.entSideHalfLength;

        return textComp;
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
        List<Vector3> verticesA = new List<Vector3>(){ position }; // 找出 testSite 的一圈顶点

        foreach( var e in site.Edges ) 
        {            
            if (e.ClippedEnds == null) continue;
            Vector3 ll = Utils.Vector2f_2_Vector3(e.ClippedEnds[LR.LEFT]);
            Vector3 rr = Utils.Vector2f_2_Vector3(e.ClippedEnds[LR.RIGHT]);


            ll = Vector3.Lerp( ll, position, 0.1f );
            rr = Vector3.Lerp( rr, position, 0.1f );
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
        float c = Mathf.Lerp( pNoise, 1f, 0.3f );
        baseColor = new Color( c, c, c, 1f );
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