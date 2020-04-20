using UnityEngine;

namespace MovementTools
{
    [CreateAssetMenu]
    public class MapSettings : ScriptableObject
    {
        public enum MeshModes
        {
            Plane,
            Cube,
            Sphere,
        }
        public MeshModes mode = MeshModes.Plane;
        [Range(2, 256)]
        public int resolution = 128;
        public int resolutionLOD = 512;
        public bool autoUpdate = true;
        public bool IsGeneraterMesh = false;
        public bool IsNavMeshSurfaceSet = false;

        public float planetRadius = 1;
        public int faceIndex = 0;
        public int TotalSides = 3;
        public NoiseLayer[] noiseLayers;
        public ShapeGenerator[] planeFaces;
        public TerrainTypes terrainMaterialLake;
        public TerrainTypes terrainMaterialMud;
        public TerrainTypes terrainMaterialMoss;

        public Color mapColor;
        public Gradient mapElevation;
        public BiomeSettings biomeSettings;
        public Material mapPlanetMaterial;
        public Material mapTerrainMaterial;

        [System.Serializable]
        public class NoiseLayer
        {
            public bool enabled = true;
            public bool useFirstLayerAsMask;
            public NoiseSettings noiseSettings;
        }

        [System.Serializable]
        public class BiomeSettings
        {
            public Biome[] biomes;
            public NoiseSettings noiseSettings;
            public float noiseOffset;
            public float noiseStrength;
            public float blendAmount;

            [System.Serializable]
            public class Biome
            {
                public enum BiomeType
                {
                    Aquatic,
                    Snowy,
                    Desert,
                    Forest
                }
                public BiomeType biomeType;
                public Gradient mapBioms;
                public Color tint;
                [Range(0, 1)]
                public float startHeight;
                [Range(0, 1)]
                public float startLatitude;
                [Range(0, 1)]
                public float tintPercent;

                // [Range (-1, 1)]
                // public float height;
                // public Color color;
                // public Color startCol;
                // public Color endCol;
                // public int numSteps;
            }
        }

        [System.Serializable]
        public class TerrainTypes
        {
            public Texture2D baseMap;
            public Texture2D maskMap;
            public Texture2D normalMap;
            public Texture2D detailHeightMap;

        }

    }
}
