#ifdef GL_ES
precision mediump float;
#endif

uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;

#define pi 3.1415926536
#define N 8
void main( void ) {

	vec2 position = ( gl_FragCoord.xy / resolution.xy );
	vec2 center=position*2.-1.;
	center.x*=resolution.x/resolution.y;
	float c=0.;
	float r=0.3;
	float o;
	for(int i=0;i<N;i++)
	{
		vec2 xy;
		o=float(i)/float(N)*2.*pi;
		xy.x=r*cos(o);
		xy.y=r*sin(o);
		xy+=center;
		c+=pow(200000.,(1.-length(xy)*1.9)*(0.99+0.1*fract(float(-i)/float(N)-time*1.5)))/20000.0;
	}
	gl_FragColor = vec4( c*vec3(0.1,.15,.2),1.0 );

}