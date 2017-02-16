#ifdef GL_ES
precision mediump float;
#endif

uniform float time;

uniform vec2 resolution;

#define PI 90

void main( void ) {

	vec2 p = ( gl_FragCoord.xy / resolution.xy ) - 0.5;
	
	float sx = 0.05 * (p.x + 0.9) * sin( 10.0 * p.x + 1. * pow(time, 0.95)*50.);
	
	float dy = 200./ ( 5000. * abs(p.y - sx));
	
	dy += 1./ (25. * length(p - vec2(p.x, 0.)));
	
	gl_FragColor = vec4( (p.x + 0.001) * dy, 0.2 * dy, dy, 1.1 );

}