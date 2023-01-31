using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

//using UnityEngine.UI;

using TMPro;


namespace AStar {
 

public class AStarBrain2 : MonoBehaviour 
{

    public GameObject debugSphere;
    public GameObject debugBlueSphere;
    public GameObject debugGreenSphere;
    public MeshFilter showQuad;
    public TextMeshProUGUI baseText; 
 

    public int polygonSideNum = 10; // 地图中 随机点个数
    public float mapSideLength = 50f; // 地图在 ws 中的边长 (米)
    [HideInInspector] public float entSideLength; // 地图上每个格子的边长
    [HideInInspector] public float entSideHalfLength;


    public int perlinScale = 15; // 噪声碎裂程度, 值越大越碎
    [SerializeField] int randomSeed = 123;
 

    VoronoiCell srcGNode = null;
    VoronoiCell dstGNode = null;


    

    Dictionary<Vector2f,VoronoiCell> voronoiCellDic = new Dictionary<Vector2f, VoronoiCell>();// 图中所有节点
    List<VoronoiCell> foundedPath = new List<VoronoiCell>(); // 算法找出的路径
    float debugSphereRadius = 1.1f;

    



    Dictionary<Vector2f, Site> sites;
    List<Edge> edges;

 

    void Start() 
    {
        Debug.Assert( debugSphere && debugBlueSphere && debugGreenSphere && showQuad );
        Debug.Assert( baseText );


        // --- 设置本 quad transform ---:
        transform.position = new Vector3( mapSideLength * 0.5f, 0f, mapSideLength * 0.5f );
        transform.localScale = new Vector3( mapSideLength, mapSideLength, mapSideLength );



        debugSphereRadius = mapSideLength / (float)polygonSideNum * 0.3f;
        debugSphereRadius = Mathf.Min( debugSphereRadius, 1f );
 
        // --------------------------:
        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points = CreateRandomPoint_2();


        // Create the bounds of the voronoi diagram
        // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        // x,y 是左下角, 不是 center
        Rectf bounds = new Rectf(0,0, mapSideLength, mapSideLength );
        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        // Here I used it with 2 iterations of the lloyd relaxation
        Voronoi voronoi = new Voronoi(points,bounds,5);
        // -------------------------:
 
        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        // 整图中 cell 的数量;
        sites = voronoi.SitesIndexedByLocation;
        // 整图中 边线的数量 (这些边被共用)
        edges = voronoi.Edges;

        // tpr debug:
        Debug.Log( "sites count = " + sites.Count );
        Debug.Log( "edges count = " + edges.Count );



        int count = 0;        
        foreach( var s in sites ) 
        {
            var cell = new VoronoiCell( this, s.Value, false );

            if( count == 2 )
            {
                srcGNode = cell;
            }
            if( count == sites.Count - 3 )
            {
                dstGNode = cell;
            }

            if( voronoiCellDic.ContainsKey( cell.site.Coord ) )
            {
                Debug.LogError( "find same idx: " + cell.idx  );
                continue;
            }
            voronoiCellDic.Add( cell.site.Coord, cell );

            count++;
        }


        //neighbors:
        foreach( var p in voronoiCellDic ) 
        {
            var cell = p.Value;
            foreach( var n in cell.site.NeighborSites() ) 
            {
                if( voronoiCellDic.ContainsKey( n.Coord ) )
                {
                    cell.neighbors.Add( voronoiCellDic[n.Coord] );
                }   
            }
        }

        //DisplayVoronoiDiagram();
    }


    void Update() 
    {

        if( Input.GetMouseButtonDown(0) ) 
        {
            Debug.Log("左键");

            if( srcGNode!=null )
            {
                srcGNode.ShowBaseCell();
            }

            srcGNode = HitCell();

            foreach( var e in foundedPath )
            {
                e.ShowBaseCell();
            }

            if( srcGNode!=null )
            {
                srcGNode.ShowCell( new Color( 0.5f, 1f, 0f, 1f ) ); // 起点
            }
        }


        if( srcGNode!=null && Input.GetMouseButton(1) ) 
        {
            Debug.Log("右键");
            dstGNode = HitCell();

            if( dstGNode != null )
            {
                dstGNode.ShowCell( new Color( 0f, 0.5f, 1f, 1f ) ); // 终点

                foreach( var e in foundedPath )
                {
                    e.ShowBaseCell();
                }

                FindPath( srcGNode, dstGNode );
                // 显示 path:
                foreach( var e in foundedPath )
                {
                    e.ShowCell( Color.red );
                }
            }
        }
    }




    // 使用 cell 碰撞区 来快速查找到目标, 不需要在 数千个 cells 里逐个排查;
    // 其实还是有遍历开销, 不过这个开销让 unity 物理引擎来负责了...
    VoronoiCell HitCell() 
    {
        Camera camera = Camera.main; // todo: 未来改用 项目里的全局相机

        // ----- 处理 非ui元素 的点击 -----:
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if( Physics.Raycast(ray, out RaycastHit hit)  )
        {
            Debug.Log( hit.collider.name );
            
            bool compRet1 = hit.collider.TryGetComponent( out VoronoiCellComp cellComp );
            Debug.Assert( compRet1 );

            return cellComp.cell;
        }
        return null;
    }



