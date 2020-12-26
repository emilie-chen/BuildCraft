using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BuildCraft.Base;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Game.Resource;
using BuildCraft.Game.World;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Silk.NET.Vulkan;
using static BuildCraft.Base.OpenGLContext;
using static BuildCraft.Base.Std.Native;
using Texture = BuildCraft.Base.GlWrappers.Texture;

namespace BuildCraft.Game.Renderer
{
    using Mat4 = Matrix4x4;
    using Vec4 = Vector4;
    using Vec3 = Vector3;
    using Vec2 = Vector2;

    public class NonFunctionalChunkRenderer : IDisposable
    {
        private static readonly string VertexShaderSource = File.ReadAllText("Assets/Shaders/Chunk/Shader.vert");
        private static readonly string FragmentShaderSource = File.ReadAllText("Assets/Shaders/Chunk/Shader.frag");
        public const int MAX_QUAD_COUNT = 10000;
        public const int MAX_TEXTURE_COUNT = 16;
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
        
        private VertexArray m_VertexArray2;
        private VertexBuffer m_VertexBuffer2;
        private IndexBuffer m_IndexBuffer2;
        private Shader m_Shader2;

        [StructLayout(LayoutKind.Explicit, Size = quadVertexFloatCount * 4)]
        public struct VertexElement
        {
            [FieldOffset(0)]
            public Vec3 Position;
            [FieldOffset(3 * 4)]
            public Vec2 TextureCoordinates;
            [FieldOffset(5 * 4)]
            public float TextureID;
            [FieldOffset(6 * 4)]
            public float LightLevel;

            public override string ToString()
            {
                return
                    $"Position: {Position}, TexCoord: {TextureCoordinates}, TextureID: {TextureID}, LightLevel: {LightLevel}";
            }
        }

        public unsafe NonFunctionalChunkRenderer()
        {
            m_TextureToSlot = new Dictionary<Texture, uint>();
            m_VertexBufferData = new NativeArray<float>(quadVertexFloatCount * MAX_QUAD_COUNT);
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
            Gl.Enable(EnableCap.CullFace);

            m_VertexArray = new VertexArray();
            m_VertexArray.Bind();

            m_VertexBuffer = new VertexBuffer(MAX_QUAD_COUNT * quadVertexFloatCount * sizeof(float));
            memcpy(m_IndexBufferData.Data, standardDrawingOrder, 6 * sizeof(uint));
            m_IndexBuffer = new IndexBuffer(m_IndexBufferData.Data, MAX_QUAD_COUNT * 6);

            m_VertexBuffer.Layout = new BufferLayout(new BufferElement[]
            {
                new(ShaderDataType.Float3, "a_Pos"),
                new(ShaderDataType.Float2, "a_TexCoord"),
                new(ShaderDataType.Float, "a_TexID"),
                new(ShaderDataType.Float, "a_LightLevel")
            });

            m_VertexArray.AddVertexBuffer(m_VertexBuffer);
            m_VertexArray.SetIndexBuffer(m_IndexBuffer);
            m_VertexArray.Unbind();
            m_VertexBuffer.Unbind();
            m_IndexBuffer.Unbind();

            m_Shader = new Shader("MainShader", VertexShaderSource, FragmentShaderSource);

            m_Shader.Unbind();
            
            //////////////
            // DEBUG
            //////////////
            m_VertexArray2 = new VertexArray();
            m_VertexArray2.Bind();

            // front is positive x axis
            float* vertices = stackalloc float[]
            {
                // X      Y      Z      U    V    TexID Light
                // back (negative x)
                -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 1.0f,

                // front (positive x)
                0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f, 1.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f,
                0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 1.0f,
                
                // left (negative z)
                0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f,   1.0f,
                0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f,   1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,  0.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f,  1.0f,

                // right (positive z)
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 1.0f,
                0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f,  1.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.0f,  1.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.0f, 1.0f,

                // bottom (negative y)
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,
                0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.0f,  1.0f,
                0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f,  1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 1.0f,

                // top (positive y)
                0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f,  1.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f,  1.0f,
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 1.0f,
            };

            NativeArray<float> cubeVerticesTemplate = new NativeArray<float>((3 + 2 + 1 + 1) * 4 * 6);
            memcpy(cubeVerticesTemplate.Data, vertices, cubeVerticesTemplate.Length * sizeof(float));

            m_VertexBuffer2 = new VertexBuffer(vertices, (3 + 2 + 1 + 1) * 4 * 6 * sizeof(float));
            m_VertexBuffer2.Layout = new BufferLayout(new BufferElement[]
            {
                new(ShaderDataType.Float3, "a_Pos"),
                new(ShaderDataType.Float2, "a_TexCoord"),
                new(ShaderDataType.Float, "a_TexID"),
                new(ShaderDataType.Float, "a_LightLevel")
            });

            uint* indices = stackalloc uint[36];
            for (uint i = 0; i < 36; i += 6)
            {
                memcpy(indices + i, standardDrawingOrder, 6 * sizeof(uint));
                for (int j = 0; j < 6; j++)
                {
                    standardDrawingOrder[j] += 4;
                }
            }

            m_IndexBuffer2 = new IndexBuffer(indices, 36);
            
            m_VertexArray2.AddVertexBuffer(m_VertexBuffer);
            m_VertexArray2.SetIndexBuffer(m_IndexBuffer);
            m_VertexArray2.Unbind();
            m_VertexBuffer2.Unbind();
            m_IndexBuffer2.Unbind();

            m_Shader2 = new Shader("M", CubeRenderer.VertexShaderSource, CubeRenderer.FragmentShaderSource);
        }

