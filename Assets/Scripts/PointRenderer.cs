using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class PointRenderer : MonoBehaviour
{
    //********Public Variables********
    // Bools for editor options
    public bool renderParticles = true;
    //public bool renderPrefabsWithColor = true;

    // Indices for columns to be assigned
    public int column1 = 0;
    public int column2 = 1;
    public int column3 = 2;

    // Full column names from CSV (as Dictionary Keys)
    public string xColumnName;
    public string yColumnName;
    public string zColumnName;

    // Scale of particlePoints within graph, WARNING: Does not scale with graph frame
    private float plotScale = 10;

    // Scale of the prefab particlePoints
    [Range(0.0f, 0.5f)]
    public float pointScale = 0.25f;

    // Changes size of particles generated
    [Range(0.0f, 2.0f)]
    public float particleScale = 5.0f;

    // The prefab for the data particlePoints that will be instantiated
    public GameObject PointPrefab;

    // Object which will contain instantiated prefabs in hiearchy
    public GameObject PointHolder;

    // Objects which will contain axis labels in hiearchy
    public GameObject XLabels;
    public GameObject YLabels;
    public GameObject ZLabels;

    // Color for the glow around the particlePoints
    private Color GlowColor;

    //********Private Variables********
    // Minimum and maximum values of columns
    private float xMin;
    private float yMin;
    private float zMin;

    private float xMax;
    private float yMax;
    private float zMax;

    // Number of rows
    private int rowCount;

    // Particle system for holding point particles
    private ParticleSystem.Particle[] particlePoints;

    public void PlotDataPoints(int newColumn1, int newColumn2, int newColumn3)
    {
        Debug.Log("---------- PlotDataPoints開始 ----------");
        
        column1 = newColumn2;
        column2 = newColumn3;
        column3 = newColumn1;

        // Store dictionary keys (column names in CSV) in a list
        List<string> columnList = new List<string>(CSVData.pointList[1].Keys);

        // Assign column names according to index indicated in columnList
        xColumnName = columnList[column1];
        yColumnName = columnList[column2];
        zColumnName = columnList[column3];

        xMax = Convert.ToSingle(CSVData.min_maxList[xColumnName][1]);
        yMax = Convert.ToSingle(CSVData.min_maxList[yColumnName][1]);
        zMax = Convert.ToSingle(CSVData.min_maxList[zColumnName][1]);

        xMin = Convert.ToSingle(CSVData.min_maxList[xColumnName][0]);
        yMin = Convert.ToSingle(CSVData.min_maxList[yColumnName][0]);
        zMin = Convert.ToSingle(CSVData.min_maxList[zColumnName][0]);

        AssignLabels();
        PlacePrefabPoints();

        // If statement to turn particles on and off
        if (renderParticles == true)
        {
            // Call CreateParticles() for particle system
            CreateParticles();

            // Set particle system, for point glow- depends on CreateParticles()
            GetComponent<ParticleSystem>().SetParticles(particlePoints, particlePoints.Length);
        }

        Debug.Log("---------- PlotDataPoints終了：(" + newColumn1 + ", " + newColumn2 + ", " + newColumn3 + ") ----------");
    }

    // Places the prefabs according to values read in
    private void PlacePrefabPoints()
    {
        //Debug.Log("---------- PlacePrefabPoints 開始 ----------");
        // Get count (number of rows in table)
        rowCount = CSVData.pointList.Count;

        for (var i = 0; i < rowCount; i++)
        {

            // Set x/y/z, standardized to between 0-1
            float x = (Convert.ToSingle(CSVData.pointList[i][xColumnName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(CSVData.pointList[i][yColumnName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(CSVData.pointList[i][zColumnName]) - zMin) / (zMax - zMin);

            // Create vector 3 for positioning particlePoints
            Vector3 position = new Vector3(x, y, z) * plotScale;

            //instantiate as gameobject variable so that it can be manipulated within loop
            GameObject dataPoint = Instantiate(PointPrefab, Vector3.zero, Quaternion.identity);

            // Make child of PointHolder object, to keep particlePoints within container in hiearchy
            dataPoint.transform.parent = PointHolder.transform;

            // Position point at relative to parent
            dataPoint.transform.localPosition = position;
            pointScale = 0.25f;
            dataPoint.transform.localScale = new Vector3(pointScale, pointScale, pointScale);

            // Converts index to string to name the point the index number
            dataPoint.transform.name = i.ToString();
            /*
            // データ点の色を指定
            if (renderPrefabsWithColor == true)
            {
                // Sets color according to x/y/z value
                dataPoint.GetComponent<Renderer>().material.color = new Color(x, y, z, 1.0f);

                // Activate emission color keyword so we can modify emission color
                dataPoint.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");

                dataPoint.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(x, y, z, 1.0f));

            }
            */

        }

        //Debug.Log("---------- PlacePrefabPoints 終了 ----------");
    }

    private void CreateParticles()
    {
        rowCount = CSVData.pointList.Count;

        particlePoints = new ParticleSystem.Particle[rowCount];

        for (int i = 0; i < rowCount; i++)
        {
            // Convert object from list into float
            float x = (Convert.ToSingle(CSVData.pointList[i][xColumnName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(CSVData.pointList[i][yColumnName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(CSVData.pointList[i][zColumnName]) - zMin) / (zMax - zMin);

            // Set point location
            particlePoints[i].position = new Vector3(x, y, z) * plotScale;

            // Set point color
            particlePoints[i].startColor = new Color(x, y, z, 1.0f);
            particlePoints[i].startSize = particleScale;
        }

    }

    private void AssignLabels()
    {
        XLabels.transform.Find("X_Title").gameObject.GetComponent<TextMesh>().text = xColumnName;
        YLabels.transform.Find("Y_Title").gameObject.GetComponent<TextMesh>().text = yColumnName;
        ZLabels.transform.Find("Z_Title").gameObject.GetComponent<TextMesh>().text = zColumnName;

        XLabels.transform.Find("X_Min_Lab").gameObject.GetComponent<TextMesh>().text = xMin.ToString("0.0");
        XLabels.transform.Find("X_Mid_Lab").gameObject.GetComponent<TextMesh>().text = (xMin + (xMax - xMin) / 2f).ToString("0.0");
        XLabels.transform.Find("X_Max_Lab").gameObject.GetComponent<TextMesh>().text = xMax.ToString("0.0");

        YLabels.transform.Find("Y_Min_Lab").gameObject.GetComponent<TextMesh>().text = yMin.ToString("0.0");
        YLabels.transform.Find("Y_Mid_Lab").gameObject.GetComponent<TextMesh>().text = (yMin + (yMax - yMin) / 2f).ToString("0.0");
        YLabels.transform.Find("Y_Max_Lab").gameObject.GetComponent<TextMesh>().text = yMax.ToString("0.0");

        ZLabels.transform.Find("Z_Min_Lab").gameObject.GetComponent<TextMesh>().text = zMin.ToString("0.0");
        ZLabels.transform.Find("Z_Mid_Lab").gameObject.GetComponent<TextMesh>().text = (zMin + (zMax - zMin) / 2f).ToString("0.0");
        ZLabels.transform.Find("Z_Max_Lab").gameObject.GetComponent<TextMesh>().text = zMax.ToString("0.0");

    }

    public void UpdateDataPoints(int newColumn1, int newColumn2, int newColumn3)
    {
        column1 = newColumn2;
        column2 = newColumn3;
        column3 = newColumn1;

        // 列名を更新
        List<string> columnList = new List<string>(CSVData.pointList[1].Keys);
        xColumnName = columnList[column1];
        yColumnName = columnList[column2];
        zColumnName = columnList[column3];

        // 最小値と最大値を更新
        xMax = Convert.ToSingle(CSVData.min_maxList[xColumnName][1]);
        yMax = Convert.ToSingle(CSVData.min_maxList[yColumnName][1]);
        zMax = Convert.ToSingle(CSVData.min_maxList[zColumnName][1]);
        xMin = Convert.ToSingle(CSVData.min_maxList[xColumnName][0]);
        yMin = Convert.ToSingle(CSVData.min_maxList[yColumnName][0]);
        zMin = Convert.ToSingle(CSVData.min_maxList[zColumnName][0]);

        // データ点の位置を更新
        StartCoroutine(MoveDataPoints());
        
        if (renderParticles)
        {
            UpdateParticles();
        }

        AssignLabels();
        Debug.Log("---------- UpdateDataPoints終了：(" + column1 + ", " + column2 + ", " + column3 + ") ----------");
    }

    private void UpdateParticles()
    {
        for (int i = 0; i < rowCount; i++)
        {
            float x = (Convert.ToSingle(CSVData.pointList[i][xColumnName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(CSVData.pointList[i][yColumnName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(CSVData.pointList[i][zColumnName]) - zMin) / (zMax - zMin);

            particlePoints[i].position = new Vector3(x, y, z) * plotScale;
            particlePoints[i].startColor = new Color(x, y, z, 1.0f);
        }

        GetComponent<ParticleSystem>().SetParticles(particlePoints, particlePoints.Length);
    }

    private IEnumerator MoveDataPoints()
    {
        float duration = 1.0f; // アニメーション時間（秒）
        float elapsedTime = 0f;

        Vector3[] startPositions = new Vector3[rowCount];
        Vector3[] endPositions = new Vector3[rowCount];

        for (int i = 0; i < rowCount; i++)
        {
            startPositions[i] = PointHolder.transform.GetChild(i).localPosition;
            
            float x = (Convert.ToSingle(CSVData.pointList[i][xColumnName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(CSVData.pointList[i][yColumnName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(CSVData.pointList[i][zColumnName]) - zMin) / (zMax - zMin);
            endPositions[i] = new Vector3(x, y, z) * plotScale;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            for (int i = 0; i < rowCount; i++)
            {
                Vector3 newPosition = Vector3.Lerp(startPositions[i], endPositions[i], t);
                PointHolder.transform.GetChild(i).localPosition = newPosition;
                /*
                if (renderPrefabsWithColor)
                {
                    Color newColor = Color.Lerp(startPositions[i] / plotScale, endPositions[i] / plotScale, t);
                    PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.color = newColor;
                    PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.SetColor("_EmissionColor", newColor);
                }
                */
            }

            yield return null;
        }

        //Debug.Log("Data points movement completed");
    }

}
