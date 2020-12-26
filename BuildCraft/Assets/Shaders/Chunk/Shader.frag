#version 330 core

uniform sampler2D u_Textures[16];

uniform vec4 u_Color;

in vec2 v_TexCoord;
in float v_LightLevel;
flat in float v_TexID;

out vec4 color;

void main() {
    color = vec4(texture(u_Textures[int(v_TexID)], v_TexCoord).xyz * v_LightLevel, 1.0f);
    //color = vec4(v_Pos.xyz, 1.0f) + u_Color * 0.001f + vec4(0.2f, 0.2f, 0.2f, 1.0f);
    //color = acolor * 0.01f + vec4(1.0f, 1.0f, 1.0f, 1.0f);
    //color = vec4(1.0f, 1.0f, 1.0f, 1.0f);
}
