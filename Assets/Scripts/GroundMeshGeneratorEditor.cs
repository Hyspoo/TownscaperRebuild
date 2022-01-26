using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VisualDebugging.Example;

[CustomEditor(typeof(GroundMeshGenerator))]
public class GroundMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
		//base.OnInspectorGUI();
		GroundMeshGenerator script = target as GroundMeshGenerator;

		if (GUILayout.Button("Generate Mesh")) script.GroundMeshDebug();
	}
}
