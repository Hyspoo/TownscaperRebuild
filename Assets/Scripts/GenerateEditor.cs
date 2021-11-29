using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 
[CustomEditor(typeof(GridCreator))]
public class GeneratorEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
		//base.OnInspectorGUI();
 
		GridCreator script = target as GridCreator;
		if (GUILayout.Button("Create Vertices"))
		{
			script.GetVertices();
		}

		if (GUILayout.Button("Create Triangles"))
		{
			script.GetTriangles();
		}

		if (GUILayout.Button("Show current Vertices"))
		{
			script.VertexDebug();
		}

		if (GUILayout.Button("Show current Triangles"))
		{
			script.TriangleDebug();
		}



		// Reset
		if (GUILayout.Button("Reset"))
		{
			script.Reset();
		}
	}
}