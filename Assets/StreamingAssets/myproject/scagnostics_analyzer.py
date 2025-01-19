import numpy as np
from scipy.spatial import Delaunay, ConvexHull
from scipy.sparse.csgraph import minimum_spanning_tree, connected_components
from scipy.stats import spearmanr
from itertools import combinations
import json
import heapq

def compute_metrics(dataset, var_indices):
    points = dataset[:, var_indices]
    
    tri = Delaunay(points)
    
    edges = set()
    for simplex in tri.simplices:
        for i in range(3):
            edge = tuple(sorted([simplex[i], simplex[(i+1)%3]]))
            edges.add(edge)

    edges = np.array(list(edges))
    distances = np.sqrt(np.sum((points[edges[:, 0]] - points[edges[:, 1]])**2, axis=1))
    graph = np.zeros((len(points), len(points)))
    graph[edges[:, 0], edges[:, 1]] = distances
    graph[edges[:, 1], edges[:, 0]] = distances
    mst = minimum_spanning_tree(graph)
    
    mst_lengths = mst.data[mst.data != 0]
    
    q75, q25 = np.percentile(mst_lengths, [75, 25])
    iqr = q75 - q25
    cutoff = q75 + 1.5 * iqr
    outlier_lengths = mst_lengths[mst_lengths >= cutoff]
    outlying = np.sum(outlier_lengths) / np.sum(mst_lengths)
    
    def get_runts(edge_length):
        mst_copy = mst.copy()
        mst_copy.data[mst_copy.data >= edge_length] = 0
        n_components, labels = connected_components(mst_copy)
        counts = np.bincount(labels)
        return np.min(counts)
    
    max_edge_length = np.max(mst_lengths)
    max_clumpy = 0
    for length in mst_lengths:
        runts = get_runts(length)
        smaller_lengths = mst_lengths[mst_lengths < length]
        
        if smaller_lengths.size > 0:
            clumpy = runts * (1 - np.max(smaller_lengths) / length)
        else:
            clumpy = runts * (1 - length / max_edge_length)
            
        max_clumpy = max(max_clumpy, clumpy)
    
    clumpy = 2 * max_clumpy / len(points)
    
    hull = ConvexHull(points)
    
    alpha_volume = hull.volume
    alpha_surface_area = np.sum(hull.area)
    c = (36 * np.pi) ** (1/6)
    skinny = 1 - c * (alpha_volume ** (1/3)) / np.sqrt(alpha_surface_area)
    
    rx = np.argsort(points[:, 0])
    ry = np.argsort(points[:, 1])
    rz = np.argsort(points[:, 2])
    
    s_xy, _ = spearmanr(rx, ry)
    s_xz, _ = spearmanr(rx, rz)
    s_yz, _ = spearmanr(ry, rz)
    
    rho1 = (s_xy - s_xz * s_yz) / np.sqrt((1 - s_xz**2) * (1 - s_yz**2))
    rho2 = (s_xz - s_xy * s_yz) / np.sqrt((1 - s_xy**2) * (1 - s_yz**2))
    rho3 = (s_yz - s_xy * s_xz) / np.sqrt((1 - s_xy**2) * (1 - s_xz**2))
    
    monotonic = max(rho1**2, rho2**2, rho3**2)
    
    return outlying, clumpy, skinny, monotonic

def initialize_data(json_data):
    point_list = json.loads(json_data)
    keys = list(point_list[0].keys())
    data = np.array([[float(point[key]) for key in keys] for point in point_list])
    
    results = []
    for var_indices in combinations(range(data.shape[1]), 3):
        outlying, clumpy, skinny, monotonic = compute_metrics(data, var_indices)
        results.append({
            'variables': var_indices,
            'outlying': outlying,
            'clumpy': clumpy,
            'skinny': skinny,
            'monotonic': monotonic
        })

    top10_results = {
        'outlying': heapq.nlargest(10, results, key=lambda x: x['outlying']),
        'clumpy': heapq.nlargest(10, results, key=lambda x: x['clumpy']),
        'skinny': heapq.nlargest(10, results, key=lambda x: x['skinny']),
        'monotonic': heapq.nlargest(10, results, key=lambda x: x['monotonic'])
    }
    
    return json.dumps({metric: [r['variables'] for r in top10] for metric, top10 in top10_results.items()})