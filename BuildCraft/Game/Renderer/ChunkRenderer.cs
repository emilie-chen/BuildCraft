using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using BuildCraft.Base;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Game.Resource;
using BuildCraft.Game.World;
using Silk.NET.OpenGL;
using static BuildCraft.Base.OpenGLContext;
using static BuildCraft.Base.Std.Native;

namespace BuildCraft.Game.Renderer
{
    using Mat4 = Matrix4x4;
    using Vec4 = Vector4;
    using Vec3 = Vector3;
    using Vec2 = Vector2;
    
    public class ChunkRenderer : IDisposable
    {
        public static readonly string VertexShaderSource = File.ReadAllText("Assets/Shaders/Chunk/Shader.vert");
        public static readonly string FragmentShaderSource = File.ReadAllText("Assets/Shaders/Chunk/Shader.frag");

        public const int MAX_QUAD_COUNT = 100000;
        public const int MAX_TEXTURE_COUNT = 36;
        private const int quadVertexFloatCount = 3 + 2 + 1 + 1;
        private const int quadVertexFloatCountTotal = quadVertexFloatCount * 4;

        private readonly NativeArray<uint> m_QuadDrawingOrder;
        private readonly NativeArray<float> m_BackQuad, m_FrontQuad, m_LeftQuad, m_RightQuad, m_BottomQuad, m_TopQuad;

        private NativeArray<float>[] m_QuadTemplateFaceDataArray;

        private readonly NativeArray<float> m_VertexBufferData;
        private readonly NativeArray<uint> m_IndexBufferData;

        private int
            m_NextAvailableVertexBufferDataIndex; // each increment represents a single float, not an entire vertex

        private int m_NextAvailableIndexBufferDataIndex; // same as above, but for uint's

        private VertexArray m_VertexArray;
        private VertexBuffer m_VertexBuffer;
        private IndexBuffer m_IndexBuffer;
        private Shader m_Shader;
        private Camera m_Camera;
        
        private IDictionary<Texture, uint> m_TextureToSlot;
        private uint m_NextAvailableTextureSlot;

