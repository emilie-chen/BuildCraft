#version 330 core

uniform sampler2D u_Texture;
uniform vec4 u_Color;

in vec4 v_Pos;
in vec2 v_TexCoord;
in float v_LightLevel;
out vec4 color;

void main() {
    color = vec4(texture(u_Texture, v_TexCoord).xyz * v_LightLevel, 1.0f);
    //color = vec4(v_Pos.xyz, 1.0f) + u_Color * 0.001f + vec4(0.2f, 0.2f, 0.2f, 1.0f);
}
