Shader "Shader Sandwich/Specific/Color Key" {//The Shaders Name
//The inputs shown in the material panel
Properties {
	_MainTex ("Texture", 2D) = "white" {}
	_SSSDespill ("Despill", Range(0.000000000,1.000000000)) = 0.396761100
	_SSSBias ("Bias", Range(1.000000000,-1.000000000)) = 0.212500000
	_SSSHardness ("Hardness", Range(1.000000000,20.000000000)) = 13.468750000
	_SSSTexture3_aMain_Color ("Texture3 - Main Color", Color) = (0.2452422,0.6176471,0.34382,1)
}

SubShader {
	Tags { "RenderType"="Opaque""Queue"="Transparent" }//A bunch of settings telling Unity a bit about the shader.
	LOD 200
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _SSSDespill;
	float _SSSBias;
	float _SSSHardness;
	float4 _SSSTexture3_aMain_Color;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLUnlit addshadow  noforwardadd noambient novertexlights nolightmap nodynlightmap nodirlightmap alpha:fade fullforwardshadows
//The Shader Target defines the maximum capabilites of the shader (Number of math operators, texture reads, etc.)
	#pragma target 3.0
//Create a struct which can contain various pixel properties, like specular colors, albedo, normals etc.
	struct CSurfaceOutput 
	{ 
		half3 Albedo; 
		half3 Normal; 
		half3 Emission; 
		half Smoothness; 
		half3 Specular; 
		half Alpha; 
		half Occlusion; 
	};
//Create an Input struct, which lets us read different things that the Surface Shader creates. These are things like the view direction, world position etc.
	struct Input {
		float3 viewDir;
		float2 uv_MainTex;
	};





//Generate simpler lighting code:
half4 LightingCLUnlit (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half4 c;
	//Just pass the color and alpha without adding any lighting.
	c.rgb = float3(1,1,1);
	c.a = s.Alpha;


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
#endif


//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Set default mask color
		float Mask0 = 1;
	//Generate layers for the Mask0 channel.
		//Generate Layer: Mask0 Copy Copy
			//Sample parts of the layer:
				half4 Mask0_Copy_CopyMask0_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Apply Effects:
				Mask0_Copy_CopyMask0_Sample1.rgb = ((distance(Mask0_Copy_CopyMask0_Sample1.rgb.xyz,float3(_SSSTexture3_aMain_Color.r,_SSSTexture3_aMain_Color.g,_SSSTexture3_aMain_Color.b))).rrrr);
				Mask0_Copy_CopyMask0_Sample1.rgb = (float3(1,1,1)-Mask0_Copy_CopyMask0_Sample1.rgb);

			//Set the mask to the new color
				Mask0 = Mask0_Copy_CopyMask0_Sample1.r;

	//Generate layers for the Alpha channel.
		//Generate Layer: Alpha2
			//Sample parts of the layer:
				half4 Alpha2Alpha_Sample1 = float4(1, 1, 1, 1);

			//Set the channel to the new color
				o.Alpha = Alpha2Alpha_Sample1.r;

		//Generate Layer: Alpha3
			//Sample parts of the layer:
				half4 Alpha3Alpha_Sample1 = float4(0, 0, 0, 1);

			//Blend the layer into the channel using the Mix blend mode
				o.Alpha = lerp(o.Alpha,Alpha3Alpha_Sample1.r,Mask0);

		//Generate Layer: Mask0 Copy
			//Sample parts of the layer:
				half4 Mask0_CopyAlpha_Sample1 = float4(o.Alpha.rrr,1);

			//Apply Effects:
				Mask0_CopyAlpha_Sample1.rgb = (Mask0_CopyAlpha_Sample1.rgb+_SSSBias);
				Mask0_CopyAlpha_Sample1.rgb = lerp(float3(0.5,0.5,0.5),Mask0_CopyAlpha_Sample1.rgb,_SSSHardness);
				Mask0_CopyAlpha_Sample1.rgb = clamp(Mask0_CopyAlpha_Sample1.rgb,0,1);

			//Set the channel to the new color
				o.Alpha = Mask0_CopyAlpha_Sample1.r;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = TextureDiffuse_Sample1.rgb;

		//Generate Layer: Texture3
			//Sample parts of the layer:
				half4 Texture3Diffuse_Sample1 = _SSSTexture3_aMain_Color;

			//Blend the layer into the channel using the Subtract blend mode
				o.Albedo -= Texture3Diffuse_Sample1.rgb*_SSSDespill*Mask0;

}
	ENDCG
}

Fallback "VertexLit"
}