        public unsafe ChunkRenderer()
        {
            m_TextureToSlot = new Dictionary<Texture, uint>();
            
            m_VertexBufferData = new NativeArray<float>(quadVertexFloatCountTotal * MAX_QUAD_COUNT);
            m_IndexBufferData = new NativeArray<uint>(6 * MAX_QUAD_COUNT);

            // setting up standard template data
            uint* standardDrawingOrder = stackalloc uint[6]
            {
                0, 1, 2,
                2, 3, 0
            };
            m_QuadDrawingOrder = new NativeArray<uint>(6 * sizeof(uint));
            memcpy(m_QuadDrawingOrder.Data, standardDrawingOrder, 6 * sizeof(uint));

            m_BackQuad = new NativeArray<float>(quadVertexFloatCountTotal);
            m_FrontQuad = new NativeArray<float>(quadVertexFloatCountTotal);
            m_LeftQuad = new NativeArray<float>(quadVertexFloatCountTotal);
            m_RightQuad = new NativeArray<float>(quadVertexFloatCountTotal);
            m_BottomQuad = new NativeArray<float>(quadVertexFloatCountTotal);
            m_TopQuad = new NativeArray<float>(quadVertexFloatCountTotal);
            
            m_QuadTemplateFaceDataArray = new NativeArray<float>[6]
            {
                m_BackQuad, m_FrontQuad, m_LeftQuad, m_RightQuad, m_BottomQuad, m_TopQuad
            };


            float* backQuad = stackalloc float[quadVertexFloatCountTotal]
            {
                // back (negative x)
                -0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f,
                -0.5f, 0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f,
            };
            float* frontQuad = stackalloc float[quadVertexFloatCountTotal]
            {
                // front (positive x)
                0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f,
                0.5f, 0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 1.0f,
                0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f,
                0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f,
            };
            float* leftQuad = stackalloc float[quadVertexFloatCountTotal]
            {
                // left (negative z)
                0.5f, 0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 1.0f,
                0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f,
                -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f,
            };
            float* rightQuad = stackalloc float[quadVertexFloatCountTotal]
            {
                // right (positive z)
                -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f,
                0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f,
                0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f,
            };
            float* bottomQuad = stackalloc float[quadVertexFloatCountTotal]
            {
                // bottom (negative y)
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f,
                0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 1.0f,
                0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f,
                -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f,
            };
            float* topQuad = stackalloc float[quadVertexFloatCountTotal]
            {
                // top (positive y)
                0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f,
                0.5f, 0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f,
            };
            memcpy(m_BackQuad.Data, backQuad, quadVertexFloatCountTotal * sizeof(float));
            memcpy(m_FrontQuad.Data, frontQuad, quadVertexFloatCountTotal * sizeof(float));
            memcpy(m_LeftQuad.Data, leftQuad, quadVertexFloatCountTotal * sizeof(float));
            memcpy(m_RightQuad.Data, rightQuad, quadVertexFloatCountTotal * sizeof(float));
            memcpy(m_BottomQuad.Data, bottomQuad, quadVertexFloatCountTotal * sizeof(float));
            memcpy(m_TopQuad.Data, topQuad, quadVertexFloatCountTotal * sizeof(float));
            
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //Gl.Enable(EnableCap.CullFace);
            
            m_VertexArray = new VertexArray();
            m_VertexArray.Bind();

           
            m_VertexBuffer = new VertexBuffer((quadVertexFloatCountTotal) * MAX_QUAD_COUNT * sizeof(float));
            m_VertexBuffer.Layout = new BufferLayout(new BufferElement[]
            {
                new(ShaderDataType.Float3, "a_Pos"),
                new(ShaderDataType.Float2, "a_TexCoord"),
                new(ShaderDataType.Float, "a_TexID"),
                new(ShaderDataType.Float, "a_LightLevel")
            });

            uint* indices = (uint*) malloc(MAX_QUAD_COUNT * 6 * sizeof(uint));
            m_IndexBuffer = new IndexBuffer(indices, MAX_QUAD_COUNT * 6);
            free(indices);
            
            m_VertexArray.AddVertexBuffer(m_VertexBuffer);
            m_VertexArray.SetIndexBuffer(m_IndexBuffer);
            m_VertexArray.Unbind();
            m_VertexBuffer.Unbind();
            m_IndexBuffer.Unbind();
            
            m_Shader = new Shader("MainShader", VertexShaderSource, FragmentShaderSource);
            
            m_Shader.Unbind();
        }
        
