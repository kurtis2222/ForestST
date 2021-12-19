Shader "Shadow"
{
	Subshader
	{
		UsePass "VertexLit/SHADOWCOLLECTOR"    
		UsePass "VertexLit/SHADOWCASTER"
	}
	Fallback off
}