/*
BeginShaderParse
1.0
BeginShaderBase
BeginShaderInput
Type #! 0 #^ CC0 #?Type
VisName #! Texture #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #! dc3f8350ad07d0443b4dc300584d813a #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 0 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 2 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Despill #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 0.3967611 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Bias #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0,0,0,0 #^ CC0 #?Color
Number #! 0.2125 #^ CC0 #?Number
Range0 #! 1 #^ CC0 #?Range0
Range1 #! -1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Hardness #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0,0,0,0 #^ CC0 #?Color
Number #! 13.46875 #^ CC0 #?Number
Range0 #! 1 #^ CC0 #?Range0
Range1 #! 20 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 1 #^ CC0 #?Type
VisName #! Texture3 - Main Color #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.2452422,0.6176471,0.34382,1 #^ CC0 #?Color
Number #! 0.2235294 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
ShaderName #! Shader Sandwich/Specific/Color Key #^ CC0 #?ShaderName
Hard Mode #! True #^ CC0 #?Hard Mode
Tech Lod #! 200 #^ CC0 #?Tech Lod
Cull #! 1 #^ CC0 #?Cull
Tech Shader Target #! 3 #^ CC0 #?Tech Shader Target
Vertex Recalculation #! False #^ CC0 #?Vertex Recalculation
Use Fog #! True #^ CC0 #?Use Fog
Use Ambient #! True #^ CC0 #?Use Ambient
Use Vertex Lights #! True #^ CC0 #?Use Vertex Lights
Use Lightmaps #! True #^ CC0 #?Use Lightmaps
Use All Shadows #! True #^ CC0 #?Use All Shadows
Forward Add #! True #^ CC0 #?Forward Add
Shadows #! True #^ CC0 #?Shadows
Interpolate View #! False #^ CC0 #?Interpolate View
Half as View #! False #^ CC0 #?Half as View
Diffuse On #! True #^ CC0 #?Diffuse On
Lighting Type #! 3 #^ CC0 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Setting1 #! 0 #^ CC0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #^ CC0 #?Wrap Color
Use Normals #! 0 #^ CC0 #?Use Normals
Specular On #! False #^ CC0 #?Specular On
Specular Type #! 0 #^ CC0 #?Specular Type
Spec Hardness #! 0.3 #^ CC0 #?Spec Hardness
Spec Color #! 0.8,0.8,0.8,1 #^ CC0 #?Spec Color
Spec Energy Conserve #! True #^ CC0 #?Spec Energy Conserve
Spec Offset #! 0 #^ CC0 #?Spec Offset
Emission On #! False #^ CC0 #?Emission On
Emission Color #! 0,0,0,0 #^ CC0 #?Emission Color
Emission Type #! 0 #^ CC0 #?Emission Type
Transparency On #! True #^ CC0 #?Transparency On
Transparency Type #! 1 #^ CC0 #?Transparency Type
ZWrite #! False #^ CC0 #?ZWrite
Use PBR #! True #^ CC0 #?Use PBR
Transparency #! 1 #^ CC0 #?Transparency
Receive Shadows #! False #^ CC0 #?Receive Shadows
ZWrite Type #! 0 #^ CC0 #?ZWrite Type
Blend Mode #! 0 #^ CC0 #?Blend Mode
Shells On #! False #^ CC0 #?Shells On
Shell Count #! 1 #^ CC0 #?Shell Count
Shells Distance #! 0.1 #^ CC0 #?Shells Distance
Shell Ease #! 1 #^ CC0 #?Shell Ease
Shell Transparency Type #! 0 #^ CC0 #?Shell Transparency Type
Shell Transparency ZWrite #! False #^ CC0 #?Shell Transparency ZWrite
Shell Cull #! 0 #^ CC0 #?Shell Cull
Shells ZWrite #! True #^ CC0 #?Shells ZWrite
Shells Use Transparency #! True #^ CC0 #?Shells Use Transparency
Shell Blend Mode #! 0 #^ CC0 #?Shell Blend Mode
Shells Transparency #! 1 #^ CC0 #?Shells Transparency
Shell Lighting #! True #^ CC0 #?Shell Lighting
Shell Front #! True #^ CC0 #?Shell Front
Parallax On #! False #^ CC0 #?Parallax On
Parallax Height #! 0.1 #^ CC0 #?Parallax Height
Parallax Quality #! 10 #^ CC0 #?Parallax Quality
Silhouette Clipping #! False #^ CC0 #?Silhouette Clipping
Tessellation On #! False #^ CC0 #?Tessellation On
Tessellation Type #! 2 #^ CC0 #?Tessellation Type
Tessellation Quality #! 10 #^ CC0 #?Tessellation Quality
Tessellation Falloff #! 1 #^ CC0 #?Tessellation Falloff
Tessellation Smoothing Amount #! 0 #^ CC0 #?Tessellation Smoothing Amount
BeginShaderLayerList
LayerListUniqueName #! Mask0 #^ CC0 #?LayerListUniqueName
LayerListName #! Mask0 #^ CC0 #?LayerListName
Is Mask #! True #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! r #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Mask0 Copy Copy #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! dc3f8350ad07d0443b4dc300584d813a   #^ CC0 #^ 0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathDistance #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Dimensions #! 2 #^ CC0 #?Dimensions
X #! 0.2452422 #^ CC0 #^ 4 #?X
Y #! 0.6176471 #^ CC1 #^ 4 #?Y
Z #! 0.34382 #^ CC2 #^ 4 #?Z
W #! 0 #^ CC0 #?W
EndShaderEffect
BeginShaderEffect
TypeS #! SSEInvert #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask1 #^ CC0 #?LayerListUniqueName
LayerListName #! Ambient #^ CC0 #?LayerListName
Is Mask #! True #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Diffuse #^ CC0 #?LayerListUniqueName
LayerListName #! Diffuse #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Texture #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 0.627451,0.8,0.8823529,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! dc3f8350ad07d0443b4dc300584d813a   #^ CC0 #^ 0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Texture3 #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 0.2452422,0.6176471,0.34382,1 #^ CC0 #^ 4 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #!  #^ CC0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 0.3967611 #^ CC0 #^ 1 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 2 #^ CC0 #?Mix Type
Stencil #! 0 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellDiffuse #^ CC0 #?LayerListUniqueName
LayerListName #! Diffuse #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Alpha #^ CC0 #?LayerListUniqueName
LayerListName #! Alpha #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! r #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Alpha2 #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #!  #^ CC0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Alpha3 #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 0,0,0,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #!  #^ CC0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! 0 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask0 Copy #^ CC0 #?Layer Name
Layer Type #! 6 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! dc3f8350ad07d0443b4dc300584d813a       #^ CC0 #^ 0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathAdd #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Add #! 0.2125 #^ CC0 #^ 2 #?Add
EndShaderEffect
BeginShaderEffect
TypeS #! SSEContrast #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Contrast #! 13.46875 #^ CC0 #^ 3 #?Contrast
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathClamp #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Min #! 0 #^ CC0 #?Min
Max #! 1 #^ CC0 #?Max
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellAlpha #^ CC0 #?LayerListUniqueName
LayerListName #! Alpha #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Specular #^ CC0 #?LayerListUniqueName
LayerListName #! Specular #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellSpecular #^ CC0 #?LayerListUniqueName
LayerListName #! Specular #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Normals #^ CC0 #?LayerListUniqueName
LayerListName #! Normals #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellNormals #^ CC0 #?LayerListUniqueName
LayerListName #! Normals #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Emission #^ CC0 #?LayerListUniqueName
LayerListName #! Emission #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellEmission #^ CC0 #?LayerListUniqueName
LayerListName #! Emission #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Height #^ CC0 #?LayerListUniqueName
LayerListName #! Height #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingDiffuse #^ CC0 #?LayerListUniqueName
LayerListName #! Diffuse #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingSpecular #^ CC0 #?LayerListUniqueName
LayerListName #! Specular #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingIndirect #^ CC0 #?LayerListUniqueName
LayerListName #! Ambient #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingDirect #^ CC0 #?LayerListUniqueName
LayerListName #! Direct #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Vertex #^ CC0 #?LayerListUniqueName
LayerListName #! Vertex #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellVertex #^ CC0 #?LayerListUniqueName
LayerListName #! Vertex #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
EndShaderBase
EndShaderParse
*/