        private void ResetIndicesForNextRenderBatch()
        {
            m_NextAvailableTextureSlot = 0;
            // reset vertex buffer and index buffer indices
            m_NextAvailableVertexBufferDataIndex = 0;
            m_NextAvailableIndexBufferDataIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RenderChunk(Chunk chunk)
        {
            Vec3 chunkBase = chunk.BasePosition;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    for (int k = 0; k < 16; k++)
                    {
                        ref Block block = ref chunk[i, j, k];
                        RenderBlock(chunkBase + new Vec3(i, j, k), in block);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RenderBlock(in Vec3 pos, in Block block)
        {
            for (int i = 0; i < 6; i++)
            {
                Texture texture = TextureManager.GetTextureFromBlockType(block.Type, (BlockFace) i);
                if (texture == null)
                    continue;
                RenderCubeFace(pos, (BlockFace) i, texture);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void RenderCubeFace(in Vec3 pos, BlockFace face, Texture texture)
        {
            bool shouldRenderAfterMethodFinishes = false;
            uint textureSlot;
            if (m_TextureToSlot.ContainsKey(texture))
            {
                // this texture is already bound
                textureSlot = m_TextureToSlot[texture];
            }
            else
            {
                // bind this texture to the next available texture slot
                texture.Bind(m_NextAvailableTextureSlot);
                textureSlot = m_NextAvailableTextureSlot;
                m_NextAvailableTextureSlot++;
                if (m_NextAvailableTextureSlot >= MAX_TEXTURE_COUNT)
                {
                    // all texture slots are used
                    shouldRenderAfterMethodFinishes = true;
                }
            }
            
            // compute next face vertices
            NonFunctionalChunkRenderer.VertexElement* nextVerticesSrc = stackalloc NonFunctionalChunkRenderer.VertexElement[4];
            memcpy(nextVerticesSrc, m_QuadTemplateFaceDataArray[(int) face].Data,
                quadVertexFloatCountTotal * sizeof(float));
            for (int i = 0; i < 4; i++)
            {
                nextVerticesSrc[i].Position += pos;
                nextVerticesSrc[i].TextureID = textureSlot;
            }
            
            // copy the block face vertices to the vertex buffer
            NonFunctionalChunkRenderer.VertexElement* nextVertex = (NonFunctionalChunkRenderer.VertexElement*) &m_VertexBufferData.Data[m_NextAvailableVertexBufferDataIndex];
            memcpy(nextVertex, nextVerticesSrc, sizeof(float) * quadVertexFloatCountTotal);
            
            // compute next face indices
            uint* nextIndicesSrc = stackalloc uint[6]
            {
                0, 1, 2,
                2, 3, 0
            };
            for (int i = 0; i < 6; i++)
            {
                nextIndicesSrc[i] += (uint) (m_NextAvailableIndexBufferDataIndex / 6 * 4);
            }
            // copy new indices to the index buffer
            memcpy(&m_IndexBufferData.Data[m_NextAvailableIndexBufferDataIndex], nextIndicesSrc, 6 * sizeof(uint));

            m_NextAvailableVertexBufferDataIndex += quadVertexFloatCountTotal;
            m_NextAvailableIndexBufferDataIndex += 6;
            if (m_NextAvailableVertexBufferDataIndex >= m_VertexBufferData.Length | m_NextAvailableIndexBufferDataIndex >= m_IndexBufferData.Length)
            {
                shouldRenderAfterMethodFinishes = true;
            }

            if (shouldRenderAfterMethodFinishes)
            {
                DoRender();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void BeginScene(Camera camera)
        {
            Gl.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            m_Camera = camera;
            m_VertexArray.Bind();
            m_Shader.Bind();
            Mat4 viewMatrix = m_Camera.CalculateViewMatrix();
            Mat4 projectionMatrix = Mat4.CreatePerspectiveFieldOfView(
                MathF.PI / 4.0f,
                (float) GlWindow.Size.X / GlWindow.Size.Y,
                0.1f,
                100.0f
            );
            int* textures = stackalloc int[MAX_TEXTURE_COUNT];
            for (int i = 0; i < MAX_TEXTURE_COUNT; i++)
            {
                textures[i] = i;
            }
            m_Shader.UploadUniformIntArray("u_Textures", textures, MAX_TEXTURE_COUNT);
            m_Shader.UploadUniformMat4("u_View", viewMatrix);
            m_Shader.UploadUniformMat4("u_Projection", projectionMatrix);
        }
        
        private unsafe void DoRender()
        {
            m_VertexArray.Bind();
            m_VertexBuffer.Bind();
            memcpy(m_VertexBufferData.Data, m_BackQuad.Data, m_BackQuad.Length * sizeof(float));
            // for (int i = 0; i < m_NextAvailableVertexBufferDataIndex / 7; i++)
            // {
            //     Console.WriteLine(((ChunkRenderer.VertexElement*) m_VertexBufferData.Data)[i]);
            // }
            // for (int i = 0; i < m_NextAvailableIndexBufferDataIndex; i++)
            // {
            //     Console.Write(m_IndexBufferData.Data[i] + " ");
            // }
            // Console.WriteLine();

            m_VertexBuffer.SetData(m_VertexBufferData.Data, sizeof(float) * m_NextAvailableVertexBufferDataIndex);
            m_IndexBuffer.Bind();
            m_IndexBuffer.SetData(m_IndexBufferData.Data, sizeof(uint) * m_NextAvailableIndexBufferDataIndex);
            m_Shader.UploadUniformMat4("u_Model", Mat4.Identity);
            Gl.DrawElements(PrimitiveType.Triangles, (uint) m_NextAvailableIndexBufferDataIndex, DrawElementsType.UnsignedInt, null);
            
            ResetIndicesForNextRenderBatch();
        }

        public void EndScene()
        {
            DoRender();
            m_Camera = null;
        }

        public void Dispose()
        {
            m_VertexArray?.Dispose();
            m_VertexBuffer?.Dispose();
            m_IndexBuffer?.Dispose();
            m_Shader?.Dispose();
        }
    }
}