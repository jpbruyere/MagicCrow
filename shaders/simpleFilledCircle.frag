#ifdef GL_ES
precision mediump float;
#endif

uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;

void main( void ) {
	
	float dist = distance(resolution.xy*0.5, gl_FragCoord.xy) / 64.0;
	float r = 1.8 - dist;
	float g = 1.6 - dist;
	float b = 1.2 - dist;
	gl_FragColor = vec4(r, g, b, 1.0);
}