//Ring of HSL lights with a mysterious black circle overlay 
//based on http://tokyodemofest.jp/2014/7lines/index.html
//by @felixturner 

#ifdef GL_ES
precision mediump float;
#endif
uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;



    const float dots = 50.; //number of point lights

vec3 hsv2rgb(vec3 c){
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
		
void main( void ) {
	float brightness = .01;
    float radius = sin(time); //radius of light ring    
    vec2 p=(gl_FragCoord.xy-.5*resolution)/min(resolution.x,resolution.y);//
    vec3 c=vec3(0,0.0,.1); //background color

    float b = brightness * (sin(time*1.)/6. + .2); //modulate brightness over time
	
    for(float i=0.;i<dots; i++){
	    
	//get location of dot
        float x = radius*cos(5.*3.14*float(i)/dots);
        float y = radius*sin(6.*3.14*float(i)/dots);
        vec2 o = vec2(x,y);
	    
	//get color of dot based on its index in the circle + time
	vec3 dotCol = hsv2rgb(vec3((i + 2. + time*10.)/dots,1.,1.0));
	    
        //get brightness of this pixel based on distance to dot
	c += b/(length(p-o))*dotCol;
    }
    gl_FragColor = vec4(c,1);
}