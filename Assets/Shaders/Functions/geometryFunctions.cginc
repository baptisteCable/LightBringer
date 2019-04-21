float2 translate(float2 pix, float translation) {
	return pix + float2(translation, translation);
}

float2 translate(float2 pix, float translationX, float translationY) {
	return pix + float2(translationX, translationY);
}

float2 rotate(float2 pix, float rotation) {
	float sinX = sin(rotation);
	float cosX = cos(rotation);
	float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
	return mul(pix, rotationMatrix);
}