        private void ResetIndicesForNextRenderBatch()
        {
            // reset textures
            m_NextAvailableTextureSlot = 0;

            // reset vertex buffer and index buffer indices
            m_NextAvailableVertexBufferDataIndex = 0;
            m_NextAvailableIndexBufferDataIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void BeginScene(Camera camera)
        {
            ResetIndicesForNextRenderBatch();
            Gl.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            m_Camera = camera;
            m_VertexArray.Bind();
            m_Shader.Bind();
            // compute MVP matrices and upload them to the shader
            Mat4 viewMatrix = m_Camera.CalculateViewMatrix();
            Mat4 projectionMatrix = Mat4.CreatePerspectiveFieldOfView(
                MathF.PI / 4.0f,
                (float) GlWindow.Size.X / GlWindow.Size.Y,
                0.1f,
                100.0f
            );
            m_Shader.UploadUniformMat4("u_Model", Matrix4x4.Identity);
            m_Shader.UploadUniformMat4("u_View", viewMatrix);
            m_Shader.UploadUniformMat4("u_Projection", projectionMatrix);
            m_Shader2.Bind();
            m_Shader2.UploadUniformMat4("u_View", viewMatrix);
            m_Shader2.UploadUniformMat4("u_Projection", projectionMatrix);
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
            VertexElement* nextVerticesSrc = stackalloc VertexElement[4];
            memcpy(nextVerticesSrc, m_QuadTemplateFaceDataArray[(int) face].Data,
                quadVertexFloatCountTotal * sizeof(float));
            for (int i = 0; i < 4; i++)
            {
                nextVerticesSrc[i].Position += pos;
                nextVerticesSrc[i].TextureID = textureSlot;
            }
            
            // copy the block face vertices to the vertex buffer
            VertexElement* nextVertex = (VertexElement*) &m_VertexBufferData.Data[m_NextAvailableVertexBufferDataIndex];
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
            memcpy(m_IndexBufferData.Data, nextIndicesSrc, 6 * sizeof(uint));

            m_NextAvailableVertexBufferDataIndex += quadVertexFloatCountTotal;
            m_NextAvailableIndexBufferDataIndex += 6;

            if (shouldRenderAfterMethodFinishes)
            {
                DoRender();
            }
        }
        
        // TODO
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void RenderCube(Vec3 coordinates, Texture texture)
        {
            m_Shader2.Bind();
            m_VertexArray2.Bind();
            m_VertexBuffer2.Bind();
            m_IndexBuffer2.Bind();
            m_Shader2.UploadUniformMat4("u_Model", Mat4.CreateTranslation(coordinates));
            texture.Bind();
            m_Shader2.UploadUniformInt("u_Texture", 0);
            Gl.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void RenderChunk(Chunk chunk)
        {
            Vec3 chunkBase = chunk.BasePosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private unsafe void DoRender()
        {
            // for (int i = 0; i < m_NextAvailableVertexBufferDataIndex / 7; i++)
            // {
            //     Console.WriteLine(((VertexElement*) m_VertexBufferData.Data)[i]);
            // }
            //
            // for (int i = 0; i < m_NextAvailableIndexBufferDataIndex; i++)
            // {
            //     Console.WriteLine(m_IndexBufferData.Data[i]);
            // }
            // Console.WriteLine();
            
            m_VertexArray.Bind();
            m_VertexBuffer.Bind();
            m_IndexBuffer.Bind();
            m_Shader.Bind();
            int* textures = stackalloc int[MAX_TEXTURE_COUNT];
            for (int i = 0; i < MAX_TEXTURE_COUNT; i++)
            {
                textures[i] = i;
            }
            //m_Shader.UploadUniformIntArray("u_Texture", textures, MAX_TEXTURE_COUNT);
            m_VertexBuffer.SetData(m_VertexBufferData.Data, m_NextAvailableVertexBufferDataIndex * sizeof(float));
            //m_IndexBuffer.SetData(m_IndexBufferData.Data, m_NextAvailableIndexBufferDataIndex * sizeof(uint));
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