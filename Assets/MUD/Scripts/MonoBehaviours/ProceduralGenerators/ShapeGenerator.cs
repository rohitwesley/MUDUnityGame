using UnityEngine;

namespace MovementTools
{
    public class ShapeGenerator : MonoBehaviour
    {

        Mesh _mesh;
        Renderer _renderer;
        Vector3[] _map2D;
        Vector3[] _map2DNormals;
        MapSettings _mapSettings;
        public TerrainInfo terrainData;
        public Vector3 _localUp;
        Vector3 _axisA;
        Vector3 _axisB;
        MinMax _elevation2DMinMax;
        public INoiseFilter[] _noiseFilter;

        Vector3[] _map3D;
        MinMax _elevation3DMinMax;

        public ShapeGenerator(MapSettings mapSettings, Mesh mesh, Renderer renderer, Texture2D texture, Vector3 localUp)
        {
            _mapSettings = mapSettings;

            _mesh = mesh;
            _renderer = renderer;
            // up
            _localUp = localUp;
            //swap cordinates of local up to get axis a on the plane
            _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            // axisB is on the plane perpendicular to axisA and lookup using cross product 
            _axisB = Vector3.Cross(_localUp, _axisA);

            _noiseFilter = new INoiseFilter[_mapSettings.noiseLayers.Length];
            for (var i = 0; i < _noiseFilter.Length; i++)
            {
                if (_noiseFilter[i] == null)
                    _noiseFilter[i] = NoiseFilterGenerator.CreateNoiseFilter(_mapSettings.noiseLayers[i].noiseSettings);

            }
            _elevation3DMinMax = new MinMax();
            _elevation2DMinMax = new MinMax();
        }


        public Mesh GenerateMap(Mesh meshTerrain)
        {
            // UpdateSphereMap();
            _map2D = new Vector3[_mapSettings.resolution * _mapSettings.resolution];
            _map2DNormals = new Vector3[_mapSettings.resolution * _mapSettings.resolution];
            Vector3[] _map2DBase = new Vector3[_mapSettings.resolution * _mapSettings.resolution];
            // total no. of triangle to form the plane (each plane is made up resolution - 1 rows and collumns which are made up of 2 triangles having 3 vertices each)
            int[] triangles = new int[(_mapSettings.resolution - 1) * (_mapSettings.resolution - 1) * 2 * 3];
            int triIndex = 0;
            float scale = _mapSettings.planetRadius;

            // Terrain data:
            terrainData = new TerrainInfo(_mapSettings.resolution);

            //run through each point on the plane row by row
            for (var y = 0; y < _mapSettings.resolution; y++)
            {
                for (var x = 0; x < _mapSettings.resolution; x++)
                {
                    // Vertex Index across the plane from top left to bottom right.
                    int i = x + y * _mapSettings.resolution;
                    //percent of vertex per row completed
                    Vector2 percent = new Vector2(x, y) / (_mapSettings.resolution - 1);
                    // point on unity plane along axisA and axisB;
                    Vector3 pointOnUnitPlane = Vector3.up + (percent.x - 0.5f) * 2 * _axisA + (percent.y - .5f) * 2 * _axisB;
                    Vector3 pointOnUnitCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - .5f) * 2 * _axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    Vector3 pointOnPlanet = CalculatePointOnScaledSphere(pointOnUnitSphere, scale);
                    Vector3 pointOnMap = pointOnPlanet;//pointOnUnitSphere * scale;
                                                       // pointOnMap = new Vector3(pointOnMap.x,pointOnMap.y + (pointOnPlanet.magnitude - scale),pointOnMap.z);
                    _map2D[i] = pointOnMap;
                    _map2DBase[i] = pointOnMap;//(pointOnUnitCube * scale);
                    float height = _map2D[i].magnitude;
                    if (i == 0)
                    {
                        _elevation2DMinMax.ResetValue(height);
                    }
                    _elevation2DMinMax.AddValue(height);

                    // Terrain data Initialised:
                    terrainData.tileCentres[x, y] = _map2D[i];//nw + new Vector3 (0.5f, 0, -0.5f);
                    terrainData.walkable[x, y] = true;

                    if (x != _mapSettings.resolution - 1 && y != _mapSettings.resolution - 1)
                    {
                        // firts trinagle of the plane
                        triangles[triIndex] = i;
                        triangles[triIndex + 1] = i + _mapSettings.resolution + 1;
                        triangles[triIndex + 2] = i + _mapSettings.resolution;

                        // second triangle of the plane
                        triangles[triIndex + 3] = i;
                        triangles[triIndex + 4] = i + 1;
                        triangles[triIndex + 5] = i + _mapSettings.resolution + 1;
                        triIndex += 6;
                    }
                }
            }

