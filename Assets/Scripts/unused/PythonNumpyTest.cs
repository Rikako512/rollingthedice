using Microsoft.CSharp;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Python.Runtime;
using System.IO;

public class PythonNumpyTest : MonoBehaviour
{
    void Start()
    {
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "test.py");
            Debug.Log("Script Path: " + scriptPath);

            using (PyModule sys = (PyModule)Py.Import("sys"))
            {
                sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
            }

            using (PyModule npTest = (PyModule)Py.Import("test"))
            {
                if (npTest == null)
                {
                    Debug.LogError("Failed to import test.py");
                    return;
                }

                float[] array1 = { 1.0f, 2.0f, 3.0f };
                float[] array2 = { 4.0f, 5.0f, 6.0f };

                using (PyObject resultAddPy = npTest.InvokeMethod("add_arrays", new PyObject[] { array1.ToPython(), array2.ToPython() }))
                using (PyObject resultMultiplyPy = npTest.InvokeMethod("multiply_arrays", new PyObject[] { array1.ToPython(), array2.ToPython() }))
                {
                    float[] resultAdd = resultAddPy.As<float[]>();
                    float[] resultMultiply = resultMultiplyPy.As<float[]>();

                    Debug.Log("Addition Result: " + string.Join(", ", resultAdd));
                    Debug.Log("Multiplication Result: " + string.Join(", ", resultMultiply));
                }
            }
        }

        PythonEngine.Shutdown();
    }
}