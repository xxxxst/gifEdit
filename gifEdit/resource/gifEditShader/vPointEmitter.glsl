// #version 150 compatibility
#version 430 core
#define M_PI 3.1415926535897932384626433832795

// layout(std140, binding = 0) buffer pointAttrs{
// 	float pointAttr[];
// };
// in float pointAttr[6];

uniform mat4 uMVP;
in vec2 index;
in vec2 vCoord;
// in int pointIndex;

// in vec2 pos;
// in vec2 speed;
// in vec2 aSpeed;
// float rotateSpeed;
// float directionSpeed;
// in float startSize;
// in float sizeSpeed;
// in float startAngle;
// in float pointRotateSpeed;
// in float startAlpha;
// in float alphaSpeed;

// layout(std430, binding = 0) buffer Attrs{
// 	vec2 pos[];
// 	vec2 speed[];
// 	vec2 aSpeed[];
// 	// float rotateSpeed;
// 	// float directionSpeed;
// 	float startSize[];
// 	float sizeSpeed[];
// 	float startAngle[];
// 	float pointRotateSpeed[];
// 	float startAlpha[];
// 	float alphaSpeed[];
// }attrs;

struct Attrs{
	vec2 pos;
	vec2 speed;
	vec2 aSpeed;
	// float rotateSpeed;
	// float directionSpeed;
	float startSize;
	float sizeSpeed;
	float startAngle;
	float pointRotateSpeed;
	float startAlpha;
	float alphaSpeed;
};

layout(std430, binding = 0) buffer AttrBuf{
	Attrs attrs[];
};

uniform float aTime;

out vec2 fCoord;
out float fAlpha;
out float fSize;

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
	// vec2 pos = 			vec2(pointAttr[gl_InstanceID + 0], pointAttr[gl_InstanceID + 1]);
	// vec2 pointSpeed = 	vec2(pointAttr[gl_InstanceID + 2], pointAttr[gl_InstanceID + 3]);
	// vec2 aSpeed = 		vec2(pointAttr[gl_InstanceID + 4], pointAttr[gl_InstanceID + 5]);
	// vec2 pos = attrs.pos[gl_InstanceID];
	// vec2 speed = attrs.speed[gl_InstanceID];
	// vec2 aSpeed = attrs.aSpeed[gl_InstanceID];
	// float startSize = attrs.startSize[gl_InstanceID];
	// float sizeSpeed = attrs.sizeSpeed[gl_InstanceID];
	// float startAngle = attrs.startAngle[gl_InstanceID];
	// float pointRotateSpeed = attrs.pointRotateSpeed[gl_InstanceID];
	// float startAlpha = attrs.startAlpha[gl_InstanceID];
	// float alphaSpeed = attrs.alphaSpeed[gl_InstanceID];
	vec2 pos = attrs[gl_InstanceID].pos;
	vec2 speed = attrs[gl_InstanceID].speed;
	vec2 aSpeed = attrs[gl_InstanceID].aSpeed;
	float startSize = attrs[gl_InstanceID].startSize;
	float sizeSpeed = attrs[gl_InstanceID].sizeSpeed;
	float startAngle = attrs[gl_InstanceID].startAngle;
	float pointRotateSpeed = attrs[gl_InstanceID].pointRotateSpeed;
	float startAlpha = attrs[gl_InstanceID].startAlpha;
	float alphaSpeed = attrs[gl_InstanceID].alphaSpeed;

	float t = aTime / 1000;
	float powt = t * t;
	vec2 pos2 = pos + speed * t + aSpeed * powt;
	// pos2 = pos2 * 0.00000001 + vec2(16 * t + 50*gl_InstanceID, 16 * t + 16);
	// vec2 pos2 = vec2(pos[0], pos[1]);
	fAlpha = startAlpha + alphaSpeed * t;

	float size = (startSize + sizeSpeed * t) / 2;
	size = max(size, 0);
	mat2 sm = scaleMatrix(size, size);
	// mat2 rm = rotateMatrix(aTime / 5000 * 2 * M_PI);
	mat2 rm = rotateMatrix(startAngle + pointRotateSpeed * t);

	pos2 = pos2 + (sm * (rm * index));
	// vec2 pos2 = vec2(16,16) + (sm * (rm * aIndex));

	vec4 pos4 = vec4(pos2, 0.0, 1);
	// gl_Position.x += aTime * 0.1;
	// pos4.x += aTime * 0.04;
	// pos4.y += 0;

	pos4 = uMVP * pos4;
	gl_Position = pos4;
	fCoord = vCoord;
	fSize = size;
}
