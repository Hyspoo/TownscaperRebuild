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
		if (GUILayout.Button("Create Grid"))
		{
			script.CreateGroundGrid ();
		}
	}
}