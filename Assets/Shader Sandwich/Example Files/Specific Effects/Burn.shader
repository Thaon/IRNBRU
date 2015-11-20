Shader "Shader Sandwich/Specific/Burn" {//The Shaders Name
//The inputs shown in the material panel
Properties {
	_MainTex ("Base", 2D) = "white" {}
	_SSSBurn_Texture ("Burn Texture", 2D) = "white" {}
	_Color ("Burn Color", Color) = (1,0.6117647,0.02941179,1)
	_Cutoff ("Burn Amount", Range(-0.500000000,1.200000000)) = 0.194714500
	_SSSGlow ("Glow", Range(1.000000000,20.000000000)) = 4.562500000
	_SSSGlow_Width ("Glow Width", Range(-1.000000000,0.000000000)) = -0.526250000
}

SubShader {
	Tags { "RenderType"="Opaque""Queue"="AlphaTest" }//A bunch of settings telling Unity a bit about the shader.
	LOD 200
	ZWrite On
	cull Off//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	sampler2D _SSSBurn_Texture;
	float4 _Color;
	float _Cutoff;
	float _SSSGlow;
	float _SSSGlow_Width;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLPBR_Standard addshadow  fullforwardshadows
//The Shader Target defines the maximum capabilites of the shader (Number of math operators, texture reads, etc.)
	#pragma target 2.0
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
		float2 uv_SSSBurn_Texture;
	};






//Generate simpler lighting code:
half4 LightingCLPBR_Standard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.
	float3 Spec;
	half3 h = normalize (lightDir + viewDir);	
	float nh = max (0, dot (s.Normal, h));
	Spec = pow (nh, s.Smoothness*128.0) * s.Specular;
	Spec = Spec * atten * 2 * lightColor.rgb;
	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/(28.26))/9.0f);


c.rgb = c.rgb*s.Albedo+Spec;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Include a bunch of PBS Code from files UnityPBSLighting.cginc and UnityStandardBRDF.cginc for the purpose of custom lighting effects.

half4 BRDF1_Unity_PBSSS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half3 SSlightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = normal;
	half3 SSalbedo = diffColor;
	half3 SSspecular = specColor;
	half3 SSemission = float3(0,0,0);
	half SSalpha = 1;

	half roughness = 1-oneMinusRoughness;
	half3 halfDir = normalize (light.dir + viewDir);

	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);
	half lv = DotClamped (light.dir, viewDir);
	half lh = DotClamped (light.dir, halfDir);

#if UNITY_BRDF_GGX
	half V = SmithGGXVisibilityTerm (nl, nv, roughness);
	half D = GGXTerm (nh, roughness);
#else
	half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
	half D = NDFBlinnPhongNormalizedTerm (nh, RoughnessToSpecPower (roughness));
#endif

	half nlPow5 = Pow5 (1-nl);
	half nvPow5 = Pow5 (1-nv);
	half Fd90 = 0.5 + 2 * lh * lh * roughness;
	half disneyDiffuse = (1 + (Fd90-1) * nlPow5) * (1 + (Fd90-1) * nvPow5);
	
	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side Non-important lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is part of single constant together with 1/4 now
half specularTerm = max(0, (V * D * nl) * unity_LightGammaCorrectionConsts_PIDiv4);// Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)
	half diffuseTerm = disneyDiffuse * nl;
	
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
	

	half3 lightColor = (gi.diffuse + light.color * diffuseTerm);
	half3 specularColor = specularTerm*light.color * FresnelTerm (specColor, lh)+(gi.specular * FresnelLerp (specColor, grazingTerm, nv));


    half3 color =	diffColor * lightColor
                    + specularColor;

	return half4(color, 1);
}			
			
// Based on Minimalist CookTorrance BRDF
// Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255
//
// * BlinnPhong as NDF
// * Modified Kelemen and Szirmay-​Kalos for Visibility term
// * Fresnel approximated with 1/LdotH
half4 BRDF2_Unity_PBSSS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half3 halfDir = normalize (light.dir + viewDir);
	half3 SSlightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = normal;
	half3 SSalbedo = diffColor;
	half3 SSspecular = specColor;
	half3 SSemission = float3(0,0,0);
	half SSalpha = 1;


	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);
	half lh = DotClamped (light.dir, halfDir);

	half roughness = 1-oneMinusRoughness;
	half specularPower = RoughnessToSpecPower (roughness);
	// Modified with approximate Visibility function that takes roughness into account
	// Original ((n+1)*N.H^n) / (8*Pi * L.H^3) didn't take into account roughness 
	// and produced extremely bright specular at grazing angles

	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side Non-important lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is cancelled with Pi in denominator

	half invV = lh * lh * oneMinusRoughness + roughness * roughness; // approx ModifiedKelemenVisibilityTerm(lh, 1-oneMinusRoughness);
	half invF = lh;
	half specular = ((specularPower + 1) * pow (nh, specularPower)) / (unity_LightGammaCorrectionConsts_8 * invV * invF + 1e-4f); // @TODO: might still need saturate(nl*specular) on Adreno/Mali

	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
	
	half3 lightColor = light.color * nl;
	half3 specularColor = specular * specColor * lightColor + gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);
	

	lightColor += gi.diffuse;
	


	
	half3 color =	diffColor* lightColor + specularColor;

	return half4(color, 1);
}

