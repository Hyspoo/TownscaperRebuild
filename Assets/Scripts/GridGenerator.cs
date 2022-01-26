using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VisualDebugging.Example;

public class Point
{
    public Point(float x, float y, float z, bool s)
    {
        mPosition = new Vector3(x, y, z);
        mSide = s;
    }
    public Point(Vector3 p, bool s)
    {
        mPosition = p;
        mSide = s;
    }
    public Vector3 mPosition;
    public bool mSide;
};

public class Triangle
{
    public Triangle(int a, int b, int c)
    {
        mA = a;
        mB = b;
        mC = c;
        mValid = true;
    }
    public int mA, mB, mC;
    public bool mValid;
};

public class Quad
{
    public Quad(int a, int b, int c, int d)
    {
        mA = a;
        mB = b;
        mC = c;
        mD = d;
    }
    public int mA, mB, mC, mD;
};

public class Neighbours
{
    public Neighbours()
    {
        mNeighbour = new List<int>();
    }
    public void Add(int i)
    {
        mNeighbour.Add(i);
    }
    public int count
    {
        get
        {
            return mNeighbour.Count;
        }
    }
    public List<int> mNeighbour;
};

public class GridGenerator : MonoBehaviour
{
	[Range(2, 12)]
    public int mSideSize = 6;

    [Range(1, 20)]
    public int mSearchIterationCount = 12;

    [Range(0, 65535)]
    public int mSeed = 15911;

    //private int mBaseQuadCount = 0;

    [Range(1, 100)]
    public int iterNum = 20;

    public int debugPointIndex;

    private List<Point> mPoints;
    private List<Triangle> mTriangles;
    private List<Quad> mBaseQuads;
    private List<Quad> mSubQuads;
    private Neighbours[] mNeighbours;

    private GameObject centerLocator;
	
	public void InitializeHexagrid()
	{
		ResetGrid();

		if (mSideSize < 2) throw (new ArgumentException("Generate fail! Your input size is less than 2."));

		float sideLength = 0.5f * Mathf.Tan(Mathf.Deg2Rad * 60); // 0.5f* tanf(60deg)

		// Set Points
		for (int x = 0; x < mSideSize * 2 - 1; ++x)
        {
            int height = (x < mSideSize) ? (mSideSize + x) : (mSideSize * 3 - 2 - x);
            float deltaHeight = mSideSize - height * 0.5f;
            for (int z = 0; z < height; z++)
            {
                bool isSide = x == 0 || x == (mSideSize * 2 - 2) || z == 0 || z == height - 1;
                mPoints.Add(new Point((x - mSideSize + 1) * sideLength, 0, z + deltaHeight, isSide));
            }
        }

		// Set Triangles
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

        DestroyImmediate(GameObject.Find("CenterLocator"));
        centerLocator = new GameObject("CenterLocator");
        centerLocator.GetComponent<Transform>().Translate(mPoints[(mPoints.Count - 1) / 2].mPosition);
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

	public void RemovingEdges()
	{
		// triangles to quads
        System.Random rand = new System.Random(mSeed);
        while (true)
        {
            int triIndex;
            int searchCount = 0;
            do
            {
                triIndex = rand.Next() % mTriangles.Count();
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

                mBaseQuads.Add(new Quad(unique[0], unique[2], unique[3], unique[1]));
                mTriangles[triIndex].mValid = false; ;
                mTriangles[adjacents[0]].mValid = false;
            }
        }
        //this.mBaseQuadCount = mBaseQuads.Count();
	}

	void Subdivide(int[] indices, Dictionary<UInt32, int> middles)
    {
        // Example: indices -> Point{4, 5, 6, 7}
        int count = indices.Length;
        int[] halfSegmentIndex = new int[count];

        // add midpoint on face center to global mPoint
        int indexCenter = mPoints.Count;
        {
            Vector3 ptCenter = Vector3.zero;
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
            mSubQuads.Add(new Quad(indexCenter, halfSegmentIndex[indexA], indices[indexB], halfSegmentIndex[indexB]));
        }
    }

    public void SubdivideFaces()
    {
        Dictionary<UInt32, int> middles = new Dictionary<UInt32, int>();

        // quads to 4 quads
        for (int i = 0; i < mBaseQuads.Count(); i++)
        {
            var quad = mBaseQuads[i];
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

    void MovePointsToNeighboursAverage()
    {
        mNeighbours = new Neighbours[mPoints.Count];
        for (int i = 0; i < mPoints.Count; ++i)
        {
            mNeighbours[i] = new Neighbours();
        }
        for (int i = 0; i < mSubQuads.Count(); ++i)
        {
            var quad = mSubQuads[i];
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
            Vector3 sum = Vector3.zero;
            for (int j = 0; j < neighbour.count; j++)
            {
                sum += mPoints[neighbour.mNeighbour[j]].mPosition;
            }
            sum /= (float)neighbour.count;
            mPoints[i].mPosition = sum;
        }
    }

	public void Relax()
	{
		for (int i = 0; i < iterNum; i++)
        {
            this.MovePointsToNeighboursAverage();
        }
	}

    void MoveEdgePointsToCircle()
    {
        float radius = mSideSize - 1.0f;
        Vector3 center = new Vector3(0, 0, (mSideSize * 2 - 1) * 0.5f);

        // for (int i = 0; i < mPoints.size(); i++) {
        foreach (var point in mPoints)
        {
            if (!point.mSide) continue;
			
            Vector3 D = point.mPosition - center;
            float distance = radius - Mathf.Sqrt(D.x * D.x + D.z * D.z);
            point.mPosition += (D * distance) * 0.1f;
        }
    }

	public void Reshape()
	{
		for (int i = 0; i < iterNum; i++)
        {
            this.MoveEdgePointsToCircle();
            this.MovePointsToNeighboursAverage();
        }
	}
    
    public void AllInOne()
    {
        InitializeHexagrid();
        RemovingEdges();
        SubdivideFaces();
        Relax();
        Reshape();
    }
    
    public List<Point> GetPoints() { return mPoints; }
    public List<Quad> GetQuads() { return mSubQuads; }
    public Neighbours[] GetNeighbours() { return mNeighbours; }

	public void ResetGrid()
	{
		mPoints = new List<Point>();
        mTriangles = new List<Triangle>();
        mBaseQuads = new List<Quad>();
        mSubQuads = new List<Quad>();
        mNeighbours = new Neighbours[0];
	}

    public void SetParamsToDefault()
    {
        mSideSize = 6;
        mSearchIterationCount = 12;
        mSeed = 15911;
        iterNum = 20;
    }

	public void pointsDebug()
	{
		GridDebug.DebugDrawPoints(mPoints);
	}

	public void trianglesDebug()
	{
		GridDebug.DebugDrawTriangles(mPoints, mTriangles);
	}

    public void quadsDebug()
	{
		GridDebug.DebugDrawQuads(mPoints, mBaseQuads);
	}

    public void subQuadsDebug()
	{
		GridDebug.DebugDrawQuads(mPoints, mSubQuads);
	}

    public void neighboursDebug()
    {
        if (debugPointIndex < 0 || debugPointIndex >= mNeighbours.Count())
        {
            Debug.Log("Find Neighbours fail! Index " + debugPointIndex.ToString() + " is out of bounds!");
            return;
        }

        GridDebug.DebugDrawNeighbours(mPoints, debugPointIndex, mNeighbours[debugPointIndex]);
    }

}
