using UnityEngine;
using WarGame;

public class DrawManyMeshesIndirect : MonoBehaviour
{
    public Mesh mesh;
    public Mesh mesh1;
    public Material material;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer positionBuffer;
    private const int INSTANCE_COUNT = 10000;
    private Matrix4x4[] matrixs = new Matrix4x4[900];

    void Start()
    {
        Debug.Log(mesh.GetInstanceID());
        Debug.Log(mesh1.GetInstanceID());

        AssetsMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Hexagons/Fire/Hex29.prefab", (prefab) =>
        {
            Debug.Log(prefab.GetComponent<MeshFilter>().sharedMesh.GetInstanceID());

            GameObject go = GameObject.Instantiate<GameObject>(prefab);
            Debug.Log(go.GetComponent<MeshFilter>().sharedMesh.GetInstanceID());
        }
        );

        //InitializeBuffers();

        for (int i = 0; i < 30; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                matrixs[i * 30 + j] = Matrix4x4.TRS(new Vector3(i, 0, j), Quaternion.identity, Vector3.one);
            }
        }
    }

    void InitializeBuffers()
    {
        // 初始化实例位置数据
        Vector3[] positions = new Vector3[INSTANCE_COUNT];
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(INSTANCE_COUNT));
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (i * gridSize + j < INSTANCE_COUNT)
                {
                    positions[i * gridSize + j] = new Vector3(i, 0, j);
                }
            }
        }

        // 创建 ComputeBuffer
        positionBuffer = new ComputeBuffer(INSTANCE_COUNT, sizeof(float) * 3);
        positionBuffer.SetData(positions);

        // 为材质设置 ComputeBuffer
        material.SetBuffer("_PositionBuffer", positionBuffer);

        // 初始化绘制参数
        uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)INSTANCE_COUNT, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void Update()
    {
        Debug.Log(matrixs.Length);
        Graphics.DrawMeshInstanced(mesh, 0, material, matrixs);
        //Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000)), argsBuffer);
    }

    void OnDisable()
    {
        if (positionBuffer != null) positionBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
    }
}
