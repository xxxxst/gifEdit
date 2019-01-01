// #version 150 compatibility
#version 430 core
#define M_PI 3.1415926535897932384626433832795

uniform mat4 uMVP;
in vec2 index;
in vec2 vCoord;

struct Attr{
	vec2 pos;
	vec2 speed;
	vec2 aSpeed;
	// float rotateSpeed;
	// float directionSpeed;
	float startSize;
	float sizeSpeed;
	float startAngle;
	float particleRotateSpeed;
	float startAlpha;
	float alphaSpeed;
	float lifeTime;
	float startTime;
};

layout(std430, binding = 0) buffer AttrBuf{
	Attr attr[];
};

uniform float nowTime;
uniform float totalLifeTime;
// uniform float zIndex;
uniform int particleCount;

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
	if(totalLifeTime <= 0){
		gl_Position = vec4(-1, -1, -1, 1);
		fCoord = vCoord;
		fSize = 0;
		return;
	}

	//int idx = gl_InstanceID;
	int idx = int((totalLifeTime - mod(nowTime, totalLifeTime)) / totalLifeTime * particleCount + gl_InstanceID);
	idx = int(mod(idx, particleCount));
	float lifeTime = attr[idx].lifeTime;
	float startTime = attr[idx].startTime;

	if(nowTime < startTime || lifeTime <= 0){
		gl_Position = vec4(-1, -1, -1, 1);
		fCoord = vCoord;
		fSize = 0;
		return;
	}

	vec2 pos = attr[idx].pos;
	vec2 speed = attr[idx].speed;
	vec2 aSpeed = attr[idx].aSpeed;
	float startSize = attr[idx].startSize;
	float sizeSpeed = attr[idx].sizeSpeed;
	float startAngle = attr[idx].startAngle;
	float particleRotateSpeed = attr[idx].particleRotateSpeed;
	float startAlpha = attr[idx].startAlpha;
	float alphaSpeed = attr[idx].alphaSpeed;

	float t = mod(nowTime - startTime, lifeTime);
	t = t / 1000;
	float powt = t * t;
	vec2 pos2 = pos + speed * t + aSpeed * powt;
	// pos2 = pos2 * 0.00000001 + vec2(16 * t + 50*gl_InstanceID, 16 * t + 16);
	// vec2 pos2 = vec2(pos[0], pos[1]);
	fAlpha = startAlpha + alphaSpeed * t;

	float size = (startSize + sizeSpeed * t) / 2;
	size = max(size, 0);
	mat2 sm = scaleMatrix(size, size);
	// mat2 rm = rotateMatrix(nowTime / 5000 * 2 * M_PI);
	mat2 rm = rotateMatrix(startAngle + particleRotateSpeed * t);

	pos2 = pos2 + (sm * (rm * index));
	// vec2 pos2 = vec2(16,16) + (sm * (rm * aIndex));

	vec4 pos4 = vec4(pos2, 0.0, 1);

	pos4 = uMVP * pos4;
	gl_Position = pos4;
	fCoord = vCoord;
	fSize = size;
}
