using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/* CSVReaderにアタッチされているスクリプト */

public class CSVData : MonoBehaviour
{
    // Name of the input file, no extension
    public string inputfile;

    // UI
    public GameObject datasetName1;
    public GameObject pointCount1;

    // List for holding data from CSV reader
    public static List<Dictionary<string, object>> pointList;
    public static List<string> occupationList = new List<string>(); // Occupationのリスト
    // List for min and max values {columnName, {min, max}}
    public static Dictionary<string, object[]> min_maxList = new Dictionary<string, object[]>();

    //********Methods********

    void Awake()
    {
        // Run CSV Reader
        List<Dictionary<string, object>> fullPointList = CSVReader.Read(inputfile);
        
        // Separate Occupation from pointList
        pointList = new List<Dictionary<string, object>>();
        foreach (var point in fullPointList)
        {
            var newPoint = new Dictionary<string, object>(point);
            if (newPoint.ContainsKey("Occupation"))
            {
                occupationList.Add(newPoint["Occupation"].ToString());
                newPoint.Remove("Occupation");
            }
            pointList.Add(newPoint);
        }
    }

    void Start ()
    {

      Debug.Log("---------- CSVData 開始 ----------");
      // Store dictionary keys (column names in CSV) in a list
      List<string> columnList = new List<string>(pointList[1].Keys);

      // 変数の数を表示
      Debug.Log("CSVData: There are " + columnList.Count + " columns in the CSV");

      // Store dictionary keys (min and max values) in min_maxList
      foreach (string key in columnList){
        // 変数名を表示
        Debug.Log("CSVData: Column name is \"" + key + "\".");

        if(pointList[0][key].GetType() != typeof(string)){
          min_maxList.Add(key, new object[]{FindMinValue(key), FindMaxValue(key)});
          // その変数の最小値と最大値を表示
          Debug.Log("Min is " + min_maxList[key][0] + ", and Max is " + min_maxList[key][1]);
        }

      }

      AssignLabels();

      Debug.Log("---------- CSVData 終了 ----------");
    }

    private void AssignLabels()
    {
      // Update point counter
      pointCount1.GetComponent<TextMeshProUGUI>().text = pointList.Count.ToString("0");

      // Update title according to inputfile name
      datasetName1.GetComponent<TextMeshProUGUI>().text = inputfile;
    }


    //Method for finding max value, assumes PointList is generated
    private float FindMaxValue(string column_Name)
    {
      //set initial value to first value
      float maxValue = Convert.ToSingle(pointList[0][column_Name]);

      //Loop through Dictionary, overwrite existing maxValue if new value is larger
      for (var i = 0; i < pointList.Count; i++)
      {
        if (maxValue < Convert.ToSingle(pointList[i][column_Name]))
        maxValue = Convert.ToSingle(pointList[i][column_Name]);
      }

      //Spit out the max value
      return maxValue;
    }

    //Method for finding minimum value, assumes PointList is generated
    private float FindMinValue(string column_Name)
    {
      //set initial value to first value
      float minValue = Convert.ToSingle(pointList[0][column_Name]);

      //Loop through Dictionary, overwrite existing minValue if new value is smaller
      for (var i = 0; i < pointList.Count; i++)
      {
        if (Convert.ToSingle(pointList[i][column_Name]) < minValue)
        minValue = Convert.ToSingle(pointList[i][column_Name]);
      }

      return minValue;
    }

    public static string GetDataForIndex(int index)
    {
        if (index >= 0 && index < pointList.Count)
        {
            Dictionary<string, object> data = pointList[index];
            string result = "";
            foreach (var kvp in data)
            {
                result += $"{kvp.Key}: {kvp.Value}\n";
            }
            //result += $"Occupation: {occupationList[index]}\n";
            return result;
        }
        return "Data not found";
    }
}
