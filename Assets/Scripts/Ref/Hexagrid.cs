using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
/*
class Point
{
    public Point(float x, float y, bool s)
    {
        mPosition = new Vector2(x, y);
        mSide = s;
    }
    public Point(Vector2 p, bool s)
    {
        mPosition = p;
        mSide = s;
    }
    public Vector2 mPosition;
    public bool mSide;
};
*/


/*

[ExecuteInEditMode]
public class Hexagrid : MonoBehaviour
{
    [Range(2, 12)]
    public int mSideSize = 8;

    [Range(1, 20)]
    public int mSearchIterationCount = 12;

    [Range(0, 65535)]
    public int mSeed = 15911;

    private int mBaseQuadCount = 0;

    public bool bTriangulation = true;
    public bool bRemovingEdges = false;
    public bool bSubdivideFaces = false;

    [Range(1, 100)]
    public int iterNum = 10;
    public bool bRelax = false;
    public bool bReshape = false;
    public bool bDrawPositions = false;

    private List<Point> mPoints;
    private List<Triangle> mTriangles;
    private List<Quad> mQuads;
    private Neighbours[] mNeighbours;

    public Vector3[] getPoints()
    {
        Vector3[] pointsPos = new Vector3[mPoints.Count];
        for (int i = 0; i < mPoints.Count; i++)
        {
            pointsPos[i] = new Vector3(mPoints[i].mPosition.x, 0, mPoints[i].mPosition.y);
        }
        return pointsPos;
    }

    public int[] getTriangles()
    {
        int[] trianglesIndex = new int[mTriangles.Count];
        for (int i = 0; i < mTriangles.Count * 3; i++)
        {
            trianglesIndex[i*3] = mTriangles[i].mA;
            trianglesIndex[i*3 + 1] = mTriangles[i].mB;
            trianglesIndex[i*3 + 2] = mTriangles[i].mC;
        }
        return trianglesIndex;
    }

    void Triangulation()
    {
        mPoints = new List<Point>();
        mTriangles = new List<Triangle>();
        mQuads = new List<Quad>();
        mNeighbours = new Neighbours[0];

        if (mSideSize < 2)
        {
            return;
        }

        float sideLength = 0.5f * Mathf.Tan(Mathf.Deg2Rad * 60); // 0.5f* tanf(60deg)
        for (int x = 0; x < mSideSize * 2 - 1; ++x)
        {
            int height = (x < mSideSize) ? (mSideSize + x) : (mSideSize * 3 - 2 - x);
            float deltaHeight = mSideSize - height * 0.5f;
            for (int y = 0; y < height; y++)
            {
                bool isSide = x == 0 || x == (mSideSize * 2 - 2) || y == 0 || y == height - 1;
                mPoints.Add(new Point((x - mSideSize + 1) * sideLength, y + deltaHeight, isSide));
            }
        }

        int offset = 0;
        for (int x = 0; x < (mSideSize * 2 - 2); x++)
        {
            int height = (x < mSideSize) ? (mSideSize + x) : (mSideSize * 3 - 2 - x);
            if (x < mSideSize - 1)
            {
                // left side
                for (int y = 0; y < height; y++)
                {
                    mTriangles.Add(new Triangle(offset + y, offset + y + height, offset + y + height + 1));
                    if (y >= height - 1)
                    {
                        break;
                    }
                    mTriangles.Add(new Triangle(offset + y + height + 1, offset + y + 1, offset + y));
                }
            }
            else
            {
                // right side
                for (int y = 0; y < height - 1; y++)
                {
                    mTriangles.Add(new Triangle(offset + y, offset + y + height, offset + y + 1));
                    if (y >= height - 2)
                    {
                        break;
                    }
                    mTriangles.Add(new Triangle(offset + y + 1, offset + y + height, offset + y + height + 1));
                }
            }
            offset += height;
        }
    }

    private int[] GetAdjacentTriangles(int triIndex)
    {
        List<int> adjacents = new List<int>();

        int[] lhs = new int[3] {
            mTriangles[triIndex].mA, mTriangles[triIndex].mB, mTriangles[triIndex].mC,
        };

        for (int otherIndex = 0; otherIndex < mTriangles.Count; ++otherIndex)
        {
            if (otherIndex == triIndex || !mTriangles[otherIndex].mValid)
            {
                continue;
            }
            int[] rhs = new int[3] {
                mTriangles[otherIndex].mA, mTriangles[otherIndex].mB, mTriangles[otherIndex].mC,
            };

            int shareCount = 0;
            for (int l = 0; l < 3; l++)
            {
                for (int r = 0; r < 3; r++)
                {
                    if (lhs[l] == rhs[r])
                    {
                        shareCount++;
                        break;
                    }
                }
            }
            Debug.Assert(shareCount < 3);
            if (shareCount == 2)
            {
                Debug.Assert(adjacents.Count < 3);
                adjacents.Add(otherIndex);
            }
        }
        return adjacents.ToArray();
    }

    void RemovingEdges()
    {
        // triangles to quads
        System.Random rand = new System.Random(mSeed);
        while (true)
        {
            int triIndex;
            int searchCount = 0;
            do
            {
                triIndex = rand.Next() % mTriangles.Count;
                searchCount++;
            } while (searchCount < mSearchIterationCount && !mTriangles[triIndex].mValid);

            if (searchCount == mSearchIterationCount)
            {
                break;
            }

            int[] adjacents = GetAdjacentTriangles(triIndex);
            if (adjacents.Length > 0)
            {
                int i1 = triIndex;
                int i2 = adjacents[0];
                int[] indices = new int[6] {
                    mTriangles[i1].mA, mTriangles[i1].mB, mTriangles[i1].mC,
                    mTriangles[i2].mA, mTriangles[i2].mB, mTriangles[i2].mC
                };

                Array.Sort(indices);
                int[] unique = indices.Distinct().ToArray();
                Debug.Assert(unique.Length == 4);

                mQuads.Add(new Quad(unique[0], unique[2], unique[3], unique[1]));
                mTriangles[triIndex].mValid = false; ;
                mTriangles[adjacents[0]].mValid = false;
            }
        }
        this.mBaseQuadCount = mQuads.Count();
    }

    void Subdivide(int[] indices, Dictionary<UInt32, int> middles)
    {
        // Example: indices -> Point{4, 5, 6, 7}
        int count = indices.Length;
        int[] halfSegmentIndex = new int[count];

        // add midpoint on face center to global mPoint
        int indexCenter = mPoints.Count;
        {
            Vector2 ptCenter = Vector2.zero;
            foreach (int i in indices)
            {
                ptCenter += mPoints[i].mPosition;
            }
            ptCenter /= count;
            mPoints.Add(new Point(ptCenter, false));
        }

        for (int x = 0; x < count; ++x)
        {
            int indexA = indices[x];
            int indexB = indices[(x + 1) % count];

            // Point{4, 5, 6, 7} -> key{4 5, 5 6, 6 7, 4 7}
            UInt32 key = Convert.ToUInt32((Mathf.Min(indexA, indexB) << 16) + Mathf.Max(indexA, indexB));

            // add midpoint on each side
            if (!middles.ContainsKey(key))
            {
                middles[key] = mPoints.Count;
                bool isSide = mPoints[indexA].mSide && mPoints[indexB].mSide;
                mPoints.Add(new Point((mPoints[indexA].mPosition + mPoints[indexB].mPosition) * 0.5f, isSide));
            }
            halfSegmentIndex[x] = middles[key];
        }

        // add quad: faceMid -> edgeMid1 -> vertice -> edgeMid2
        for (int x = 0; x < count; ++x)
        {
            int indexA = x;
            int indexB = (x + 1) % count;
            mQuads.Add(new Quad(indexCenter, halfSegmentIndex[indexA], indices[indexB], halfSegmentIndex[indexB]));
        }
    }

    void SubdivideFaces()
    {
        Dictionary<UInt32, int> middles = new Dictionary<UInt32, int>();

        // quads to 4 quads
        for (int i = 0; i < mBaseQuadCount; i++)
        {
            var quad = mQuads[i];
            int[] indices = new int[4] {
                quad.mA, quad.mB, quad.mC, quad.mD
            };
            this.Subdivide(indices, middles);
        }

        // triangles to 3 quads
        foreach (var triangle in mTriangles)
        {
            if (triangle.mValid)
            {
                int[] indices = new int[3] {
                    triangle.mA, triangle.mB, triangle.mC
                };
                this.Subdivide(indices, middles);
            }
        }
    }

    void Relax()
    {
        mNeighbours = new Neighbours[mPoints.Count];
        for (int i = 0; i < mPoints.Count; ++i)
        {
            mNeighbours[i] = new Neighbours();
        }
        for (int i = mBaseQuadCount; i < mQuads.Count(); ++i)
        {
            var quad = mQuads[i];
            int[] indices = new int[4] {
                quad.mA, quad.mB, quad.mC, quad.mD
            };
            for (int j = 0; j < 4; j++)
            {
                int index1 = indices[j];
                int index2 = indices[(j + 1) & 3];
                {
                    var neighbour = mNeighbours[index1];
                    // check
                    bool good = true;
                    for (int k = 0; k < neighbour.count; k++)
                    {
                        if (neighbour.mNeighbour[k] == index2)
                        {
                            good = false;
                            break;
                        }
                    }
                    if (good)
                    {
                        Debug.Assert(neighbour.count < 6);
                        neighbour.Add(index2);
                    }
                }
                {
                    var neighbour = mNeighbours[index2];
                    // check
                    bool good = true;
                    for (int k = 0; k < neighbour.count; k++)
                    {
                        if (neighbour.mNeighbour[k] == index1)
                        {
                            good = false;
                            break;
                        }
                    }
                    if (good)
                    {
                        Debug.Assert(neighbour.count < 6);
                        neighbour.Add(index1);
                    }
                }
            }
        }

        for (int i = 0; i < mPoints.Count; i++)
        {
            if (mPoints[i].mSide)
            {
                continue;
            }
            var neighbour = mNeighbours[i];
            Vector2 sum = Vector2.zero;
            for (int j = 0; j < neighbour.count; j++)
            {
                sum += mPoints[neighbour.mNeighbour[j]].mPosition;
            }
            sum /= (float)neighbour.count;
            mPoints[i].mPosition = sum;
        }
    }

    void Reshape()
    {
        float radius = mSideSize - 1.0f;
        Vector2 center = new Vector2(0, (mSideSize * 2 - 1) * 0.5f);

        // for (int i = 0; i < mPoints.size(); i++) {
        foreach (var point in mPoints)
        {
            if (!point.mSide)
            {
                continue;
            }
            Vector2 D = point.mPosition - center;
            float distance = radius - Mathf.Sqrt(D.x * D.x + D.y * D.y);
            point.mPosition += (D * distance) * 0.1f;
        }
    }

    private void DrawLine(int a, int b)
    {
        Vector3 posA = new Vector3(mPoints[a].mPosition.x, 0 ,mPoints[a].mPosition.y);
        Vector3 posB = new Vector3(mPoints[b].mPosition.x, 0 ,mPoints[b].mPosition.y);
        Gizmos.DrawLine(posA, posB);
    }

    void OnDrawGizmos()
    {
        bool hidePoint = !bDrawPositions;
        bool hideSector = bTriangulation && bRemovingEdges && bSubdivideFaces && bRelax;

        Gizmos.color = Color.yellow;

        if (!hidePoint)
        {
            foreach (var point in mPoints)
            {
                Vector3 pos = new Vector3(point.mPosition.x, 0, point.mPosition.y);
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }

        Gizmos.color = Color.green;

        if (!hideSector)
        {
            foreach (var tri in mTriangles)
            {
                if (tri.mValid)
                {
                    DrawLine(tri.mA, tri.mB);
                    DrawLine(tri.mB, tri.mC);
                    DrawLine(tri.mC, tri.mA);
                }
            }

            foreach (var quad in mQuads)
            {
                DrawLine(quad.mA, quad.mB);
                DrawLine(quad.mB, quad.mC);
                DrawLine(quad.mC, quad.mD);
                DrawLine(quad.mD, quad.mA);
            }
        }
        else
        {
            for (int i = 0; i < mPoints.Count(); i++)
            {
                var neighbour = mNeighbours[i];
                for (int j = 0; j < neighbour.count; j++)
                {
                    DrawLine(i, neighbour.mNeighbour[j]);
                }
            }
        }
    }

    private void OnValidate()
    {
        mPoints = new List<Point>();
        mTriangles = new List<Triangle>();
        mQuads = new List<Quad>();
        mNeighbours = new Neighbours[0];

        if (bTriangulation)
        {
            this.Triangulation();

            if (bRemovingEdges)
            {
                this.RemovingEdges();

                if (bSubdivideFaces)
                {
                    this.SubdivideFaces();

                    if (bRelax)
                    {
                        if (bReshape)
                        {
                            for (int i = 0; i < iterNum; i++)
                            {
                                this.Reshape();
                                this.Relax();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < iterNum; i++)
                            {
                                this.Relax();
                            }
                        }
                        
                    }
                }
            }
        }
        
        
        
    }

}
*/