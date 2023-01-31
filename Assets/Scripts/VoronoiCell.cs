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

    // 最短路径的 前一个节点:
    public VoronoiCell predecessor;


    // -------
    AStarBrain2 brain;
    public Site site;

    public float pNoise; // [0f,1f]

    public float weight;

    Transform worldCanvasTF;


    MeshRenderer    mRenderer; // newQuad
    MeshFilter      mFilter; // newQuad 
    MeshCollider    mCollider; // newQuad 


    TextMeshProUGUI text_W, text_G, text_H, text_F;


    Transform predecessorArrowTF;


    static Transform parentTF = null;


    Color baseColor;

    VoronoiCellStateType currentStateType = VoronoiCellStateType.Idle;


    public VoronoiCell( AStarBrain2 brain_, Site site_, bool isBorder_ )
    {
        brain = brain_;
        site = site_;
        position = Utils.Vector2f_2_Vector3(site.Coord);
        idx = site.SiteIndex;
        
        CalcPerlinNoise(); 

        weight = (1-pNoise) * 5f;
        G = 1f;

        GetSiteVertics();
        CreateTexts();

        // todo: 根据 weight 设置颜色:
        float c = Mathf.Lerp( pNoise, 1f, 0.3f );
        baseColor = new Color( c, c, c, 1f );

        SwitchState( VoronoiCellStateType.Idle );

        // -------:
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "predecessorArrow_" + idx;
            go.SetActive(false);
            predecessorArrowTF = go.transform;
            predecessorArrowTF.parent = parentTF;
            Object.Destroy( go.GetComponent<MeshCollider>() );
            var mr = go.GetComponent<MeshRenderer>();
            //mr.material.SetColor( "_BaseColor", new Color( 0.85f, 0.75f, 0.4f ) );
            mr.material.SetColor( "_BaseColor", new Color( 0.6f, 0.6f, 0.6f ) );
        }
        
        
    }



    public bool Walkable()
    {
        return (pNoise > 0.2f);
        //return true;
    }


    void CalcPerlinNoise()
    {
        float x = position.x / brain.mapSideLength;
        float z = position.z / brain.mapSideLength;

        float sclae1 = brain.perlinScale;
        float p1 = 1f - Mathf.PerlinNoise( x * sclae1, z * sclae1 );

        // 疯狂的 smooth 来让曲线在 [0f,1f] 区间无限接近 0f 和 1f, 两级分明
        for( int i=0; i<4; i++ ) // 13
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

        if( brain.isShowTexts )
        {
            text_W = CreateSingleText( VoronoiCellTextData.ui_W_data );
            text_G = CreateSingleText( VoronoiCellTextData.ui_G_data );
            text_H = CreateSingleText( VoronoiCellTextData.ui_H_data );
            text_F = CreateSingleText( VoronoiCellTextData.ui_F_data );
        }
        
    }


    TextMeshProUGUI CreateSingleText( VoronoiCellTextData textData_ )
    {
        Vector3 _pos = textData_.GetPos( position, brain.entSideHalfLength );

        var go = Object.Instantiate( brain.baseText.gameObject, _pos, Quaternion.Euler( 90f,0f,0f ), worldCanvasTF );
        go.transform.localScale *= textData_.localScale;
        go.SetActive( brain.isShowTexts );

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
        GameObject newQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);// 自带 MeshCollider
        var newQuadTF = newQuad.transform;
        newQuad.name = "cell_" + idx;
        newQuadTF.parent = parentTF;
        newQuadTF.position = position;
        VoronoiCellComp cellComp = newQuad.AddComponent<VoronoiCellComp>();
        cellComp.cell = this;

        bool compRet1 = newQuad.TryGetComponent( out mRenderer );
        bool compRet2 = newQuad.TryGetComponent( out mFilter );
        bool compRet3 = newQuad.TryGetComponent( out mCollider );
        Debug.Assert( compRet1 && compRet2 && compRet3 );
        var material = mRenderer.material; // todo: 暂不关心它的释放问题


        // ----
        List<Vector3> verticesA = new List<Vector3>(){ Vector3.zero }; // 找出 testSite 的一圈顶点
        foreach( var e in site.Edges ) 
        {            
            if (e.ClippedEnds == null) continue;
            Vector3 ll = Utils.Vector2f_2_Vector3(e.ClippedEnds[LR.LEFT]);
            Vector3 rr = Utils.Vector2f_2_Vector3(e.ClippedEnds[LR.RIGHT]);

            ll = Vector3.Lerp( ll, position, 0.1f );
            rr = Vector3.Lerp( rr, position, 0.1f );
            
            ll -= position; // objectPos
            rr -= position; // objectPos

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
        var mesh = new Mesh { name = "cell_Mesh_" + site.SiteIndex };

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
    }



    public void ShowPredecessorArrow( bool isShow )
    {
        if( predecessor == null )
        {
            predecessorArrowTF.gameObject.SetActive( false );
            return;
        }

        predecessorArrowTF.gameObject.SetActive( isShow );
        if( isShow == false )
        {
            return;
        }

        Vector3 srcPos = position;
        Vector3 dstPos = predecessor.position;
        Vector3 midPos = (srcPos + dstPos) * 0.5f;

        float cubeScale = 0.2f * brain.entSideLength;

        Quaternion rot = Quaternion.LookRotation( dstPos - srcPos, Vector3.up ); // +z 方向

        predecessorArrowTF.position = midPos;

        predecessorArrowTF.rotation = rot;
        
        predecessorArrowTF.localScale = new Vector3(
            cubeScale,
            cubeScale,
            (dstPos-srcPos).magnitude
        );
    }

    

    public void SetAndSwitchState( VoronoiCellStateType type_ ) 
    {
        SetState( type_ );
        SwitchState( type_ );
    }

    public void SetState( VoronoiCellStateType type_ ) 
    {
        currentStateType = type_;
    }

    public void SwitchState()
    {
        SwitchState( currentStateType );
    }

    public void SwitchState( VoronoiCellStateType type_ )
    {
        var backColor = (type_ == VoronoiCellStateType.Idle) ? baseColor : VoronoiCellState.states[type_].backColor;
        var textColor = VoronoiCellState.states[type_].textColor;
        mRenderer.material.SetColor( "_BaseColor", backColor );

        if(type_ == VoronoiCellStateType.Idle)
        {
            predecessor = null;
        }

        if( brain.isShowTexts )
        {
            text_W.color = textColor;
            text_G.color = textColor;
            text_H.color = textColor;
            text_F.color = textColor;

            //---:
            text_W.text = weight.ToString("0.0");
            text_G.text = G.ToString("0.0");
            text_H.text = H.ToString("0.0");
            text_F.text = F.ToString("0.0");
        }
    }




}


}