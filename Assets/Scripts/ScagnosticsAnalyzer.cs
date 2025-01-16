using UnityEngine;
using System.Collections.Generic;
using Python.Runtime;
using System.IO;
using Newtonsoft.Json;

public class ScagnosticsAnalyzer : MonoBehaviour
{
    public class ScagnosticsResult
    {
        public List<int[]> top_variables { get; set; }
        public List<Dictionary<string, object>> sorted_results { get; set; }
    }


    void Start()
    {
        string metric = "outlying";
        int topN = 3;
        List<int[]> list = AnalyzeData(metric, topN);

        if (list != null)
        {
            foreach (int[] combination in list)
            {
                Debug.Log(string.Join(", ", combination));
            }
        }
        else
        {
            Debug.LogError("Analysis failed or returned null.");
        }
    }

    public List<int[]> AnalyzeData(string metric, int topN)
    {
        PythonEngine.Initialize();

        try
        {
            using (Py.GIL())
            {
                string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "scagnostics_analyzer.py");

                using (PyModule sys = (PyModule)Py.Import("sys"))
                {
                    sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
                }

                using (PyModule scagnosticsAnalyzer = (PyModule)Py.Import("scagnostics_analyzer"))
                {
                    if (scagnosticsAnalyzer == null)
                    {
                        Debug.LogError("Failed to import scagnostics_analyzer.py");
                        return null;
                    }

                    var pointList = CSVData.pointList;
                    string jsonData = JsonConvert.SerializeObject(pointList);

                    using (PyObject resultPy = scagnosticsAnalyzer.InvokeMethod("analyze_unity_data", 
                        new PyObject[] { jsonData.ToPython(), metric.ToPython(), topN.ToPython() }))
                    {
                        var jsonResult = resultPy.As<string>();
                        var scagnosticsResult = JsonConvert.DeserializeObject<ScagnosticsResult>(jsonResult);
                        return scagnosticsResult.top_variables;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in AnalyzeData: {e.Message}");
            return null;
        }
        finally
        {
            PythonEngine.Shutdown();
        }
    }
}
