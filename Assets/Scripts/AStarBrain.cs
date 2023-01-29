using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AStar {


/*
    A-Star 算法 的核心

*/


public class AStarBrain : MonoBehaviour
{

    [SerializeField] MeshRenderer mapMeshRenderer;

    // 地图 center pos:
    [SerializeField] Vector3 mapCenterPos = new Vector3( 0f,0f,0f );

    // 地图总尺寸, 长宽, 米
    [SerializeField] Vector2 mapTotalSideLength = new Vector2( 10f, 10f );

    // 地图中 节点数量, 长宽
    [SerializeField] Vector2Int mapEntNums = new Vector2Int( 10,10 );

    [SerializeField] int randomSeed = 123;


    // 是否在 Scene Camera 中显示 debug 信息:
    [SerializeField] bool IsShowGNodes = true;
    [SerializeField] bool IsShowGNodeLines = true;
    [SerializeField] bool IsShowFoundedPath = true;

    // -------------------------:
    Material mapMaterial;


    List<GNode> gNodes = new List<GNode>(); // 图中所有节点, (mapEntNums.x*mapEntNums.y) 个元素


    List<GNode> foundedPath = new List<GNode>(); // 算法找出的路径


    static Vector2Int[] neighborIdxOffsets = new Vector2Int[]{
        new Vector2Int( -1,-1 ),
        new Vector2Int(  0,-1 ) ,
        new Vector2Int(  1,-1 ),
        new Vector2Int( -1, 0 ),
        new Vector2Int(  1, 0 ),
        new Vector2Int( -1, 1 ),
        new Vector2Int(  0, 1 ),
        new Vector2Int(  1, 1 )
    };


    void Start()
    {
        Debug.Assert( mapMeshRenderer );
        mapMaterial = mapMeshRenderer.sharedMaterial;


        // -----:

        CreateGNodes();

    
        // -----:

        GNode srcGNode = gNodes[ GetGNodesIdx( 4,3 ) ];
        GNode dstGNode = gNodes[ GetGNodesIdx( 9,9 ) ];

        FindPath( srcGNode, dstGNode );
        

        // -----:
        SetMaterial();


    }


    void Update()
    {
    }


    int GetGNodesIdx( int w_, int h_ ) 
    {
        Debug.Assert( w_ >= 0 && w_ < mapEntNums.x && h_ >= 0 && h_ < mapEntNums.y );
        return w_ + h_ * mapEntNums.x;
    }



    // 节点 y高度值 留着用来表示 weight; (y值偏离越大, 越难通行)
    void CreateGNodes() 
    {
        Random.InitState(randomSeed);

        // 地图上单个方块的尺寸
        Vector2 entSize = new Vector2(
            mapTotalSideLength.x / (float)mapEntNums.x,
            mapTotalSideLength.y / (float)mapEntNums.y
        );
        Vector2 randomRadius = entSize * 0.45f; // 给 ent 留一圈边界;

        Vector3 mapLeftBottomPos = mapCenterPos - 0.5f * new Vector3( mapTotalSideLength.x, 0f, mapTotalSideLength.y ); 

        // 生成节点:
        for( int j=0; j<mapEntNums.y; j++ ) 
        {
            for( int i=0; i<mapEntNums.x; i++ ) 
            {
                Vector3 centerPos = new Vector3(
                    mapLeftBottomPos.x + (i + 0.5f) * entSize.x,
                    0f,
                    mapLeftBottomPos.z + (j + 0.5f) * entSize.y
                );

                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f,1f) * randomRadius.x,
                    0f,
                    Random.Range(-1f,1f) * randomRadius.y
                );
                
                var gNode = new GNode( centerPos + randomOffset );
                gNodes.Add( gNode );
            }
        }

        // 查找 neighbors:
        for( int j=0; j<mapEntNums.y; j++ ) 
        {
            for( int i=0; i<mapEntNums.x; i++ ) 
            {
                var gNode = gNodes[ GetGNodesIdx(i,j) ];

                foreach( var idxOffset in neighborIdxOffsets ) 
                {
                    int w = i + idxOffset.x;
                    int h = j + idxOffset.y; 

                    if( w>=0 && w<mapEntNums.x && h>=0 && h<mapEntNums.y )// 越界的不要
                    {
                        gNode.neighbors.Add( gNodes[ GetGNodesIdx(w,h) ] );
                    }
                }
            } 
        }

        //---:
        string outLog = "gNodes count = " + gNodes.Count;
        Debug.Log(outLog);
    }


    // A* 算法本体
    void FindPath( GNode srcNode_, GNode dstNode_ ) 
    {

        List<GNode> toSearch = new List<GNode>(){ srcNode_ };

        List<GNode> processed = new List<GNode>(); // 处理过的



        while( toSearch.Count > 0 ) 
        {
            // 找出 toSearch 中最近的节点:
            GNode current = toSearch[0];
            foreach( var e in toSearch )
            {
                if( e.F < current.F || (e.F == current.F && e.H < current.H) ) 
                {
                    current = e;
                }
            }

            processed.Add( current );
            toSearch.Remove( current );

            foreach( var neighbor in current.neighbors )
            {
                if( processed.Contains(neighbor) == true ) 
                {
                    continue;
                }

                bool isNeighborInToSearch = toSearch.Contains( neighbor );

                float costToNeighbor = current.G + CalcDistance(current,neighbor);

                if(     isNeighborInToSearch == false  
                    ||  costToNeighbor < neighbor.G
                ){
                    neighbor.G = costToNeighbor;
                    neighbor.previous = current;

                    if( isNeighborInToSearch == false )
                    {
                        neighbor.H = CalcDistance( neighbor, dstNode_ );
                        toSearch.Add( neighbor );
                    }
                }
            }
        }

        // 找到路径:
        foundedPath.Clear();
        GNode ptr = dstNode_;
        while( ptr != null )
        {
            foundedPath.Add( ptr );
            ptr = ptr.previous;
        }
        foundedPath.Reverse();

        Debug.Log( "foundedPath count = " + foundedPath.Count );

    }



    float CalcDistance( GNode a_, GNode b_ )
    {
        return (a_.pos - b_.pos).magnitude;
    }


    void SetMaterial() 
    {
        Debug.Assert( mapMaterial );
        List<Vector4> datas = new List<Vector4>();
        foreach( var e in gNodes )
        {
            float k = foundedPath.Contains(e) ? 1f : 0f;

            datas.Add(new Vector4(
                e.pos.x,
                e.pos.y,
                e.pos.z,
                k
            ));
        }
        mapMaterial.SetVectorArray( "_gNodeDatas", datas );
    }






