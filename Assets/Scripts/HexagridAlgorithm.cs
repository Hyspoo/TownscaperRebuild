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
            VisualDebug.Initialize();
            VisualDebug.SetColour(Colours.lightRed, Colours.veryDarkGrey);

            if (mSideSize < 2) throw (new ArgumentException("Generate fail! Your input size is less than 2."));

            List<Vector3> verticeList = new List<Vector3>();

            float sideLength = 0.5f * Mathf.Tan(Mathf.Deg2Rad * 60); // 0.5f* tanf(60deg)
            for (int x = 0; x < mSideSize * 2 - 1; ++x)
            {
                int height = (x < mSideSize) ? (mSideSize + x) : (mSideSize * 3 - 2 - x);
                float deltaHeight = mSideSize - height * 0.5f;
                for (int z = 0; z < height; z++)
                {
                    bool isSide = x == 0 || x == (mSideSize * 2 - 2) || z == 0 || z == height - 1;
                    verticeList.Add(new Vector3((x - mSideSize + 1) * sideLength, 0, z + deltaHeight));
                    
                    VisualDebug.BeginFrame("Draw point", true);
                    VisualDebug.DrawPoint(verticeList.Last(), .1f);
                }
            }

            Vector3[] vertices = verticeList.ToArray();

            //VisualDebug.Save();
            return vertices;
        }

        public static int[] GenerateTriangles(int mSideSize)
        {
            VisualDebug.Initialize();

            if (mSideSize < 2) throw (new ArgumentException("Generate fail! Your input size is less than 2."));

            List<int> TriangleList = new List<int>();
            Vector3[] vertices = GenerateVertices(mSideSize);

            VisualDebug.SetColour(Colours.lightGreen, Colours.darkGrey);

            int offset = 0;
            for (int x = 0; x < (mSideSize * 2 - 2); x++)
            {
                int height = (x < mSideSize) ? (mSideSize + x) : (mSideSize * 3 - 2 - x);
                if (x < mSideSize - 1)
                {
                    // left side
                    for (int y = 0; y < height; y++)
                    {
                        VisualDebug.BeginFrame("Draw Triangle", true);

                        TriangleList.Add(offset + y);
                        TriangleList.Add(offset + y + height);
                        TriangleList.Add(offset + y + height + 1);

                        VisualDebug.DrawArrow(vertices[offset + y], vertices[offset + y + height], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y + height], vertices[offset + y + height + 1], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y + height + 1], vertices[offset + y], .1f);

                        if (y >= height - 1)
                        {
                            break;
                        }

                        VisualDebug.BeginFrame("Draw Triangle", true);

                        TriangleList.Add(offset + y + height + 1);
                        TriangleList.Add(offset + y + 1);
                        TriangleList.Add(offset + y);

                        VisualDebug.DrawArrow(vertices[offset + y + height + 1], vertices[offset + y + 1], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y + 1], vertices[offset + y], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y], vertices[offset + y + height + 1], .1f);
                    }
                }
                else
                {
                    // right side
                    for (int y = 0; y < height - 1; y++)
                    {
                        VisualDebug.BeginFrame("Draw Triangle", true);
                        //TriangleList.Add(new Triangle(offset + y, offset + y + height, offset + y + 1));
                        TriangleList.Add(offset + y);
                        TriangleList.Add(offset + y + height);
                        TriangleList.Add(offset + y + 1);

                        VisualDebug.DrawArrow(vertices[offset + y], vertices[offset + y + height], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y + height], vertices[offset + y + 1], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y + 1], vertices[offset + y], .1f);

                        if (y >= height - 2)
                        {
                            break;
                        }

                        VisualDebug.BeginFrame("Draw Triangle", true);
                        //TriangleList.Add(new Triangle(offset + y + 1, offset + y + height, offset + y + height + 1));
                        TriangleList.Add(offset + y + 1);
                        TriangleList.Add(offset + y + height);
                        TriangleList.Add(offset + y + height + 1);

                        VisualDebug.DrawArrow(vertices[offset + y + 1], vertices[offset + y + height], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y + height], vertices[offset + y + height + 1], .1f);
                        VisualDebug.DrawArrow(vertices[offset + y + height + 1], vertices[offset + y + 1], .1f);
                    }
                }
                offset += height;
            }

            int[] triangles = TriangleList.ToArray();

            VisualDebug.Save();
            return triangles;
        }
    }
}
