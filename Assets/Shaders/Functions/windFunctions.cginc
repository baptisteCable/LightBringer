float4 explosionMovement(float4 worldPos, float2 center, float amplitude, float radius, float height) {
	float2 blow = worldPos.xz - center;
	float2 blowDir = normalize(blow);
	float dist = length(blow) / radius;
	float strength = amplitude * exp(-dist * dist * dist * dist / 2) * .01;
	float heightCoeff = worldPos.y / height * 10;
	float4 movement = float4(0, 0, 0, 0);
	movement.xz += blowDir.xy * strength * heightCoeff * heightCoeff;
	movement.y -= abs(strength) * worldPos.y * worldPos.y / (height * height / 15);
	return movement;
}

float f(float x) {
	return .3 + 0.15 * sin(x) + .35 * cos(2 * x) + .35 * cos(3 * x) + .25 * sin(12 * x);
}

float4 windMovement(float4 worldPos, float4 windDir, float windFrequency, float windStrength, float height) {
	float x = windFrequency * (_Time.x * 2 + worldPos.x / 100) + .3 * sin(worldPos.z / 10);
	float strength = 0.01 * f(x) * windStrength;
	float heightCoeff = worldPos.y / height * 10;
	float4 movement = float4(0, 0, 0, 0);
	movement.xz += normalize(windDir.xz) * strength * heightCoeff * heightCoeff;
	movement.y -= abs(strength) * worldPos.y * worldPos.y / (height * height / 15);
	return movement;
}