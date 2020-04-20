using UnityEngine;

namespace MovementTools
{
    public class MeshGenerator : MonoBehaviour
    {

        [HideInInspector]
        public bool foldout;

        public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }

        public FaceRenderMask faceRenderMask;
        FaceRenderMask currentFaceRenderMask = FaceRenderMask.All;

        public string LayerMaskName = "Map";

        public MapSettings _mapSettings;
        [HideInInspector]
        [SerializeField] GameObject[] _meshObjects;

        void Start()
        {
            GenerateObject();
        }

        private void Update()
        {
            if (currentFaceRenderMask != faceRenderMask)
            {
                UpdateRenderMask();
            }

        }

        // draw/update mesh and material
        public void GenerateObject()
        {
            Initialize();
            UpdateObject();
        }

        public void UpdateObject()
        {
            GenerateMesh();
            GenerateMaterials();
        }

        private void Initialize()
        {
            int TotalSides = 0;
            if (_mapSettings.mode == MapSettings.MeshModes.Plane)
            {
                TotalSides = 1;
            }
            else
            {
                TotalSides = 6;
            }

            _mapSettings.planeFaces = new ShapeGenerator[TotalSides];

            Vector3[] directions;

            if (_mapSettings.mode == MapSettings.MeshModes.Plane)
            {
                directions = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            }
            else
            {
                directions = new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            }

            if (_meshObjects != null)
            {
                // destroy object in editor and at run time
                foreach (GameObject obj in _meshObjects)
                    DestroyImmediate(obj);

                Debug.Log("destroyed objects");
            }
            _meshObjects = new GameObject[TotalSides];

            for (var i = 0; i < TotalSides; i++)
            {
                // if mesh not already created create it else just update it
                // if(_meshFilters[i] == null){
                //create new component called mesh
                GameObject meshObj = new GameObject("meshFace");
                //parent it to this gameobject
                meshObj.gameObject.transform.parent = transform;
                meshObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                meshObj.transform.localRotation = Quaternion.identity;
                // assign default material to the gameobject
                MeshFilter meshFilters = meshObj.AddComponent<MeshFilter>();
                // meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("HDRP/LitTessellation"));
                meshObj.AddComponent<MeshRenderer>();
                meshFilters.sharedMesh = new Mesh();
                meshFilters.GetComponent<MeshRenderer>().sharedMaterial = Instantiate(_mapSettings.mapPlanetMaterial);
                meshFilters.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor", _mapSettings.mapColor);
                Texture2D _meshTexture = new Texture2D(_mapSettings.resolution, _mapSettings.resolution);
                meshFilters.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BaseColorMap", _meshTexture);
                _meshObjects[i] = meshObj;
                MeshCollider meshCollider = _meshObjects[i].AddComponent<MeshCollider>();
                // _meshFilters.sharedMesh = meshCollider;
                // assign map settings material material to the gameobject
                // }

                _meshObjects[i].layer = LayerMask.NameToLayer(LayerMaskName);
                _mapSettings.planeFaces[i] = new ShapeGenerator(_mapSettings, meshFilters.sharedMesh, meshFilters.GetComponent<MeshRenderer>(), _meshTexture, directions[i]);

            }
            ResetMesh();
            Debug.Log("Initialized");
        }

        void UpdateRenderMask()
        {
            int TotalSides = 6;
            for (var i = 0; i < TotalSides; i++)
            {
                bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
                _meshObjects[i].SetActive(renderFace);
            }
            currentFaceRenderMask = faceRenderMask;
        }

        void ResetMesh()
        {
            foreach (GameObject faceMesh in _meshObjects)
            {
                faceMesh.transform.localPosition = Vector3.zero;
                faceMesh.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            faceRenderMask = FaceRenderMask.All;
            UpdateRenderMask();

        }

        void GenerateMesh()
        {
            for (int i = 0; i < _mapSettings.planeFaces.Length; i++)
            {
                ShapeGenerator face = _mapSettings.planeFaces[i];
                if (_meshObjects[i].activeSelf) face.ConstructMesh();
            }
            _mapSettings.IsGeneraterMesh = true;
            Debug.Log("Generated Mesh");

        }

        void GenerateMaterials()
        {
            for (int i = 0; i < _mapSettings.planeFaces.Length; i++)
            {
                ShapeGenerator face = _mapSettings.planeFaces[i];
                //face.UpdateUVsToBioms();
                //face.UpdateShaderGraphMaterial();
            }
            Debug.Log("Generate Material");
        }

        // draw/update object
        public void OnObjectSettingsUpdated()
        {
            if (_mapSettings.autoUpdate)
            {
                if (_mapSettings.mode != MapSettings.MeshModes.Plane)
                {
                    _mapSettings.mode = MapSettings.MeshModes.Plane;
                    GenerateObject();
                }
                else if (_mapSettings.mode != MapSettings.MeshModes.Cube)
                {
                    _mapSettings.mode = MapSettings.MeshModes.Cube;
                    GenerateObject();
                }
                else if (_mapSettings.mode != MapSettings.MeshModes.Sphere)
                {
                    _mapSettings.mode = MapSettings.MeshModes.Sphere;
                    GenerateObject();
                }

            }
        }

    }

}