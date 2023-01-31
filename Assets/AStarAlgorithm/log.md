# ========================================== #
#         A * Algorithm
# ========================================== #


# wiki:
    https://en.wikipedia.org/wiki/A*_search_algorithm

    Dijkstra's 会每个节点生成 最短路径, 所以它缺少了 从 dst 出发的 启发式信息 这一环;





# ++++++++++++++++++++++++++++ #
#    第三方 csDelaunay 使用指南
# ++++++++++++++++++++++++++++ #
https://github.com/PouletFrit/csDelaunay


#  Site  (class)
    表示一个 cell, 
    -- SiteIndex        -- idx
    -- Coord            -- pos
    -- NeighborSites()  -- 临近 cell
    -- Region()         -- cell 所在区域
    -- 

# Edge (class)
    -- LeftSite     -- 边一侧的 cell
    -- RightSite    -- 边另一侧的 cell


    -- edge.ClippedEnds[LR.LEFT]  -- 边的两个端点
    -- edge.ClippedEnds[LR.RIGHT] -- 边的两个端点


# 注意:
    Site.Coord 是个奇怪的值...

    Rectf bounds = new Rectf(0,0, 512, 512 );

    设置这个的时候, 最好设置为 512, 不要去动;

    然后 图会生成在 (0,0,0) 为 左下角, 边长为 512 的平面上;

    然后手动这个坐标系 转换为我们想要的...



# voronoi.Edges 混入的顺序....
    首先有些边的 ClippedEnds 可能为 null, 比如当它在 map 边界时...

    其次这些边的 方向是随机的;




# 另一种确定边界的方法, 检查它距离 四边的距离...





# ++++++++++++++++++++++++++++ #
#     todo List
# ++++++++++++++++++++++++++++ #

# scene camera 中提供两个球 src 和 dst, 
    用户可拖动它们, 
    用户松手后, 它们会自动吸附到最近的 地图节点上去;
    以此来表示 路径的 起始 和 结尾;

# 像示例教程那样, 标出每个 节点的 状态, 红色 和 绿色;


#  尝试来程序生成 mesh 网格;


# 用 perlin noise 来表达 地图上行走的困难程度;


# 真实接壤问题:
    博洛诺伊图, 一个节点格 和 周围 8 个格不一定真的接壤;
    是否接壤需要 一道检测, 然后删减 neighbors 中的成员...

    Cellular Noise
    Voronoi


# -1- 如何依靠 Voronoi 的节点 反向求得每个边的 顶点信息

    https://blog.csdn.net/K346K346/article/details/52244123

    https://forum.unity.com/threads/delaunay-voronoi-diagram-library-for-unity.248962/



# -2- 如何知道一个 Voronoi 细胞的 有效相邻细胞

# -3- Voronoi cell 可能是多边形, 如果要用 顶点来建 cell, 那么这个 cell 可能由数个 三角形构成;


# -- 快速方案: --
    -- mesh 使用 普通方形cell 的顶点, 目的仅仅是为了降低 shader for循环开销
    -- 想个办法剔除掉 Voronoi cell 的 不相邻 cell;


# 借用第三方库: csDelaunay
    -- 目前知道每个 cell 的相邻 cell 了;

    -- 知道每个 cell 的一圈顶点了
        意味着可以为每个 cell 手动制作一个 mesh 来渲染;
        还能指定它的颜色...


# 统一管理 方块的 颜色

    方块有状态


# cell 状态管理
    不同的状态,
        -- cell 底色不同 
        -- 三个 text 的颜色不同,  
        -- text 也可选择不显示;

    状态是整个儿切换的