#if UNITY_EDITOR

    static Color color1 = new Color( 0f, 0f, 0f );
    static Color color2 = new Color( 0f, 0f, 1f );
    static Color color3 = new Color( 1f, 0f, 0f );

    void OnDrawGizmos()
    {
        if( gNodes.Count == 0 )
        {
            Debug.Log("CreateGNodes in Editor");
            CreateGNodes();
        }

        if(IsShowGNodes) 
        {
            // 绘制节点:
            Gizmos.color = color1;
            foreach( var gNode in gNodes )
            {
                Gizmos.DrawSphere( gNode.pos, 0.5f );
            }
        }

        if(IsShowGNodeLines)
        {
            // 绘制节点间连线:
            Gizmos.color = color2;
            foreach( var gNode in gNodes )
            {
                foreach( var nei in gNode.neighbors ) 
                {
                    Gizmos.DrawLine( gNode.pos, nei.pos );
                }
            }
        }

        if(IsShowFoundedPath) 
        {
            // 绘制 最短路径
            Gizmos.color = color3;
            // foreach( var e in gNodes )
            // {
            //     if( e.previous != null )
            //     {
            //         Gizmos.DrawLine( e.pos, e.previous.pos );
            //     }
            // }

            for( int i=0; i<foundedPath.Count-1; i++ ) // 剔除尾元素
            {
                Gizmos.DrawLine( foundedPath[i].pos, foundedPath[i+1].pos );
            }


        }

    }

#endif

}


}