            Debug.Log("map3D ready");
            meshTerrain.Clear();
            meshTerrain.vertices = _map2DBase;
            meshTerrain.triangles = triangles;
            Vector2[] uvs = new Vector2[_map2D.Length];
            // for (int i = 0; i < uvs.Length; i++)
            for (var y = 0; y < _mapSettings.resolution; y++)
            {
                for (var x = 0; x < _mapSettings.resolution; x++)
                {
                    // Vertex Index across the plane from top left to bottom right.
                    int i = x + y * _mapSettings.resolution;
                    Vector3 scaledPoint = _map2D[i] / scale;
                    // if(_localUp == Vector3.up) uvs[i] = new Vector2((scaledPoint.x-1.0f)/2, 1-(scaledPoint.z - 1.0f)/2);
                    // if(_localUp == Vector3.down) uvs[i] = new Vector2(1-(scaledPoint.x-1.0f)/2, 1-(scaledPoint.z - 1.0f)/2);

                    // if(_localUp == Vector3.left) uvs[i] = new Vector2(1-(scaledPoint.z-1.0f)/2, 1-(scaledPoint.y - 1.0f)/2);
                    // if(_localUp == Vector3.right) uvs[i] = new Vector2((scaledPoint.z-1.0f)/2, 1-(scaledPoint.y - 1.0f)/2);

                    // if(_localUp == Vector3.forward) uvs[i] = new Vector2((scaledPoint.y-1.0f)/2, 1-(scaledPoint.x - 1.0f)/2);
                    // if(_localUp == Vector3.back) uvs[i] = new Vector2(1-(scaledPoint.y-1.0f)/2, 1-(scaledPoint.x - 1.0f)/2);
                    // scaledPoint = new Vector2(_mapSettings.resolution/(float)x, _mapSettings.resolution/(float)y);
                    uvs[i] = new Vector2(((float)x) / _mapSettings.resolution, ((float)y) / _mapSettings.resolution);
                    // uvs[i] = new Vector2((scaledPoint.x-1.0f)/2, 1-(scaledPoint.z - 1.0f)/2);
                }
            }
            meshTerrain.uv = uvs;
            // meshTerrain.RecalculateNormals();
            // meshTerrain.normals = CalculateNormals(triangles, _map2D);
            _map2DNormals = CalculateNormals(triangles, _map2D);
            meshTerrain.normals = _map2DNormals;
            return meshTerrain;
        }

        public void ConstructMesh()
        {
            // _mapTesselation3D = new Vector3[_mapSettings.resolution * _mapSettings.resolution];
            // total no. of triangle to form the plane (each plane is made up resolution - 1 rows and collumns which are made up of 2 triangles having 3 vertices each)
            int[] triangles = new int[(_mapSettings.resolution - 1) * (_mapSettings.resolution - 1) * 2 * 3];
            int triIndex = 0;

            //run through each point on the plane row by row
            for (var y = 0; y < _mapSettings.resolution; y++)
            {
                for (var x = 0; x < _mapSettings.resolution; x++)
                {
                    // Vertex Index across the plane from top left to bottom right.
                    int i = x + y * _mapSettings.resolution;

                    if (x != _mapSettings.resolution - 1 && y != _mapSettings.resolution - 1)
                    {
                        // firts trinagle of the plane
                        triangles[triIndex] = i;
                        triangles[triIndex + 1] = i + _mapSettings.resolution + 1;
                        triangles[triIndex + 2] = i + _mapSettings.resolution;

                        // second triangle of the plane
                        triangles[triIndex + 3] = i;
                        triangles[triIndex + 4] = i + 1;
                        triangles[triIndex + 5] = i + _mapSettings.resolution + 1;
                        triIndex += 6;
                    }
                }
            }

            Debug.Log("map3D ready");
            _mesh.Clear();
            UpdateSphereMap();
            _mesh.vertices = _map3D;
            _mesh.triangles = triangles;
            Vector2[] uvs = new Vector2[_map3D.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                if (_localUp == Vector3.up) uvs[i] = new Vector2((_map3D[i].x - 1.0f) / 2, 1 - (_map3D[i].z - 1.0f) / 2);
                if (_localUp == Vector3.down) uvs[i] = new Vector2(1 - (_map3D[i].x - 1.0f) / 2, 1 - (_map3D[i].z - 1.0f) / 2);
                if (_localUp == Vector3.left) uvs[i] = new Vector2((_map3D[i].x - 1.0f) / 2, (1.0f - _map3D[i].z) / 2);
                if (_localUp == Vector3.right) uvs[i] = new Vector2((_map3D[i].x - 1.0f) / 2, (1.0f - _map3D[i].z) / 2);
                if (_localUp == Vector3.forward) uvs[i] = new Vector2((_map3D[i].x - 1.0f) / 2, (1.0f - _map3D[i].z) / 2);
                if (_localUp == Vector3.back) uvs[i] = new Vector2((_map3D[i].x - 1.0f), (1.0f - _map3D[i].z));
                // uvs[i] = new Vector2((map3D[i].x-1.0f)/2, (1.0f - map3D[i].z)/2);
            }
            _mesh.uv = uvs;
            _mesh.RecalculateNormals();

        }

