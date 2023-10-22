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
            
    private void Start()
    {
        // Define MeshData
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        meshData.SetVertexBufferParams(numFaces * 4, new[]
        {
            new VertexAttributeDescriptor( VertexAttribute.Position, dimension: 3, stream: 0),
            new VertexAttributeDescriptor( VertexAttribute.Normal, dimension: 3, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4, stream: 0 ),
            new VertexAttributeDescriptor( VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, dimension: 2, stream: 1 ),
            new VertexAttributeDescriptor( VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4, stream: 2 ),
        });
        meshData.SetIndexBufferParams(numFaces * 6, IndexFormat.UInt16);
        
        
        // Populate MeshData
        var vertexStream0 = meshData.GetVertexData<VertexStream0>(0);
        var triangles = meshData.GetIndexData<ushort>();
        var texCoords = meshData.GetVertexData<half2>(1);
        var colors = meshData.GetVertexData<Color32>(2);
        
        for (var i = 0; i < numFaces; i++)
        {
            // VertexStream0: Position, Normal, Tangent
            var voxelXyz = new float3( Random.Range(0, 16), 1, Random.Range(0, 16));
            vertexStream0[i*4 + 0] = new VertexStream0 {Position = new float3(0f, 1f, 0f) + voxelXyz};
            vertexStream0[i*4 + 1] = new VertexStream0 {Position = new float3(0f, 1f, 1f) + voxelXyz};
            vertexStream0[i*4 + 2] = new VertexStream0 {Position = new float3(1f, 1f, 1f) + voxelXyz};
            vertexStream0[i*4 + 3] = new VertexStream0 {Position = new float3(1f, 1f, 0f) + voxelXyz};
            
            // Triangles
            triangles[i*6 + 0] = (ushort)(i*4 + 0);
            triangles[i*6 + 1] = (ushort)(i*4 + 1);
            triangles[i*6 + 2] = (ushort)(i*4 + 3);
            triangles[i*6 + 3] = (ushort)(i*4 + 1);
            triangles[i*6 + 4] = (ushort)(i*4 + 2);
            triangles[i*6 + 5] = (ushort)(i*4 + 3);

            // Texcoords
            texCoords[i*4 + 0] = (half)0;
            texCoords[i*4 + 1] = half2((half)0, (half)1);
            texCoords[i*4 + 2] = (half)1;
            texCoords[i*4 + 3] = half2((half)1, (half)0);
            
            // Colors
            colors[i * 4 + 0] = new Color32(1, 1, 1, 1);
            colors[i * 4 + 1] = new Color32(1, 1, 1, 1);
            colors[i * 4 + 2] = new Color32(1, 1, 1, 1);
            colors[i * 4 + 3] = new Color32(1, 1, 1, 1); 
        }

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length));
        
        var mesh = new Mesh();
        mesh.name = "ChunkMesh";
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
        
        
        // GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Blocks.Instance.opaqueTexture2DArray);
    }

    
    public struct VertexStream0
    {
        public float3 Position;
        public float3 Normal;
        public half4 Tangent;
    }

}