// Old school, not microfacet based Modified Normalized Blinn-Phong BRDF
// Implementation uses Lookup texture for performance
//
// * Normalized BlinnPhong in RDF form
// * Implicit Visibility term
// * No Fresnel term
//
// TODO: specular is too weak in Linear rendering mode
half4 BRDF3_Unity_PBSSS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp
	half3 SSlightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = normal;
	half3 SSalbedo = diffColor;
	half3 SSspecular = specColor;
	half3 SSemission = float3(0,0,0);
	half SSalpha = 1;


	half3 reflDir = reflect (viewDir, normal);
	half3 halfDir = normalize (light.dir + viewDir);

	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);

	// Vectorize Pow4 to save instructions
	half2 rlPow4AndFresnelTerm = Pow4 (half2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
	half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
	half fresnelTerm = rlPow4AndFresnelTerm.y;
#if 1 // Lookup texture to save instructions

	half specular = tex2D(unity_NHxRoughness, half2(rlPow4, 1-oneMinusRoughness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;
#else
	half roughness = 1-oneMinusRoughness;
	half n = RoughnessToSpecPower (roughness) * .25;
	half specular = (n + 2.0) / (2.0 * UNITY_PI * UNITY_PI) * pow(dot(reflDir, light.dir), n) * nl;// / unity_LightGammaCorrectionConsts_PI;
	//half specular = (1.0/(UNITY_PI*roughness*roughness)) * pow(dot(reflDir, light.dir), n) * nl;// / unity_LightGammaCorrectionConsts_PI;
#endif
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));

	half3 lightColor = light.color * nl;
	half3 specularColor = specular * specColor * lightColor + gi.specular * lerp (specColor, grazingTerm, fresnelTerm);

lightColor += gi.diffuse;	


	
    half3 color =	diffColor* lightColor + specularColor;

	return half4(color, 1);
}
#if !defined (UNITY_BRDF_PBSSS) // allow to explicitly override BRDF in custom shader
	#if (SHADER_TARGET < 30) || defined(SHADER_API_PSP2)
		// Fallback to low fidelity one for pre-SM3.0
		#define UNITY_BRDF_PBSSS BRDF3_Unity_PBSSS
	#elif defined(SHADER_API_MOBILE)
		// Somewhat simplified for mobile
		#define UNITY_BRDF_PBSSS BRDF2_Unity_PBSSS
	#else
		// Full quality for SM3+ PC / consoles
		#define UNITY_BRDF_PBSSS BRDF1_Unity_PBSSS
	#endif
#endif
//Generate lighting code similar to the Unity Standard Shader. Not gonna deny, I have no clue how much of it works.
half4 LightingCLPBR_Standard (CSurfaceOutput s, half3 viewDir, UnityGI gi){
	s.Normal = normalize(s.Normal);
	// energy conservation
	half oneMinusReflectivity;
	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);
	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);
	half4 c = UNITY_BRDF_PBSSS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
	c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
	c.a = outputAlpha;
	return c;
}

inline void LightingCLPBR_Standard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, s.Smoothness, s.Normal);
#endif

}
#endif


