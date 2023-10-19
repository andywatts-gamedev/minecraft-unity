using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

public class MeshTest : MonoBehaviour
{
    public int numFaces;
            
    public struct MyVertex
    {
        public float3 Position;
        public float3 Normal;
        public half4 Tangent;
    }

    public struct MyVertex2
    {
        public half2 TexCoord0;
        public Color32 Color;
    }

    private void Start()
    {
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        meshData.SetVertexBufferParams(numFaces * 4, new[]
        {
            new VertexAttributeDescriptor( VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor( VertexAttribute.Normal, dimension: 3, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.TexCoord0, dimension: 2, stream: 1 ),
            new VertexAttributeDescriptor( VertexAttribute.Color, dimension: 4, stream: 1 ),
        });
        
        
        
        var vertices = meshData.GetVertexData<MyVertex>();
        meshData.SetIndexBufferParams(numFaces * 6, IndexFormat.UInt16);
        var triangles = meshData.GetIndexData<ushort>();
        
        for (var i = 0; i < numFaces; i++)
        {
            var voxelXyz = new float3( Random.Range(0, 16), 1, Random.Range(0, 16));
            vertices[i*4 + 0] = new MyVertex {Position = new float3(0f, 1f, 0f) + voxelXyz};
            vertices[i*4 + 1] = new MyVertex {Position = new float3(0f, 1f, 1f) + voxelXyz};
            vertices[i*4 + 2] = new MyVertex {Position = new float3(1f, 1f, 1f) + voxelXyz};
            vertices[i*4 + 3] = new MyVertex {Position = new float3(1f, 1f, 0f) + voxelXyz};
            triangles[i*6 + 0] = (ushort)(i*4 + 0);
            triangles[i*6 + 1] = (ushort)(i*4 + 1);
            triangles[i*6 + 2] = (ushort)(i*4 + 3);
            triangles[i*6 + 3] = (ushort)(i*4 + 1);
            triangles[i*6 + 4] = (ushort)(i*4 + 2);
            triangles[i*6 + 5] = (ushort)(i*4 + 3);
        }

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length));
        
        var mesh = new Mesh();
        mesh.name = "ChunkMesh";
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }

}