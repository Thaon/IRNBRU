Shader "Shader Sandwich/Enhanced Graphics/Hair" {//The Shaders Name
//The inputs shown in the material panel
Properties {
	[HideInInspector]Texcoord ("Generic UV Coords (You shouldn't be seeing this aaaaah!)", 2D) = "white" {}
	_MainTex ("Texture", 2D) = "white" {}
	_Cutoff ("Transparency", Range(0.000000000,1.000000000)) = 0.500000000
	_Shininess ("Specular Hardness", Range(0.000100000,1.000000000)) = 0.643715600
	_SpecColor ("Specular Color", Color) = (0.3,0.3,0.3,1)
	_SSSWind ("Wind", Range(0.000000000,0.100000000)) = 0.006562499
	_SSSWind_Scale ("Wind Scale", Float) = 1.000000000
}

SubShader {
	Tags { "RenderType"="Opaque""Queue"="Transparent" }//A bunch of settings telling Unity a bit about the shader.
	LOD 200
	cull Off
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _Cutoff;
	float _Shininess;
	float _SSSWind;
	float _SSSWind_Scale;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
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
	Spec = abs(dot(s.Normal,reflect(-lightDir, -viewDir)));
	Spec = (half3(1.0f,1.0f,1.0f)-(pow(sqrt(Spec),2 - s.Smoothness)));
	Spec = saturate(Spec)*s.Specular;	Spec = Spec * atten * 2 * lightColor.rgb;
	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/(28.26))/9.0f);


c.rgb = c.rgb*s.Albedo+Spec;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.
	float3 Spec;
	Spec = abs(dot(s.Normal,reflect(-lightDir, -viewDir)));
	Spec = (half3(1.0f,1.0f,1.0f)-(pow(sqrt(Spec),2 - s.Smoothness)));
	Spec = saturate(Spec)*s.Specular;	Spec = Spec  * 2 * lightColor.rgb;
	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/(28.26))/9.0f);


c.rgb = c.rgb*s.Albedo+Spec;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
half4 LightingCLStandard (CSurfaceOutput s, half3 viewDir, UnityGI gi) {
	half4 c;
	c = LightingCLStandardLight(s,viewDir,gi.light);
	#if defined(DIRLIGHTMAP_SEPARATE)
		#ifdef LIGHTMAP_ON
			c += LightingCLStandardLight(s,viewDir,gi.light2);
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			c += LightingCLStandardLight(s,viewDir,gi.light3);
		#endif
	#endif
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, s.Smoothness, s.Normal,false);
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
	//Generate layers for the Mask0 channel.
		//Generate Layer: Mask0
			//Sample parts of the layer:
				half4 Mask0Mask0_Sample1 = float4(((float3((v.texcoord.xyz.xy),0))),1);

			//Apply Effects:
				Mask0Mask0_Sample1.rgb = (float3(1,1,1)-Mask0Mask0_Sample1.rgb);

			//Set the mask to the new color
				Mask0 = Mask0Mask0_Sample1.g;

	//Generate layers for the Vertex channel.
		//Generate Layer: Vertex
			//Sample parts of the layer:
				half4 VertexVertex_Sample1 = (float((Noise2D(((((((v.texcoord.xyz.xy+float2(_Time.y,0))*float2(_SSSWind_Scale,_SSSWind_Scale)))))*3))+1)/2).rrrr);

			//Blend the layer into the channel using the Add blend mode
				Vertex += ((VertexVertex_Sample1)*float4(v.normal.rgb,1)).rgba*_SSSWind*Mask0;


	v.vertex.rgb = Vertex;
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = _Shininess*2;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Set default mask color
		float Mask0 = 1;
	//Generate layers for the Mask0 channel.
		//Generate Layer: Mask0
			//Sample parts of the layer:
				half4 Mask0Mask0_Sample1 = float4(((float3((IN.uvTexcoord.xy),0))),1);

			//Apply Effects:
				Mask0Mask0_Sample1.rgb = (float3(1,1,1)-Mask0Mask0_Sample1.rgb);

			//Set the mask to the new color
				Mask0 = Mask0Mask0_Sample1.g;

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyAlpha_Sample1.a;

	clip(o.Alpha-_Cutoff);
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = TextureDiffuse_Sample1.rgb;

	//Generate layers for the Gloss channel.
		//Generate Layer: Specular
			//Sample parts of the layer:
				half4 SpecularSpecular_Sample1 = _SpecColor;

			//Set the channel to the new color
				o.Specular = SpecularSpecular_Sample1.rgb;

}
	ENDCG
	ZWrite On
	cull Off//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _Cutoff;
	float _Shininess;
	float _SSSWind;
	float _SSSWind_Scale;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
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
	Spec = abs(dot(s.Normal,reflect(-lightDir, -viewDir)));
	Spec = (half3(1.0f,1.0f,1.0f)-(pow(sqrt(Spec),2 - s.Smoothness)));
	Spec = saturate(Spec)*s.Specular;	Spec = Spec * atten * 2 * lightColor.rgb;
	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/(28.26))/9.0f);