//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	float2 uv_SSSBurn_Texture = IN.uv_SSSBurn_Texture;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0.3;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Set default mask color
		float Mask1 = 1;
	//Generate layers for the Mask1 channel.
		//Generate Layer: Mask0 Copy
			//Sample parts of the layer:
				half4 Mask0_CopyMask1_Sample1 = tex2D(_SSSBurn_Texture,(((uv_SSSBurn_Texture.xy))));

			//Apply Effects:
				Mask0_CopyMask1_Sample1 = (Mask0_CopyMask1_Sample1-(_Cutoff));
				Mask0_CopyMask1_Sample1 = (Mask0_CopyMask1_Sample1-(_SSSGlow_Width));
				Mask0_CopyMask1_Sample1 = (float4(1,1,1,1)-Mask0_CopyMask1_Sample1);
				Mask0_CopyMask1_Sample1 = lerp(float4(0.5,0.5,0.5,0.5),Mask0_CopyMask1_Sample1,3);
				Mask0_CopyMask1_Sample1 = clamp(Mask0_CopyMask1_Sample1,0,1);

			//Set the mask to the new color
				Mask1 = Mask0_CopyMask1_Sample1.a;

	//Generate layers for the Alpha channel.
		//Generate Layer: Alpha
			//Sample parts of the layer:
				half4 AlphaAlpha_Sample1 = tex2D(_SSSBurn_Texture,(((uv_SSSBurn_Texture.xy))));

			//Set the channel to the new color
				o.Alpha = AlphaAlpha_Sample1.a;

	clip(o.Alpha-_Cutoff);
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = TextureDiffuse_Sample1.rgb;

	//Generate layers for the Emission channel.
		//Generate Layer: Alpha Copy
			//Sample parts of the layer:
				half4 Alpha_CopyEmission_Sample1 = tex2D(_SSSBurn_Texture,(((uv_SSSBurn_Texture.xy))));

			//Apply Effects:
				Alpha_CopyEmission_Sample1.rgb = float3(dot(float3(0.3,0.59,0.11),Alpha_CopyEmission_Sample1.rgb),dot(float3(0.3,0.59,0.11),Alpha_CopyEmission_Sample1.rgb),dot(float3(0.3,0.59,0.11),Alpha_CopyEmission_Sample1.rgb));

			//Blend the layer into the channel using the Mix blend mode
				Emission = lerp(Emission,Alpha_CopyEmission_Sample1.rgba,Mask1);

		//Generate Layer: Emission 2 Copy
			//Sample parts of the layer:
				half4 Emission_2_CopyEmission_Sample1 = _Color;

			//Blend the layer into the channel using the Multiply blend mode
				Emission *= Emission_2_CopyEmission_Sample1.rgba;

		//Generate Layer: Emission Copy
			//Sample parts of the layer:
				half4 Emission_CopyEmission_Sample1 = Emission;

			//Apply Effects:
				Emission_CopyEmission_Sample1.rgb = (Emission_CopyEmission_Sample1.rgb*_SSSGlow);

			//Set the channel to the new color
				Emission = Emission_CopyEmission_Sample1.rgba;

	o.Emission = Emission.rgb;
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
VisName #! Base #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #! df74a0cbfe3033e48af5c068e2386265 #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0,0,0,0 #^ CC0 #?Color
Number #! 0 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 2 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 0 #^ CC0 #?Type
VisName #! Burn Texture #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #! 354f7eb7f261f094399e01c57c0c2a0d #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0,0,0,0 #^ CC0 #?Color
Number #! 0 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 1 #^ CC0 #?Type
VisName #! Burn Color #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 1,0.6117647,0.02941179,1 #^ CC0 #?Color
Number #! 0 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 1 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Burn Amount #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0,0,0,0 #^ CC0 #?Color
Number #! 0.1947145 #^ CC0 #?Number
Range0 #! -0.5 #^ CC0 #?Range0
Range1 #! 1.2 #^ CC0 #?Range1
MainType #! 10 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Glow #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0,0,0,0 #^ CC0 #?Color
Number #! 4.5625 #^ CC0 #?Number
Range0 #! 1 #^ CC0 #?Range0
Range1 #! 20 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Glow Width #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! -0.52625 #^ CC0 #?Number
Range0 #! -1 #^ CC0 #?Range0
Range1 #! 0 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
ShaderName #! Shader Sandwich/Specific/Burn #^ CC0 #?ShaderName
Hard Mode #! True #^ CC0 #?Hard Mode
Tech Lod #! 200 #^ CC0 #?Tech Lod
Cull #! 0 #^ CC0 #?Cull
Tech Shader Target #! 2 #^ CC0 #?Tech Shader Target
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
Lighting Type #! 4 #^ CC0 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Setting1 #! 0 #^ CC0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #^ CC0 #?Wrap Color
Use Normals #! 0 #^ CC0 #?Use Normals
Specular On #! True #^ CC0 #?Specular On
Specular Type #! 0 #^ CC0 #?Specular Type
Spec Hardness #! 0.3 #^ CC0 #?Spec Hardness
Spec Color #! 0.8,0.8,0.8,1 #^ CC0 #?Spec Color
Spec Energy Conserve #! True #^ CC0 #?Spec Energy Conserve
Spec Offset #! 0 #^ CC0 #?Spec Offset
Emission On #! True #^ CC0 #?Emission On
Emission Color #! 0,0,0,0 #^ CC0 #?Emission Color
Emission Type #! 0 #^ CC0 #?Emission Type
Transparency On #! True #^ CC0 #?Transparency On
Transparency Type #! 0 #^ CC0 #?Transparency Type
ZWrite #! False #^ CC0 #?ZWrite
Use PBR #! True #^ CC0 #?Use PBR
Transparency #! 0.1947145 #^ CC0 #^ 3 #?Transparency
Receive Shadows #! False #^ CC0 #?Receive Shadows
ZWrite Type #! 0 #^ CC0 #?ZWrite Type
Blend Mode #! 0 #^ CC0 #?Blend Mode
Shells On #! False #^ CC0 #?Shells On
Shell Count #! 4 #^ CC0 #?Shell Count
Shells Distance #! 0.02592593 #^ CC0 #?Shells Distance
Shell Ease #! 0 #^ CC0 #?Shell Ease
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
EndTag #! a #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Mask0 #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 354f7eb7f261f094399e01c57c0c2a0d    #^ CC0 #^ 1 #?Main Texture
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
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask1 #^ CC0 #?LayerListUniqueName
LayerListName #! Mask1 #^ CC0 #?LayerListName
Is Mask #! True #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Mask0 Copy #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 354f7eb7f261f094399e01c57c0c2a0d    #^ CC0 #^ 1 #?Main Texture
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
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathSub #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 1 #^ CC0 #?UseAlpha
Subtract #! 0.1947145 #^ CC0 #^ 3 #?Subtract
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathSub #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 1 #^ CC0 #?UseAlpha
Subtract #! -0.52625 #^ CC0 #^ 5 #?Subtract
EndShaderEffect
BeginShaderEffect
TypeS #! SSEInvert #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 1 #^ CC0 #?UseAlpha
EndShaderEffect
BeginShaderEffect
TypeS #! SSEContrast #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 1 #^ CC0 #?UseAlpha
Contrast #! 3 #^ CC0 #?Contrast
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathClamp #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 1 #^ CC0 #?UseAlpha
Min #! 0 #^ CC0 #?Min
Max #! 1 #^ CC0 #?Max
EndShaderEffect
EndShaderLayer
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
Main Color #! 0.8,0.8,0.8,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! df74a0cbfe3033e48af5c068e2386265  #^ CC0 #^ 0 #?Main Texture
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
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellDiffuse #^ CC0 #?LayerListUniqueName
LayerListName #! Diffuse #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Texture Copy #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 0.8,0.8,0.8,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 46e87a5d3e5194d4bbb1be104ac31eda       #^ CC0 #^ 0 #?Main Texture
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
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Alpha #^ CC0 #?LayerListUniqueName
LayerListName #! Alpha #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Alpha #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 354f7eb7f261f094399e01c57c0c2a0d    #^ CC0 #^ 1 #?Main Texture
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
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellAlpha #^ CC0 #?LayerListUniqueName
LayerListName #! Alpha #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Alpha Copy 2 #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 7d1eba9f36d4566438c9bd1f26bd00ac      #^ CC0 #^ 1 #?Main Texture
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
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
EndShaderLayer
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
BeginShaderLayer
Layer Name #! Alpha Copy #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 354f7eb7f261f094399e01c57c0c2a0d  #^ CC0 #^ 1 #?Main Texture
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
Stencil #! 1 #^ CC0 #?Stencil
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEDesaturate #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Emission 2 Copy #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 1,0.6117647,0.02941179,1 #^ CC0 #^ 2 #?Main Color
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
Mix Type #! 3 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Emission Copy #^ CC0 #?Layer Name
Layer Type #! 6 #^ CC0 #?Layer Type
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
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathMul #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Multiply #! 4.5625 #^ CC0 #^ 4 #?Multiply
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellEmission #^ CC0 #?LayerListUniqueName
LayerListName #! Emission #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Alpha Copy Copy #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 1,1,1,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 7d1eba9f36d4566438c9bd1f26bd00ac      #^ CC0 #^ 1 #?Main Texture
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
Stencil #! 1 #^ CC0 #?Stencil
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEHueShift #^ CC0 #?TypeS
IsVisible #! False #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Hue #! 0 #^ CC0 #?Hue
Saturation #! 1 #^ CC0 #?Saturation
Value #! 0 #^ CC0 #?Value
EndShaderEffect
BeginShaderEffect
TypeS #! SSEDesaturate #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Emission 2 Copy Copy #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 1,0.6117647,0.02941179,1 #^ CC0 #^ 2 #?Main Color
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
Mix Type #! 3 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Emission #^ CC0 #?Layer Name
Layer Type #! 6 #^ CC0 #?Layer Type
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
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathMul #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Multiply #! 4.5625 #^ CC0 #^ 4 #?Multiply
EndShaderEffect
EndShaderLayer
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
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingSpecular #^ CC0 #?LayerListUniqueName
LayerListName #! Specular #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
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
EndTag #! rgb #^ CC0 #?EndTag
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
