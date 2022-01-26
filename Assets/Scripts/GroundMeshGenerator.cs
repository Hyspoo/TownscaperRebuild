using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VisualDebugging.Example;

public class GroundMeshGenerator : MonoBehaviour
{
    public GridGenerator mGridGenerator;

    public Material mDetectorMaterial;

    public int pointIndexInput; // Temp

    List<Point> mPoints;
    List<Quad> mQuads;
    Neighbours[] mNeighbours;
    List<Quad>[] sharedByWhichQuads;
    List<Vector3>[] mBottomVertex; // used for mesh generation

    Camera viewCamera;

    GameObject lastMouseHoverDetector;
    GameObject currentMouseHoverDetector;
    bool onHit = false;

    Vector3 LMBPressedPos;
    Vector3 RMBPressedPos;
    float LMBPressedTime;
    float RMBPressedTime;

    Color detectorColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    Color detectorHighlighted = Color.yellow; //new Color(0.5f, 0.5f, 0.5f, 0.8f);

    float layerHeight = 0.5f;

    void Initialize()
    {
        mPoints = mGridGenerator.GetPoints();
        mQuads = mGridGenerator.GetQuads();
        mNeighbours = mGridGenerator.GetNeighbours();

        sharedByWhichQuads = new List<Quad>[mPoints.Count];
        mBottomVertex = new List<Vector3>[mPoints.Count];

        // Initialize each array item to an empty List<Quad> ( if not, all the item will be null !!! )
        for (int i = 0; i < mPoints.Count; i++)
        {
            sharedByWhichQuads[i] = new List<Quad>();
            mBottomVertex[i] = new List<Vector3>();
        }

        foreach (Quad q in mQuads)
        {
            sharedByWhichQuads[q.mA].Add(q);
            sharedByWhichQuads[q.mB].Add(q);
            sharedByWhichQuads[q.mC].Add(q);
            sharedByWhichQuads[q.mD].Add(q);
        }

        // put midpoint of each neighbouring edge, and center of each neighbouring Quad together
        for (int i = 0; i < mPoints.Count; i++)
        {
            List<Vector3> vertexList = new List<Vector3>(); 
            foreach (Quad q in sharedByWhichQuads[i])
            {
                vertexList.Add(GetMidpos(q));
            }
            foreach (int j in mNeighbours[i].mNeighbour)
            {
                vertexList.Add( (mPoints[i].mPosition + mPoints[j].mPosition) / 2 );
            }
            
            // Reorder positions in anti-clockwise
            void SwitchVec(int a, int b)
            {
                Vector3 temp = vertexList[a];
                vertexList[a] = vertexList[b];
                vertexList[b] = temp;
            }
            for (int m = 0; m < vertexList.Count; m++)
            {
                for (int n = vertexList.Count - 1; n > m; n--)
                {
                    Vector3 a = (vertexList[n-1] - mPoints[i].mPosition).normalized;
                    Vector3 b = (vertexList[n] - mPoints[i].mPosition).normalized;
                    if (a.z >= 0)
                    {
                        if (b.z >= 0)
                        {
                            if (a.x < b.x) SwitchVec(n-1, n);
                        }
                    }
                    else
                    {
                        if (b.z < 0)
                        {
                            if (a.x > b.x) SwitchVec(n-1, n);
                        }
                        else SwitchVec(n-1, n);
                    }
                }
            }

            // Scale down by 0.9
            /*
            for (int j = 0; j < vertexList.Count; j++)
            {
                vertexList[j] = vertexList[j] + (mPoints[i].mPosition - vertexList[j]) * 0.1f;
            }*/

            vertexList.Insert(0, mPoints[i].mPosition);
            mBottomVertex[i] = vertexList;
        }
    }

    void GenerateGroundDetectors()
    {
        DestroyImmediate(GameObject.Find("Ground Detectors"));
        Transform t = new GameObject("Ground Detectors").GetComponent<Transform>();

        for (int i = 0; i < mPoints.Count; i++)
        {
            if (mPoints[i].mSide) continue;
            GenerateHorizonalDetector(i, 0, true).GetComponent<Transform>().parent = t;
        }
    }