        public void UpdateSphereMap()
        {
            _map3D = new Vector3[_mapSettings.resolution * _mapSettings.resolution];
            //run through each point on the plane row by row
            for (var y = 0; y < _mapSettings.resolution; y++)
            {
                for (var x = 0; x < _mapSettings.resolution; x++)
                {
                    // Vertex Index across the plane from top left to bottom right.
                    int i = x + y * _mapSettings.resolution;
                    //percent of vertex per row completed
                    Vector2 percent = new Vector2(x, y) / (_mapSettings.resolution - 1);
                    //move planes points 1 unit up on the localUP axis
                    Vector3 pointOnUnitCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - .5f) * 2 * _axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    _map3D[i] = pointOnUnitSphere;
                    _map3D[i] = CalculatePointOnScaledSphere(pointOnUnitSphere, _mapSettings.planetRadius);

                    if (i == 0)
                    {
                        _elevation3DMinMax.ResetValue(_map3D[i].magnitude);
                    }
                    _elevation3DMinMax.AddValue(_map3D[i].magnitude);
                }
            }
        }

        Vector3[] CalculateNormals(int[] triangles, Vector3[] vertices)
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 trinagleNormal = SurfaceNormalFromIndices(vertices, vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += trinagleNormal;
                vertexNormals[vertexIndexB] += trinagleNormal;
                vertexNormals[vertexIndexC] += trinagleNormal;

            }
            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            return vertexNormals;

        }

        Vector3 SurfaceNormalFromIndices(Vector3[] vertices, int indexA, int indexB, int indexC)
        {
            Vector3 pointA = vertices[indexA];
            Vector3 pointB = vertices[indexB];
            Vector3 pointC = vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        public Vector3 CalculatePointOnScaledSphere(Vector3 pointOnUnitSphere, float planetRadius)
        {
            float firstLayerValue = 0;
            float elevation = 0;

            // evaluate for first noise filter
            if (_noiseFilter.Length > 0)
            {
                firstLayerValue = _noiseFilter[0].Evaluate(pointOnUnitSphere);
                if (_mapSettings.noiseLayers[0].enabled)
                    elevation = firstLayerValue;
            }

            //evaluate for the rest of the noise filters
            for (var i = 1; i < _noiseFilter.Length; i++)
            {
                if (_mapSettings.noiseLayers[i].enabled)
                {
                    // check to use first layer as mask or not
                    float mask = (_mapSettings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                    elevation += _noiseFilter[i].Evaluate(pointOnUnitSphere) * mask;
                }
            }

            elevation = planetRadius * (1 + elevation);

            return pointOnUnitSphere * elevation;
        }

        [System.Serializable]
        public class TerrainInfo
        {
            public int size;
            public Texture2D textureWalkableMat;
            public MapSettings.TerrainTypes terrainMaterialTextures;
            public Texture2D textureHeightMap;
            public Texture2D textureBiomeMap;
            public Vector3[,] tileCentres;
            public bool[,] walkable;
            public bool[,] shore;
            public int numTiles;
            public int numLandTiles;
            public int numWaterTiles;
            public float waterPercent;
            public bool centralize = true;
            public float waterDepth = .2f;
            public float edgeDepth = .2f;

            public TerrainInfo(int size)
            {
                this.size = size;
                tileCentres = new Vector3[size, size];
                walkable = new bool[size, size];
                shore = new bool[size, size];
                numLandTiles = 0;
                numWaterTiles = 0;
                Debug.Log("Map Initialised");
            }
        }

        public class MinMax
        {

            public float Min { get; private set; }
            public float Max { get; private set; }

            public MinMax()
            {
                // TODO better way to initialise the min max with starting values
                Min = 1;//float.MinValue;
                Max = 0;//float.MaxValue;
            }

            public void AddValue(float v)
            {
                if (v > Max)
                {
                    Max = v;
                }
                if (v < Min)
                {
                    Min = v;
                }
            }

            public void ResetValue(float magnitude)
            {
                Min = magnitude;
                Max = magnitude;
            }
        }


    }
}
