using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 
[CustomEditor(typeof(MeshCreator))]
public class GeneratorEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
 
		MeshCreator script = (MeshCreator)target;
		if (GUILayout.Button("Create Mesh"))
		{
			script.CreateMesh ();
		}
	}
}