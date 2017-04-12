attribute vec4 vPosition;
uniform mat4 modelViewProjectionMatrix;

void main()
{
	gl_Position = modelViewProjectionMatrix *  vPosition;
}