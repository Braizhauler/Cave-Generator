using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {
	public SquareGrid squareGrid;
	public MeshFilter cave;
	public MeshFilter walls;
	public bool is2D;

	List<Vector3> vertices;
	List<int> triangles;



	Dictionary <int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();

	List<List<int>>outlines = new List<List<int>>();
	HashSet<int> checkedVertices = new HashSet<int> ();

	public void GenerateMesh(int[,] map, float squareSize ) {
		outlines.Clear ();
		checkedVertices.Clear ();
		triangleDictionary.Clear ();
		squareGrid = new SquareGrid (map, squareSize);

		vertices = new List<Vector3>();
		triangles = new List<int>();

		for (int x = 0; x < squareGrid.squares.GetLength (0); x++) {
			for (int y = 0; y < squareGrid.squares.GetLength (1); y++) {
				TriangulateSquare (squareGrid.squares [x, y]);
			}
		}
		Mesh mesh = new Mesh ();
		cave.mesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		if (is2D) {
			Generate2DColliders ();
		} else {
			CreateWallMesh ();
		}
	}

	void Generate2DColliders ()	{
		EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D> ();
		for (int i = 0; i < currentColliders.Length; ++i) {
			Destroy (currentColliders [i]);
		}

		CalculateMeshOutlines ();

		foreach (List<int> outline in outlines)	{
			EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D> ();
			Vector2[] edgePoints = new Vector2[outline.Count];
			for (int i = 0; i < outline.Count; ++i) {
				edgePoints[i] = new Vector2(vertices [outline [i]].x, vertices [outline [i]].z);
			}
			edgeCollider.points = edgePoints;
		}
	}

	void CreateWallMesh()	{
		CalculateMeshOutlines ();

		List<Vector3> wallVertices = new List<Vector3> ();
		List<int> wallTriangles = new List<int> ();
		Mesh wallMesh = new Mesh ();
		float wallHeight = 5f;

		foreach (List<int> outline in outlines) {
			for (int i = 0; i < outline.Count - 1; ++i) {
				int startIndex = wallVertices.Count;

				wallVertices.Add (vertices [outline [i]]);
				wallVertices.Add (vertices [outline [i+1]]);
				wallVertices.Add (vertices [outline [i+1]] + Vector3.down * wallHeight);
				wallVertices.Add (vertices [outline [i]] + Vector3.down * wallHeight);

				wallTriangles.Add (startIndex + 0);
				wallTriangles.Add (startIndex + 2);
				wallTriangles.Add (startIndex + 1);

				wallTriangles.Add (startIndex + 0);
				wallTriangles.Add (startIndex + 3);
				wallTriangles.Add (startIndex + 2);
			}
		}
		wallMesh.vertices = wallVertices.ToArray ();
		wallMesh.triangles = wallTriangles.ToArray ();
		walls.mesh = wallMesh;


		MeshCollider wallCollider= walls.gameObject.AddComponent<MeshCollider> ();
		wallCollider.sharedMesh = wallMesh;

	}

	void TriangulateSquare(Square square) 	{
		switch (square.configuration) {
		case 0:
			break;
		//1 point
		case 1:
			MeshFromPoints (square.west, square.south, square.southWest);
			//checkedVertices.Add (square.southWest.vertexIndex);
			break;
		case 2:
			MeshFromPoints (square.southEast, square.south, square.east);
			//checkedVertices.Add (square.southEast.vertexIndex);
			break;
		case 4:
			MeshFromPoints (square.northEast, square.east, square.north);
			//checkedVertices.Add (square.northEast.vertexIndex);
			break;
		case 8:
			MeshFromPoints (square.west, square.northWest, square.north);
			//checkedVertices.Add (square.northWest.vertexIndex);
			break;
		//2 point adjacent
		case 3:
			MeshFromPoints (square.east, square.southEast, square.southWest, square.west);
			//checkedVertices.Add (square.southWest.vertexIndex);
			//checkedVertices.Add (square.southEast.vertexIndex);
			break;
		case 6:
			MeshFromPoints (square.north, square.northEast, square.southEast, square.south);
			//checkedVertices.Add (square.southEast.vertexIndex);
			//checkedVertices.Add (square.northEast.vertexIndex);
			break;
		case 9:
			MeshFromPoints (square.northWest, square.north, square.south, square.southWest);
			//checkedVertices.Add (square.southWest.vertexIndex);
			//checkedVertices.Add (square.northWest.vertexIndex);
			break;
		case 12:
			MeshFromPoints (square.northWest, square.northEast, square.east, square.west);
			//checkedVertices.Add (square.northEast.vertexIndex);
			//checkedVertices.Add (square.northWest.vertexIndex);
			break;
		//2 point cross
		case 5:
			MeshFromPoints (square.northEast, square.east, square.south, square.southWest, square.west, square.north);
			//checkedVertices.Add (square.southWest.vertexIndex);
			//checkedVertices.Add (square.northEast.vertexIndex);
			break;
		case 10:
			MeshFromPoints (square.northWest, square.north, square.east, square.southEast, square.south, square.west);
			//checkedVertices.Add (square.southEast.vertexIndex);
			//checkedVertices.Add (square.northWest.vertexIndex);
			break;
		//3 point
		case 7:
			MeshFromPoints (square.southEast, square.southWest, square.west, square.north, square.northEast);
			//checkedVertices.Add (square.southWest.vertexIndex);
			//checkedVertices.Add (square.southEast.vertexIndex);
			//checkedVertices.Add (square.northEast.vertexIndex);
			break;
		case 11:
			MeshFromPoints (square.southWest, square.northWest, square.north, square.east, square.southEast);
			//checkedVertices.Add (square.southWest.vertexIndex);
			//checkedVertices.Add (square.southEast.vertexIndex);
			//checkedVertices.Add (square.northWest.vertexIndex);
			break;
		case 13:
			MeshFromPoints (square.northWest, square.northEast, square.east, square.south, square.southWest);
			//checkedVertices.Add (square.southWest.vertexIndex);
			//checkedVertices.Add (square.northEast.vertexIndex);
			//checkedVertices.Add (square.northWest.vertexIndex);
			break;
		case 14:
			MeshFromPoints (square.northEast, square.southEast, square.south, square.west, square.northWest);
			//checkedVertices.Add (square.southEast.vertexIndex);
			//checkedVertices.Add (square.northEast.vertexIndex);
			//checkedVertices.Add (square.northWest.vertexIndex);
			break;
		//4 points
		case 15:
			MeshFromPoints (square.northWest, square.northEast, square.southEast, square.southWest);
			checkedVertices.Add (square.southWest.vertexIndex);
			checkedVertices.Add (square.southEast.vertexIndex);
			checkedVertices.Add (square.northEast.vertexIndex);
			checkedVertices.Add (square.northWest.vertexIndex);
			break;
			
		}
	}

	//creates a fan mesh from points
	void MeshFromPoints(params Node[] points)	{
		AssignVertices (points);
		for(int i=2; i<points.Length; ++i)	{
			CreateTriangle (points [0], points [i-1], points [i]);
		}
	}

	void AssignVertices(Node[] points)	{
		for (int i = 0; i < points.Length; ++i) {
			if (points [i].vertexIndex == -1) {
				points [i].vertexIndex = vertices.Count;
				vertices.Add (points [i].position);
			}
		}
	}

	void CreateTriangle(Node a, Node b, Node c)	{
		triangles.Add (a.vertexIndex);
		triangles.Add (b.vertexIndex);
		triangles.Add (c.vertexIndex);

		Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);
		AddTriangleToDictionary (triangle.vertexIndexA, triangle);
		AddTriangleToDictionary (triangle.vertexIndexB, triangle);
		AddTriangleToDictionary (triangle.vertexIndexC, triangle);
	}

	void AddTriangleToDictionary(int vertexIndexKey,Triangle triangle)	{
		if (triangleDictionary.ContainsKey (vertexIndexKey)) {
			triangleDictionary [vertexIndexKey].Add (triangle);
		} else {
			List<Triangle> triangleList = new List<Triangle> ();
			triangleList.Add (triangle);
			triangleDictionary.Add (vertexIndexKey, triangleList);
		}
	}

	void CalculateMeshOutlines()	{
		for (int vertexIndex = 0; vertexIndex < vertices.Count; ++vertexIndex) {
			if (!checkedVertices.Contains (vertexIndex)) {
				int newOutlineVertex = GetConnectedOutlineVertex (vertexIndex);
				if (newOutlineVertex != -1) {
					checkedVertices.Add (vertexIndex);
					List<int> newOutline = new List<int> ();
					newOutline.Add (vertexIndex);
					outlines.Add (newOutline);
					FollowOutline (newOutlineVertex, outlines.Count - 1);
					outlines [outlines.Count - 1].Add (vertexIndex);
				}
			}
		}
	}

	void FollowOutline (int vertexIndex, int outlineIndex)	{
		outlines [outlineIndex].Add (vertexIndex);
		checkedVertices.Add (vertexIndex);
		int nextVertexIndex = GetConnectedOutlineVertex (vertexIndex);
		if (nextVertexIndex != -1) {
			FollowOutline (nextVertexIndex, outlineIndex);
		}
	}

	int GetConnectedOutlineVertex(int vertexIndex)	{
		List<Triangle>	trianglesContainingVertex = triangleDictionary [vertexIndex];

		for (int i = 0; i < trianglesContainingVertex.Count; ++i) {
			Triangle triangle = trianglesContainingVertex[i];
			for (int j = 0; j < 3; ++j) {
				int vertexB = triangle [j];
				if (vertexIndex != vertexB && !checkedVertices.Contains(vertexB)) {
					if (IsOutlineEdge (vertexIndex, vertexB)) {
						return vertexB;
					}
				}
			}
		}
		return -1;
	}

	//Returns true if exactly 1 triangles in the triangle list contains vertexA and vertexB
	bool IsOutlineEdge( int vertexA, int vertexB )	{
		List<Triangle> trianglesContainingA = triangleDictionary [vertexA];
		int sharedTriangleCount = 0;
		for (int i = 0; i < trianglesContainingA.Count; ++i) {
			if(trianglesContainingA[i].ContainsVertex(vertexB))
				++sharedTriangleCount;
			if (sharedTriangleCount > 1)
				break;
		}
		return sharedTriangleCount == 1;
	}

	struct Triangle 	{
		public int vertexIndexA;
		public int vertexIndexB;
		public int vertexIndexC;

		public Triangle ( int vertexIndexA, int vertexIndexB, int vertexIndexC)	{
			this.vertexIndexA = vertexIndexA;
			this.vertexIndexB = vertexIndexB;
			this.vertexIndexC = vertexIndexC;
		}

		public int this[int i]	{
			get{
				switch(i)	{
				case 0:
					return vertexIndexA;
				case 1:
					return vertexIndexB;
				case 2:
					return vertexIndexC;
				default:
					break;
				}
				return -1;
			}
			set{
				switch(i)	{
				case 0:
					vertexIndexA=value;
					break;
				case 1:
					vertexIndexB=value;
					break;
				case 2:
					vertexIndexC=value;
					break;
				default:
					break;
				}
			}
		}

		public bool ContainsVertex(int vertexIndex)	{
			return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
		}
	}

	public class SquareGrid {

		public Square[,] squares;
		public SquareGrid(int[,] map, float squareSize) {
			int nodeCountX = map.GetLength(0); 
			int nodeCountY = map.GetLength(1);
			float mapWidth = nodeCountX *squareSize;
			float mapHeight = nodeCountY *squareSize;
			ControlNode[,] controlNodes = new ControlNode[nodeCountX,nodeCountY];

			for(int x = 0; x<nodeCountX; x++)	{
				for(int y = 0; y<nodeCountY; y++)	{
					Vector3 pos = new Vector3((-mapWidth*0.5f+x)*squareSize+squareSize*0.5f,0,(-mapHeight*0.5f+y)*squareSize+squareSize*0.5f);
					controlNodes[x,y]=new ControlNode(pos,map[x,y]>0,squareSize);
				}
			}
			squares = new Square[nodeCountX -1,nodeCountY-1];
			for(int x = 0; x<nodeCountX-1; x++)	{
				for(int y = 0; y<nodeCountY-1; y++)	{
					squares [x,y] = new Square(controlNodes[x,y+1],controlNodes[x+1,y+1],controlNodes[x+1,y],controlNodes[x,y]);
				}
			}
		}
	}
	public class Square {
		public ControlNode northWest, northEast, southEast, southWest;
		public Node north, east, south, west;
		public int configuration;

		public Square(ControlNode _northWest, ControlNode _northEast,ControlNode _southEast,ControlNode _southWest) {
			northWest= _northWest;
			northEast= _northEast;
			southEast= _southEast;
			southWest = _southWest;

			north = northWest.east;
			east = southEast.north;
			south = southWest.east;
			west = southWest.north;

			configuration=0;
			if(northWest.active)
				configuration+=8;
			if(northEast.active)
				configuration+=4;
			if(southEast.active)
				configuration+=2;
			if(southWest.active)
				configuration+=1;
		}
	}

	public class Node {
		public Vector3 position;
		public int vertexIndex = -1;

		public Node(Vector3 _pos)	{
			position = _pos;
		}
	}

	public class ControlNode: Node {
		public bool active;
		public Node north, east;

		public ControlNode(Vector3 _pos, bool _active, float squareSize) :base(_pos)	{
			active = _active;
			north =  new Node(position+Vector3.forward * squareSize * 0.5f);
			east =  new Node(position+Vector3.right * squareSize * 0.5f);
		}

	}


		
}
