//#define DEBUG_EXAMPLE_ALGORITHM
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualDebugging.Example
{
    public static class HexagridAlgorithm
    {

        public static Vector3[] GenerateVertices(int mSideSize)
        {
#if DEBUG_EXAMPLE_ALGORITHM
            VisualDebug.Initialize();
            VisualDebug.SetColour(Colours.lightRed, Colours.veryDarkGrey);
#endif
            if (mSideSize < 2) throw (new ArgumentException("Generate fail! Your input size is less than 2."));

            List<Vector3> vertexList = new List<Vector3>();
            int index = 0;

            float sideLength = 0.5f * Mathf.Tan(Mathf.Deg2Rad * 60); // 0.5f* tanf(60deg)
            for (int x = 0; x < mSideSize * 2 - 1; ++x)
            {
                int height = (x < mSideSize) ? (mSideSize + x) : (mSideSize * 3 - 2 - x);
                float deltaHeight = mSideSize - height * 0.5f;
                for (int z = 0; z < height; z++)
                {
                    bool isSide = x == 0 || x == (mSideSize * 2 - 2) || z == 0 || z == height - 1;
                    vertexList.Add(new Vector3((x - mSideSize + 1) * sideLength, 0, z + deltaHeight));
#if DEBUG_EXAMPLE_ALGORITHM
                    VisualDebug.BeginFrame("Draw point", true);
                    VisualDebug.DrawPointWithLabel(vertexList.Last(), .1f, index.ToString(), 2);
#endif
                    index += 1;
                }
            }

            Vector3[] vertices = vertexList.ToArray();
#if DEBUG_EXAMPLE_ALGORITHM
            VisualDebug.Save();
#endif
            return vertices;
        }

        public static int[] GenerateTriangles(int mSideSize)
        {
            //VisualDebug.Initialize();

            if (mSideSize < 2) throw (new ArgumentException("Generate fail! Your input size is less than 2."));

            List<int> TriangleList = new List<int>();
            Vector3[] vertices = GenerateVertices(mSideSize);
#if DEBUG_EXAMPLE_ALGORITHM
            VisualDebug.SetColour(Colours.lightGreen, Colours.darkGrey);
#endif
            int offset = 0;
            for (int x = 0; x < (mSideSize * 2 - 2); x++)
            {
                int height = (x < mSideSize) ? (mSideSize + x) : (mSideSize * 3 - 2 - x);
                if (x < mSideSize - 1)
                {
                    // left side
                    for (int z = 0; z < height; z++)
                    {
                        TriangleList.Add(offset + z);
                        TriangleList.Add(offset + z + height);
                        TriangleList.Add(offset + z + height + 1);
#if DEBUG_EXAMPLE_ALGORITHM
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z], vertices[offset + z + height], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + height], vertices[offset + z + height + 1], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + height + 1], vertices[offset + z], .1f);
#endif
                        if (z >= height - 1)
                        {
                            break;
                        }

                        TriangleList.Add(offset + z + height + 1);
                        TriangleList.Add(offset + z + 1);
                        TriangleList.Add(offset + z);
#if DEBUG_EXAMPLE_ALGORITHM
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + height + 1], vertices[offset + z + 1], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + 1], vertices[offset + z], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z], vertices[offset + z + height + 1], .1f);
#endif
                    }
                }
                else
                {
                    // right side
                    for (int z = 0; z < height - 1; z++)
                    {
                        //TriangleList.Add(new Triangle(offset + y, offset + y + height, offset + y + 1));
                        TriangleList.Add(offset + z);
                        TriangleList.Add(offset + z + height);
                        TriangleList.Add(offset + z + 1);
#if DEBUG_EXAMPLE_ALGORITHM
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z], vertices[offset + z + height], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + height], vertices[offset + z + 1], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + 1], vertices[offset + z], .1f);
#endif
                        if (z >= height - 2)
                        {
                            break;
                        }

                        //TriangleList.Add(new Triangle(offset + y + 1, offset + y + height, offset + y + height + 1));
                        TriangleList.Add(offset + z + 1);
                        TriangleList.Add(offset + z + height);
                        TriangleList.Add(offset + z + height + 1);
#if DEBUG_EXAMPLE_ALGORITHM
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + 1], vertices[offset + z + height], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + height], vertices[offset + z + height + 1], .1f);
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        VisualDebug.DrawArrow(vertices[offset + z + height + 1], vertices[offset + z + 1], .1f);
#endif
                    }
                }
                offset += height;
            }

            int[] triangles = TriangleList.ToArray();
#if DEBUG_EXAMPLE_ALGORITHM
            VisualDebug.Save();
#endif
            return triangles;
        }

        public static void DebugDrawVertices(Vector3[] vertices)
        {
            VisualDebug.Initialize();
            VisualDebug.SetColour(Colours.lightRed, Colours.veryDarkGrey);

            for (int i = 0; i < vertices.Count(); i++)
            {
                VisualDebug.BeginFrame("Draw point", true);
                VisualDebug.DrawPointWithLabel(vertices[i], .1f, i.ToString(), 2);
            }
            VisualDebug.Save();
        }

        public static void DebugDrawTriangles(Vector3[] vertices, int[] triangles)
        {
            DebugDrawVertices(vertices);
            
            VisualDebug.SetColour(Colours.lightGreen, Colours.darkGrey);

            for (int i = 0; i < triangles.Count(); i = i + 3)
            {
                VisualDebug.BeginFrame("Draw Triangle", true);
                VisualDebug.DrawArrow(vertices[triangles[i]], vertices[triangles[i+1]], .1f);
                VisualDebug.BeginFrame("Draw Triangle", true);
                VisualDebug.DrawArrow(vertices[triangles[i+1]], vertices[triangles[i+2]], .1f);
                VisualDebug.BeginFrame("Draw Triangle", true);
                VisualDebug.DrawArrow(vertices[triangles[i+2]], vertices[triangles[i]], .1f);
            }
            VisualDebug.Save();
        }



        public static void ResetDebug()
        {
            VisualDebug.Initialize();
            VisualDebug.Save();
        }
    }
}
