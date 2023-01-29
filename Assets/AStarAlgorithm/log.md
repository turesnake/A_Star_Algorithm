# ========================================== #
#         A * Algorithm
# ========================================== #




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










