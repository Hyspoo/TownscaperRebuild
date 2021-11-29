using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VisualDebugging.Example;

public class GridCreator : MonoBehaviour
{
	public int sideSize;

	Vector3[] vertices;
	int[] triangles;

	public void GetVertices()
	{
		vertices = HexagridAlgorithm.GenerateVertices(sideSize);
	}

	public void GetTriangles()
	{
		triangles = HexagridAlgorithm.GenerateTriangles(sideSize);
	}

	public void VertexDebug()
	{
		HexagridAlgorithm.DebugDrawVertices(vertices);
	}

	public void TriangleDebug()
	{
		HexagridAlgorithm.DebugDrawTriangles(vertices, triangles);
	}

	public void Reset()
	{
		vertices = new Vector3[0];
		triangles = new int[0];
		HexagridAlgorithm.ResetDebug();
	}
}
