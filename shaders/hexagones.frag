#ifdef GL_ES
precision mediump float;
#endif

uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;
#define deg2rad 3.14159 / 180.

float hex(vec2 p, vec2 hexPos, float rad, float fill)
{
	float r = 0. + time;
	mat2 rot;
	//first column
	rot[0] = vec2(cos(r), sin(r));
	//2nd column
	rot[1] = vec2(-sin(r), cos(r));
	
	vec2 v = hexPos*rot - (p*rot);
		
	vec2 topBottomEdge = vec2(0., 1.);
	vec2 leftEdges = vec2(cos(30.*deg2rad), sin(30.*deg2rad));
	vec2 rightEdges = vec2(cos(30.*deg2rad), sin(30.*deg2rad));

	float dot1 = dot(abs(v), topBottomEdge);
	float dot2 = dot(abs(v), leftEdges);
	float dot3 = dot(abs(v), rightEdges);

	float dotMax = max(max((dot1), (dot2)), (dot3));
	
	return mix (0., 1.0, floor(rad - dotMax*1.1 + 0.99));
	//return  max(0.0, mix(0.0,mix (1., fill, floor(rad - dotMax*1.1 + 1.99 )), floor(rad - dotMax + 0.99 )));
//	return  max(0.0, mix(0.0,mix (1., fill, floor(rad - dotMax*1.1 + 1.991 )), floor(rad - dotMax + 0.99 )));
}
void main(void)
{
	vec2 uv = -1.0 + 2.0 * gl_FragCoord.xy / resolution.y;
	uv.x -= 1.0;

	float size = 2.0;
	float hex = hex(uv*1.5, vec2(0.0, 0.0), size,0.0 );

	vec3 col = mix(hex,0.0,0.0,0.40);// (length(hex)));
	
	gl_FragColor = vec4(col,1.0);
}