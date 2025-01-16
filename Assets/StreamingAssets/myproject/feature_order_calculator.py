import numpy as np
from scipy.stats import spearmanr
from python_tsp.exact import solve_tsp_dynamic_programming

def calculate_feature_orders(pointlist):
    try:
        print("Type of pointlist:", type(pointlist))
        print("Length of pointlist:", len(pointlist))

        # Extract feature names
        feature_names = list(pointlist[0].keys())
        print("Feature names:", feature_names)

        # Convert pointlist to numpy array
        X = np.array([[float(point[feature]) for feature in feature_names] for point in pointlist])
        print("X shape:", X.shape)

        # Calculate Spearman correlation
        corr_matrix, _ = spearmanr(X)
        print("Correlation matrix shape:", corr_matrix.shape)

        # Calculate similarity and dissimilarity metrics
        D = np.abs(corr_matrix)
        S = 1 - D

        # Solve TSP problem
        col_order, _ = solve_tsp_dynamic_programming(S)
        row_order, _ = solve_tsp_dynamic_programming(D)

        print("col_order type:", type(col_order))
        print("row_order type:", type(row_order))

        return list(col_order), list(row_order)
    except Exception as e:
        print("Error:", str(e))
        return [], []
