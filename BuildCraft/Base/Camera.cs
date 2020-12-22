using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace BuildCraft.Base
{
    using Mat4 = Matrix4x4;
    using Vec4 = Vector4;
    using Vec3 = Vector3;
    using Vec2 = Vector2;

    public class Camera
    {
        private IWindow m_Window;
        private Vec3 m_Pos, m_Front, m_Up, m_Right, m_UpWorld;
        private float m_Pitch, m_Yaw;
        private float m_MoveSpeed, m_TurnSpeed;
        private Vector2 LastMousePosition;

        public Camera(IWindow wnd, Vec3 pos, Vec3 up, float pitch, float yaw, float moveSpeed, float turnSpeed)
        {
            m_Window = wnd;
            m_Pos = pos;
            m_Front = new(0.0f, 0.0f, -1.0f);
            m_UpWorld = new(0.0f, 1.0f, 0.0f);
            m_Pitch = pitch;
            m_Yaw = yaw;
            m_MoveSpeed = moveSpeed;
            m_TurnSpeed = turnSpeed;
        }

        public void OnMouseMove(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = m_TurnSpeed;
            if (LastMousePosition == default)
            {
                LastMousePosition = position;
            }
            else
            {
                float xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
                float yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
                LastMousePosition = position;
                
                // Console.Write($"Mouse Moved {xOffset} {yOffset}");

                m_Yaw += xOffset;
                m_Pitch -= yOffset;

                m_Pitch = Math.Clamp(m_Pitch, -89.0f, 89.0f);
            }
        }

        public void Update(float ts)
        {
            m_Front.X = MathF.Cos(m_Yaw) * MathF.Cos(m_Pitch);
            m_Front.Y = MathF.Sin(m_Pitch);
            m_Front.Z = MathF.Sin(m_Yaw) * MathF.Cos(m_Pitch);
            m_Front = Vector3.Normalize(m_Front);

            m_Right = Vector3.Normalize(Vector3.Cross(m_Front, m_UpWorld));
            m_Up = Vector3.Normalize(Vector3.Cross(m_Right, m_Front));
            IKeyboard primaryKeyboard = OpenGLContext.InputContext.Keyboards.FirstOrDefault();
            if (primaryKeyboard!.IsKeyPressed(Key.W))
            {
                //Move forwards
                m_Pos += m_MoveSpeed * m_Front * ts;
            }
            if (primaryKeyboard.IsKeyPressed(Key.S))
            {
                //Move backwards
                m_Pos -= m_MoveSpeed * m_Front * ts;
            }
            if (primaryKeyboard.IsKeyPressed(Key.A))
            {
                //Move left
                m_Pos -= Vector3.Normalize(Vector3.Cross(m_Front, m_Up)) * m_MoveSpeed * ts;
            }
            if (primaryKeyboard.IsKeyPressed(Key.D))
            {
                //Move right
                m_Pos += Vector3.Normalize(Vector3.Cross(m_Front, m_Up)) * m_MoveSpeed * ts;
            }
        }

        public Mat4 CalculateViewMatrix()
        {
            return Matrix4x4.CreateLookAt(m_Pos, m_Pos + m_Front, m_Up);
        }
    }
}