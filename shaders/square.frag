#ifdef GL_ES
precision mediump float;
#endif

uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;

/* Orbitating unit "circles"
 * 24/12/13 -hektor
*/

float taxicabDistance(vec2 a, vec2 b) {
	return abs(a.x - b.x) + abs(a.y - b.y);
}

float supDistance(vec2 a, vec2 b) {
	return max(abs(a.x - b.x), abs(a.y - b.y));
}

void main( void ) {

	float zoom = 50.0;	
	float screenratio = 1.;
	vec2 pos = ( gl_FragCoord.xy / vec2(resolution.x, resolution.y*screenratio) ) * zoom;
 
	float thickness, radius, color;
	float circle, tircle, scircle;
	vec2 center;
	
	center = vec2(zoom/2.0,zoom/2.0);
	
	thickness = 1.0/30.0;
	radius = 20.5; 	
	scircle = supDistance(pos, center) - radius;
	
	color+= thickness/sqrt(scircle);
	

	gl_FragColor = vec4( color/1.5, color/1.3, color/1.0, 1.0 );

}