    private List<Vector2f> CreateRandomPoint_2() 
    {

        Random.InitState(randomSeed);

        List<Vector2f> points = new List<Vector2f>();

        // 地图上单个方块的尺寸
        entSideLength = mapSideLength / (float)polygonSideNum;
        entSideHalfLength = entSideLength * 0.5f;
        float randomRadius = entSideLength * 0.48f; // 给 ent 留一圈边界;

        //Vector3 mapLeftBottomPos = mapCenterPos - 0.5f * new Vector3( mapSideLength, 0f, mapSideLength ); 
        Vector3 mapLeftBottomPos = Vector3.zero;

        // 生成节点:
        for( int j=0; j<polygonSideNum; j++ ) 
        {
            for( int i=0; i<polygonSideNum; i++ ) 
            {
                // 运行在单个 cell 内生成 [0,3] 个点, 从而提高 map 的分布随机性 (暂时被关闭)
                float random01 = Random.Range(0f,1f);
                int innNum = 1;

                if(random01 < 0.1f)
                {
                    innNum = 0;
                }
                else if( random01 > 0.7f && random01 < 0.95f )  
                {
                    innNum = 2;
                }
                else 
                {
                    innNum = 3;
                }
                innNum = 1; // todo 先手动关闭

                // -----:
                Vector3 centerPos = new Vector3(
                    mapLeftBottomPos.x + (i + 0.5f) * entSideLength,
                    0f,
                    mapLeftBottomPos.z + (j + 0.5f) * entSideLength
                );

                for( int k=0; k<innNum; k++ )
                {

                    Vector3 randomOffset = new Vector3(
                        Random.Range(-1f,1f) * randomRadius,
                        0f,
                        Random.Range(-1f,1f) * randomRadius
                    );

                    Vector3 newPos = centerPos + randomOffset;
                    Vector2f posf = new Vector2f( newPos.x, newPos.z );
                    points.Add(posf);
                }
            }
        }

        Debug.Log( "points count = " + points.Count );
        return points;
    }






    // ===============================================================:
    // A* 算法本体
    void FindPath( VoronoiCell srcNode_, VoronoiCell dstNode_ ) 
    {

        List<VoronoiCell> toSearch = new List<VoronoiCell>(){ srcNode_ };
        List<VoronoiCell> processed = new List<VoronoiCell>(); // 处理过的

        float minDis = float.MaxValue;
        float maxDis = float.MinValue;

        while( toSearch.Count > 0 ) 
        {
            // 找出 toSearch 中最近的节点:
            VoronoiCell current = toSearch[0];
            foreach( var e in toSearch )
            {
                if( e.F < current.F || e.F == current.F && e.H < current.H ) 
                {
                    current = e;
                }
            }

            processed.Add( current );
            toSearch.Remove( current );

            foreach( var neighbor in current.neighbors )
            {
                if( neighbor.Walkable() == false || processed.Contains(neighbor) == true ) 
                {
                    continue;
                }

                bool isNeighborInToSearch = toSearch.Contains( neighbor );

                float costToNeighbor = current.G + Utils.CalcDistance(current,neighbor);

                var ddis = Utils.CalcDistance(current,neighbor);
                minDis = Mathf.Min( minDis, ddis );
                maxDis = Mathf.Max( maxDis, ddis );
    

                if(     isNeighborInToSearch == false  
                    ||  costToNeighbor < neighbor.G
                ){
                    neighbor.G = costToNeighbor;
                    neighbor.previous = current;

                    if( isNeighborInToSearch == false )
                    {
                        neighbor.H = Utils.CalcDistance( neighbor, dstNode_ );
                        toSearch.Add( neighbor );
                    }
                }
            }
        }

        Debug.Log( "minDis = " + minDis + "; maxDis = " + maxDis );

        // 找到路径:
        foundedPath.Clear();
        VoronoiCell ptr = dstNode_;
        while( ptr != null && foundedPath.Count < 200 )
        {
            foundedPath.Add( ptr );
            ptr = ptr.previous;
        }
        foundedPath.Reverse();
        Debug.Log( "foundedPath count = " + foundedPath.Count );
    }




 
    
    // 原例中代替了 shader 的功能;
    // 

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    private void DisplayVoronoiDiagram() 
    {
        Texture2D tx = new Texture2D(512,512);
        foreach (KeyValuePair<Vector2f,Site> kv in sites) {
            tx.SetPixel((int)kv.Key.x, (int)kv.Key.y, Color.red);
        }
        foreach (Edge edge in edges) {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;
 
            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, Color.black);
        }
        tx.Apply();
 
        this.GetComponent<Renderer>().material.mainTexture = tx;
    }
 
    // Bresenham line algorithm
    private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0) 
    {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;
       
        int dx = Mathf.Abs(x1-x0);
        int dy = Mathf.Abs(y1-y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx-dy;
       
        while (true) {
            tx.SetPixel(x0+offset,y0+offset,c);
           
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2*err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
    }
    

    


#if UNITY_EDITOR

    void OnDrawGizmos()
    {

        // 绘制 最短路径
        //Gizmos.color = Color.red;


        // for( int i=0; i<foundedPath.Count-1; i++ ) // 剔除尾元素
        // {
        //     Gizmos.DrawLine( foundedPath[i].position, foundedPath[i+1].position );
        // }

    }

#endif

} 

}
