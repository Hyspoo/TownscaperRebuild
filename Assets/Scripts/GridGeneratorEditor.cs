using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VisualDebugging.Example;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
		//base.OnInspectorGUI();
		GridGenerator script = target as GridGenerator;

		GUILayout.Space(10);
		GUILayout.Label("Generate Grid");

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button("Hexagrid Initialization")) script.InitializeHexagrid();
		if (GUILayout.Button("Random Remove Edges")) script.RemovingEdges();
		if (GUILayout.Button("Subdivide Faces")) script.SubdivideFaces();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button("Relax")) script.Relax();
		if (GUILayout.Button("Reshape")) script.Reshape();
		if (GUILayout.Button("ALL IN ONE!!")) script.AllInOne();
		EditorGUILayout.EndHorizontal ();

		GUILayout.Space(10);
		GUILayout.Label("Debug");

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button("Debug Vertices")) script.pointsDebug();
		if (GUILayout.Button("Debug Triangles")) script.trianglesDebug();
		if (GUILayout.Button("Debug Base Quads")) script.quadsDebug();
		if (GUILayout.Button("Debug SubQuads")) script.subQuadsDebug();
		EditorGUILayout.EndHorizontal ();

		if (GUILayout.Button("Debug Neighbours")) script.neighboursDebug();

		// Reset
		if (GUILayout.Button("Reset Grid", EditorStyles.miniButton))
		{
			script.ResetGrid();
			GridDebug.ResetDebug();
		}
	}
}