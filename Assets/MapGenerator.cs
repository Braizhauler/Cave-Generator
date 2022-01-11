using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
	enum TILE_TYPE :int	{
		EMPTY= 0,
		WALL = 1
	};
	enum MOUSE_BUTTON :int {
		LEFT = 0,
		RIGHT = 1,
		MIDDLE = 2
	};

	public int width;
	public int height;

	public string seed;

	public bool useRandomSeed;

	public int minimumPillarSize;
	public int minimumCaveSize;

	[Range(0,100)]
	public int randomFillPercent;


	int[,] map;
	// Use this for initialization
	void Start () {
		GenerateMap();
	}

	void Update() {
		if (Input.GetMouseButtonDown ((int)MOUSE_BUTTON.LEFT)) {
			GenerateMap ();
		}
	}


	void GenerateMap() {
		map = new int[width, height];
		RandomFillMap ();
		for (int i = 0; i < 5; i++) {
			SmoothMap ();
		}

		OverwriteSmallAreas (TILE_TYPE.WALL, minimumPillarSize, TILE_TYPE.EMPTY);
		List<Room> rooms = OverwriteSmallAreas (TILE_TYPE.EMPTY, minimumCaveSize, TILE_TYPE.WALL);
		if (rooms.Count > 0) {
			rooms.Sort ();
			//foreach (Room room in rooms) {
			//	print (room.roomSize);
			//}
			rooms [0].setLargestRoom ();
			ConnectClosestRooms	(rooms);
		}

		int borderSize = 5;
		int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2]; 
		for (int x = 0; x < borderedMap.GetLength(0); x++) {
			for (int y = 0; y < borderedMap.GetLength(1); y++) {
				if (x < borderSize || y < borderSize || x >= width + borderSize || y >= height + borderSize) {
					borderedMap [x, y] = (int)TILE_TYPE.WALL;
				} else {
					borderedMap [x, y] = map [x - borderSize, y - borderSize];
				}
			}
		}

		MeshGenerator meshGen = GetComponent<MeshGenerator> ();
		meshGen.GenerateMesh (borderedMap, 1f);
	}



	void RandomFillMap () {
		if (useRandomSeed) {
			seed = System.DateTime.Now.Ticks.ToString();
				//"test";
		}
		System.Random randNum = new System.Random (seed.GetHashCode ());
		for (int x = 0; x < width; ++x) {
			for (int y = 0; y < height; ++y) {
				if (x==0||x==width-1||y==0||y==height-1 ) {
					map [x, y] = (int)TILE_TYPE.WALL;
				} else {
					int fillWall = (randNum.Next (0, 100) < randomFillPercent)? (int)TILE_TYPE.WALL : (int)TILE_TYPE.EMPTY;
					map [x, y] = fillWall ;
				}
			}
		}
	}

	void SmoothMap() {
		int[,] mapWeights1 = new int[5, 5] { { 0, 0, 1, 0, 0 }, { 0, 1, 2, 1, 0 }, { 1, 2, 0, 2, 1 }, { 0, 1, 2, 1, 0 }, { 0, 0, 1, 0, 0 } };
		for (int x = 1; x < width-1; x++) {
			for (int y = 1; y < height - 1; y++) {

				int neighborTiles = WeighNeighborWalls (x, y, mapWeights1, map); //Walls are Red, Magenta, Yellow, Black
				if (neighborTiles > 9) {
					map [x, y] = (int)TILE_TYPE.WALL;
				}
				if (neighborTiles < 7) {
					map [x, y] = (int)TILE_TYPE.EMPTY;
				} 
			}
		}
	}

	int CountNeighborWalls(int gridX, int gridY, int[,] map) {
		int wallCount=0;
		if (!InGrid(gridX, gridY)) { //on our out of bounderies
			wallCount = 8;
		} else {
			for (int neighborX = gridX - 1; neighborX <= gridX + 1; ++neighborX) {
				for (int neighborY = gridY - 1; neighborY <= gridY + 1; ++neighborY) {
					if (neighborX != gridX || neighborY != gridY) {
						if (map [neighborX, neighborY] == (int)TILE_TYPE.WALL) {
							++wallCount;
						}
					}
				}
			}
		}
		return wallCount;
	}

	int WeighNeighborWalls(int gridX, int gridY, int[,] weightGrid, int[,] map) {
		int weight=0;


		for (int relPosX = -2; relPosX <= 2; ++relPosX) {
			for (int relPosY = -2; relPosY <= 2; ++relPosY) {
				if (InGrid (gridX + relPosX, gridY + relPosY)) {
					if (map [gridX + relPosX, gridY + relPosY] == 1) {
						weight += weightGrid [relPosX+2, relPosY+2];
					}
				} else {
					weight += weightGrid [relPosX+2, relPosY+2];
				}
			}
		}
		return weight;
	}

	List<Coord> GetRegionTiles(Coord startCoord)	{
		return GetRegionTiles (startCoord.X, startCoord.Y);
	}
	List<Coord> GetRegionTiles(int startX, int startY)	{
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[width, height];
		int tiletype = map[startX,startY];
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord(startX, startY));
		mapFlags [startX,startY]=(int)TILE_TYPE.WALL;
		while (queue.Count > 0 ) {
			Coord tile = queue.Dequeue();
			tiles.Add (tile);
			//check the four tiles orthoganal
			for (int i = 0; i < 4; ++i) {
				Coord neighborTile = tile.Ortho (i);
				if (InGrid(neighborTile)) {
					if (mapFlags [neighborTile.X, neighborTile.Y] == (int)TILE_TYPE.EMPTY && map [neighborTile.X, neighborTile.Y] == tiletype) {
						mapFlags [neighborTile.X, neighborTile.Y] = (int)TILE_TYPE.WALL;
						queue.Enqueue (neighborTile);
					}
				}
			}
		} 
		return tiles;
	}

	List<List<Coord>> GetRegions (TILE_TYPE tileType)	{
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] mapFlags = new int[width, height]; 

		for (int x = 0; x < map.GetLength (0); ++x) {
			for (int y = 0; y < map.GetLength (1); ++y) {
				if (mapFlags [x, y] == 0 && map [x, y] == (int)tileType) {
					List<Coord> newRegion = GetRegionTiles (x, y);
					regions.Add (newRegion);
					foreach (Coord tile in newRegion) {
						mapFlags [tile.X, tile.Y] = (int)TILE_TYPE.WALL;
					}
				}
			}
		}
		return regions;
	}

	//Returns all rooms regions greater then threshhold
	List<Room> OverwriteSmallAreas (TILE_TYPE soughtTile, int threshhold, TILE_TYPE replacementTile)	{
		List<List<Coord>> regions = GetRegions (soughtTile);
		List<Room> remainingRegions = new List<Room>();
		foreach (List<Coord> region in regions) {
			if (region.Count < threshhold) {
				foreach (Coord tile in region) {
					map [tile.X, tile.Y] = (int)replacementTile;
				}
			} else {
				if(soughtTile == TILE_TYPE.EMPTY)
					remainingRegions.Add (new Room(region, map));
			}
		}
		return remainingRegions;
	}

	void ConnectClosestRooms (List<Room> rooms, bool forceAccessability = false)	{
		List<Room> roomAccessable = new List<Room> ();
		List<Room> roomInaccessable = new List<Room> ();

		if (forceAccessability) {
			foreach (Room room in rooms) {
				if (room.AccessableFromLargestRoom()) {
					roomAccessable.Add (room);
				} else {
					roomInaccessable.Add (room);
				}
			}
		} else {
			roomAccessable = rooms;
			roomInaccessable = rooms;
		}


		int smallestKnownSquareDistance;

		Room bestRoomA= new Room();
		Coord bestTileA = new Coord ();

		Room bestRoomB = new Room();
		Coord bestTileB = new Coord ();

		bool connectionFound = false;
		smallestKnownSquareDistance = SquareDistance(new Coord(0,0),new Coord(width,height));
		foreach (Room roomA in roomAccessable) {
			if (!forceAccessability && roomA.connectedRooms.Count>0) {
				continue;
			} 
			foreach (Room roomB in roomInaccessable) {
				if (roomA == roomB || roomA.IsConnected(roomB)) {
					continue;
				}
				foreach (Coord tileA in roomA.edgeTiles) {
					foreach (Coord tileB in roomB.edgeTiles) {
						if ((SquareDistance (tileA, tileB) < smallestKnownSquareDistance) || !connectionFound) {
							smallestKnownSquareDistance = SquareDistance (tileA, tileB);
							bestRoomA = roomA;
							bestTileA = tileA;
							bestRoomB = roomB;
							bestTileB = tileB; 
							connectionFound = true;
						}
					}
				}
			}
			if (connectionFound && !forceAccessability) {
				CreatePassage (bestRoomA, bestTileA, bestRoomB, bestTileB);
				connectionFound = false;
			}
		}
		if (connectionFound && forceAccessability) {
			CreatePassage (bestRoomA, bestTileA, bestRoomB, bestTileB);
			ConnectClosestRooms (rooms, true);
		}
		if (!forceAccessability) {
			ConnectClosestRooms (rooms, true);
		}

	}

	void CreatePassage(Room roomA, Coord A, Room roomB,  Coord B)	{
		Room.ConnectRooms (roomA, roomB);
		Debug.DrawLine (CoordToWorldPos (A), CoordToWorldPos (B),Color.green,4f);
		List<Coord> line = LineToTiles (A, B);
		foreach (Coord c in line) {
			PlotCircle (c, 3, TILE_TYPE.EMPTY);
		}

	}

	void PlotCircle (Coord c,int r, TILE_TYPE tile)	{
		for (int x = -r; x <= r; ++x) {
			for (int y = -r; y <= r; ++y) {
				if (x * x + y * y <= r * r) {
					Coord newPoint = new Coord(c.X + x, c.Y + y);
					if (InGrid(newPoint))	{
						map[newPoint.X, newPoint.Y] = (int)tile;
					}
				}
			}
		}
	}

	List<Coord> LineToTiles(Coord start, Coord end)	{

		Debug.DrawLine( CoordToWorldPos (start), CoordToWorldPos (start)+2*Vector3.up,Color.red, 5f);
		Debug.DrawLine( CoordToWorldPos (end), CoordToWorldPos (end)+2*Vector3.up,Color.magenta, 5f);
		List<Coord> line = new List<Coord> ();
		Coord currentPoint = start;

		int dx = end.X - start.X;
		int dy = end.Y - start.Y;
		int axialDistance;
		int altAxisGradient;

		Coord step;
		Coord gradientStep;

		if (Math.Abs (dx) >= Math.Abs (dy)  ) {
			step = new Coord (Math.Sign (dx), 0);
			gradientStep = new Coord (0, Math.Sign (dy));
			axialDistance = Math.Abs(dx);
			altAxisGradient = Math.Abs(dy);
		} else {
			step = new Coord (0, Math.Sign (dy));
			gradientStep = new Coord (Math.Sign (dx), 0);
			axialDistance = Math.Abs(dy);
			altAxisGradient = Math.Abs(dx);
		}

		int gradientAccumulation = axialDistance / 2;

		for (int i=0; i<axialDistance; ++i)	{
			line.Add(currentPoint);

			Debug.DrawLine( CoordToWorldPos (currentPoint), CoordToWorldPos (currentPoint)+Vector3.up,Color.blue, 5f);
			currentPoint += step;
			gradientAccumulation += altAxisGradient;
			if (gradientAccumulation >= axialDistance) {
				gradientAccumulation -= axialDistance;
				currentPoint += gradientStep;
			}
		}
		return line;
	}

	Vector3 CoordToWorldPos(Coord tile)	{
		return new Vector3 (-width * 0.5f + 0.5f + tile.X, 2, -height * 0.5f + 0.5f + tile.Y);
	}


	int SquareDistance(Coord start, Coord dest)	{
		int deltaX = start.X - dest.X;
		int deltaY = start.Y - dest.Y;
		return (deltaX * deltaX) + (deltaY * deltaY);
	}

	bool InGrid(Coord pos) {
		return InGrid (pos.X, pos.Y);
	} 
	bool InGrid(int gridX, int gridY) {
		return (gridX >= 0 && gridX < this.width && gridY >= 0 && gridY < height);
	}

	void OnDrawGizmos () {
		/*if(map  != null)	{
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (map [x, y] == (int)TILE_TYPE.WALL) {
						Gizmos.color = Color.black;
					} else {
						Gizmos.color = Color.white;
					}
					Vector3 pos = new Vector3 (-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
					Gizmos.DrawCube (pos, Vector3.one*0.2f);
				}
			}
		}*/
	}

	class Room : IComparable<Room>	{
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;

		bool isTheLargestRoom = false;
		Room LargestRoom;

		public Room()	{
		}


		public bool isLargestRoom ()	{
			return isTheLargestRoom;
		}
		public void setLargestRoom()	{
			isTheLargestRoom = true;
			LargestRoom = this;
			SpreadAccessablity ();
		}
		public bool AccessableFromLargestRoom()	{
			if (LargestRoom == null) {
				return false;
			}
			return LargestRoom.isTheLargestRoom;
		}
		public void SpreadAccessablity()	{
			if (AccessableFromLargestRoom ()) {
				foreach (Room room in connectedRooms)	{
					if (!room.AccessableFromLargestRoom ()) {
						room.LargestRoom = LargestRoom;
						room.SpreadAccessablity ();
					}
				}
			}
		}

		public Room(List<Coord> roomTiles, int[,] map)	{
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms =  new List<Room>();

			edgeTiles = new List<Coord>();
			foreach (Coord tile in tiles)	{
				for(int i = 0; i<4;++i)	{
					Coord newTile = tile.Ortho(i);
					if(map[newTile.X, newTile.Y] == (int)TILE_TYPE.WALL)	{
						edgeTiles.Add(tile);
						break;
					}
				}
			}
		}
		public static void ConnectRooms(Room roomA, Room roomB)	{
			if(!roomA.connectedRooms.Contains(roomB))	{
				roomA.connectedRooms.Add (roomB);
				roomA.SpreadAccessablity ();
			}
			if(!roomB.connectedRooms.Contains(roomA))	{
				roomB.connectedRooms.Add (roomA);
				roomB.SpreadAccessablity ();
			}
		}
		public bool IsConnected(Room roomA)	{
			return connectedRooms.Contains (roomA);
		}

		public int CompareTo(Room otherRoom)	{
			return otherRoom.roomSize.CompareTo (roomSize);
		}
	}



	struct Coord	{
		public int X;
		public int Y;
		public static Coord west = new Coord(-1,0);
		public static Coord east= new Coord(1,0);
		public static Coord north= new Coord(0,-1);
		public static Coord south = new Coord (0, 1);

		public static Coord operator+(Coord a, Coord b)	{
			return new Coord (a.X + b.X, a.Y + b.Y);
		}

		public Coord Ortho(int i)	{
			switch (i) {
			case 0:
				return this + north;
			case 1:
				return this + east;
			case 2:
				return this + south;
			case 3:
				return this + west;
			default:
				return this;
			}
		}


		public Coord(int x, int y)	{
			X = x;
			Y = y;
		}

		public static implicit operator string(Coord c)	{
			return ("X: " + c.X + ", Y: " + c.Y);
		}
	}
}
