//YAY HSL!

#ifdef GL_ES
precision mediump float;
#endif
uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(5.9898,3.233))) * 0.5453);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
		
void main( void ) {
    vec2 p=(gl_FragCoord.xy-.5*resolution)/min(resolution.x,resolution.y);//
    vec3 c=vec3(0,0,0); //background color
    const float dots = 40.;
    const float brightness = 0.01;
    const float radius = .35;

    //float b = brightness;// * (sin(time*4.)/2. + .8);
    for(float i=0.;i<dots; i++){
        float x = radius*cos(2.*3.14*float(i)/dots);
        float y = radius*sin(2.*3.14*float(i)/dots);
	    
	float b = (.5 + rand(vec2(i*time))*.3) * brightness; //add slight flicker
        vec2 o = vec2(x,y);
        c += b/(length(p-o))*hsv2rgb(vec3((i + time*20.)/dots,1.,1.0));
    }
    gl_FragColor = vec4(c,1);
}