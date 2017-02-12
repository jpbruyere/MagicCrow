#version 330
#extension GL_ARB_shader_subroutine : require

precision lowp float;

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec2 in_tex;
layout (location = 4) in mat4 in_model;
layout (location = 8) in int in_picked;

uniform mat4 mvp;
uniform int selectedIndex = -1;

out vec2 texCoord;
out vec4 colorMult;
flat out int texId;
flat out float instanceID;

subroutine void vsTeck_t ();
subroutine uniform vsTeck_t vsTeck;

subroutine (vsTeck_t) void normalPass() {
	texId = in_picked;
	gl_Position = mvp * in_model * vec4(in_position, 1);
}
subroutine (vsTeck_t) void selectionPass() {
	vec3 pos = in_position;
	//if (in_picked > 0.0)
	//	pos -= vec3(100.0,100.0,100.0);
	instanceID = float(gl_InstanceID)/255.0;
	gl_Position = mvp * in_model * vec4(pos, 1);
}

void main(void)
{				
	texCoord = in_tex;
	//texId = gl_InstanceID;
	if (selectedIndex == gl_InstanceID)
		colorMult = vec4(1.0,1.0,1.0,1.0);
	else
		colorMult = vec4(0.85,0.85,0.85,1.0);
	vsTeck();
}