    // Generate on ground, always y = 0
    GameObject GenerateHorizonalDetector(int pointIndex, int layer, bool isUp)
    {
        int layerOffset = isUp ? 1 : -1;
        GameObject obj = new GameObject ($"Detector {pointIndex} {layer + layerOffset}");
        obj.layer = 6;
		MeshFilter meshFilter = obj.AddComponent<MeshFilter> ();
        MeshCollider meshCollider = obj.AddComponent<MeshCollider> ();

        // Create mesh
		Mesh mesh = new Mesh ();
		meshFilter.mesh = mesh;
        
		MeshRenderer renderer = obj.AddComponent<MeshRenderer> ();
		// Add standard material
		Material mat = mDetectorMaterial;
		mat.color = detectorColor;
		renderer.material = mat;

        // Create vertices and UV
		Vector3[] vertices = mBottomVertex[pointIndex].ToArray();
		Vector2[] uv = new Vector2[vertices.Count()];

        mesh.vertices = vertices;
		mesh.uv = uv;

        // Triangle index
        int[] triangles = new int[(vertices.Count() - 1) * 3];
        for (int j = 0; j < vertices.Count() - 1; j++)
        {
            triangles[j*3] = 0;
            triangles[j*3 + 2] = j + 1;
            if (j != vertices.Count() - 2) { triangles[j*3 + 1] = j + 2; }
            else { triangles[j*3 + 1] = 1; }
        }

        mesh.triangles = triangles;
		mesh.RecalculateNormals ();

        meshCollider.sharedMesh = mesh;

        return obj;
    }

    GameObject GenerateSideDetector(int pointIndex, int target, int layer)
    {
        GameObject obj = new GameObject ($"Detector {target} {layer}");
        obj.layer = 6;
		MeshFilter meshFilter = obj.AddComponent<MeshFilter> ();
        MeshCollider meshCollider = obj.AddComponent<MeshCollider> ();

        // Create mesh
		Mesh mesh = new Mesh ();
		meshFilter.mesh = mesh;
        
		MeshRenderer renderer = obj.AddComponent<MeshRenderer> ();
		// Add standard material
		Material mat = mDetectorMaterial;
		mat.color = detectorColor;
		renderer.material = mat;

        // Create vertices and UV
		Vector3[] vertices = FindSideDetectorVertices(pointIndex, target).ToArray();
		Vector2[] uv = new Vector2[vertices.Count()];

        mesh.vertices = vertices;
		mesh.uv = uv;

        // Triangle index
        int[] triangles = new int[12] {0, 1, 3, 1, 4, 3, 1, 2, 4, 2, 5, 4};

        mesh.triangles = triangles;
		mesh.RecalculateNormals ();

        meshCollider.sharedMesh = mesh;

        return obj;
    }

    void AddBlock(GameObject detector)
    {
        Debug.Log("AddBlock entered!");

        string[] info = detector.name.Split(' ');

        int pointIndex = int.Parse(info[1]);
        int layer = int.Parse(info[2]);

        if (layer <= 0 || mPoints[pointIndex].mSide) return;

        Transform block = new GameObject($"Block {pointIndex} {layer}").GetComponent<Transform>();

        var upBlockDetector = GameObject.Find($"Block {pointIndex} {layer + 1}/Detector {pointIndex} down");
        if (upBlockDetector == null)
        {
            Transform upDetector = GenerateHorizonalDetector(pointIndex, layer, true).GetComponent<Transform>();
            upDetector.parent = block;
            upDetector.Translate(Vector3.up * layerHeight / 2);
        }
        else
        {
            Destroy(upBlockDetector);
        }
        
        var downBlockDetector = GameObject.Find($"Block {pointIndex} {layer - 1}/Detector {pointIndex} up");
        if (downBlockDetector == null)
        {
            Transform downDetector = GenerateHorizonalDetector(pointIndex, layer, false).GetComponent<Transform>();
            downDetector.parent = block;
            downDetector.Translate(Vector3.down * layerHeight / 2);
        }
        else
        {
            Destroy(downBlockDetector);
        }
        
        foreach (int neighbourIndex in mNeighbours[pointIndex].mNeighbour)
        {
            var sideBlockDetector = GameObject.Find($"Block {neighbourIndex} {layer}/Detector {pointIndex} {layer}");
            if (sideBlockDetector == null)
            {
                Transform sideDetector = GenerateSideDetector(pointIndex, neighbourIndex, layer).GetComponent<Transform>();
                sideDetector.parent = block;
            }
            else
            {
                Destroy(sideBlockDetector);
            }
        }

        block.Translate(Vector3.up * layerHeight * (layer - 0.5f));
    }

    void DeleteBlock(GameObject detector)
    {
        Debug.Log("PONG! block deleted!");
    }

    public void GroundMeshDebug()
    {
        Initialize();
        GridDebug.DebugDrawQuads(mPoints, mQuads);
        GenerateGroundDetectors();
    }

