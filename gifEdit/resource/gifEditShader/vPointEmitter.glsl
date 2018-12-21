// #version 150 compatibility
#version 430 core
#define M_PI 3.1415926535897932384626433832795

uniform mat4 uMVP;
in vec2 aIndex;
in vec2 aCoord;
in int pointIndex;
out vec2 vTexCoord;

uniform float aTime;

buffer pointAttr{
	float attr[];
};

// in vec2 pos;
// in vec2 startSpeed;
// in vec2 startAngle;

mat2x3 moveMatrix(float x, float y){
	return mat2x3(
		1, 0, x,
		0, 1, y
	);
}

mat2 rotateMatrix(float angle) {
	float s = sin(angle);
	float c = cos(angle);
	return mat2(
		 c, s,
		-s, c
	);
}

mat2 scaleMatrix(float sx, float sy){
	return mat2(
		sx,  0,
		 0, sy
	);
}

void main() {
	vec2 pos = 			vec2(pointAttr[pointIndex + 0], pointAttr[pointIndex + 1]);
	vec2 pointSpeed = 	vec2(pointAttr[pointIndex + 2], pointAttr[pointIndex + 3]);
	vec2 aSpeed = 		vec2(pointAttr[pointIndex + 4], pointAttr[pointIndex + 5]);

	float t = aTime / 1000;
	float powt = t * t;
	// float x = pos.x + startSpeed.x * t + startAngle.x * powt;
	// float y = pos.y + startSpeed.y * t + startAngle.y * powt;
	vec2 pos2 = pos + pointSpeed * t + aSpeed * powt;

	// mat2x3 mm = moveMatrix(x, y);
	mat2 sm = scaleMatrix(16, 16);
	mat2 rm = rotateMatrix(aTime / 5000 * 2 * M_PI);

	pos2 = pos2 + (sm * (rm * aIndex));

	vec4 pos4 = vec4(pos2, 0.0, 1);
	// gl_Position.x += aTime * 0.1;
	// pos4.x += aTime * 0.04;
	// pos4.y += 0;

	pos4 = uMVP * pos4;
	gl_Position = pos4;
	vTexCoord = aCoord;
}
