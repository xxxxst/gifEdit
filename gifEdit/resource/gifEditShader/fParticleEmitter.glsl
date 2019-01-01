// #version 150 compatibility
#version 430 core

uniform sampler2D tex;
in vec2 fCoord;

in float fAlpha;
in float fSize;
// uniform int particleCount;

out vec4 out_Color;

void main() {
	// gl_FragColor = vColor;
	float a = max(0, min(1, fAlpha));

	// vec4 color = vec4(1);

	// if(particleCount < 8000){
	// 	vec2 p1 = clamp(fCoord + vec2(-1/fSize, -1/fSize), vec2(0), vec2(1));
	// 	vec2 p2 = clamp(fCoord + vec2(       0, -1/fSize), vec2(0), vec2(1));
	// 	vec2 p3 = clamp(fCoord + vec2( 1/fSize, -1/fSize), vec2(0), vec2(1));
	// 	vec2 p4 = clamp(fCoord + vec2(-1/fSize,        0), vec2(0), vec2(1));
	// 	vec2 p5 = clamp(fCoord + vec2(       0,        0), vec2(0), vec2(1));
	// 	vec2 p6 = clamp(fCoord + vec2( 1/fSize,        0), vec2(0), vec2(1));
	// 	vec2 p7 = clamp(fCoord + vec2(-1/fSize, -1/fSize), vec2(0), vec2(1));
	// 	vec2 p8 = clamp(fCoord + vec2(       0,  1/fSize), vec2(0), vec2(1));
	// 	vec2 p9 = clamp(fCoord + vec2( 1/fSize,  1/fSize), vec2(0), vec2(1));

	// 	vec4 c1 = texture(tex, p1);
	// 	vec4 c2 = texture(tex, p2);
	// 	vec4 c3 = texture(tex, p3);
	// 	vec4 c4 = texture(tex, p4);
	// 	vec4 c5 = texture(tex, p5);
	// 	vec4 c6 = texture(tex, p6);
	// 	vec4 c7 = texture(tex, p7);
	// 	vec4 c8 = texture(tex, p8);
	// 	vec4 c9 = texture(tex, p9);

	// 	c1*=c1.a;
	// 	c2*=c2.a;
	// 	c3*=c3.a;
	// 	c4*=c4.a;
	// 	c5*=c5.a;
	// 	c6*=c6.a;
	// 	c7*=c7.a;
	// 	c8*=c8.a;
	// 	c9*=c9.a;

	// 	float p = 16.0;
	// 	if(fSize < 8){
	// 		p = fSize * (16.0 / 8);
	// 	}
	// 	color = (c1+c2+c3+c4+c5*p+c6+c7+c8+c9)/(8+p);
	// 	color.a = c5.a * a;
	// }else{
	// 	color = texture(tex, fCoord);
	// 	color.a *= a;
	// }
	
	vec4 color = texture(tex, fCoord);
	color.a *= a;
	
	out_Color = color;
	// discard;
}