    // Helping Methods
    Vector3 GetMidpos(Quad mQuad)
    {
        Vector3 posA = mPoints[mQuad.mA].mPosition;
        Vector3 posB = mPoints[mQuad.mB].mPosition;
        Vector3 posC = mPoints[mQuad.mC].mPosition;
        Vector3 posD = mPoints[mQuad.mD].mPosition;
        return (posA + posB + posC + posD) / 4;
    }
    
    List<Vector3> FindSideDetectorVertices(int pointIndex, int target)
    {
        List<Vector3> sideVerticeList = new List<Vector3>();
        foreach (Quad q in sharedByWhichQuads[pointIndex])
        {
            if (target == q.mA || target == q.mB || target == q.mC || target == q.mD) 
            {
                sideVerticeList.Add(GetMidpos(q));
            }
        }
        
        if (sideVerticeList.Count != 2) throw (new Exception($"Fail to find sideVertices! Expected 2 target Quads, but get {sideVerticeList.Count}"));

        // make sure vertices are anti-clockwise
        Vector3 sideVector = sideVerticeList[1] - sideVerticeList[0];
        Vector3 targetVector = mPoints[target].mPosition - mPoints[pointIndex].mPosition;
        if (Vector3.Cross(targetVector, sideVector).y < 0)
        {
            Vector3 temp = sideVerticeList[1];
            sideVerticeList[1] = sideVerticeList[0];
            sideVerticeList[0] = temp;
        }

        sideVerticeList.Insert(1, (mPoints[target].mPosition + mPoints[pointIndex].mPosition) / 2);

        List<Vector3> dualList = new List<Vector3>();
        foreach (Vector3 pos in sideVerticeList)
        {
            dualList.Add(pos + Vector3.down * layerHeight / 2);
        }
        foreach (Vector3 pos in sideVerticeList)
        {
            dualList.Add(pos + Vector3.up * layerHeight / 2);
        }

        return dualList;
    }

    void Awake()
    {
        mGridGenerator.AllInOne();
        Initialize();
        GenerateGroundDetectors();
        viewCamera = Camera.main;
    }

    void Update()
    {
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, 1000f, 1 << 6))
        {
            if (onHit == false) {
                onHit = true;
                currentMouseHoverDetector = hitInfo.transform.gameObject;
                lastMouseHoverDetector = currentMouseHoverDetector;
                currentMouseHoverDetector.GetComponent<MeshRenderer>().material.color = detectorHighlighted;//currentMouseHoverDetector.GetComponent<Animator>().SetBool("MouseHover", true);
            } else {
                currentMouseHoverDetector = hitInfo.transform.gameObject;
                if (currentMouseHoverDetector != lastMouseHoverDetector) {
                    lastMouseHoverDetector.GetComponent<MeshRenderer>().material.color = detectorColor;//lastMouseHoverDetector.GetComponent<Animator>().SetBool("MouseHover", false);
                    currentMouseHoverDetector.GetComponent<MeshRenderer>().material.color = detectorHighlighted;//currentMouseHoverDetector.GetComponent<Animator>().SetBool("MouseHover", true);
                }
            }

            lastMouseHoverDetector = currentMouseHoverDetector;

            if (Input.GetMouseButtonDown(0)) {
                LMBPressedPos = Input.mousePosition;
                LMBPressedTime = Time.time;
            }
            if (Input.GetMouseButtonDown(1)) {
                RMBPressedPos = Input.mousePosition;
                RMBPressedTime = Time.time;
            }
            if (Input.GetMouseButtonUp(1)) {
                float dT = Time.time - RMBPressedTime;
                float dP = (Input.mousePosition - RMBPressedPos).magnitude;
                if ((dT < 0.8f && dP < 30f) || dT < 0.4f || dP < 8f) {
                    AddBlock(currentMouseHoverDetector); //GenerateSingleCube(getCoordFromPosition(hitInfo.normal * cellSize + hitInfo.transform.position));
                }
                
            }
            if (Input.GetMouseButtonUp(0)) {
                float dT = Time.time - LMBPressedTime;
                float dP = (Input.mousePosition - LMBPressedPos).magnitude;
                if ((dT < 0.8f && dP < 30f) || dT < 0.4f || dP < 8f) {
                    DeleteBlock(currentMouseHoverDetector); //DeleteCube(getCoordFromPosition(hitInfo.transform.position));
                    onHit = false;
                }
            }

        } else {
            if (onHit == true) {
                onHit = false;
                currentMouseHoverDetector.GetComponent<MeshRenderer>().material.color = detectorColor;
                //currentMouseHoverDetector.GetComponent<Animator>().SetBool("MouseHover", false);
            }
        }
    }
}
