#version 330 core
uniform mat4 u_Model;
uniform mat4 u_View;
uniform mat4 u_Projection;

layout (location = 0) in vec3 a_Pos;
layout (location = 1) in vec2 a_TexCoord;
layout (location = 2) in float a_TexID;
layout (location = 3) in float a_LightLevel;

out vec4 v_Pos;
out vec2 v_TexCoord;

void main() {
    gl_Position = u_Projection * u_View * u_Model * vec4(a_Pos, 1.0f);
    v_Pos = vec4(clamp(a_Pos.xyz, 0.0f, 1.0f), 1.0f);
    v_TexCoord = a_TexCoord;
}
