#version 330
#extension GL_ARB_shader_subroutine : require
#extension GL_ARB_shader_texture_image_samples : require
#extension GL_ARB_sample_shading : require

precision lowp float;

uniform sampler2DArray texCards;
//uniform sampler2DMS texMS
uniform sampler2D texUI;

in vec2 texCoord;
in vec4 colorMult;
flat in int texId;
flat in float instanceID;

out vec4 out_frag_color;

subroutine void computeColor_t ();
subroutine uniform computeColor_t computeColor;

subroutine (computeColor_t) void cardsPass() {
	vec4 c;
	if (gl_FrontFacing)
		c = texture( texCards, vec3(texCoord, float(texId)));
	else
		c = texture( texCards, vec3(texCoord, 0.0));
	if (c.a < 0.3)
		discard;
	out_frag_color = c * colorMult;
}
/*
subroutine (computeColor_t) void cachePass() {
	int samples = textureSamples(texMS);
	ivec2 texcoord = ivec2 (textureSize (texMS) * texCoord);
	int i = gl_SampleID;

	vec4 c = texelFetch (texMS, texcoord, i);
	if (c.a == 0.0)
		discard;

	out_frag_color = vec4(c.rgb,1.0);
}
*/
subroutine (computeColor_t) void uiPass() {
	vec4 c = texture( texUI, texCoord);
	if (c.a == 0.0)
		discard;
	out_frag_color = c;
	//out_frag_color = vec4(1.0,0.0,0.0,1.0);
}
subroutine (computeColor_t) void selectionPass() {
	const float unit = 1.0/255.0;
	out_frag_color =  vec4(instanceID+unit);
}
void main(void)
{	
	computeColor();
}