c.rgb = c.rgb*s.Albedo+Spec;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.
	float3 Spec;
	Spec = abs(dot(s.Normal,reflect(-lightDir, -viewDir)));
	Spec = (half3(1.0f,1.0f,1.0f)-(pow(sqrt(Spec),2 - s.Smoothness)));
	Spec = saturate(Spec)*s.Specular;	Spec = Spec  * 2 * lightColor.rgb;
	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/(28.26))/9.0f);


c.rgb = c.rgb*s.Albedo+Spec;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
half4 LightingCLStandard (CSurfaceOutput s, half3 viewDir, UnityGI gi) {
	half4 c;
	c = LightingCLStandardLight(s,viewDir,gi.light);
	#if defined(DIRLIGHTMAP_SEPARATE)
		#ifdef LIGHTMAP_ON
			c += LightingCLStandardLight(s,viewDir,gi.light2);
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			c += LightingCLStandardLight(s,viewDir,gi.light3);
		#endif
	#endif
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, s.Smoothness, s.Normal,false);
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
	//Generate layers for the Mask0 channel.
		//Generate Layer: Mask0
			//Sample parts of the layer:
				half4 Mask0Mask0_Sample1 = float4(((float3((v.texcoord.xyz.xy),0))),1);

			//Apply Effects:
				Mask0Mask0_Sample1.rgb = (float3(1,1,1)-Mask0Mask0_Sample1.rgb);

			//Set the mask to the new color
				Mask0 = Mask0Mask0_Sample1.g;

	//Generate layers for the Vertex channel.
		//Generate Layer: Vertex
			//Sample parts of the layer:
				half4 VertexVertex_Sample1 = (float((Noise2D(((((((v.texcoord.xyz.xy+float2(_Time.y,0))*float2(_SSSWind_Scale,_SSSWind_Scale)))))*3))+1)/2).rrrr);

			//Blend the layer into the channel using the Add blend mode
				Vertex += ((VertexVertex_Sample1)*float4(v.normal.rgb,1)).rgba*_SSSWind*Mask0;


	v.vertex.rgb = Vertex;
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = _Shininess*2;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Set default mask color
		float Mask0 = 1;
	//Generate layers for the Mask0 channel.
		//Generate Layer: Mask0
			//Sample parts of the layer:
				half4 Mask0Mask0_Sample1 = float4(((float3((IN.uvTexcoord.xy),0))),1);

			//Apply Effects:
				Mask0Mask0_Sample1.rgb = (float3(1,1,1)-Mask0Mask0_Sample1.rgb);

			//Set the mask to the new color
				Mask0 = Mask0Mask0_Sample1.g;

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = TextureDiffuse_Sample1.rgb;

	//Generate layers for the Gloss channel.
		//Generate Layer: Specular
			//Sample parts of the layer:
				half4 SpecularSpecular_Sample1 = _SpecColor;

			//Set the channel to the new color
				o.Specular = SpecularSpecular_Sample1.rgb;

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
Image #!  #^ CC0 #?Image
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
VisName #! Transparency #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 0.5 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 10 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Specular Hardness #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 0.6437156 #^ CC0 #?Number
Range0 #! 0.0001 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 6 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 1 #^ CC0 #?Type
VisName #! Specular Color #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.3,0.3,0.3,1 #^ CC0 #?Color
Number #! 0 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 5 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 3 #^ CC0 #?Type
VisName #! Vertex - X Offset #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 272.6179 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 1 #^ CC0 #?SpecialType
InEditor #! 0 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Wind #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 0.006562499 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 0.1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 3 #^ CC0 #?Type
VisName #! Wind Scale #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 1 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
ShaderName #! Shader Sandwich/Enhanced Graphics/Hair #^ CC0 #?ShaderName
Hard Mode #! True #^ CC0 #?Hard Mode
Tech Lod #! 200 #^ CC0 #?Tech Lod
Cull #! 0 #^ CC0 #?Cull
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
Lighting Type #! 0 #^ CC0 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Setting1 #! 0 #^ CC0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #^ CC0 #?Wrap Color
Use Normals #! 0 #^ CC0 #?Use Normals
Specular On #! True #^ CC0 #?Specular On
Specular Type #! 2 #^ CC0 #?Specular Type
Spec Hardness #! 0.6437156 #^ CC0 #^ 2 #?Spec Hardness
Spec Color #! 0.8,0.8,0.8,1 #^ CC0 #?Spec Color
Spec Energy Conserve #! True #^ CC0 #?Spec Energy Conserve
Spec Offset #! 0 #^ CC0 #?Spec Offset
Emission On #! False #^ CC0 #?Emission On
Emission Color #! 0,0,0,0 #^ CC0 #?Emission Color
Emission Type #! 0 #^ CC0 #?Emission Type
Transparency On #! True #^ CC0 #?Transparency On
Transparency Type #! 1 #^ CC0 #?Transparency Type
ZWrite #! True #^ CC0 #?ZWrite
Use PBR #! True #^ CC0 #?Use PBR
Transparency #! 0.5 #^ CC0 #^ 1 #?Transparency
Receive Shadows #! False #^ CC0 #?Receive Shadows
ZWrite Type #! 1 #^ CC0 #?ZWrite Type
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
EndTag #! g #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Mask0 #^ CC0 #?Layer Name
Layer Type #! 7 #^ CC0 #?Layer Type
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
BeginShaderEffect
TypeS #! SSEInvert #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
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
Main Color #! 0.627451,0.8,0.8823529,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #!  #^ CC0 #^ 0 #?Main Texture
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
EndTag #! a #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Texture Copy #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 0.627451,0.8,0.8823529,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 13746f3718cc32a4596c2538433a9952         #^ CC0 #^ 0 #?Main Texture
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
BeginShaderLayer
Layer Name #! Specular #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 0.3,0.3,0.3,1 #^ CC0 #^ 3 #?Main Color
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
BeginShaderLayer
Layer Name #! Vertex #^ CC0 #?Layer Name
Layer Type #! 5 #^ CC0 #?Layer Type
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
Mix Amount #! 0.006562499 #^ CC0 #^ 5 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 1 #^ CC0 #?Mix Type
Stencil #! 0 #^ CC0 #?Stencil
Vertex Mask #! 1 #^ CC0 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEUVOffset #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
X Offset #! 272.6179 #^ CC0 #^ 4 #?X Offset
Y Offset #! 0 #^ CC0 #?Y Offset
Z Offset #! 0 #^ CC0 #?Z Offset
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVScale #^ CC0 #?TypeS
IsVisible #! True #^ CC0 #?IsVisible
UseAlpha #! 0 #^ CC0 #?UseAlpha
Seperate #! False #^ CC0 #?Seperate
Scale #! 1 #^ CC0 #^ 6 #?Scale
Y Scale #! 1 #^ CC0 #^ 6 #?Y Scale
Z Scale #! 1 #^ CC0 #^ 6 #?Z Scale
EndShaderEffect
EndShaderLayer
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
