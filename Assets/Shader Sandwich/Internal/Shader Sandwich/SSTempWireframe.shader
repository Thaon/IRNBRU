Shader "Hidden/SSTempWireframe" {//The Shaders Name
//The inputs shown in the material panel
Properties {
	[HideInInspector]Texcoord ("Generic UV Coords (You shouldn't be seeing this aaaaah!)", 2D) = "white" {}
	_SSSTexture_aMain_Texture ("Texture - Main Texture", 2D) = "white" {}
	_SSSTransparency ("Transparency", Range(-1000,1000)) = 0
	_SSSSpecular_aMain_Color ("Specular - Main Color", Color) = (1,1,1,1)
	_SSSSpec_Hardness ("Spec Hardness", Range(-1000,1000)) = 0
	_SSSMask0_aZ_Offset ("Mask0 - Z Offset", Float) = 0
	_SSSMask0_aScale ("Mask0 - Scale", Float) = 0
	_SSSMask0_Copy_aScale ("Mask0 Copy - Scale", Float) = 0
	_SSSMask0_Copy_Copy_aScale ("Mask0 Copy Copy - Scale", Float) = 0
	_SSSTexture_Copy_aMain_Color ("Texture Copy - Main Color", Color) = (1,1,1,1)
	_SSSAlpha_aPower ("Alpha - Power", Float) = 0
	_SSSAlpha2_Copy_aMain_Color ("Alpha2 Copy - Main Color", Color) = (1,1,1,1)
	_SSSEmission2_aMain_Color ("Emission2 - Main Color", Color) = (1,1,1,1)
	_SSSEmission2_aMultiply ("Emission2 - Multiply", Float) = 0
	_SSSNormal_Map_aScale ("Normal Map - Scale", Float) = 0
	_SSSEmission_aMain_Color ("Emission - Main Color", Color) = (1,1,1,1)
	_SSSMask2_aSize ("Mask2 - Size", Range(-1000,1000)) = 0
	_SSSMask2_aZ_Offset ("Mask2 - Z Offset", Float) = 0
	_SSSMask2_aScale ("Mask2 - Scale", Float) = 0
	_SSSAlpha2_Copy_2_Copy_aMain_Color ("Alpha2 Copy 2 Copy - Main Color", Color) = (1,1,1,1)
	_SSSAlpha2_Copy_2_Copy_aMix_Amount ("Alpha2 Copy 2 Copy - Mix Amount", Range(-1000,1000)) = 0
	_SSSMask2_Copy_aScale ("Mask2 Copy - Scale", Float) = 0
	_SSSMask2_Copy_aSize ("Mask2 Copy - Size", Range(-1000,1000)) = 0
	_SSSMask3_aX_Offset ("Mask3 - X Offset", Float) = 0
	_SSSEmission3_aMain_Color ("Emission3 - Main Color", Color) = (1,1,1,1)
	_SSSMask32_aPower ("Mask32 - Power", Float) = 0
	_SSSFlicker_aX_Offset ("Flicker - X Offset", Float) = 0
	_SSSFlicker_Axis ("Flicker Axis", Color) = (1,1,1,1)
	_SSSFlickerNoise_aScale ("FlickerNoise - Scale", Float) = 0
	_SSSFlickerNoise_aY_Offset ("FlickerNoise - Y Offset", Float) = 0
	_SSSForce_Flicker ("Force Flicker", Range(-1000,1000)) = 0
	_SSSPull_Height ("Pull Height", Float) = 0
	_SSSPull_Axis ("Pull Axis", Color) = (1,1,1,1)
}

SubShader {
	Tags { "RenderType"="Opaque""Queue"="Transparent" }//A bunch of settings telling Unity a bit about the shader.
	LOD 200
	ZWrite On
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _SSSTexture_aMain_Texture;
	float _SSSTransparency;
	float4 _SSSSpecular_aMain_Color;
	float _SSSSpec_Hardness;
	float _SSSMask0_aZ_Offset;
	float _SSSMask0_aScale;
	float _SSSMask0_Copy_aScale;
	float _SSSMask0_Copy_Copy_aScale;
	float4 _SSSTexture_Copy_aMain_Color;
	float _SSSAlpha_aPower;
	float4 _SSSAlpha2_Copy_aMain_Color;
	float4 _SSSEmission2_aMain_Color;
	float _SSSEmission2_aMultiply;
	float _SSSNormal_Map_aScale;
	float4 _SSSEmission_aMain_Color;
	float _SSSMask2_aSize;
	float _SSSMask2_aZ_Offset;
	float _SSSMask2_aScale;
	float4 _SSSAlpha2_Copy_2_Copy_aMain_Color;
	float _SSSAlpha2_Copy_2_Copy_aMix_Amount;
	float _SSSMask2_Copy_aScale;
	float _SSSMask2_Copy_aSize;
	float _SSSMask3_aX_Offset;
	float4 _SSSEmission3_aMain_Color;
	float _SSSMask32_aPower;
	float _SSSFlicker_aX_Offset;
	float4 _SSSFlicker_Axis;
	float _SSSFlickerNoise_aScale;
	float _SSSFlickerNoise_aY_Offset;
	float _SSSForce_Flicker;
	float _SSSPull_Height;
	float4 _SSSPull_Axis;
//Setup some time stuff for the Shader Sandwich preview
	float4 _SSTime;
	float4 _SSSinTime;
	float4 _SSCosTime;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLPBR_Standard vertex:vert  addshadow  noambient novertexlights nolightmap nodynlightmap nodirlightmap
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
		float3 worldPos;
		float3 viewDir;
		float2 uv_SSSTexture_aMain_Texture;
		float2 uvTexcoord;
	};

//Some noise code based on the fantastic library by Brian Sharpe, he deserves a ton of credit :)
//brisharpe CIRCLE_A yahoo DOT com
//http://briansharpe.wordpress.com
//https://github.com/BrianSharpe
float2 Interpolation_C2( float2 x ) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
void FastHash2D(float2 Pos,out float4 hash_0, out float4 hash_1){
	float2 Offset = float2(26,161);
	float Domain = 71;
	float2 SomeLargeFloats = float2(951.135664,642.9478304);
	float4 P = float4(Pos.xy,Pos.xy+1);
	P = P-floor(P*(1.0/Domain))*Domain;
	P += Offset.xyxy;
	P *= P;
	P = P.xzxz*P.yyww;
	hash_0 = frac(P*(1/SomeLargeFloats.x));
	hash_1 = frac(P*(1/SomeLargeFloats.y));
}
float Noise2D(float2 P)
{
	float2 Pi = floor(P);
	float4 Pf_Pfmin1 = P.xyxy-float4(Pi,Pi+1);
	float4 HashX, HashY;
	FastHash2D(Pi,HashX,HashY);
	float4 GradX = HashX-0.499999;
	float4 GradY = HashY-0.499999;
	float4 GradRes = rsqrt(GradX*GradX+GradY*GradY)*(GradX*Pf_Pfmin1.xzxz+GradY*Pf_Pfmin1.yyww);
	
	GradRes *= 1.4142135623730950488016887242097;
	float2 blend = Interpolation_C2(Pf_Pfmin1.xy);
	float4 blend2 = float4(blend,float2(1.0-blend));
	return (dot(GradRes,blend2.zxzx*blend2.wwyy));
}
float3 Interpolation_C2( float3 x ) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
void FastHash3D(float3 Pos,out float4 hash_0, out float4 hash_1,out float4 hash_2, out float4 hash_3,out float4 hash_4, out float4 hash_5){
	float2 Offset = float2(50,161);
	float Domain = 69;
	float3 SomeLargeFloats = float3(635.298681, 682.357502, 668.926525 );
	float3 Zinc = float3( 48.500388, 65.294118, 63.934599 );
	
	Pos = Pos-floor(Pos*(1.0/Domain))*Domain;
	float3 Pos_Inc1 = step(Pos,float(Domain-1.5).rrr)*(Pos+1);
	
	float4 P = float4(Pos.xy,Pos_Inc1.xy)+Offset.xyxy;
	P *= P;
	P = P.xzxz*P.yyww;
	
	float3 lowz_mod = float3(1/(SomeLargeFloats+Pos.zzz*Zinc));//Pos.zzz
	float3 highz_mod = float3(1/(SomeLargeFloats+Pos_Inc1.zzz*Zinc));//Pos_Inc1.zzz
	
	hash_0 = frac(P*lowz_mod.xxxx);
	hash_1 = frac(P*lowz_mod.yyyy);
	hash_2 = frac(P*lowz_mod.zzzz);
	hash_3 = frac(P*highz_mod.xxxx);
	hash_4 = frac(P*highz_mod.yyyy);
	hash_5 = frac(P*highz_mod.zzzz);
}
float Noise3D(float3 P)
{
	float3 Pi = floor(P);
	float3 Pf = P-Pi;
	float3 Pf_min1 = Pf-1.0;
	
	float4 HashX0, HashY0, HashZ0, HashX1, HashY1, HashZ1;
	FastHash3D(Pi, HashX0, HashY0, HashZ0, HashX1, HashY1, HashZ1);
	
	float4 GradX0 = HashX0-0.49999999;
	float4 GradX1 = HashX1-0.49999999;
	float4 GradY0 = HashY0-0.49999999;
	float4 GradY1 = HashY1-0.49999999;
	float4 GradZ0 = HashZ0-0.49999999;
	float4 GradZ1 = HashZ1-0.49999999;

	float4 GradRes = rsqrt( GradX0 * GradX0 + GradY0 * GradY0 + GradZ0 * GradZ0) * ( float2( Pf.x, Pf_min1.x ).xyxy * GradX0 + float2( Pf.y, Pf_min1.y ).xxyy * GradY0 + Pf.zzzz * GradZ0 );
	float4 GradRes2 = rsqrt( GradX1 * GradX1 + GradY1 * GradY1 + GradZ1 * GradZ1) * ( float2( Pf.x, Pf_min1.x ).xyxy * GradX1 + float2( Pf.y, Pf_min1.y ).xxyy * GradY1 + Pf_min1.zzzz * GradZ1 );
	
	float3 Blend = Interpolation_C2(Pf);
	
	float4 Res = lerp(GradRes,GradRes2,Blend.z);
	float4 Blend2 = float4(Blend.xy,float2(1.0-Blend.xy));
	float Final = dot(Res,Blend2.zxzx*Blend2.wwyy);
	Final *= 1.1547005383792515290182975610039;
	return Final;
}
float Unique1D(float t){
	//return frac(sin(floor(t.x))*43558.5453);
	return frac(sin(dot(t ,12.9898)) * 43758.5453);
	//return frac(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
	//return frac(sin(n)*43758.5453);
}
float Lerpify(float P){
	float ft = P * 3.1415927;
	float f = (1 - cos(ft)) * 0.5;
	return f;
}
float D1Lerp(float P, float Col1,float Col2){
	float ft = P * 3.1415927;
	float f = (1 - cos(ft)) * 0.5;
	return Col1+((Col2-Col1)*f);//(Col1*P)+(Col2*(1-P));
}
float Unique2D(float2 t){
	float x = frac(sin(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453);
	//float x = frac(frac(tan(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453)*7.35);
	return x;
}
float Lerp2D(float2 P, float Col1,float Col2,float Col3,float Col4){
	float2 ft = P * 3.1415927;
	float2 f = (1 - cos(ft)) * 0.5;
	P = f;
	float S1 = lerp(Col1,Col2,P.x);
	float S2 = lerp(Col3,Col4,P.x);
	float L = lerp(S1,S2,P.y);
	return L;
}
float NoiseB2D(float2 P)
{
	float SS = Unique2D(P);
	float SE = Unique2D(P+float2(1,0));
	float ES = Unique2D(P+float2(0,1));
	float EE = Unique2D(P+float2(1,1));
	float xx = Lerp2D(frac(P),SS,SE,ES,EE);
	return xx;
}

float NoiseB1D(float P)
{
	float SS = Unique1D(P);
	float SE = Unique1D(P+1);
	float xx = D1Lerp(frac(P),SS,SE);
	return xx;
}
float Unique3D(float3 t){
	float x = frac(tan(dot(tan(floor(t)),float3(12.9898,78.233,35.344))) * 9.5453);
	return x;
}

float Lerp3D(float3 P, float SSS,float SES,float ESS,float EES, float SSE,float SEE,float ESE,float EEE){
	float3 ft = P * 3.1415927;
	float3 f = (1 - cos(ft)) * 0.5;
	float S1 = lerp(SSS,SES,f.x);
	float S2 = lerp(ESS,EES,f.x);
	float F1 = lerp(S1,S2,f.y);
	float S3 = lerp(SSE,SEE,f.x);
	float S4 = lerp(ESE,EEE,f.x);
	float F2 = lerp(S3,S4,f.y);
	float L = lerp(F1,F2,f.z);//F1;
	return L;
}
float NoiseB3D(float3 P)
{
	float SSS = Unique3D(P+float3(0,0,0));
	float SES = Unique3D(P+float3(1,0,0));
	float ESS = Unique3D(P+float3(0,1,0));
	float EES = Unique3D(P+float3(1,1,0));
	float SSE = Unique3D(P+float3(0,0,1));
	float SEE = Unique3D(P+float3(1,0,1));
	float ESE = Unique3D(P+float3(0,1,1));
	float EEE = Unique3D(P+float3(1,1,1));
	float xx = Lerp3D(frac(P),SSS,SES,ESS,EES,SSE,SEE,ESE,EEE);
	return xx;
}
void FastHash2D(float2 Pos,out float4 hash_0, out float4 hash_1, out float4 hash_2){
	float2 Offset = float2(26,161);
	float Domain = 71;
	float3 SomeLargeFloats = float3(951.135664,642.9478304,803.202459);
	float4 P = float4(Pos.xy,Pos.xy+1);
	P = P-floor(P*(1.0/Domain))*Domain;
	P += Offset.xyxy;
	P *= P;
	P = P.xzxz*P.yyww;
	hash_0 = frac(P*(1/SomeLargeFloats.x));
	hash_1 = frac(P*(1/SomeLargeFloats.y));
	hash_2 = frac(P*(1/SomeLargeFloats.z));
}
float NoiseC2D(float2 P,float2 Vals)
{
	float2 Pi = floor(P);
	float4 Pf_Pfmin1 = P.xyxy-float4(Pi,Pi+1);
	float4 HashX, HashY, HashValue;
	FastHash2D(Pi,HashX,HashY,HashValue);
	float4 GradX = HashX-0.499999;
	float4 GradY = HashY-0.499999;
	float4 GradRes = rsqrt(GradX*GradX+GradY*GradY)*(GradX*Pf_Pfmin1.xzxz+GradY*Pf_Pfmin1.yyww);
	GradRes = ( HashValue - 0.5 ) * ( 1.0 / GradRes );
	
	GradRes *= 1.4142135623730950488016887242097;
	float2 blend = Interpolation_C2(Pf_Pfmin1.xy);
	float4 blend2 = float4(blend,float2(1.0-blend));
	float final = (dot(GradRes,blend2.zxzx*blend2.wwyy));
	return clamp((final+Vals.x)*Vals.y,0.0,1.0);
}


void FastHash3D(float3 Pos,out float4 hash_0, out float4 hash_1,out float4 hash_2, out float4 hash_3,out float4 hash_4, out float4 hash_5,out float4 hash_6, out float4 hash_7){
	float2 Offset = float2(50,161);
	float Domain = 69;
	float4 SomeLargeFloats = float4(635.298681, 682.357502, 668.926525, 588.255119 );
	float4 Zinc = float4( 48.500388, 65.294118, 63.934599, 63.279683 );
	
	Pos = Pos-floor(Pos*(1.0/Domain))*Domain;
	float3 Pos_Inc1 = step(Pos,float(Domain-1.5).rrr)*(Pos+1);
	
	float4 P = float4(Pos.xy,Pos_Inc1.xy)+Offset.xyxy;
	P *= P;
	P = P.xzxz*P.yyww;
	
	float4 lowz_mod = float4(1/(SomeLargeFloats+Pos.zzzz*Zinc));//Pos.zzz
	float4 highz_mod = float4(1/(SomeLargeFloats+Pos_Inc1.zzzz*Zinc));//Pos_Inc1.zzz
	
	hash_0 = frac(P*lowz_mod.xxxx);
	hash_1 = frac(P*lowz_mod.yyyy);
	hash_2 = frac(P*lowz_mod.zzzz);
	hash_3 = frac(P*highz_mod.xxxx);
	hash_4 = frac(P*highz_mod.yyyy);
	hash_5 = frac(P*highz_mod.zzzz);
	hash_6 = frac(P*highz_mod.wwww);
	hash_7 = frac(P*highz_mod.wwww);
}
float NoiseC3D(float3 P,float2 Vals)
{
	float3 Pi = floor(P);
	float3 Pf = P-Pi;
	float3 Pf_min1 = Pf-1.0;
	
	float4 HashX0, HashY0, HashZ0, HashX1, HashY1, HashZ1, HashValue0, HashValue1;
	FastHash3D(Pi, HashX0, HashY0, HashZ0, HashX1, HashY1, HashZ1, HashValue0, HashValue1);
	
	float4 GradX0 = HashX0-0.49999999;
	float4 GradX1 = HashX1-0.49999999;
	float4 GradY0 = HashY0-0.49999999;
	float4 GradY1 = HashY1-0.49999999;
	float4 GradZ0 = HashZ0-0.49999999;
	float4 GradZ1 = HashZ1-0.49999999;

	float4 GradRes = rsqrt( GradX0 * GradX0 + GradY0 * GradY0 + GradZ0 * GradZ0) * ( float2( Pf.x, Pf_min1.x ).xyxy * GradX0 + float2( Pf.y, Pf_min1.y ).xxyy * GradY0 + Pf.zzzz * GradZ0 );
	float4 GradRes2 = rsqrt( GradX1 * GradX1 + GradY1 * GradY1 + GradZ1 * GradZ1) * ( float2( Pf.x, Pf_min1.x ).xyxy * GradX1 + float2( Pf.y, Pf_min1.y ).xxyy * GradY1 + Pf_min1.zzzz * GradZ1 );

	GradRes = ( HashValue0 - 0.5 ) * ( 1.0 / GradRes );
	GradRes2 = ( HashValue1 - 0.5 ) * ( 1.0 / GradRes2 );
	
	float3 Blend = Interpolation_C2(Pf);
	
	float4 Res = lerp(GradRes,GradRes2,Blend.z);
	float4 Blend2 = float4(Blend.xy,float2(1.0-Blend.xy));
	float Final = dot(Res,Blend2.zxzx*Blend2.wwyy);
	return clamp((Final+Vals.x)*Vals.y,0.0,1.0);
}

float4 CellularWeightSamples( float4 Samples )
{
	Samples = Samples * 2.0 - 1;
	//return (1.0 - Samples * Samples) * sign(Samples);
	return (Samples * Samples * Samples) - sign(Samples);
}
float NoiseD2D(float2 P,float Jitter)
{
	float2 Pi = floor(P);
	float2 Pf = P-Pi;
	float4 HashX, HashY;
	FastHash2D(Pi,HashX,HashY);
	HashX = CellularWeightSamples(HashX)*Jitter+float4(0,1,0,1);
	HashY = CellularWeightSamples(HashY)*Jitter+float4(0,0,1,1);
	float4 dx = Pf.xxxx - HashX;
	float4 dy = Pf.yyyy - HashY;
	float4 d = dx*dx+dy*dy;
	d.xy = min(d.xy,d.zw);
	return min(d.x,d.y)*(1.0/1.125);
}
float NoiseD3D(float3 P,float Jitter)
{
	float3 Pi = floor(P);
	float3 Pf = P-Pi;
	
	float4 HashX0, HashY0, HashZ0, HashX1, HashY1, HashZ1;
	FastHash3D(Pi, HashX0, HashY0, HashZ0, HashX1, HashY1, HashZ1);
	
	HashX0 = CellularWeightSamples(HashX0)*Jitter+float4(0,1,0,1);
	HashY0 = CellularWeightSamples(HashY0)*Jitter+float4(0,0,1,1);
	HashZ0 = CellularWeightSamples(HashZ0)*Jitter+float4(0,0,0,0);
	HashX1 = CellularWeightSamples(HashX1)*Jitter+float4(0,1,0,1);
	HashY1 = CellularWeightSamples(HashY1)*Jitter+float4(0,0,1,1);
	HashZ1 = CellularWeightSamples(HashZ1)*Jitter+float4(1,1,1,1);
	
	float4 dx1 = Pf.xxxx - HashX0;
	float4 dy1 = Pf.yyyy - HashY0;
	float4 dz1 = Pf.zzzz - HashZ0;
	float4 dx2 = Pf.xxxx - HashX1;
	float4 dy2 = Pf.yyyy - HashY1;
	float4 dz2 = Pf.zzzz - HashZ1;
	float4 d1 = dx1 * dx1 + dy1 * dy1 + dz1 * dz1;
	float4 d2 = dx2 * dx2 + dy2 * dy2 + dz2 * dz2;
	d1 = min(d1, d2);
	d1.xy = min(d1.xy, d1.wz);
	return min(d1.x, d1.y) * ( 9.0 / 12.0 );
}

float DotFalloff( float xsq ) { xsq = 1.0 - xsq; return xsq*xsq*xsq; }
float4 FastHash2D(float2 Pos){
	float2 Offset = float2(26,161);
	float Domain = 71;
	float SomeLargeFloat = 951.135664;
	float4 P = float4(Pos.xy,Pos.xy+1);
	P = P-floor(P*(1.0/Domain))*Domain;
	P += Offset.xyxy;
	P *= P;
	return frac(P.xzxz*P.yyww*(1.0/SomeLargeFloat));
}
float NoiseE2D(float2 P,float3 Rad)
{
	float radius_low = Rad.x;
	float radius_high = Rad.y;
	float2 Pi = floor(P);
	float2 Pf = P-Pi;

	float3 Hash = FastHash2D(Pi);
	
	float Radius = max(0.0,radius_low+Hash.z*(radius_high-radius_low));
	float Value = Radius/max(radius_high,radius_low);
	
	Radius = 2.0/Radius;
	Pf *= Radius;
	Pf -= (Radius - 1.0);
	Pf += Hash.xy*(Radius - 2);
	Pf = pow(Pf,Rad.z);
	return DotFalloff(min(dot(Pf,Pf),1.0))*Value;
}
float4 FastHash3D(float3 Pos){
	float2 Offset = float2(26,161);
	float Domain = 69;
	float4 SomeLargeFloats = float4( 635.298681, 682.357502, 668.926525, 588.255119 );
	float4 Zinc = float4( 48.500388, 65.294118, 63.934599, 63.279683 );

	Pos = Pos - floor(Pos*(1/Domain))*Domain;
	Pos.xy += Offset;
	Pos.xy *= Pos.xy;
	return frac(Pos.x*Pos.y*(1/(SomeLargeFloats+Pos.zzzz*Zinc) ) );
}
float NoiseE3D(float3 P,float3 Rad)
{
	P.z+=0.5;
	float3 Pi = floor(P);
	float3 Pf = P-Pi;
	float radius_low = Rad.x;
	float radius_high = Rad.y;	
	float4 Hash = FastHash3D(Pi);

	float Radius = max(0.0,radius_low+Hash.w*(radius_high-radius_low));
	float Value = Radius/max(radius_high,radius_low);
	
	Radius = 2.0/Radius;
	Pf *= Radius;
	Pf -= (Radius - 1.0);
	Pf += Hash.xyz*(Radius - 2);
	Pf = pow(Pf,Rad.z);
	return DotFalloff(min(dot(Pf,Pf),1.0))*Value;	
}











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
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0;

	float4 Vertex = v.vertex;
	//Set default mask color
		float Mask0 = 1;
	//Generate layers for the Dust channel.
		//Generate Layer: Mask0
			//Sample parts of the layer:
				half4 Mask0Mask0_Sample1 = (float(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz*float3(_SSSMask0_aScale,_SSSMask0_aScale,_SSSMask0_aScale))+float3(0,0,_SSSMask0_aZ_Offset)))))*3))).rrrr);

			//Set the mask to the new color
				Mask0 = Mask0Mask0_Sample1.r;

	//Set default mask color
		float Mask1 = 1;
	//Generate layers for the Sparkles channel.
		//Generate Layer: Mask0 Copy 2
			//Sample parts of the layer:
				half4 Mask0_Copy_2Mask1_Sample1 = (float(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz*float3(_SSSMask0_Copy_aScale,_SSSMask0_Copy_aScale,_SSSMask0_Copy_aScale))+float3(0,0,_SSSMask0_aZ_Offset)))))*3))).rrrr);

			//Set the mask to the new color
				Mask1 = Mask0_Copy_2Mask1_Sample1.r;

		//Generate Layer: Mask0 Copy Copy
			//Sample parts of the layer:
				half4 Mask0_Copy_CopyMask1_Sample1 = (float(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz*float3(_SSSMask0_Copy_Copy_aScale,_SSSMask0_Copy_Copy_aScale,_SSSMask0_Copy_Copy_aScale))+float3(0,0,_SSSMask0_aZ_Offset)))))*3))).rrrr);

			//The layer has a Mix Amount of 0, which means forget about it :)


		//Generate Layer: Mask0 Copy
			//Sample parts of the layer:
				half4 Mask0_CopyMask1_Sample1 = (float(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz*float3(_SSSMask0_aScale,_SSSMask0_aScale,_SSSMask0_aScale))+float3(0,0,_SSSMask0_aZ_Offset)))))*3))).rrrr);

			//Blend the layer into the channel using the Multiply blend mode
				Mask1 *= Mask0_CopyMask1_Sample1.r;

		//Generate Layer: Mask1
			//Sample parts of the layer:
				half4 Mask1Mask1_Sample1 = float4(Mask1.rrr,1);

			//Apply Effects:
				Mask1Mask1_Sample1.rgb = (round(Mask1Mask1_Sample1.rgb*1)/1);

			//Set the mask to the new color
				Mask1 = Mask1Mask1_Sample1.r;

	//Set default mask color
		float Mask2 = 1;
	//Generate layers for the Squares channel.
		//Generate Layer: Mask2
			//Sample parts of the layer:
				half4 Mask2Mask2_Sample1 = (float(NoiseB3D((((((((round(mul(_Object2World, v.vertex).xyz/float3(_SSSMask2_aSize,_SSSMask2_aSize,_SSSMask2_aSize))*float3(_SSSMask2_aSize,_SSSMask2_aSize,_SSSMask2_aSize))+float3(0,0,_SSSMask2_aZ_Offset))*float3(_SSSMask2_aScale,_SSSMask2_aScale,_SSSMask2_aScale)))))*3))).rrrr);

			//Set the mask to the new color
				Mask2 = Mask2Mask2_Sample1.r;

		//Generate Layer: Mask2 Copy
			//Sample parts of the layer:
				half4 Mask2_CopyMask2_Sample1 = (float(NoiseB3D(((((((round(mul(_Object2World, v.vertex).xyz/float3(_SSSMask2_Copy_aSize,_SSSMask2_Copy_aSize,_SSSMask2_Copy_aSize))*float3(_SSSMask2_Copy_aSize,_SSSMask2_Copy_aSize,_SSSMask2_Copy_aSize))*float3(_SSSMask2_Copy_aScale,_SSSMask2_Copy_aScale,_SSSMask2_Copy_aScale)))))*3))).rrrr);

			//Blend the layer into the channel using the Multiply blend mode
				Mask2 *= Mask2_CopyMask2_Sample1.r;

	//Set default mask color
		float Mask3 = 1;
	//Generate layers for the Lines channel.
		//Generate Layer: Mask3
			//Sample parts of the layer:
				half4 Mask3Mask3_Sample1 = float4(((((float3(v.texcoord.xyz.xy,0)+float3(_SSSMask3_aX_Offset,0,0))))),1);

			//Apply Effects:
				Mask3Mask3_Sample1.rgb = (Mask3Mask3_Sample1.rgb*52.92);
				Mask3Mask3_Sample1.rgb = ((sin(Mask3Mask3_Sample1.rgb)+1)/2);

			//Set the mask to the new color
				Mask3 = Mask3Mask3_Sample1.r;

		//Generate Layer: Mask3 Copy
			//Sample parts of the layer:
				half4 Mask3_CopyMask3_Sample1 = float4(((((float3(v.texcoord.xyz.xy,0)+float3(_SSSMask3_aX_Offset,0,0))))),1);

			//Apply Effects:
				Mask3_CopyMask3_Sample1.rgb = (Mask3_CopyMask3_Sample1.rgb*22.1);
				Mask3_CopyMask3_Sample1.rgb = sin(Mask3_CopyMask3_Sample1.rgb);

			//Blend the layer into the channel using the Multiply blend mode
				Mask3 *= Mask3_CopyMask3_Sample1.r;

		//Generate Layer: Mask32
			//Sample parts of the layer:
				half4 Mask32Mask3_Sample1 = float4(Mask3.rrr,1);

			//Apply Effects:
				Mask32Mask3_Sample1.rgb = clamp(Mask32Mask3_Sample1.rgb,0,1);
				Mask32Mask3_Sample1.rgb = pow(Mask32Mask3_Sample1.rgb,_SSSMask32_aPower);

			//Set the mask to the new color
				Mask3 = Mask32Mask3_Sample1.r;

	//Set default mask color
		float Mask4 = 1;
	//Generate layers for the Flicker channel.
		//Generate Layer: Flicker
			//Sample parts of the layer:
				half4 FlickerMask4_Sample1 = float4(((((float3((v.texcoord.xyz.xy*float2(0.1,0.1)),0)+float3(_SSSFlicker_aX_Offset,0,0))))),1);

			//Apply Effects:
				FlickerMask4_Sample1.rgb = sin(FlickerMask4_Sample1.rgb);
				FlickerMask4_Sample1.rgb = (FlickerMask4_Sample1.rgb*56.1);
				FlickerMask4_Sample1.rgb = (FlickerMask4_Sample1.rgb-(53.54));
				FlickerMask4_Sample1.rgb = (round(FlickerMask4_Sample1.rgb*1)/1);
				FlickerMask4_Sample1.rgb = clamp(FlickerMask4_Sample1.rgb,0,1);

			//Set the mask to the new color
				Mask4 = FlickerMask4_Sample1.r;

		//Generate Layer: Flicker2
			//Sample parts of the layer:
				half4 Flicker2Mask4_Sample1 = float4(1, 1, 1, 1);

			//Blend the layer into the channel using the Mix blend mode
				Mask4 = lerp(Mask4,Flicker2Mask4_Sample1.r,_SSSForce_Flicker);

	//Set default mask color
		float Mask5 = 1;
	//Generate layers for the FlickerNoise channel.
		//Generate Layer: FlickerNoise
			//Sample parts of the layer:
				half4 FlickerNoiseMask5_Sample1 = (float((Noise3D(((((((mul(_Object2World, v.vertex).xyz+float3(0,_SSSFlickerNoise_aY_Offset,0))*float3(_SSSFlickerNoise_aScale,_SSSFlickerNoise_aScale,_SSSFlickerNoise_aScale)))))*3))+1)/2).rrrr);

			//Blend the layer into the channel using the Mix blend mode
				Mask5 = lerp(Mask5,FlickerNoiseMask5_Sample1.r,Mask4);

	//Set default mask color
		float Mask6 = 1;
	//Generate layers for the Pull channel.
		//Generate Layer: Mask6
			//Sample parts of the layer:
				half4 Mask6Mask6_Sample1 = float4((((mul(_Object2World, v.vertex).xyz))),1);

			//Apply Effects:
				Mask6Mask6_Sample1.rgb = (Mask6Mask6_Sample1.rgb+_SSSPull_Height);
				Mask6Mask6_Sample1.rgb = (float3(1,1,1)-Mask6Mask6_Sample1.rgb);
				Mask6Mask6_Sample1.rgb = pow(Mask6Mask6_Sample1.rgb,5.95);
				Mask6Mask6_Sample1.rgb = clamp(Mask6Mask6_Sample1.rgb,0,1);

			//Set the mask to the new color
				Mask6 = Mask6Mask6_Sample1.g;

	//Generate layers for the Vertex channel.
		//Generate Layer: Vertex3
			//Sample parts of the layer:
				half4 Vertex3Vertex_Sample1 = _SSSFlicker_Axis;

			//Blend the layer into the channel using the Subtract blend mode
				Vertex -= ((Vertex3Vertex_Sample1)*float4(v.normal.rgb,1)).rgba*Mask5;

		//Generate Layer: Vertex
			//Sample parts of the layer:
				half4 VertexVertex_Sample1 = _SSSPull_Axis;

			//Blend the layer into the channel using the Mix blend mode
				Vertex = lerp(Vertex,((VertexVertex_Sample1)*v.vertex).rgba,Mask6);


	v.vertex.rgb = Vertex;
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
}
	ENDCG
}

Fallback "VertexLit"
}
