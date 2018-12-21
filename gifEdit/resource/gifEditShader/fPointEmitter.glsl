// #version 150 compatibility
#version 430 core

in vec2 vTexCoord;
uniform sampler2D tex;
void main() {
	// gl_FragColor = vColor;
	gl_FragColor = texture(tex, vTexCoord);
	// discard;
}
