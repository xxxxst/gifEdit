// #version 150 compatibility
#version 430 core

uniform sampler2D tex;
in vec2 vTexCoord;

in float alpha;

void main() {
	// gl_FragColor = vColor;
	float a = max(0, min(1, alpha));

	vec4 color = texture(tex, vTexCoord);
	color.a *= a;
	gl_FragColor = color;
	// discard;
}
