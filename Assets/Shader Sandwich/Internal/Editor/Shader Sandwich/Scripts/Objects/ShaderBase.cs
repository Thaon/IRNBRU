#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
#define PRE_UNITY_5
#else
#define UNITY_5
#endif
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using UEObject = UnityEngine.Object;
//This code is such a mess :(
[System.Serializable]
public enum ShaderPass{Base,Light,Parallax,ShellBase,ShellLight,Mask,MaskLight,MaskBase}

[System.Serializable]
public class ShaderBase : ScriptableObject{// : UnityEngine.Object{

public static ShaderBase Current{
		get{
			if (ShaderSandwich.Instance!=null&&ShaderSandwich.Instance.OpenShader!=null)
			return ShaderSandwich.Instance.OpenShader;
			else
			return null;
		}
		set{
		}
	}
public bool IsPBR{
		get{
			if (DiffuseLightingType.Type==4)
			return true;
			
			return false;
		}
		set{
		}
	}

	public ShaderVar DiffMode = new ShaderVar("Hard Mode",false);

	public ShaderVar ShaderName=new ShaderVar("ShaderName","Untitled Shader");
	public List<ShaderInput> ShaderInputs = new List<ShaderInput>();
	public List<int> ShaderInputsOrder;// = new List<int>();
	public int ShaderInputCount;
	
	public ShaderGenerate SG;

	public ShaderLayerList ShaderLayersDiffuse;
	public ShaderLayerList ShaderLayersShellDiffuse;
	public ShaderLayerList ShaderLayersAlpha;
	public ShaderLayerList ShaderLayersShellAlpha;
	public ShaderLayerList ShaderLayersSpecular;
	public ShaderLayerList ShaderLayersShellSpecular;
	public ShaderLayerList ShaderLayersNormal;
	public ShaderLayerList ShaderLayersShellNormal;
	public ShaderLayerList ShaderLayersEmission;
	public ShaderLayerList ShaderLayersShellEmission;
	public ShaderLayerList ShaderLayersHeight;
	public ShaderLayerList ShaderLayersVertex;
	public ShaderLayerList ShaderLayersLightingDiffuse;
	public ShaderLayerList ShaderLayersLightingSpecular;
	public ShaderLayerList ShaderLayersLightingAmbient;
	public ShaderLayerList ShaderLayersLightingAll;
	public ShaderLayerList ShaderLayersMaskTemp;
	public ShaderLayerList ShaderLayersShellVertex;
	public List<ShaderLayerList> ShaderLayersMasks = new List<ShaderLayerList>();
	public int ShaderLayerMaskCount = 0;
	public void AddMask(){
		ShaderLayerList SLL = new ShaderLayerList("Mask"+ShaderLayersMasks.Count.ToString(),"Mask"+ShaderLayersMasks.Count.ToString(),"Mask"+ShaderLayersMasks.Count.ToString(),"Mask"+ShaderLayersMasks.Count.ToString(),"r","",new Color(1f,1f,1f,1f));
		SLL.IsMask.On=true;
		ShaderLayersMasks.Add(SLL);
		
		ShaderLayersMaskTemp = new ShaderLayerList("Mask"+ShaderLayersMasks.Count.ToString(),"Mask"+ShaderLayersMasks.Count.ToString(),"Mask"+ShaderLayersMasks.Count.ToString(),"Mask"+ShaderLayersMasks.Count.ToString(),"r","",new Color(1f,1f,1f,1f));
		SLL.IsMask.On=true;
	}
	public ShaderBase(){
		AddMask();
		ShaderLayersDiffuse = new ShaderLayerList("Diffuse","Diffuse","The color of the surface.","Texture","o.Albedo","rgb","",new Color(0.8f,0.8f,0.8f,1f));
		ShaderLayersAlpha = new ShaderLayerList("Alpha","Alpha","Which parts are see through.","Transparency","o.Alpha","a","",new Color(1f,1f,1f,1f));
		ShaderLayersSpecular = new ShaderLayerList("Specular","Specular","Where the shine appears.","Gloss","o.Specular","rgb","",new Color(0.3f,0.3f,0.3f,1f));
		ShaderLayersNormal = new ShaderLayerList("Normals","Normals","Used to add fake bumps.","NormalMap","o.Normal","rgb","",new Color(0f,0f,1f,1f));
		//agrr new Color(0.5f,0.5f,1f,1f) SSEUnpackNormal
		ShaderLayersEmission = new ShaderLayerList("Emission","Emission","Where and what color the glow is.","Emission","Emission","rgba","",new Color(0f,0f,0f,1f));
		ShaderLayersHeight = new ShaderLayerList("Height","Height","Which parts of the shader is higher than the other.","Height","Height","a","",new Color(1f,1f,1f,1));
		ShaderLayersVertex = new ShaderLayerList("Vertex","Vertex","Used to move the models vertices.","Vertex","rgba","",new Color(1f,1f,1f,1));
		ShaderLayersLightingDiffuse = new ShaderLayerList("LightingDiffuse","Diffuse","Customize the diffuse lighting.","Lighting","c","rgba","",new Color(0.8f,0.8f,0.8f,1f));
		ShaderLayersLightingDiffuse.IsLighting.On = true;
		ShaderLayersLightingSpecular = new ShaderLayerList("LightingSpecular","Specular","Custom speculars highlights and reflections.","Lighting","c","rgba","",new Color(0.8f,0.8f,0.8f,1f));
		ShaderLayersLightingSpecular.IsLighting.On = true;
		ShaderLayersLightingAmbient = new ShaderLayerList("LightingAmbient","Ambient","Customize the ambient lighting.","Lighting","c","rgba","",new Color(0.8f,0.8f,0.8f,1f));
		ShaderLayersLightingAmbient.IsLighting.On = true;
		ShaderLayersLightingAll = new ShaderLayerList("LightingDirect","Direct","Custom both direct diffuse and specular lighting.","Lighting","c","rgba","",new Color(0.8f,0.8f,0.8f,1f));
		ShaderLayersLightingAll.IsLighting.On = true;

		ShaderLayersShellDiffuse = new ShaderLayerList("ShellDiffuse","Diffuse","The color of the surface.","Texture","o.Albedo","rgb","",new Color(0.8f,0.8f,0.8f,1));
		ShaderLayersShellAlpha = new ShaderLayerList("ShellAlpha","Alpha","Which parts are see through.","Transparency","o.Alpha","a","",new Color(1f,1f,1f,1f));
		ShaderLayersShellSpecular = new ShaderLayerList("ShellSpecular","Specular","Where the shine appears.","Gloss","o.Specular","rgb","",new Color(0.3f,0.3f,0.3f,0.3f));
		ShaderLayersShellNormal = new ShaderLayerList("ShellNormals","Normals","Used to add fake bumps.","NormalMap","o.Normal","rgb","",new Color(0f,0f,1f,1f));
		ShaderLayersShellEmission = new ShaderLayerList("ShellEmission","Emission","Where and what color the glow is.","Emission","Emission","rgba","",new Color(0f,0f,0f,1f));
		ShaderLayersShellVertex = new ShaderLayerList("ShellVertex","Vertex","Used to move the models vertices.","Vertex","Vertex","rgba","",new Color(1f,1f,1f,1f));
		TechCull.Type = 1;
		TessellationType.Type = 2;
	}

	public List<List<ShaderLayer>> ShaderLayers(){
		List<List<ShaderLayer>> tempList = new List<List<ShaderLayer>>();
		tempList.Add(ShaderLayersDiffuse.SLs);
		tempList.Add(ShaderLayersShellDiffuse.SLs);
		tempList.Add(ShaderLayersAlpha.SLs);
		tempList.Add(ShaderLayersShellAlpha.SLs);
		tempList.Add(ShaderLayersSpecular.SLs);
		tempList.Add(ShaderLayersShellSpecular.SLs);
		tempList.Add(ShaderLayersNormal.SLs);
		tempList.Add(ShaderLayersShellNormal.SLs);
		tempList.Add(ShaderLayersEmission.SLs);
		ShaderLayersEmission.EndTag.Text="rgba";
		tempList.Add(ShaderLayersShellEmission.SLs);
		ShaderLayersShellEmission.EndTag.Text="rgba";
		tempList.Add(ShaderLayersHeight.SLs);
		tempList.Add(ShaderLayersLightingDiffuse.SLs);
		tempList.Add(ShaderLayersLightingSpecular.SLs);
		tempList.Add(ShaderLayersLightingAmbient.SLs);
		tempList.Add(ShaderLayersLightingAll.SLs);
		tempList.Add(ShaderLayersVertex.SLs);
		tempList.Add(ShaderLayersShellVertex.SLs);
		foreach(ShaderLayerList SLL in ShaderLayersMasks)
		tempList.Add(SLL.SLs);
		return tempList;
	}	
	public List<ShaderLayerList> GetShaderLayerLists(){
		List<ShaderLayerList> tempList = new List<ShaderLayerList>();
		foreach(ShaderLayerList SLL in ShaderLayersMasks)
		tempList.Add(SLL);		
		tempList.Add(ShaderLayersDiffuse);
		tempList.Add(ShaderLayersShellDiffuse);
		tempList.Add(ShaderLayersAlpha);
		tempList.Add(ShaderLayersShellAlpha);
		tempList.Add(ShaderLayersSpecular);
		tempList.Add(ShaderLayersShellSpecular);
		tempList.Add(ShaderLayersNormal);
		tempList.Add(ShaderLayersShellNormal);
		tempList.Add(ShaderLayersEmission);
		tempList.Add(ShaderLayersShellEmission);
		tempList.Add(ShaderLayersHeight);
		tempList.Add(ShaderLayersLightingDiffuse);
		tempList.Add(ShaderLayersLightingSpecular);
		tempList.Add(ShaderLayersLightingAmbient);
		tempList.Add(ShaderLayersLightingAll);
		tempList.Add(ShaderLayersVertex);
		tempList.Add(ShaderLayersShellVertex);
		return tempList;
	}

	//SHADER BASE

	//TECHNICAL
	public bool TechDropDown;
	public bool TechHelp;
	public ShaderVar TechLOD = new ShaderVar("Tech Lod",200);
	public ShaderVar TechCull = new ShaderVar("Cull",new string[]{"All","Front","Back"},new string[]{"","",""},new string[]{"Off","Back","Front"}); 

	public ShaderVar TechShaderTarget = new ShaderVar("Tech Shader Target",3f);

	//Misc
	public bool MiscDropDown;
	public bool MiscHelp;
	public ShaderVar MiscVertexRecalculation = new ShaderVar("Vertex Recalculation",false);
	public ShaderVar MiscFog = new ShaderVar("Use Fog",true);
	public ShaderVar MiscAmbient = new ShaderVar("Use Ambient",true);
	public ShaderVar MiscVertexLights = new ShaderVar("Use Vertex Lights",true);
	public ShaderVar MiscLightmap = new ShaderVar("Use Lightmaps",true);
	public ShaderVar MiscForwardAdd = new ShaderVar("Forward Add",true);
	public ShaderVar MiscShadows = new ShaderVar("Shadows",true);
	public ShaderVar MiscFullShadows = new ShaderVar("Use All Shadows",true);
	public ShaderVar MiscInterpolateView = new ShaderVar("Interpolate View",false);
	public ShaderVar MiscHalfView = new ShaderVar("Half as View",false);

	//DIFFUSE
	public bool DiffuseDropDown;
	public bool DiffuseHelp;
	public ShaderVar DiffuseOn = new ShaderVar("Diffuse On",true);

	public ShaderVar DiffuseLightingType = new ShaderVar("Lighting Type",new string[] {"Standard", "Microfaceted", "Translucent","Unlit","PBR Standard","Custom"},new string[]{"ImagePreviews/DiffuseStandard.png","ImagePreviews/DiffuseRough.png","ImagePreviews/DiffuseTranslucent.png","ImagePreviews/DiffuseUnlit.png","ImagePreviews/DiffusePBRStandard.png",""},"",new string[] {"Smooth/Lambert - A good approximation of hard, but smooth surfaced objects.\n(Wood,Plastic)", "Rough/Oren-Nayar - Useful for rough surfaces, or surfaces with billions of tiny indents.\n(Carpet,Skin)", "Translucent/Wrap - Good for simulating sub-surface scattering, or translucent objects.\n(Skin,Plants)","Unlit/Shadeless - No lighting, full brightness.\n(Sky,Globe)","PBR Standard - A physically based version of the Standard option (Unity 5+)","Custom - Create your own lighting calculations in the lighting tab."});//);

	public ShaderVar DiffuseColor = new ShaderVar("Color", new Vector4(0.8f,0.8f,0.8f,1f));

	public ShaderVar DiffuseSetting1 = new ShaderVar("Setting1",0f);
	public ShaderVar DiffuseSetting2 = new ShaderVar("Wrap Color",new Vector4(0.4f,0.2f,0.2f,1f));
	public ShaderVar DiffuseNormals = new ShaderVar("Use Normals",0f);

	//SPECULAR
	public bool SpecularDropDown;
	public bool SpecularHelp;
	public ShaderVar SpecularOn = new ShaderVar("Specular On",false);
	public ShaderVar SpecularLightingType = new ShaderVar("Specular Type",new string[] {"Standard", "Circular","Wave"},new string[]{"ImagePreviews/SpecularNormal.png","ImagePreviews/SpecularCircle.png","ImagePreviews/SpecularWave.png"},"",new string[] {"BlinnPhong - Standard specular highlights.", "Circular - Circular specular highlights(Unrealistic)","Wave - A strange wave like highlight."});	

	public ShaderVar SpecularHardness = new ShaderVar("Spec Hardness",0.3f);
	public ShaderVar SpecularColor = new ShaderVar("Spec Color",new Vector4(0.8f,0.8f,0.8f,1f));
	public ShaderVar SpecularEnergy = new ShaderVar("Spec Energy Conserve",true);
	public ShaderVar SpecularOffset = new ShaderVar("Spec Offset",0f);

	//EMISSION
	public bool EmissionDropDown;
	public bool EmissionHelp;
	public ShaderVar EmissionOn = new ShaderVar("Emission On",false);
	public ShaderVar EmissionColor = new ShaderVar("Emission Color",new Vector4(0f,0f,0f,0f));
	public ShaderVar EmissionType = new ShaderVar(
	"Emission Type",new string[]{"Standard","Multiply","Set"},new string[]{"ImagePreviews/EmissionOn.png","ImagePreviews/EmissionMul.png","ImagePreviews/EmissionSet.png"},"",new string[]{"Standard - Simply add the emission on top of the base color.","Multiply - Add the emission multiplied by the base color. An emission color of white adds the base color to itself.","Set - Mixes the shadeless color on top based on the alpha."}
	); 	

	//Transparency
	public bool TransparencyDropDown;
	public bool TransparencyHelp;
	public ShaderVar TransparencyOn = new ShaderVar("Transparency On",false);
	public ShaderVar TransparencyType = new ShaderVar(
	"Transparency Type",new string[]{"Cutout","Fade"},new string[]{"ImagePreviews/TransparentCutoff.png","ImagePreviews/TransparentFade.png"},"",new string[]{"Cutout - Only allows alpha to be on or off, fast, but creates sharp edges. This can have aliasing issues and is slower on mobile.","Fade - Can have many levels of transparency, but can have depth sorting issues (Objects can appear in front of each other incorrectly)."}
	); 
	public ShaderVar TransparencyZWrite = new ShaderVar("ZWrite",false); 
	public ShaderVar TransparencyPBR = new ShaderVar("Use PBR",true); 
	public ShaderVar TransparencyReceive = new ShaderVar("Receive Shadows",false);
	public ShaderVar TransparencyZWriteType = new ShaderVar("ZWrite Type",new string[]{"Full","Cutoff"},new string[]{"","",""},new string[]{"Full","Cutoff"});
	public bool TransparencyOnOff;
	public ShaderVar BlendMode = new ShaderVar("Blend Mode",new string[]{"Mix","Add","Mul"},new string[]{"","",""},new string[]{"Mix","Add","Mul"});

	public ShaderVar TransparencyAmount = new ShaderVar("Transparency",1f);

	//Blend Mode
	public bool BlendModeDropDown;
	public bool BlendModeHelp;
	public int BlendModeType = 0;


	//Shells
	public bool ShellsDropDown;
	public bool ShellsHelp;
	public ShaderVar ShellsOn = new ShaderVar("Shells On",false);
	//public int ShellsCount = 0;
	public ShaderVar ShellsCount = new ShaderVar("Shell Count",1,0,50);	
	public ShaderVar ShellsDistance = new ShaderVar("Shells Distance",0.1f);
	public ShaderVar ShellsEase = new ShaderVar("Shell Ease",1,0f,3f);	
	//public float ShellsEase = 1f;

	//Shells Transparency
	public bool ShellsTransparencyDropDown;
	public bool ShellsTransparencyHelp;
	public ShaderVar ShellsTransparencyType = new ShaderVar(
	"Shell Transparency Type",new string[]{"Cutout","Fade"},new string[]{"ImagePreviews/TransparentCutoff.png","ImagePreviews/TransparentFade.png"},"",new string[]{"Cutout - Only allows alpha to be on or off, fast, but creates sharp edges. This can have aliasing issues and is slower on mobile.","Fade - Can have many levels of transparency, but can have depth sorting issues (Objects can appear in front of each other incorrectly)."}
	); 	
	public bool ShellsTransparencyOnOff; 

	public ShaderVar ShellsTransparencyZWrite = new ShaderVar("Shell Transparency ZWrite",false); 
	//public int ShellsCull = 1; 
	public ShaderVar ShellsCull = new ShaderVar("Shell Cull",new string[]{"All","Front","Back"},new string[]{"","",""},new string[]{"Off","Back","Front"});
	public ShaderVar ShellsBlendMode = new ShaderVar("Shell Blend Mode",new string[]{"Mix","Add","Multiply"},new string[]{"","",""},new string[]{"Mix","Add","Mul"});
	public ShaderVar ShellsTransparencyAmount = new ShaderVar("Shells Transparency",1f);
	public ShaderVar ShellsZWrite = new ShaderVar("Shells ZWrite",true);
	public ShaderVar ShellsUseTransparency = new ShaderVar("Shells Use Transparency",true);

	public ShaderVar ShellsLighting = new ShaderVar("Shell Lighting",true); 
	public ShaderVar ShellsFront = new ShaderVar("Shell Front",true); 

	//Parallax Occlusion
	public bool ParallaxDropDown;
	public bool ParallaxHelp;
	public ShaderVar ParallaxOn = new ShaderVar("Parallax On",false);

	public ShaderVar ParallaxHeight = new ShaderVar("Parallax Height",0.1f); 
	public int ParallaxLinearQuality = 5;
	public ShaderVar ParallaxBinaryQuality = new ShaderVar("Parallax Quality",10,0,50);
	public ShaderVar ParallaxSilhouetteClipping = new ShaderVar("Silhouette Clipping",false);
	
	
	//Tessellation
	public ShaderVar TessellationOn = new ShaderVar("Tessellation On",false);
	
	public ShaderVar TessellationType = new ShaderVar("Tessellation Type",new string[]{"Equal","Size","Distance"},new string[]{"Tessellate all faces the same amount (Not Recommended!).","Tessellate faces based on their size (larger = more tessellation).","Tessellate faces based on distance and screen area."},new string[]{"Equal","Size","Distance"});
	public ShaderVar TessellationQuality = new ShaderVar("Tessellation Quality",10,1,50);
	public ShaderVar TessellationFalloff = new ShaderVar("Tessellation Falloff",1,1,3);
	//public ShaderVar TessellationSmoothing = new ShaderVar("Tessellation Smoothing",false);
	public ShaderVar TessellationSmoothingAmount = new ShaderVar("Tessellation Smoothing Amount",0f,-3,3);

	public bool Initiated = false;

	public string[] TechShaderTargetNames = {"2.0", "3.0", "4.0", "5.0"};
	public string[] DiffuseLightingTypeNames = {"Smooth", "Rough", "Translucent","Unlit"};
	public string[] DiffuseLightingTypeDescriptions = {"Smooth/Lambert - A good approximation of hard, but smooth surfaced objects.(Wood,Plastic)", "Rough/Oren-Nayar - Useful for showing rough surfaces.(Carpet,Skin)", "Translucent/Wrap - Good for simulating sub-surface scattering, or translucent objects.(Skin,Plants)","Unlit/Shadeless - No lighting, full brightness.(Sky,Globe)"};
	public string[] SpecularLightingTypeNames = {"None","Standard", "Circular","Wave"};
	public string[] TransparencyTypeNames = {"None","Cutoff", "Fade"};
	public string[] BlendModeTypeNames = {"Normal","Add"};
	//public string[] CullNames = {"All","Front","Back"};

	[XmlIgnore,NonSerialized]public List<int> NameInputs;
	[XmlIgnore,NonSerialized]public Dictionary<string, int> NameToInt;

	public ShaderVar OtherTypes = new ShaderVar("Parallax Type",new string[] {"Off", "On","Off","Off","Off","On"},new string[]{"ImagePreviews/ParallaxOff.png","ImagePreviews/ParallaxOn.png","ImagePreviews/TransparentOff.png","ImagePreviews/EmissionOff.png","ImagePreviews/ShellsOff.png","ImagePreviews/ShellsOn.png","ImagePreviews/TessellationOff.png","ImagePreviews/TessellationOn.png"},"",new string[] {"", "","","","",""});	
	public string GCTop(ShaderGenerate SG){
		if (SG.Wireframe)
		return "Shader \"Hidden/SSTempWireframe\" {//The Shaders Name\n";
		else
		if (SG.Temp){
			if (ShaderPreview.Instance!=null&&ShaderPreview.Instance.Expose)
			return "Shader \"Shader Sandwich/SSTemp\" {//The Shaders Name\n";
			else
			return "Shader \"Hidden/SSTemp\" {//The Shaders Name\n";
		}
		else
		return "Shader \""+ShaderName.Text+"\" {//The Shaders Name\n";
	}
	public string GCProperties(){
		return "Properties {\n";
	}
	public string GCShells(ShaderGenerate SG){
		string ShaderCode = "";
		if (ShellsOn.On==true)
		{
			for (int i = 1;i<=ShellsCount.Float;i+=1){
				ShaderCode+=GCPassMask(SG,ShaderPass.ShellBase,(float)i/ShellsCount.Float);
				ShaderCode+=GCPass(SG,ShaderPass.ShellBase,(float)i/ShellsCount.Float);
			}
		}
		return ShaderCode;
	}
	public string GCSubShader(ShaderGenerate SG){
		string ShaderCode = "";
		
		string Tags = "\"RenderType\"=\"Opaque\"";
		
		if ((TransparencyOn.On&&TransparencyType.Type==1&&!TransparencyReceive.On)||!ShellsZWrite.On)
		Tags+="\"Queue\"=\"Transparent\"";
		if (TransparencyOn.On&&TransparencyType.Type==0)
		Tags+="\"Queue\"=\"AlphaTest\"";
		
		ShaderCode+="SubShader {\n"+
		"	Tags { "+Tags+" }//A bunch of settings telling Unity a bit about the shader.\n"+
		"	LOD "+((int)TechLOD.Float).ToString()+"\n";
		
		ShaderCode+=GCGrabPass(SG);
		
		if (!SG.Wireframe)
		ShaderCode+=GCPassMask(SG,ShaderPass.Base);
		
		if (!ShellsFront.On)
		ShaderCode+=GCShells(SG);

		ShaderCode+=GCPass(SG,ShaderPass.Base);

		if (ShellsFront.On)
		ShaderCode+=GCShells(SG);
		

		ShaderCode += "}\n";

		return ShaderCode;
	}
	public string GCLabTags(ShaderPass SP)
	{
		string ShaderCode = "";
		ShaderCode += "	blend off //Disabled blending (No Transparency)\n";
		if (!SG.Wireframe){
			if (MiscFog.On==false)
			ShaderCode+="Fog {Mode Off}\n";	
			
			if (TransparencyReceive.On&&TransparencyOn.On)
			ShaderCode+="blend SrcAlpha OneMinusSrcAlpha//Standard Transparency\nZWrite Off\n";
			
			if (ShaderPassShells(SP)&&ShellsBlendMode.Type==1)
			ShaderCode+="blend One One//Add Blend Mode\n";
			if (ShaderPassShells(SP)&&ShellsBlendMode.Type==2)
			ShaderCode+="blend DstColor Zero//Multiply Blend Mode\n";
			
			if (ShaderPassStandard(SP)&&BlendMode.Type==1&&TransparencyType.Type==1&&TransparencyOn.On)
			ShaderCode+="blend One One//Add Blend Mode\n";
			if (ShaderPassStandard(SP)&&BlendMode.Type==2&&TransparencyType.Type==1&&TransparencyOn.On)
			ShaderCode+="blend DstColor Zero//Multiply Blend Mode\n";
		}

		return ShaderCode;
	}
	public bool ShaderPassBase(ShaderPass SP){
		if (SP == ShaderPass.Base||SP == ShaderPass.ShellBase||ShaderPassMask(SP))
		return true;
		return false;
	}
	public bool ShaderPassLight(ShaderPass SP){
		if (SP == ShaderPass.Light||SP == ShaderPass.ShellLight)
		return true;
		return false;
	}
	public bool ShaderPassStandard(ShaderPass SP){
		if (SP == ShaderPass.Base||SP == ShaderPass.Light)
		return true;
		return false;
	}
	public bool ShaderPassShells(ShaderPass SP){
		if (SP == ShaderPass.ShellBase||SP == ShaderPass.ShellLight)
		return true;
		return false;
	}
	public bool ShaderPassMask(ShaderPass SP){
		if (SP == ShaderPass.MaskBase||SP == ShaderPass.MaskLight||SP == ShaderPass.Mask)
		return true;
		return false;
	}
	public string GCPass(ShaderGenerate SG,ShaderPass SP){
		return GCPass_Real(SG,SP,0f);
	}
	public string GCPass(ShaderGenerate SG,ShaderPass SP,float Dist){
		return GCPass_Real(SG,SP,Dist);
	}
	public string GCPass_Real(ShaderGenerate SG,ShaderPass SP,float Dist){
		string ShaderCode = "";
		//SG.Reset();
//		int OldTransType = TransparencyType.Type;
		float OldTransAmount = TransparencyAmount.Float;
		ShaderInput OldTransInput = TransparencyAmount.Input;
		if (TransparencyZWrite.On&&TransparencyZWriteType.Type==1){
			TransparencyAmount.Float = 1f;
			TransparencyAmount.Input = null;
		}
		
		ShaderCode+="	ZWrite On\n";
		if (ShaderPassShells(SP)){
			if (ShellsZWrite.On)
			ShaderCode+="	ZWrite On\n";
			else
			ShaderCode+="	ZWrite Off\n";
		}

			if (TransparencyOn.On&&TransparencyType.Type==1&&TransparencyZWrite.On==false)
			ShaderCode+="	ZWrite Off\n";
			//else
			//ShaderCode+="	ZWrite On\n";

		if (ShaderPassBase(SP)){
			if (ShaderPassShells(SP))
			ShaderCode+="	cull "+ShellsCull.CodeNames[ShellsCull.Type]+"//Culling specifies which sides of the models faces to hide.\n";
			else
			ShaderCode+="	cull "+TechCull.CodeNames[TechCull.Type]+"//Culling specifies which sides of the models faces to hide.\n";
		}
		ShaderCode+=GCLabTags(SP);

		ShaderCode+="	CGPROGRAM\n\n";
		ShaderCode+=GCUniforms(SG);
		ShaderCode+=GCPragma(SG,SP);
		ShaderCode+=GCSurfaceOutput();
		ShaderCode+=GCInputs(SG);
		ShaderCode+=GCFunctions(SG);
		ShaderCode+=GCLighting(SG,SP);
		ShaderCode+=GCVertex(SG,SP,Dist);
		ShaderCode+=GCFragment(SG,SP,Dist);

		ShaderCode+="	ENDCG\n";

		
		if (TransparencyZWrite.On&&TransparencyZWriteType.Type==1){
			TransparencyAmount.Input = OldTransInput;
			TransparencyAmount.Float = OldTransAmount;
		}

		return ShaderCode;
	}
	
	
	
	public string GCPassMask(ShaderGenerate SG,ShaderPass SP){
		if (TransparencyZWrite.On&&TransparencyOn.On&&TransparencyType.Type==1)
		return GCPassMask_Real(SG,SP,0f);
		return "";
	}
	public string GCPassMask(ShaderGenerate SG,ShaderPass SP,float Dist){
		if (TransparencyZWrite.On&&TransparencyOn.On&&TransparencyType.Type==1)
		return GCPassMask_Real(SG,SP,Dist);
		return "";
	}
	public string GCPassMask_Real(ShaderGenerate SG,ShaderPass SP,float Dist){
		string ShaderCode = "";
		//SG.Reset();
		int OldTransType = TransparencyType.Type;
//		float OldTransAmount = TransparencyAmount.Float;
//		ShaderInput OldTransInput = TransparencyAmount.Input;
		TransparencyType.Type = 0;
	
		if (TransparencyZWriteType.Type==0)
		ShaderCode+="\nPass\n"
		+"{\n"
		+"	Name \"ALPHAMASK\"\n"
		+"	ColorMask 0\n";
		
		if (ShaderPassShells(SP))
		ShaderCode+="	cull "+ShellsCull.CodeNames[ShellsCull.Type]+"\n";
		else
		ShaderCode+="	cull "+TechCull.CodeNames[TechCull.Type]+"\n";

		ShaderCode+=GCLabTags(SP);

		ShaderCode+="	CGPROGRAM\n\n";
		ShaderCode+=GCUniforms(SG);	
		if (TransparencyZWriteType.Type==1){
			ShaderCode+=GCPragma(SG,SP);
			ShaderCode+=GCSurfaceOutput();
			ShaderCode+=GCInputs(SG);
		}
		else{
			ShaderCode+="	//The Shader Target defines the maximum capabilites of the shader (Number of math operators, texture reads, etc.)\n	#pragma target "+((int)TechShaderTarget.Float).ToString()+".0\n"+
			"	//Tell Unity which parts of the shader affect the vertexes or the pixel colors.\n"+
			"	#pragma vertex vert\n"+
            "	#pragma fragment frag\n"+
            "	#include \"UnityCG.cginc\" //Include some base Unity stuff.\n";
		}
		ShaderCode+=GCFunctions(SG);
		if (TransparencyZWriteType.Type==1)
		ShaderCode+=GCLighting(SG,SP);
		if (TransparencyZWriteType.Type==0)
		ShaderCode+=GCVertexMask(SG,SP,Dist);
		else
		ShaderCode+=GCVertex(SG,SP,Dist);
		if (TransparencyZWriteType.Type==0)
		ShaderCode+=GCFragmentMask(SG,SP,Dist);
		else
		ShaderCode+=GCFragment(SG,SP,Dist);

		ShaderCode+="	ENDCG\n";

		if ((TransparencyZWriteType.Type==0))
		ShaderCode+="}\n";
		
		//if (TransparencyZWrite.On&&TransparencyZWriteType.Type==1){
		TransparencyType.Type = OldTransType;
		//}

		return ShaderCode;
	}
	public string GCParallax(ShaderGenerate SG){
		string ShaderCode = "";
		if (ParallaxOn.On&&SG.UsedParallax)
		{
			ShaderCode+="IN.viewDir = normalize(IN.viewDir);\n"+
			"	float3 view = IN.viewDir*(-1*"+ParallaxHeight.Get()+");\n";
			
			//if (SG.UsedWorldPos==true)
			if (SG.UsedWorldNormals&&SG.UsedWorldPos)
			ShaderCode+=
			"	float3 worldView = IN.worldNormal*(-1*"+ParallaxHeight.Get()+");\n";
			//"	float3 worldView = WorldNormalVector(IN,IN.viewDir)*(-1*"+ParallaxHeight.Get()+");\n";

			//shaderCode+="	float ray_intersect_rm(in v2f_surf IN, in float3 ds)\n";
			
			//shaderCode+="	{\n"+
			ShaderCode+="\n"+
			"		float size = 1.0/LINEAR_SEARCH; // stepping size\n"+
			"		float depth = 0;//pos\n"+
			"		int i;\n"+
			"		float Height = 1;\n"+
			"		for(i = 0; i < LINEAR_SEARCH-1; i++)// search until it steps over (Front to back)\n"+
			"		{\n"+
			//"			float4 t = tex2D(reliefmap,dp+ds*(depth)).a;\n"+
			GCLayers(SG,"Parallax",ShaderLayersHeight,"Height","a","",true,true)+"\n"+
			"			\n"+
			"			if(depth < (1-Height))\n"+
			"				depth += size;				\n"+
			"		}\n"+
			"		//depth = best_depth;\n"+
			"		for(i = 0; i < BINARY_SEARCH; i++) // look around for a closer match\n"+
			"		{\n"+
			"			size*=0.5;\n"+
			"			\n"+
			//"			float4 t = tex2D(reliefmap,dp+ds*(depth)).a;\n"+
			GCLayers(SG,"Parallax",ShaderLayersHeight,"Height","a","",true,true)+"\n"+
			"			\n"+
			"			if(depth < (1-Height))\n"+
			"				depth += (2*size);\n"+
			"			\n"+
			"			depth -= size;			\n"+	
			"		}\n"+
			"		\nSSParallaxDepth = depth;\n";
			//"		return depth;\n";
			//"	}\n";
			//ShaderCode+="	float depth = ray_intersect_rm(IN, view);\n";// distance(_WorldSpaceCameraPos, IN.worldPos)

			string UvClipName = "uvTexcoord";
//			string UvClipName2 = "";
			foreach (ShaderInput SI in ShaderInputs){
				if (SI.Type==0)
				{
					//if (SI.UsedMapTypeUv1==true||SI.UsedMapTypeUv2==true)
					{
						ShaderCode+="	uv"+SI.Get()+".xy += view.xy*depth;\n";
						if ("uv"+SI.Get()+".zw"==SG.GeneralUV)
						ShaderCode+="	uv"+SI.Get()+".zw += view.xy*depth;\n";
						//if (SI.UsedMapTypeCube==true)
						{
							UvClipName = "uv"+SI.Get();
							//UvClipName2 = SI.Get()+"_ST";
						}
					}
				}
			}
			//UnityEngine.Debug.Log(SG.GeneralUV);
			if (SG.GeneralUV=="uvTexcoord")
			ShaderCode+="IN.uvTexcoord.xy += view.xy*depth;\n";
			if (SG.UsedWorldNormals&&SG.UsedWorldPos)
			ShaderCode+="	IN.worldPos += worldView*depth;\n";
			if (ParallaxOn.On==true&&ParallaxSilhouetteClipping.On==true&&UvClipName!="")
			{
				/*ShaderCode+="	clip("+UvClipName+".x / "+UvClipName2+".x);\n"+
				"	clip("+UvClipName+".y / "+UvClipName2+".y);\n"+
				"	clip(-("+UvClipName+".x / "+UvClipName2+".x -1));\n"+
				"	clip(-("+UvClipName+".y / "+UvClipName2+".y -1));\n";*/
				ShaderCode+="	clip(IN."+SG.GeneralUV+".x);\n"+
				"	clip(IN."+SG.GeneralUV+".y);\n"+
				"	clip(-(IN."+SG.GeneralUV+".x-1));\n"+
				"	clip(-(IN."+SG.GeneralUV+".y-1));\n";
			}
		}
		SG.UsedBases.Clear();
		return ShaderCode;

	}
	public string GCFragment(ShaderGenerate SG, ShaderPass SP){
		return GCFragment(SG,SP,0f);
	}
	public string GCFragmentMask(ShaderGenerate SG, ShaderPass SP){
		return GCFragmentMask(SG,SP,0f);
	}
	public string GCFragmentMask(ShaderGenerate SG, ShaderPass SP,float Depth){
		return " fixed4 frag() : SV_Target {\n"+
			"    return fixed4(1.0,0.0,0.0,1.0);\n"+
			"}\n";
	}
	public string GCFragment(ShaderGenerate SG, ShaderPass SP,float Depth){
		string ShaderCode = "";

		ShaderCode+="\n";
		ShaderCode+="//Generate the fragment shader (Operates on pixels)\n";
		ShaderCode+="void frag_surf (Input IN, inout CSurfaceOutput o) {\n";
		
		#if PRE_UNITY_5
		if (DiffuseLightingType.Type==4){
		ShaderCode+=@"	o.worldRefl = IN.worldRefl;
		";
		}
		#endif
if (!SG.Wireframe){
		ShaderCode+="	float SSShellDepth = 1-"+Depth.ToString()+";\n";
		ShaderCode+="	float SSParallaxDepth = 0;\n";
		
		if (SG.UsedScreenPos)
		ShaderCode+="IN.screenPos.xy /= IN.screenPos.w;\n";
		

		//if (SG.TooManyTexcoords){
			foreach (ShaderInput SI in ShaderInputs){
				if (SI.Type==0)
				{
					if (SI.UsedMapType0==true||SI.UsedMapType1==true)
					{
						if (SG.TooManyTexcoords)
							ShaderCode+="	float2 uv"+SI.Get()+" = IN.uvTexcoord;\n";
						else
							ShaderCode+="	float2 uv"+SI.Get()+" = IN.uv"+SI.Get()+";\n";
					}
				}				
			}
		//}
		

		ShaderCode+="	//Set reasonable defaults for the fragment outputs.\n		o.Albedo = float3(0.8,0.8,0.8);\n"+
		"		float4 Emission = float4(0,0,0,0);\n";
		
		if (GCLightingName(SP)=="CLPBR_Standard"){
			if (SpecularOn.On)
				ShaderCode+="		o.Smoothness = "+SpecularHardness.Get()+";\n";
			else
				ShaderCode+="		o.Smoothness = 0;\n";
		}
		else{
			if (SpecularOn.On)
				ShaderCode+="		o.Smoothness = "+SpecularHardness.Get()+"*2;\n";
			else
				ShaderCode+="		o.Smoothness = 0;\n";
		}
		//if (SG.UsedNormals||SG.UsedShellsNormals)
		//ShaderCode+="	o.Normal = float3(0.5,0.5,1);\n";
		if (SG.UsedNormals||SG.UsedShellsNormals)
		ShaderCode+="		o.Normal = float3(0,0,1);\n";
		ShaderCode+="		o.Alpha = 1.0;\n"+
		"		o.Occlusion = 1.0;\n"+
		"		o.Specular = float3(0.3,0.3,0.3);\n";

		if (SG.UsedMapGenerate==true)
		ShaderCode+=//"	half2 UVy;\n"+
		//"	half2 UVx;\n"+
		//"	half2 UVz;\n\n"+
		//"	half4 TEXy;\n"+					
		//"	half4 TEXx;\n"+					
		//"	half4 TEXz;\n\n"+
		//"	half3 blend = normalize(abs(IN.worldNormal));\n";
		"	half3 blend = pow((abs(WorldNormalVector(IN, o.Normal))),5);\n	blend /= blend.x+blend.y+blend.z;\n";
		foreach(string Ty in ShaderSandwich.EffectsList){
			bool IsUsed = false;
			foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
				foreach(ShaderEffect SE in SL.LayerEffects)
				{
					if (Ty==SE.TypeS&&SE.Visible)
					IsUsed = true;
				}
			}
			if (IsUsed){
				ShaderEffect NewEffect = ShaderEffect.CreateInstance<ShaderEffect>();
				NewEffect.ShaderEffectIn(Ty);
				if (ShaderEffect.GetMethod(NewEffect.TypeS,"GenerateStart")!=null)
				ShaderCode+= (string)ShaderEffect.GetMethod(NewEffect.TypeS,"GenerateStart").Invoke(null,new object[]{SG})+"\n";
			}
		}
		
		ShaderCode+="#PARALLAX";
		ShaderCode+="#GENERATEMULTIUSEDBASES\n";

		foreach(ShaderLayerList SLL in ShaderLayersMasks)
		{
			if (SG.UsedMasks[SLL]>0&&!SLL.IsLighting.On){
				ShaderCode+=SLL.GCVariable();
				ShaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
			}
		}		
		if (ShaderPassStandard(SP)||ShaderPassMask(SP))
		{
			ShaderCode+=GCLayers(SG,"Normals",ShaderLayersNormal,"o.Normal","rgb","",false,true);
			//if (SG.UsedNormals)
			//ShaderCode+="o.Normal = (o.Normal-0.5)*2;\n";
			if (TransparencyOn.On)
			ShaderCode+=GCLayers(SG,"Alpha",ShaderLayersAlpha,"o.Alpha","a","",false,false);

			if (TransparencyOn.On&&TransparencyType.Type==0){
				ShaderCode+="	clip(o.Alpha-"+TransparencyAmount.Get()+");\n";
			}
			if (TransparencyOn.On&&TransparencyType.Type==1){
				ShaderCode+="	o.Alpha *= "+TransparencyAmount.Get()+";\n";
			}
			ShaderCode+=GCLayers(SG,"Diffuse",ShaderLayersDiffuse,"o.Albedo","rgb","",false,true);
			if (EmissionOn.On)
			ShaderCode+=GCLayers(SG,"Emission",ShaderLayersEmission,"Emission","rgba","",false,true);
			if (SpecularOn.On)
			ShaderCode+=GCLayers(SG,"Gloss",ShaderLayersSpecular,"o.Specular","rgb","",false,true);
		}		
		if (ShaderPassShells(SP))
		{
			ShaderCode+=GCLayers(SG,"Normals",ShaderLayersShellNormal,"o.Normal","rgb","",false,true);
			//if (SG.UsedShellsNormals)
			//ShaderCode+="o.Normal = (o.Normal-0.5)*2;\n";
			
			if (TransparencyOn.On)
			ShaderCode+=GCLayers(SG,"Alpha",ShaderLayersShellAlpha,"o.Alpha","a","",false,true);

			if (TransparencyOn.On&&TransparencyType.Type==0){
				ShaderCode+="	clip(o.Alpha-"+TransparencyAmount.Get()+");\n";
			}

			if (TransparencyOn.On&&TransparencyType.Type==1){
				ShaderCode+="	o.Alpha *= "+TransparencyAmount.Get()+";\n";
			}
			ShaderCode+=GCLayers(SG,"Diffuse",ShaderLayersShellDiffuse,"o.Albedo","rgb","",false,true);

			if (EmissionOn.On)
			ShaderCode+=GCLayers(SG,"Emission",ShaderLayersShellEmission,"Emission","rgba","",false,true);
			
			if (SpecularOn.On)
			ShaderCode+=GCLayers(SG,"Gloss",ShaderLayersShellSpecular,"o.Specular","rgb","",false,true);
		}
		if (EmissionOn.On){
			ShaderCode+="	o.Emission = Emission.rgb;\n";
			if (EmissionType.Type==1)
				ShaderCode+="	o.Emission*=o.Albedo;\n";
			if (EmissionType.Type==2){
				ShaderCode+="	o.Albedo *= 1-Emission.a;\n";
				//ShaderCode+="	o.Emission = float3(0,0,0);\n";
			}
		}
}
		ShaderCode+="}\n";
		string GenMultiUseBases = "";
		//int GenMultiUseBasesCount = 0;
		/*foreach(KeyValuePair<string, int> entry in SG.UsedBases){
			if (entry.Value>=2)
			{
				GenMultiUseBasesCount+=1;
				GenMultiUseBases+="float4 MultiUse"+GenMultiUseBasesCount.ToString()+" = "+entry.Key+";//"+entry.Value.ToString()+"\n";
				ShaderCode = ShaderCode.Replace(entry.Key,"MultiUse"+GenMultiUseBasesCount.ToString());
			}
		}*/
		ShaderCode = ShaderCode.Replace("#GENERATEMULTIUSEDBASES",GenMultiUseBases);
		ShaderCode = ShaderCode.Replace("#PARALLAX",GCParallax(SG));
		SG.UsedBases.Clear();
		return ShaderCode;
	}
	public string GCLightingName(ShaderPass SP){
		if (UsesShellLighting(SP))
		return "CL"+ShaderUtil.CodeName(DiffuseLightingType.Names[DiffuseLightingType.Type]);
		else
		return "CLUnlit";
	}
	public string GCUniforms(ShaderGenerate SG){
		string ShaderCode = "//Make our inputs accessible by declaring them here.\n";
		#if PRE_UNITY_5
		if (DiffuseLightingType.Type==4){
			ShaderCode+="samplerCUBE _Cube;\n";
		}
		#endif
		foreach(ShaderInput SI in ShaderInputs){
			if (SI.InEditor||SG.Temp){
				if (SI.Type==0)
				ShaderCode+="	sampler2D "+SI.Get()+";\n";
				if (SI.Type==1&&(SI.Get()!="_SpecColor"))
				ShaderCode+="	float4 "+SI.Get()+";\n";
				if (SI.Type==2)
				ShaderCode+="	samplerCUBE "+SI.Get()+";\n";
				if (SI.Type==3||SI.Type==4)
				ShaderCode+="	float "+SI.Get()+";\n";
			}
		}
		if (SG.UsedGrabPass){
			ShaderCode+="//Setup inputs for the grab pass texture and some meta information about it.\n"+
			"sampler2D _GrabTexture;\n"+
			"float4 _GrabTexture_TexelSize;\n";
		}
		if (SG.UsedDepthTexture){
			ShaderCode+="//Setup inputs for the depth texture.\n"+
			"sampler2D_float _CameraDepthTexture;\n";
		}
		if (SG.Temp){
				ShaderCode+="//Setup some time stuff for the Shader Sandwich preview\n	float4 _SSTime;\n"+
							"	float4 _SSSinTime;\n"+
							"	float4 _SSCosTime;\n";
		}
		return ShaderCode;
	}
	public string GCSurfaceOutput(){
		string ShaderCode = "//Create a struct which can contain various pixel properties, like specular colors, albedo, normals etc.\n";
		ShaderCode+="	struct CSurfaceOutput \n"+
		"	{ \n";
		#if PRE_UNITY_5
		if (DiffuseLightingType.Type==4){
		ShaderCode+=@"	half3 worldRefl;
		";
		}
		#endif
		ShaderCode+="		half3 Albedo; \n"+
		"		half3 Normal; \n"+
		"		half3 Emission; \n"+
		"		half Smoothness; \n"+
		"		half3 Specular; \n"+
		"		half Alpha; \n"+
		"		half Occlusion; \n"+
		"	};\n";		
		return ShaderCode;
	}	
	public string GCPragma(ShaderGenerate SG,ShaderPass SP)
	{
		string shaderCode = "";
		if (ParallaxOn.On)
		shaderCode+="//Set up some Parallax Occlusion Mapping Settings\n#define LINEAR_SEARCH "+(Math.Round(ParallaxBinaryQuality.Float/2)).ToString()+"\n"+
		"#define BINARY_SEARCH "+ParallaxBinaryQuality.Get()+"\n";

			shaderCode+="	#pragma surface frag_surf "+GCLightingName(SP);
			if (ShellsOn.On||ShaderLayersVertex.Count>0||TessellationOn.On)
			shaderCode+=" vertex:vert ";
			if (!TransparencyReceive.On||!TransparencyOn.On)
			shaderCode+=" addshadow ";
			
			if (GCLightingName(SP)=="CLUnlit"){
			#if UNITY_5
			shaderCode+= " noforwardadd noambient novertexlights nolightmap nodynlightmap nodirlightmap";
			#else
			shaderCode+= " noforwardadd noambient novertexlights nolightmap nodirlightmap";
			#endif
			}
			if ((TransparencyType.Type==1&&TransparencyOn.On&&!TransparencyReceive.On&&!(ShaderPassShells(SP)&&ShellsBlendMode.Type!=0)&&!(ShaderPassStandard(SP)&&BlendMode.Type!=0))&&!SG.Wireframe){
				shaderCode+= " alpha";
				#if UNITY_5
				if (GCLightingName(SP)!="CLPBR_Standard"||!TransparencyPBR.On)
				shaderCode+=":fade";
				#endif
			}
			if (TransparencyReceive.On&&TransparencyOn.On)
			shaderCode+=" keepalpha ";
			if (MiscAmbient.On==false)
				shaderCode+=" noambient";
			if (MiscVertexLights.On==false)
				shaderCode+=" novertexlights";
			if (MiscLightmap.On==false){
				#if UNITY_5
				shaderCode+=" nolightmap nodynlightmap nodirlightmap";
				#else
				shaderCode+=" nolightmap nodirlightmap";
				#endif
			}
			if (MiscFullShadows.On)
				shaderCode+=" fullforwardshadows";
			if (MiscHalfView.On)
				shaderCode+=" halfasview";
			if (!MiscForwardAdd.On)
				shaderCode+=" noforwardadd";
			#if UNITY_5
			if (MiscInterpolateView.On)
				shaderCode+=" interpolateview";
			if (!MiscShadows.On)
				shaderCode+=" noshadow";
			#endif
			if (TessellationOn.On){
				shaderCode+=" tessellate:tess";
				if (TessellationSmoothingAmount.Get()!="0"){
					shaderCode+=" tessphong:"+TessellationSmoothingAmount.Get();
				}
			}
			
			shaderCode=" //Set up Unity Surface Shader Settings.\n"+shaderCode+"\n";
		
		shaderCode+="//The Shader Target defines the maximum capabilites of the shader (Number of math operators, texture reads, etc.)\n	#pragma target "+((int)TechShaderTarget.Float).ToString()+".0\n";
		if (TessellationOn.On)
		shaderCode+="#include \"Tessellation.cginc\" //Include some Unity code for tessellation.\n";
		return shaderCode;
	}
	public string GCLayers(ShaderGenerate SG,string Name, ShaderLayerList SLs2,string CodeName,string EndTag,string Function,bool Parallax,bool UseEffects)
	{
		string ShaderCode = "";

		if (SLs2.SLs.Count>0)
		ShaderCode += "	//Generate layers for the "+Name+" channel.\n";
		
		int ShaderNumber = 0;
		foreach (ShaderLayer SL in SLs2.SLs)
		{
			ShaderNumber+=1;
			SL.LayerEffects.Reverse();
			string Map = SL.GCUVs(SG);
			SL.LayerEffects.Reverse();

			SL.Parent = SLs2;
			string PixelColor = SL.GCPixel(SG,Map)+"\n";
			//if (EndTag!="")
			//PixelColor+="."+EndTag;

			//PixelColor = CodeName+" = "+PixelColor;

			PixelColor += SL.GCCalculateMix(SG,CodeName,SL.GetSampleNameFirst(),Function,ShaderNumber)+"\n\n";

			ShaderCode+=PixelColor;
		}

		return ShaderCode;
	}
	public List<ShaderVar> GetMyShaderVars(){
		List<ShaderVar> SVs = new List<ShaderVar>();
		SVs.Add(TechCull);
		SVs.Add(DiffuseLightingType);
		SVs.Add(DiffuseColor);
		SVs.Add(DiffuseSetting1);
		SVs.Add(DiffuseSetting2);
		SVs.Add(DiffuseNormals);
		SVs.Add(SpecularLightingType);
		SVs.Add(SpecularHardness);
		SVs.Add(SpecularColor);
		SVs.Add(SpecularEnergy);
		SVs.Add(SpecularOffset);
		SVs.Add(EmissionColor);
		SVs.Add(TransparencyAmount);
		SVs.Add(ShellsDistance);
		SVs.Add(ShellsEase);
		SVs.Add(ShellsTransparencyAmount);

		SVs.Add(ParallaxHeight);
		SVs.Add(ParallaxBinaryQuality);
		SVs.Add(ParallaxSilhouetteClipping);
		
		SVs.Add(TechLOD);
		SVs.Add(TechCull);
		SVs.Add(TechShaderTarget);

		SVs.Add(DiffuseOn);
		SVs.Add(DiffuseLightingType);
		SVs.Add(DiffuseColor);
		SVs.Add(DiffuseSetting1);
		SVs.Add(DiffuseSetting2);
		SVs.Add(DiffuseNormals);

		SVs.Add(SpecularOn);
		SVs.Add(SpecularLightingType);
		SVs.Add(SpecularHardness);
		SVs.Add(SpecularColor);
		SVs.Add(SpecularEnergy);
		SVs.Add(SpecularOffset);

		SVs.Add(EmissionOn);
		SVs.Add(EmissionColor);
		SVs.Add(EmissionType);

		SVs.Add(TransparencyOn);
		SVs.Add(TransparencyType);
		SVs.Add(TransparencyZWrite);
		SVs.Add(TransparencyPBR);
		SVs.Add(TransparencyAmount);
		SVs.Add(TransparencyReceive);
		SVs.Add(TransparencyZWriteType);
		SVs.Add(BlendMode);

		SVs.Add(ShellsOn);
		SVs.Add(ShellsCount);
		SVs.Add(ShellsDistance);
		SVs.Add(ShellsEase);
		SVs.Add(ShellsTransparencyType);
		SVs.Add(ShellsTransparencyZWrite);
		SVs.Add(ShellsCull);
		SVs.Add(ShellsZWrite);
		SVs.Add(ShellsUseTransparency);
		SVs.Add(ShellsBlendMode);
		
		SVs.Add(ShellsTransparencyAmount);
		SVs.Add(ShellsLighting);
		SVs.Add(ShellsFront);

		SVs.Add(ParallaxOn);
		SVs.Add(ParallaxHeight);
		SVs.Add(ParallaxBinaryQuality);
		SVs.Add(ParallaxSilhouetteClipping);

		SVs.Add(TessellationOn);
		SVs.Add(TessellationType);
		SVs.Add(TessellationQuality);
		SVs.Add(TessellationFalloff);
		SVs.Add(TessellationSmoothingAmount);

		return SVs;
	}
	public void RecalculateAutoInputs(){
		foreach (ShaderInput SI in ShaderInputs){
		
			
			bool NameCollision = false;
			while (1==1){
				NameCollision = false;
				//foreach (ShaderInput SI2 in ShaderInputs){
				for(int i = ShaderInputs.Count - 1; i > -1; i--){
				ShaderInput SI2 = ShaderInputs[i];
					if (ShaderUtil.CodeName(SI.VisName)==ShaderUtil.CodeName(SI2.VisName)&&SI!=SI2){
						NameCollision = true;
						//new String(text.Where(Char.IsDigit).ToArray());
						if (char.IsDigit(SI.VisName[SI.VisName.Length-1])){
							SI.VisName =  SI.VisName.Substring(0, SI.VisName.Length - 1)+(int.Parse(SI.VisName.Substring(SI.VisName.Length - 1,1))+1).ToString();
						}
						else{
							SI.VisName += " 2";
						}
					}
				}
				if (NameCollision == false)
				break;
			}
		}
	}

	public string CGProperties(ShaderGenerate SG){
		string ShaderCode = "//The inputs shown in the material panel\nProperties {\n";
		if (SG.GeneralUV=="uvTexcoord"&&SG.UsedGenericUV)
		ShaderCode+="	[HideInInspector]Texcoord (\"Generic UV Coords (You shouldn't be seeing this aaaaah!)\", 2D) = \"white\" {}\n";
		
		#if PRE_UNITY_5
		if (DiffuseLightingType.Type==4){
		ShaderCode+="_Cube (\"Reflection Cubemap\", Cube) = \"_Skybox\" {}\n";
		}
		#endif
		foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
			SL.UVTexture = SL.Image.Input;
		}

		if (SG.Temp){
			foreach (ShaderInput SI in ShaderInputs){
				if (SI.Type==0){
					if (SI.NormalMap)
						ShaderCode+="	"+"[Normal]"+SI.Get()+" (\""+SI.VisName+"\", 2D) = \"bump\" {}\n";
					else
						ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", 2D) = \"white\" {}\n";
				}
				if (SI.Type==1)
				ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Color) = (1,1,1,1)\n";
				if (SI.Type==2)
				ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Cube) = \"white\"{}\n";
				if (SI.Type==3)
				ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Float) = 0\n";
				if (SI.Type==4)
				ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Range(-1000,1000)) = 0\n";
			}
		}
		else{
			foreach (ShaderInput SI in ShaderInputs){
				if (SI.InEditor){
					if (SI.Type==0){
						if (SI.NormalMap)
							ShaderCode+="	"+"[Normal]"+SI.Get()+" (\""+SI.VisName+"\", 2D) = \"bump\" {}\n";
						else
							ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", 2D) = \"white\" {}\n";
					}
					if (SI.Type==1)
					ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Color) = ("+SI.Color.ToString()+")\n";
					if (SI.Type==2)
					ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Cube) = \"_Skybox\"{}\n";
					if (SI.Type==3)
					ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Float) = "+SI.Number.ToString("F9")+"\n";
					if (SI.Type==4)
					ShaderCode+="	"+SI.Get()+" (\""+SI.VisName+"\", Range("+SI.Range0.ToString("F9")+","+SI.Range1.ToString("F9")+")) = "+SI.Number.ToString("F9")+"\n";
				}
			}
		}
		ShaderCode+="}\n\n";
		return ShaderCode;
	}
	public string GCFunctions(ShaderGenerate SG){
		string shaderCode = "";
		#if PRE_UNITY_5
		if (DiffuseLightingType.Type==4){
			shaderCode+="\n"+
"	#define SHADER_TARGET (30)\n"+
"#define U4Imposter\n"+
"#ifndef UNITY_PBS_LIGHTING_INCLUDED\n"+
"#define UNITY_PBS_LIGHTING_INCLUDED\n"+

"#include \"UnityShaderVariables.cginc\"\n"+
"#ifndef UNITY_STANDARD_CONFIG_INCLUDED\n"+
"#define UNITY_STANDARD_CONFIG_INCLUDED\n"+

"// Define Specular cubemap constants\n"+
"#define UNITY_SPECCUBE_LOD_EXPONENT (1.5)\n"+
"#define UNITY_SPECCUBE_LOD_STEPS (7) // TODO: proper fix for different cubemap resolution needed. My assumptions were actually wrong!\n"+

"// Energy conservation for Specular workflow is Monochrome. For instance: Red metal will make diffuse Black not Cyan\n"+
"#define UNITY_CONSERVE_ENERGY 1\n"+
"#define UNITY_CONSERVE_ENERGY_MONOCHROME 1\n"+
"\n"+
"// High end platforms support Box Projection and Blending\n"+
"#define UNITY_SPECCUBE_BOX_PROJECTION ( !defined(SHADER_API_MOBILE) && (SHADER_TARGET >= 30) )\n"+
"#define UNITY_SPECCUBE_BLENDING ( !defined(SHADER_API_MOBILE) && (SHADER_TARGET >= 30) )\n"+
"\n"+
"#define UNITY_SAMPLE_FULL_SH_PER_PIXEL 0\n"+
"\n"+
"#define UNITY_GLOSS_MATCHES_MARMOSET_TOOLBAG2 1\n"+
"#define UNITY_BRDF_GGX 0\n"+

@"// Orthnormalize Tangent Space basis per-pixel
// Necessary to support high-quality normal-maps. Compatible with Maya and Marmoset.
// However xNormal expects oldschool non-orthnormalized basis - essentially preventing good looking normal-maps :(
// Due to the fact that xNormal is probably _the most used tool to bake out normal-maps today_ we have to stick to old ways for now.
// 
// Disabled by default, until xNormal has an option to bake proper normal-maps.
"+
"#define UNITY_TANGENT_ORTHONORMALIZE 0\n"+
"\n"+
"#endif // UNITY_STANDARD_CONFIG_INCLUDED\n"+
"#ifndef UNITY_LIGHTING_COMMON_INCLUDED\n"+
"#define UNITY_LIGHTING_COMMON_INCLUDED\n"+
"\n"+
"\n"+
"\n"+
"struct UnityLight\n"+
"{\n"+
"	half3 color;\n"+
"	half3 dir;\n"+
"	half  ndotl;\n"+
"};\n"+
"\n"+
"struct UnityIndirect\n"+
"{\n"+
"	half3 diffuse;\n"+
"	half3 specular;\n"+
"};\n"+
"\n"+
"struct UnityGI\n"+
"{\n"+
"	UnityLight light;\n"+
"	#ifdef DIRLIGHTMAP_SEPARATE\n"+
"		#ifdef LIGHTMAP_ON\n"+
"			UnityLight light2;\n"+
"		#endif\n"+
"		#ifdef DYNAMICLIGHTMAP_ON\n"+
"			UnityLight light3;\n"+
"		#endif\n"+
"	#endif\n"+
"	UnityIndirect indirect;\n"+
"};\n"+
"\n"+
"struct UnityGIInput \n"+
"{\n"+
"	UnityLight light; // pixel light, sent from the engine\n"+
"\n"+
"	float3 worldPos;\n"+
"	float3 worldViewDir;\n"+
"	half atten;\n"+
"	half3 ambient;\n"+
"	float4 lightmapUV; // .xy = static lightmap UV, .zw = dynamic lightmap UV\n"+
"\n"+
"	float4 boxMax[2];\n"+
"	float4 boxMin[2];\n"+
"	float4 probePosition[2];\n"+
"	float4 probeHDR[2];\n"+
"};\n"+
"\n"+
"#endif\n"+
"#ifndef UNITY_GLOBAL_ILLUMINATION_INCLUDED\n"+
"#define UNITY_GLOBAL_ILLUMINATION_INCLUDED\n"+
"\n"+
"// Functions sampling light environment data (lightmaps, light probes, reflection probes), which is then returned as the UnityGI struct.\n"+
"\n"+
"\n"+
"#ifndef UNITY_STANDARD_BRDF_INCLUDED\n"+
"#define UNITY_STANDARD_BRDF_INCLUDED\n"+
"\n"+
"#include "+"\""+"UnityCG.cginc"+"\""+"\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"half4 unity_LightGammaCorrectionConsts = float4(100,100,100,100);\n"+
"#define unity_LightGammaCorrectionConsts_PIDiv4 (0.7853975)\n"+
"#define unity_LightGammaCorrectionConsts_PI (3.14159)\n"+
"#define unity_LightGammaCorrectionConsts_HalfDivPI (0.008)\n"+
"#define unity_LightGammaCorrectionConsts_8 (64)\n"+
"#define unity_LightGammaCorrectionConsts_SqrtHalfPI (0.04)\n"+
"#define unity_ColorSpaceDielectricSpec float4(0,0,0,0)\n"+
"#define UNITY_PI (3.14159)\n"+
"\n"+
"half DotClamped (half3 a, half3 b)\n"+
"{\n"+
"	#if (SHADER_TARGET < 30)\n"+
"		return saturate(dot(a, b));\n"+
"	#else\n"+
"		return max(0.0f, dot(a, b));\n"+
"	#endif\n"+
"}\n"+
"half4 DecodeHDR (half4 a, half3 b)\n"+
"{\n"+
"	return a;\n"+
"}\n"+
"\n"+
"half Pow4 (half x)\n"+
"{\n"+
"	return x*x*x*x;\n"+
"}\n"+
"\n"+
"half2 Pow4 (half2 x)\n"+
"{\n"+
"	return x*x*x*x;\n"+
"}\n"+
"\n"+
"half3 Pow4 (half3 x)\n"+
"{\n"+
"	return x*x*x*x;\n"+
"}\n"+
"\n"+
"half4 Pow4 (half4 x)\n"+
"{\n"+
"	return x*x*x*x;\n"+
"}\n"+
"\n"+
"// Pow5 uses the same amount of instructions as generic pow(), but has 2 advantages:\n"+
"// 1) better instruction pipelining\n"+
"// 2) no need to worry about NaNs\n"+
"half Pow5 (half x)\n"+
"{\n"+
"	return x*x * x*x * x;\n"+
"}\n"+
"\n"+
"half2 Pow5 (half2 x)\n"+
"{\n"+
"	return x*x * x*x * x;\n"+
"}\n"+
"\n"+
"half3 Pow5 (half3 x)\n"+
"{\n"+
"	return x*x * x*x * x;\n"+
"}\n"+
"\n"+
"half4 Pow5 (half4 x)\n"+
"{\n"+
"	return x*x * x*x * x;\n"+
"}\n"+
"\n"+
"half LambertTerm (half3 normal, half3 lightDir)\n"+
"{\n"+
"	return DotClamped (normal, lightDir);\n"+
"}\n"+
"\n"+
"half BlinnTerm (half3 normal, half3 halfDir)\n"+
"{\n"+
"	return DotClamped (normal, halfDir);\n"+
"}\n"+
"\n"+
"half3 FresnelTerm (half3 F0, half cosA)\n"+
"{\n"+
"	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation\n"+
"	return F0 + (1-F0) * t;\n"+
"}\n"+
"half3 FresnelLerp (half3 F0, half3 F90, half cosA)\n"+
"{\n"+
"	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation\n"+
"	return lerp (F0, F90, t);\n"+
"}\n"+
"// approximage Schlick with ^4 instead of ^5\n"+
"half3 FresnelLerpFast (half3 F0, half3 F90, half cosA)\n"+
"{\n"+
"	half t = Pow4 (1 - cosA);\n"+
"	return lerp (F0, F90, t);\n"+
"}\n"+
"half3 LazarovFresnelTerm (half3 F0, half roughness, half cosA)\n"+
"{\n"+
"	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation\n"+
"	t /= 4 - 3 * roughness;\n"+
"	return F0 + (1-F0) * t;\n"+
"}\n"+
"half3 SebLagardeFresnelTerm (half3 F0, half roughness, half cosA)\n"+
"{\n"+
"	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation\n"+
"	return F0 + (max (F0, roughness) - F0) * t;\n"+
"}\n"+
"\n"+
"// NOTE: Visibility term here is the full form from Torrance-Sparrow model, it includes Geometric term: V = G / (N.L * N.V)\n"+
"// This way it is easier to swap Geometric terms and more room for optimizations (except maybe in case of CookTorrance geom term)\n"+
"\n"+
"// Cook-Torrance visibility term, doesn't take roughness into account\n"+
"half CookTorranceVisibilityTerm (half NdotL, half NdotV,  half NdotH, half VdotH)\n"+
"{\n"+
"	VdotH += 1e-5f;\n"+
"	half G = min (1.0, min (\n"+
"		(2.0 * NdotH * NdotV) / VdotH,\n"+
"		(2.0 * NdotH * NdotL) / VdotH));\n"+
"	return G / (NdotL * NdotV + 1e-4f);\n"+
"}\n"+
"\n"+
"// Kelemen-Szirmay-Kalos is an approximation to Cook-Torrance visibility term\n"+
"// http://sirkan.iit.bme.hu/~szirmay/scook.pdf\n"+
"half KelemenVisibilityTerm (half LdotH)\n"+
"{\n"+
"	return 1.0 / (LdotH * LdotH);\n"+
"}\n"+
"\n"+
"// Modified Kelemen-Szirmay-Kalos which takes roughness into account, based on: http://www.filmicworlds.com/2014/04/21/optimizing-ggx-shaders-with-dotlh/ \n"+
"half ModifiedKelemenVisibilityTerm (half LdotH, half roughness)\n"+
"{\n"+
"	// c = sqrt(2 / Pi)\n"+
"	half c = unity_LightGammaCorrectionConsts_SqrtHalfPI;\n"+
"	half k = roughness * roughness * c;\n"+
"	half gH = LdotH * (1-k) + k;\n"+
"	return 1.0 / (gH * gH);\n"+
"}\n"+
"\n"+
"// Generic Smith-Schlick visibility term\n"+
"half SmithVisibilityTerm (half NdotL, half NdotV, half k)\n"+
"{\n"+
"	half gL = NdotL * (1-k) + k;\n"+
"	half gV = NdotV * (1-k) + k;\n"+
"	return 1.0 / (gL * gV + 1e-4f);\n"+
"}\n"+
"\n"+
"// Smith-Schlick derived for Beckmann\n"+
"half SmithBeckmannVisibilityTerm (half NdotL, half NdotV, half roughness)\n"+
"{\n"+
"	// c = sqrt(2 / Pi)\n"+
"	half c = unity_LightGammaCorrectionConsts_SqrtHalfPI;\n"+
"	half k = roughness * roughness * c;\n"+
"	return SmithVisibilityTerm (NdotL, NdotV, k);\n"+
"}\n"+
"\n"+
"// Smith-Schlick derived for GGX \n"+
"half SmithGGXVisibilityTerm (half NdotL, half NdotV, half roughness)\n"+
"{\n"+
"	half k = (roughness * roughness) / 2; // derived by B. Karis, http://graphicrants.blogspot.se/2013/08/specular-brdf-reference.html\n"+
"	return SmithVisibilityTerm (NdotL, NdotV, k);\n"+
"}\n"+
"\n"+
"half ImplicitVisibilityTerm ()\n"+
"{\n"+
"	return 1;\n"+
"}\n"+
"\n"+
"half RoughnessToSpecPower (half roughness)\n"+
"{\n"+
"roughness+=0.017;\n"+
"#if UNITY_GLOSS_MATCHES_MARMOSET_TOOLBAG2\n"+
"	// from https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html\n"+
"	half n = 10.0 / log2((1-roughness)*0.968 + 0.03);\n"+
"#if defined(SHADER_API_PS3)\n"+
"	n = max(n,-255.9370);  //i.e. less than sqrt(65504)\n"+
"#endif\n"+
"	return n * n;\n"+
"\n"+
"	// NOTE: another approximate approach to match Marmoset gloss curve is to\n"+
"	// multiply roughness by 0.7599 in the code below (makes SpecPower range 4..N instead of 1..N)\n"+
"#else\n"+
"	half m = roughness * roughness * roughness + 1e-4f;	// follow the same curve as unity_SpecCube\n"+
"	half n = (2.0 / m) - 2.0;							// http://jbit.net/%7Esparky/academic/mm_brdf.pdf\n"+
"	n = max(n, 1e-4f);									// prevent possible cases of pow(0,0), which could happen when roughness is 1.0 and NdotH is zero\n"+
"	return n;\n"+
"#endif\n"+
"}\n"+
"\n"+
"// BlinnPhong normalized as normal distribution function (NDF)\n"+
"// for use in micro-facet model: spec=D*G*F\n"+
"// http://www.thetenthplanet.de/archives/255\n"+
"half NDFBlinnPhongNormalizedTerm (half NdotH, half n)\n"+
"{\n"+
"	// norm = (n+1)/(2*pi)\n"+
"	half normTerm = (n + 1.0) * unity_LightGammaCorrectionConsts_HalfDivPI;\n"+
"\n"+
"	half specTerm = pow (NdotH, n);\n"+
"	return specTerm * normTerm;\n"+
"}\n"+
"\n"+
"// BlinnPhong normalized as reflecδion denγity funcδion (RDF)\n"+
"// ready for use directly as specular: spec=D\n"+
"// http://www.thetenthplanet.de/archives/255\n"+
"half RDFBlinnPhongNormalizedTerm (half NdotH, half n)\n"+
"{\n"+
"	half normTerm = (n + 2.0) / (8.0 * UNITY_PI);\n"+
"	half specTerm = pow (NdotH, n);\n"+
"	return specTerm * normTerm;\n"+
"}\n"+
"\n"+
"half GGXTerm (half NdotH, half roughness)\n"+
"{\n"+
"	half a = roughness * roughness;\n"+
"	half a2 = a * a;\n"+
"	half d = NdotH * NdotH * (a2 - 1.f) + 1.f;\n"+
"	return a2 / (UNITY_PI * d * d);\n"+
"}\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"/*\n"+
"// https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html\n"+
"\n"+
"const float k0 = 0.00098, k1 = 0.9921;\n"+
"// pass this as a constant for optimization\n"+
"const float fUserMaxSPow = 100000; // sqrt(12M)\n"+
"const float g_fMaxT = ( exp2(-10.0/fUserMaxSPow) - k0)/k1;\n"+
"float GetSpecPowToMip(float fSpecPow, int nMips)\n"+
"{\n"+
"   // Default curve - Inverse of TB2 curve with adjusted constants\n"+
"   float fSmulMaxT = ( exp2(-10.0/sqrt( fSpecPow )) - k0)/k1;\n"+
"   return float(nMips-1)*(1.0 - clamp( fSmulMaxT/g_fMaxT, 0.0, 1.0 ));\n"+
"}\n"+
"\n"+
"	//float specPower = RoughnessToSpecPower (roughness);\n"+
"	//float mip = GetSpecPowToMip (specPower, 7);\n"+
"*/\n"+
"\n"+
"// Decodes HDR textures\n"+
"// handles dLDR, RGBM formats\n"+
"// Modified version of DecodeHDR from UnityCG.cginc\n"+
"/*half3 DecodeHDR_NoLinearSupportInSM2 (half4 data, half4 decodeInstructions)\n"+
"{\n"+
"	// If Linear mode is not supported we can skip exponent part\n"+
"\n"+
"	// In Standard shader SM2.0 and SM3.0 paths are always using different shader variations\n"+
"	// SM2.0: hardware does not support Linear, we can skip exponent part\n"+
"	#if defined(UNITY_NO_LINEAR_COLORSPACE) && (SHADER_TARGET < 30)\n"+
"		return (data.a * decodeInstructions.x) * data.rgb;\n"+
"	#else\n"+
"		return DecodeHDR(data, decodeInstructions);\n"+
"	#endif\n"+
"}\n"+
"\n"+
"half3 Unity_GlossyEnvironment (samplerCUBE tex, half4 hdr, half3 worldNormal, half roughness){\n"+
"#if !UNITY_GLOSS_MATCHES_MARMOSET_TOOLBAG2 || (SHADER_TARGET < 30)\n"+
"	float mip = roughness * UNITY_SPECCUBE_LOD_STEPS;\n"+
"#else\n"+
"	// TODO: remove pow, store cubemap mips differently\n"+
"	float mip = pow(roughness,3.0/4.0) * UNITY_SPECCUBE_LOD_STEPS;\n"+
"#endif\n"+
"\n"+
"	half4 rgbm = SampleCubeReflection(tex, worldNormal.xyz, mip);\n"+
"	return DecodeHDR_NoLinearSupportInSM2 (rgbm, hdr);\n"+
"}\n"+
"*/\n"+
"//-------------------------------------------------------------------------------------\n"+
"\n"+
"// Note: BRDF entry points use oneMinusRoughness (aka "+"\""+"smoothness"+"\""+") and oneMinusReflectivity for optimization\n"+
"// purposes, mostly for DX9 SM2.0 level. Most of the math is being done on these (1-x) values, and that saves\n"+
"// a few precious ALU slots.\n"+
"\n"+
"\n"+
"// Main Physically Based BRDF\n"+
"// Derived from Disney work and based on Torrance-Sparrow micro-facet model\n"+
"//\n"+
"//   BRDF = kD / pi + kS * (D * V * F) / 4\n"+
"//   I = BRDF * NdotL\n"+
"//\n"+
"// * NDF (depending on UNITY_BRDF_GGX):\n"+
"//  a) Normalized BlinnPhong\n"+
"//  b) GGX\n"+
"// * Smith for Visiblity term\n"+
"// * Schlick approximation for Fresnel\n"+
"half4 BRDF1_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,\n"+
"	half3 normal, half3 viewDir,\n"+
"	UnityLight light, UnityIndirect gi)\n"+
"{\n"+
"	half roughness = 1-oneMinusRoughness;\n"+
"	half3 halfDir = normalize (light.dir + viewDir);\n"+
"\n"+
"	half nl = light.ndotl;\n"+
"	half nh = BlinnTerm (normal, halfDir);\n"+
"	half nv = DotClamped (normal, viewDir);\n"+
"	half lv = DotClamped (light.dir, viewDir);\n"+
"	half lh = DotClamped (light.dir, halfDir);\n"+
"\n"+
"#if UNITY_BRDF_GGX\n"+
"	half V = SmithGGXVisibilityTerm (nl, nv, roughness);\n"+
"	half D = GGXTerm (nh, roughness);\n"+
"#else\n"+
"	half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);\n"+
"	half D = NDFBlinnPhongNormalizedTerm (nh, RoughnessToSpecPower (roughness));\n"+
"#endif\n"+
"\n"+
"	half nlPow5 = Pow5 (1-nl);\n"+
"	half nvPow5 = Pow5 (1-nv);\n"+
"	half Fd90 = 0.5 + 2 * lh * lh * roughness;\n"+
"	half disneyDiffuse = (1 + (Fd90-1) * nlPow5) * (1 + (Fd90-1) * nvPow5);\n"+
"	\n"+
"	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!\n"+
"	// BUT 1) that will make shader look significantly darker than Legacy ones\n"+
"	// and 2) on engine side "+"\""+"Non-important"+"\""+" lights have to be divided by Pi to in cases when they are injected into ambient SH\n"+
"	// NOTE: multiplication by Pi is part of single constant together with 1/4 now\n"+
"\n"+
"	half specularTerm = max(0, (V * D * nl) * unity_LightGammaCorrectionConsts_PIDiv4);// Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)\n"+
"	half diffuseTerm = disneyDiffuse * nl;\n"+
"	\n"+
"	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));\n"+
"    half3 color =	diffColor * (gi.diffuse + light.color * diffuseTerm)\n"+
"                    + specularTerm * light.color * FresnelTerm (specColor, lh)\n"+
"					+ gi.specular * FresnelLerp (specColor, grazingTerm, nv);\n"+
"\n"+
"	return half4(color, 1);\n"+
"}\n"+
"\n"+
"// Based on Minimalist CookTorrance BRDF\n"+
"// Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255\n"+
"//\n"+
"// * BlinnPhong as NDF\n"+
"// * Modified Kelemen and Szirmay-?Kalos for Visibility term\n"+
"// * Fresnel approximated with 1/LdotH\n"+
"half4 BRDF2_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,\n"+
"	half3 normal, half3 viewDir,\n"+
"	UnityLight light, UnityIndirect gi)\n"+
"{\n"+
"	half3 halfDir = normalize (light.dir + viewDir);\n"+
"\n"+
"	half nl = light.ndotl;\n"+
"	half nh = BlinnTerm (normal, halfDir);\n"+
"	half nv = DotClamped (normal, viewDir);\n"+
"	half lh = DotClamped (light.dir, halfDir);\n"+
"\n"+
"	half roughness = 1-oneMinusRoughness;\n"+
"	half specularPower = RoughnessToSpecPower (roughness);\n"+
"	// Modified with approximate Visibility function that takes roughness into account\n"+
"	// Original ((n+1)*N.H^n) / (8*Pi * L.H^3) didn't take into account roughness \n"+
"	// and produced extremely bright specular at grazing angles\n"+
"\n"+
"	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!\n"+
"	// BUT 1) that will make shader look significantly darker than Legacy ones\n"+
"	// and 2) on engine side "+"\""+"Non-important"+"\""+" lights have to be divided by Pi to in cases when they are injected into ambient SH\n"+
"	// NOTE: multiplication by Pi is cancelled with Pi in denominator\n"+
"\n"+
"	half invV = lh * lh * oneMinusRoughness + roughness * roughness; // approx ModifiedKelemenVisibilityTerm(lh, 1-oneMinusRoughness);\n"+
"	half invF = lh;\n"+
"	half specular = ((specularPower + 1) * pow (nh, specularPower)) / (unity_LightGammaCorrectionConsts_8 * invV * invF + 1e-4f); // @TODO: might still need saturate(nl*specular) on Adreno/Mali\n"+
"\n"+
"	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));\n"+
"    half3 color =	(diffColor + specular * specColor) * light.color * nl\n"+
"    				+ gi.diffuse * diffColor\n"+
"					+ gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);\n"+
"\n"+
"	return half4(color, 1);\n"+
"}\n"+
"\n"+
"// Old school, not microfacet based Modified Normalized Blinn-Phong BRDF\n"+
"// Implementation uses Lookup texture for performance\n"+
"//\n"+
"// * Normalized BlinnPhong in RDF form\n"+
"// * Implicit Visibility term\n"+
"// * No Fresnel term\n"+
"//\n"+
"// TODO: specular is too weak in Linear rendering mode\n"+
"sampler2D unity_NHxRoughness;\n"+
"half4 BRDF3_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,\n"+
"	half3 normal, half3 viewDir,\n"+
"	UnityLight light, UnityIndirect gi)\n"+
"{\n"+
"	half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp\n"+
"\n"+
"	half3 reflDir = reflect (viewDir, normal);\n"+
"	half3 halfDir = normalize (light.dir + viewDir);\n"+
"\n"+
"	half nl = light.ndotl;\n"+
"	half nh = BlinnTerm (normal, halfDir);\n"+
"	half nv = DotClamped (normal, viewDir);\n"+
"\n"+
"	// Vectorize Pow4 to save instructions\n"+
"	half2 rlPow4AndFresnelTerm = Pow4 (half2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions\n"+
"	half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp\n"+
"	half fresnelTerm = rlPow4AndFresnelTerm.y;\n"+
"\n"+
"#if 1 // Lookup texture to save instructions\n"+
"	half specular = tex2D(unity_NHxRoughness, half2(rlPow4, 1-oneMinusRoughness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;\n"+
"#else\n"+
"	half roughness = 1-oneMinusRoughness;\n"+
"	half n = RoughnessToSpecPower (roughness) * .25;\n"+
"	half specular = (n + 2.0) / (2.0 * UNITY_PI * UNITY_PI) * pow(dot(reflDir, light.dir), n) * nl;// / unity_LightGammaCorrectionConsts_PI;\n"+
"	//half specular = (1.0/(UNITY_PI*roughness*roughness)) * pow(dot(reflDir, light.dir), n) * nl;// / unity_LightGammaCorrectionConsts_PI;\n"+
"#endif\n"+
"	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));\n"+
"\n"+
"    half3 color =	(diffColor + specular * specColor) * light.color * nl\n"+
"    				+ gi.diffuse * diffColor\n"+
"					+ gi.specular * lerp (specColor, grazingTerm, fresnelTerm);\n"+
"\n"+
"	return half4(color, 1);\n"+
"}\n"+
"\n"+
"\n"+
"#endif // UNITY_STANDARD_BRDF_INCLUDED\n"+
"\n"+
"#ifndef UNITY_STANDARD_UTILS_INCLUDED\n"+
"#define UNITY_STANDARD_UTILS_INCLUDED\n"+
"\n"+
"#include "+"\""+"UnityCG.cginc"+"\""+"\n"+
"\n"+
"// Helper functions, maybe move into UnityCG.cginc\n"+
"\n"+
"half SpecularStrength(half3 specular)\n"+
"{\n"+
"	#if (SHADER_TARGET < 30)\n"+
"		// SM2.0: instruction count limitation\n"+
"		// SM2.0: simplified SpecularStrength\n"+
"		return specular.r; // Red channel - because most metals are either monocrhome or with redish/yellowish tint\n"+
"	#else\n"+
"		return max (max (specular.r, specular.g), specular.b);\n"+
"	#endif\n"+
"}\n"+
"\n"+
"// Diffuse/Spec Energy conservation\n"+
"half3 EnergyConservationBetweenDiffuseAndSpecular (half3 albedo, half3 specColor, out half oneMinusReflectivity)\n"+
"{\n"+
"	oneMinusReflectivity = 1 - SpecularStrength(specColor);\n"+
"	#if !UNITY_CONSERVE_ENERGY\n"+
"		return albedo;\n"+
"	#elif UNITY_CONSERVE_ENERGY_MONOCHROME\n"+
"		return albedo * oneMinusReflectivity;\n"+
"	#else\n"+
"		return albedo * (half3(1,1,1) - specColor);\n"+
"	#endif\n"+
"}\n"+
"\n"+
"half3 DiffuseAndSpecularFromMetallic (half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)\n"+
"{\n"+
"	specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);\n"+
"	// We'll need oneMinusReflectivity, so\n"+
"	//   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)\n"+
"	// store (1-dielectricSpec) in unity_ColorSpaceDielectricSpec.a, then\n"+
"	//	 1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) = \n"+
"	//                  = alpha - metallic * alpha\n"+
"	half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;\n"+
"	oneMinusReflectivity = oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;\n"+
"	return albedo * oneMinusReflectivity;\n"+
"}\n"+
"\n"+
"half3 PreMultiplyAlpha (half3 diffColor, half alpha, half oneMinusReflectivity, out half outModifiedAlpha)\n"+
"{\n"+
"	#if defined(_ALPHAPREMULTIPLY_ON)\n"+
"		// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)\n"+
"\n"+
"		// Transparency 'removes' from Diffuse component\n"+
" 		diffColor *= alpha;\n"+
" 		\n"+
" 		#if (SHADER_TARGET < 30)\n"+
" 			// SM2.0: instruction count limitation\n"+
" 			// Instead will sacrifice part of physically based transparency where amount Reflectivity is affecting Transparency\n"+
" 			// SM2.0: uses unmodified alpha\n"+
" 			outModifiedAlpha = alpha;\n"+
" 		#else\n"+
"	 		// Reflectivity 'removes' from the rest of components, including Transparency\n"+
"	 		// outAlpha = 1-(1-alpha)*(1-reflectivity) = 1-(oneMinusReflectivity - alpha*oneMinusReflectivity) =\n"+
"	 		//          = 1-oneMinusReflectivity + alpha*oneMinusReflectivity\n"+
"	 		outModifiedAlpha = 1-oneMinusReflectivity + alpha*oneMinusReflectivity;\n"+
" 		#endif\n"+
" 	#else\n"+
" 		outModifiedAlpha = alpha;\n"+
" 	#endif\n"+
" 	return diffColor;\n"+
"}\n"+
"\n"+
"// Same as ParallaxOffset in Unity CG, except:\n"+
"//  *) precision - half instead of float\n"+
"half2 ParallaxOffset1Step (half h, half height, half3 viewDir)\n"+
"{\n"+
"	h = h * height - height/2.0;\n"+
"	half3 v = normalize(viewDir);\n"+
"	v.z += 0.42;\n"+
"	return h * (v.xy / v.z);\n"+
"}\n"+
"\n"+
"half LerpOneTo(half b, half t)\n"+
"{\n"+
"	half oneMinusT = 1 - t;\n"+
"	return oneMinusT + b * t;\n"+
"}\n"+
"\n"+
"half3 LerpWhiteTo(half3 b, half t)\n"+
"{\n"+
"	half oneMinusT = 1 - t;\n"+
"	return half3(oneMinusT, oneMinusT, oneMinusT) + b * t;\n"+
"}\n"+
"\n"+
"half3 UnpackScaleNormal(half4 packednormal, half bumpScale)\n"+
"{\n"+
"	#if defined(UNITY_NO_DXT5nm)\n"+
"		return packednormal.xyz * 2 - 1;\n"+
"	#else\n"+
"		half3 normal;\n"+
"		normal.xy = (packednormal.wy * 2 - 1);\n"+
"		#if (SHADER_TARGET >= 30)\n"+
"			// SM2.0: instruction count limitation\n"+
"			// SM2.0: normal scaler is not supported\n"+
"			normal.xy *= bumpScale;\n"+
"		#endif\n"+
"		normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));\n"+
"		return normal;\n"+
"	#endif\n"+
"}		\n"+
"\n"+
"half3 BlendNormals(half3 n1, half3 n2)\n"+
"{\n"+
"	return normalize(half3(n1.xy + n2.xy, n1.z*n2.z));\n"+
"}\n"+
"\n"+
"half3x3 CreateTangentToWorldPerVertex(half3 normal, half3 tangent, half3 flip)\n"+
"{\n"+
"	half3 binormal = cross(normal, tangent) * flip;\n"+
"	return half3x3(tangent, binormal, normal);\n"+
"}\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"half3 BoxProjectedCubemapDirection (half3 worldNormal, float3 worldPos, float4 cubemapCenter, float4 boxMin, float4 boxMax)\n"+
"{\n"+
"	// Do we have a valid reflection probe?\n"+
"	\n"+
"	if (cubemapCenter.w > 0.0)\n"+
"	{\n"+
"		half3 nrdir = normalize(worldNormal);\n"+
"\n"+
"		#if 1				\n"+
"			half3 rbmax = (boxMax.xyz - worldPos) / nrdir;\n"+
"			half3 rbmin = (boxMin.xyz - worldPos) / nrdir;\n"+
"\n"+
"			half3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;\n"+
"\n"+
"		#else // Optimized version\n"+
"			half3 rbmax = (boxMax.xyz - worldPos);\n"+
"			half3 rbmin = (boxMin.xyz - worldPos);\n"+
"\n"+
"			half3 select = step (half3(0,0,0), nrdir);\n"+
"			half3 rbminmax = lerp (rbmax, rbmin, select);\n"+
"			rbminmax /= nrdir;\n"+
"		#endif\n"+
"\n"+
"		half fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);\n"+
"\n"+
"		float3 aabbCenter = (boxMax.xyz + boxMin.xyz) * 0.5;\n"+
"		float3 offset = aabbCenter - cubemapCenter.xyz;\n"+
"		float3 posonbox = offset + worldPos + nrdir * fa;\n"+
"\n"+
"		worldNormal = posonbox - aabbCenter;\n"+
"	}\n"+
"	return worldNormal;\n"+
"}\n"+
"\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"// Derivative maps\n"+
"// http://www.rorydriscoll.com/2012/01/11/derivative-maps/\n"+
"// For future use.\n"+
"\n"+
"// Project the surface gradient (dhdx, dhdy) onto the surface (n, dpdx, dpdy)\n"+
"half3 CalculateSurfaceGradient(half3 n, half3 dpdx, half3 dpdy, half dhdx, half dhdy)\n"+
"{\n"+
"	half3 r1 = cross(dpdy, n);\n"+
"	half3 r2 = cross(n, dpdx);\n"+
"	return (r1 * dhdx + r2 * dhdy) / dot(dpdx, r1);\n"+
"}\n"+
"\n"+
"// Move the normal away from the surface normal in the opposite surface gradient direction\n"+
"half3 PerturbNormal(half3 n, half3 dpdx, half3 dpdy, half dhdx, half dhdy)\n"+
"{\n"+
"	//TODO: normalize seems to be necessary when scales do go beyond the 2...-2 range, should we limit that?\n"+
"	//how expensive is a normalize? Anything cheaper for this case?\n"+
"	return normalize(n - CalculateSurfaceGradient(n, dpdx, dpdy, dhdx, dhdy));\n"+
"}\n"+
"\n"+
"// Calculate the surface normal using the uv-space gradient (dhdu, dhdv)\n"+
"half3 CalculateSurfaceNormal(half3 position, half3 normal, half2 gradient, half2 uv)\n"+
"{\n"+
"	half3 dpdx = ddx(position);\n"+
"	half3 dpdy = ddy(position);\n"+
"\n"+
"	half dhdx = dot(gradient, ddx(uv));\n"+
"	half dhdy = dot(gradient, ddy(uv));\n"+
"\n"+
"	return PerturbNormal(normal, dpdx, dpdy, dhdx, dhdy);\n"+
"}\n"+
"\n"+
"\n"+
"#endif // UNITY_STANDARD_UTILS_INCLUDED\n"+
"\n"+
"\n"+
"half3 DecodeDirectionalSpecularLightmap (half3 color, fixed4 dirTex, half3 normalWorld, bool isRealtimeLightmap, fixed4 realtimeNormalTex, out UnityLight o_light)\n"+
"{\n"+
"	o_light.color = color;\n"+
"	o_light.dir = dirTex.xyz * 2 - 1;\n"+
"\n"+
"	// The length of the direction vector is the light's "+"\""+"directionality"+"\""+", i.e. 1 for all light coming from this direction,\n"+
"	// lower values for more spread out, ambient light.\n"+
"	half directionality = length(o_light.dir);\n"+
"	o_light.dir /= directionality;\n"+
"\n"+
"	#ifdef DYNAMICLIGHTMAP_ON\n"+
"	if (isRealtimeLightmap)\n"+
"	{\n"+
"		// Realtime directional lightmaps' intensity needs to be divided by N.L\n"+
"		// to get the incoming light intensity. Baked directional lightmaps are already\n"+
"		// output like that (including the max() to prevent div by zero).\n"+
"		half3 realtimeNormal = realtimeNormalTex.zyx * 2 - 1;\n"+
"		o_light.color /= max(0.125, dot(realtimeNormal, o_light.dir));\n"+
"	}\n"+
"	#endif\n"+
"\n"+
"	o_light.ndotl = LambertTerm(normalWorld, o_light.dir);\n"+
"\n"+
"	// Split light into the directional and ambient parts, according to the directionality factor.\n"+
"	half3 ambient = o_light.color * (1 - directionality);\n"+
"	o_light.color = o_light.color * directionality;\n"+
"\n"+
"	// Technically this is incorrect, but helps hide jagged light edge at the object silhouettes and\n"+
"	// makes normalmaps show up.\n"+
"	ambient *= o_light.ndotl;\n"+
"	return ambient;\n"+
"}\n"+
"\n"+
"half3 MixLightmapWithRealtimeAttenuation (half3 lightmapContribution, half attenuation, fixed4 bakedColorTex)\n"+
"{\n"+
"	// Let's try to make realtime shadows work on a surface, which already contains\n"+
"	// baked lighting and shadowing from the current light.\n"+
"	// Generally do min(lightmap,shadow), with "+"\""+"shadow"+"\""+" taking overall lightmap tint into account.\n"+
"	half3 shadowLightmapColor = bakedColorTex.rgb * attenuation;\n"+
"	half3 darkerColor = min(lightmapContribution, shadowLightmapColor);\n"+
"\n"+
"	// However this can darken overbright lightmaps, since "+"\""+"shadow color"+"\""+" will\n"+
"	// never be overbright. So take a max of that color with attenuated lightmap color.\n"+
"	return max(darkerColor, lightmapContribution * attenuation);\n"+
"}\n"+
"\n"+
"void ResetUnityLight(out UnityLight outLight)\n"+
"{\n"+
"	outLight.color = 0;\n"+
"	outLight.dir = 0;\n"+
"	outLight.ndotl = 0;\n"+
"}\n"+
"\n"+
"void ResetUnityGI(out UnityGI outGI)\n"+
"{\n"+
"	ResetUnityLight(outGI.light);\n"+
"	#ifdef DIRLIGHTMAP_SEPARATE\n"+
"		#ifdef LIGHTMAP_ON\n"+
"			ResetUnityLight(outGI.light2);\n"+
"		#endif\n"+
"		#ifdef DYNAMICLIGHTMAP_ON\n"+
"			ResetUnityLight(outGI.light3);\n"+
"		#endif\n"+
"	#endif\n"+
"	outGI.indirect.diffuse = 0;\n"+
"	outGI.indirect.specular = 0;\n"+
"}\n"+
"\n"+
"/*UnityGI UnityGlobalIllumination (UnityGIInput data, half occlusion, half oneMinusRoughness, half3 normalWorld, bool reflections)\n"+
"{\n"+
"	UnityGI o_gi;\n"+
"	UNITY_INITIALIZE_OUTPUT(UnityGI, o_gi);\n"+
"\n"+
"	// Explicitly reset all members of UnityGI\n"+
"	ResetUnityGI(o_gi);\n"+
"\n"+
"	#if UNITY_SHOULD_SAMPLE_SH\n"+
"		#if UNITY_SAMPLE_FULL_SH_PER_PIXEL\n"+
"			half3 sh = ShadeSH9(half4(normalWorld, 1.0));\n"+
"		#elif (SHADER_TARGET >= 30)\n"+
"			half3 sh = data.ambient + ShadeSH12Order(half4(normalWorld, 1.0));\n"+
"		#else\n"+
"			half3 sh = data.ambient;\n"+
"		#endif\n"+
"	\n"+
"		o_gi.indirect.diffuse += sh;\n"+
"	#endif\n"+
"\n"+
"	#if !defined(LIGHTMAP_ON)\n"+
"		o_gi.light = data.light;\n"+
"		o_gi.light.color *= data.atten;\n"+
"\n"+
"	#else\n"+
"		// Baked lightmaps\n"+
"		fixed4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, data.lightmapUV.xy); \n"+
"		half3 bakedColor = DecodeLightmap(bakedColorTex);\n"+
"		\n"+
"		#ifdef DIRLIGHTMAP_OFF\n"+
"			o_gi.indirect.diffuse = bakedColor;\n"+
"\n"+
"			#ifdef SHADOWS_SCREEN\n"+
"				o_gi.indirect.diffuse = MixLightmapWithRealtimeAttenuation (o_gi.indirect.diffuse, data.atten, bakedColorTex);\n"+
"			#endif // SHADOWS_SCREEN\n"+
"\n"+
"		#elif DIRLIGHTMAP_COMBINED\n"+
"			fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER (unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);\n"+
"			o_gi.indirect.diffuse = DecodeDirectionalLightmap (bakedColor, bakedDirTex, normalWorld);\n"+
"\n"+
"			#ifdef SHADOWS_SCREEN\n"+
"				o_gi.indirect.diffuse = MixLightmapWithRealtimeAttenuation (o_gi.indirect.diffuse, data.atten, bakedColorTex);\n"+
"			#endif // SHADOWS_SCREEN\n"+
"\n"+
"		#elif DIRLIGHTMAP_SEPARATE\n"+
"			// Left halves of both intensity and direction lightmaps store direct light; right halves - indirect.\n"+
"\n"+
"			// Direct\n"+
"			fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);\n"+
"			o_gi.indirect.diffuse += DecodeDirectionalSpecularLightmap (bakedColor, bakedDirTex, normalWorld, false, 0, o_gi.light);\n"+
"\n"+
"			// Indirect\n"+
"			half2 uvIndirect = data.lightmapUV.xy + half2(0.5, 0);\n"+
"			bakedColor = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, uvIndirect));\n"+
"			bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, uvIndirect);\n"+
"			o_gi.indirect.diffuse += DecodeDirectionalSpecularLightmap (bakedColor, bakedDirTex, normalWorld, false, 0, o_gi.light2);\n"+
"		#endif\n"+
"	#endif\n"+
"	\n"+
"	#ifdef DYNAMICLIGHTMAP_ON\n"+
"		// Dynamic lightmaps\n"+
"		fixed4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, data.lightmapUV.zw);\n"+
"		half3 realtimeColor = DecodeRealtimeLightmap (realtimeColorTex);\n"+
"\n"+
"		#ifdef DIRLIGHTMAP_OFF\n"+
"			o_gi.indirect.diffuse += realtimeColor;\n"+
"\n"+
"		#elif DIRLIGHTMAP_COMBINED\n"+
"			half4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, data.lightmapUV.zw);\n"+
"			o_gi.indirect.diffuse += DecodeDirectionalLightmap (realtimeColor, realtimeDirTex, normalWorld);\n"+
"\n"+
"		#elif DIRLIGHTMAP_SEPARATE\n"+
"			half4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, data.lightmapUV.zw);\n"+
"			half4 realtimeNormalTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicNormal, unity_DynamicLightmap, data.lightmapUV.zw);\n"+
"			o_gi.indirect.diffuse += DecodeDirectionalSpecularLightmap (realtimeColor, realtimeDirTex, normalWorld, true, realtimeNormalTex, o_gi.light3);\n"+
"		#endif\n"+
"	#endif\n"+
"	o_gi.indirect.diffuse *= occlusion;\n"+
"\n"+
"	if (reflections)\n"+
"	{\n"+
"		half3 worldNormal = reflect(-data.worldViewDir, normalWorld);\n"+
"\n"+
"		#if UNITY_SPECCUBE_BOX_PROJECTION		\n"+
"			half3 worldNormal0 = BoxProjectedCubemapDirection (worldNormal, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);\n"+
"		#else\n"+
"			half3 worldNormal0 = worldNormal;\n"+
"		#endif\n"+
"\n"+
"		half3 env0 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], worldNormal0, 1-oneMinusRoughness);\n"+
"		#if UNITY_SPECCUBE_BLENDING\n"+
"			const float kBlendFactor = 0.99999;\n"+
"			float blendLerp = data.boxMin[0].w;\n"+
"			UNITY_BRANCH\n"+
"			if (blendLerp < kBlendFactor)\n"+
"			{\n"+
"				#if UNITY_SPECCUBE_BOX_PROJECTION\n"+
"					half3 worldNormal1 = BoxProjectedCubemapDirection (worldNormal, data.worldPos, data.probePosition[1], data.boxMin[1], data.boxMax[1]);\n"+
"				#else\n"+
"					half3 worldNormal1 = worldNormal;\n"+
"				#endif\n"+
"\n"+
"				half3 env1 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube1), data.probeHDR[1], worldNormal1, 1-oneMinusRoughness);\n"+
"				o_gi.indirect.specular = lerp(env1, env0, blendLerp);\n"+
"			}\n"+
"			else\n"+
"			{\n"+
"				o_gi.indirect.specular = env0;\n"+
"			}\n"+
"		#else\n"+
"			o_gi.indirect.specular = env0;\n"+
"		#endif\n"+
"	}\n"+
"	o_gi.indirect.specular *= occlusion;\n"+
"\n"+
"	return o_gi;\n"+
"}*/\n"+
"\n"+
"/*UnityGI UnityGlobalIllumination (UnityGIInput data, half occlusion, half oneMinusRoughness, half3 normalWorld)\n"+
"{\n"+
"	return UnityGlobalIllumination (data, occlusion, oneMinusRoughness, normalWorld, true);	\n"+
"}*/\n"+
"\n"+
"#endif\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"// Default BRDF to use:\n"+
"#if !defined (UNITY_BRDF_PBS) // allow to explicitly override BRDF in custom shader\n"+
"	#if (SHADER_TARGET < 30) || defined(SHADER_API_PSP2)\n"+
"		// Fallback to low fidelity one for pre-SM3.0\n"+
"		#define UNITY_BRDF_PBS BRDF3_Unity_PBS\n"+
"	#elif defined(SHADER_API_MOBILE)\n"+
"		// Somewhat simplified for mobile\n"+
"		#define UNITY_BRDF_PBS BRDF2_Unity_PBS\n"+
"	#else\n"+
"		// Full quality for SM3+ PC / consoles\n"+
"		#define UNITY_BRDF_PBS BRDF1_Unity_PBS\n"+
"	#endif\n"+
"#endif\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"// BRDF for lights extracted from *indirect* directional lightmaps (baked and realtime).\n"+
"// Baked directional lightmap with *direct* light uses UNITY_BRDF_PBS.\n"+
"// For better quality change to BRDF1_Unity_PBS.\n"+
"// No directional lightmaps in SM2.0.\n"+
"\n"+
"#if !defined(UNITY_BRDF_PBS_LIGHTMAP_INDIRECT)\n"+
"	#define UNITY_BRDF_PBS_LIGHTMAP_INDIRECT BRDF2_Unity_PBS\n"+
"#endif\n"+
"#if !defined (UNITY_BRDF_GI)\n"+
"	#define UNITY_BRDF_GI BRDF_Unity_Indirect\n"+
"#endif\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"\n"+
"\n"+
"half3 BRDF_Unity_Indirect (half3 baseColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness, half3 normal, half3 viewDir, half occlusion, UnityGI gi)\n"+
"{\n"+
"	half3 c = 0;\n"+
"	#if defined(DIRLIGHTMAP_SEPARATE)\n"+
"		gi.indirect.diffuse = 0;\n"+
"		gi.indirect.specular = 0;\n"+
"\n"+
"		#ifdef LIGHTMAP_ON\n"+
"			c += UNITY_BRDF_PBS_LIGHTMAP_INDIRECT (baseColor, specColor, oneMinusReflectivity, oneMinusRoughness, normal, viewDir, gi.light2, gi.indirect).rgb * occlusion;\n"+
"		#endif\n"+
"		#ifdef DYNAMICLIGHTMAP_ON\n"+
"			c += UNITY_BRDF_PBS_LIGHTMAP_INDIRECT (baseColor, specColor, oneMinusReflectivity, oneMinusRoughness, normal, viewDir, gi.light3, gi.indirect).rgb * occlusion;\n"+
"		#endif\n"+
"	#endif\n"+
"	return c;\n"+
"}\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"\n"+
"\n"+
"\n"+
"// Surface shader output structure to be used with physically\n"+
"// based shading model.\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"// Metallic workflow\n"+
"\n"+
"struct SurfaceOutputStandard\n"+
"{\n"+
"	fixed3 Albedo;		// base (diffuse or specular) color\n"+
"	fixed3 Normal;		// tangent space normal, if written\n"+
"	half3 Emission;\n"+
"	half Metallic;		// 0=non-metal, 1=metal\n"+
"	half Smoothness;	// 0=rough, 1=smooth\n"+
"	half Occlusion;		// occlusion (default 1)\n"+
"	fixed Alpha;		// alpha for transparencies\n"+
"};\n"+
"\n"+
"half4 LightingStandard (SurfaceOutputStandard s, half3 viewDir, UnityGI gi)\n"+
"{\n"+
"	s.Normal = normalize(s.Normal);\n"+
"\n"+
"	half oneMinusReflectivity;\n"+
"	half3 specColor;\n"+
"	s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);\n"+
"\n"+
"	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)\n"+
"	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha\n"+
"	half outputAlpha;\n"+
"	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);\n"+
"\n"+
"	half4 c = UNITY_BRDF_PBS (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);\n"+
"	c.rgb += UNITY_BRDF_GI (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);\n"+
"	c.a = outputAlpha;\n"+
"	return c;\n"+
"}\n"+
"\n"+
"half4 LightingStandard_Deferred (SurfaceOutputStandard s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)\n"+
"{\n"+
"	half oneMinusReflectivity;\n"+
"	half3 specColor;\n"+
"	s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);\n"+
"\n"+
"	half4 c = UNITY_BRDF_PBS (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);\n"+
"	c.rgb += UNITY_BRDF_GI (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);\n"+
"\n"+
"	outDiffuseOcclusion = half4(s.Albedo, s.Occlusion);\n"+
"	outSpecSmoothness = half4(specColor, s.Smoothness);\n"+
"	outNormal = half4(s.Normal * 0.5 + 0.5, 1);\n"+
"	half4 emission = half4(s.Emission + c.rgb, 1);\n"+
"	return emission;\n"+
"}\n"+
"\n"+
"/*void LightingStandard_GI (\n"+
"	SurfaceOutputStandard s,\n"+
"	UnityGIInput data,\n"+
"	inout UnityGI gi)\n"+
"{\n"+
"#if UNITY_VERSION >= 520\n"+
"UNITY_GI(gi, s, data);\n"+
"#else\n"+
"	gi = UnityGlobalIllumination (data, s.Occlusion, s.Smoothness, s.Normal);\n"+
"#endif\n"+
"}*/\n"+
"\n"+
"//-------------------------------------------------------------------------------------\n"+
"// Specular workflow\n"+
"\n"+
"struct SurfaceOutputStandardSpecular\n"+
"{\n"+
"	fixed3 Albedo;		// diffuse color\n"+
"	fixed3 Specular;	// specular color\n"+
"	fixed3 Normal;		// tangent space normal, if written\n"+
"	half3 Emission;\n"+
"	half Smoothness;	// 0=rough, 1=smooth\n"+
"	half Occlusion;		// occlusion (default 1)\n"+
"	fixed Alpha;		// alpha for transparencies\n"+
"};\n"+
"\n"+
"half4 LightingStandardSpecular (SurfaceOutputStandardSpecular s, half3 viewDir, UnityGI gi)\n"+
"{\n"+
"	s.Normal = normalize(s.Normal);\n"+
"\n"+
"	// energy conservation\n"+
"	half oneMinusReflectivity;\n"+
"	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);\n"+
"\n"+
"	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)\n"+
"	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha\n"+
"	half outputAlpha;\n"+
"	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);\n"+
"\n"+
"	half4 c = UNITY_BRDF_PBS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);\n"+
"	c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);\n"+
"	c.a = outputAlpha;\n"+
"	return c;\n"+
"}\n"+
"\n"+
"half4 LightingStandardSpecular_Deferred (SurfaceOutputStandardSpecular s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)\n"+
"{\n"+
"	// energy conservation\n"+
"	half oneMinusReflectivity;\n"+
"	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);\n"+
"\n"+
"	half4 c = UNITY_BRDF_PBS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);\n"+
"	c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);\n"+
"\n"+
"	outDiffuseOcclusion = half4(s.Albedo, s.Occlusion);\n"+
"	outSpecSmoothness = half4(s.Specular, s.Smoothness);\n"+
"	outNormal = half4(s.Normal * 0.5 + 0.5, 1);\n"+
"	half4 emission = half4(s.Emission + c.rgb, 1);\n"+
"	return emission;\n"+
"}\n"+
"\n"+
"/*void LightingStandardSpecular_GI (\n"+
"	SurfaceOutputStandardSpecular s,\n"+
"	UnityGIInput data,\n"+
"	inout UnityGI gi)\n"+
"{\n"+
"	gi = UnityGlobalIllumination (data, s.Occlusion, s.Smoothness, s.Normal);\n"+
"}*/\n"+
"\n"+
"#endif // UNITY_PBS_LIGHTING_INCLUDED			\n"+
"			\n";
		}
		#endif
		if (SG.UsedNoise==true)
		{
			//Improved Perlin Noise!
			shaderCode+=@"
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
}";
			
			shaderCode+=@"
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
}";

shaderCode+=@"
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
";
shaderCode+=@"
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
";

shaderCode+=@"
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
";
			
		}
		if (SG.UsedNormalBlend==true)
		{
			shaderCode += "float3 NormalBlend(float3 Tex1,float3 Tex2){\n"+
			//"float3 t = Tex1*float3( 2,  2, 2) + float3(-1, -1,  0);\n"+
			//"float3 u = Tex2*float3(-2, -2, 2) + float3( 1,  1, -1);\n"+
			//"float3 r = t*dot(t, u)/t.z - u;\n"+
			"return normalize(float3(Tex1.xy + Tex2.xy, Tex1.z*Tex2.z));\n"+//normalize(float3(Tex1.xy + Tex2.xy, Tex1.z));\n"+
			"}\n";
		}
		foreach(string Ty in ShaderSandwich.EffectsList){
			bool IsUsed = false;
			foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
				foreach(ShaderEffect SE in SL.LayerEffects)
				{
					if (Ty==SE.TypeS&&SE.Visible)
					IsUsed = true;
				}
			}
			if (IsUsed){
				ShaderEffect NewEffect = ShaderEffect.CreateInstance<ShaderEffect>();
				NewEffect.ShaderEffectIn(Ty);			
				shaderCode+=NewEffect.Function+"\n";
			}
		}
		return shaderCode;
	}

	public string GCVertexInternal(ShaderGenerate SG, ShaderPass SP, float Dist){
		string shaderCode = "";
		shaderCode+="	float SSShellDepth = "+Dist.ToString()+";\n";
		
		if (Dist>0){
			string Disp = "";
			if (ShellsDistance.Get()==ShellsDistance.Float.ToString()){
				if (ShellsEase.Get()!="1"){
					if (ShellsEase.Get()==ShellsEase.Float.ToString())
						Disp=(ShellsDistance.Float*Mathf.Pow(Dist,ShellsEase.Float)).ToString();
					else
						Disp=(ShellsDistance.Float.ToString()+"*pow("+Dist.ToString()+","+ShellsEase.Get()+")");
				}
				else
				Disp = (ShellsDistance.Float*Dist).ToString();
			}
			else{
				if (ShellsEase.Get()=="1")
				Disp=ShellsDistance.Get()+"*"+Dist.ToString();
				else
				Disp=(ShellsDistance.Get()+"*"+"pow("+Dist.ToString().ToString()+","+ShellsEase.Get()+")");
			}
			shaderCode+="	v.vertex.xyz += v.normal*("+Disp+");\n";
		}

		if (SG.UsedMapGenerate==true)
		shaderCode+=
		"	half3 blend = pow((abs(UnityObjectToWorldNormal(v.normal))),5);\n	blend /= blend.x+blend.y+blend.z;\n";
		
		if (ShaderLayersVertex.Count>0){
			shaderCode+="\n	float4 Vertex = v.vertex;\n";
			
			foreach(ShaderLayerList SLL in ShaderLayersMasks)
			{
				if (SG.UsedMasks[SLL]>0&&!SLL.IsLighting.On){
					shaderCode+=SLL.GCVariable();
					shaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
				}
			}		
			
			shaderCode+=GCLayers(SG,"Vertex",ShaderLayersVertex,"Vertex","rgba","",false,true);
			shaderCode+="\n	v.vertex.rgb = Vertex;\n";
		}
		return shaderCode;
	}
	public string GCVertex(ShaderGenerate SG, ShaderPass SP, float Dist){
		SG.InVertex = true;
		string shaderCode = "";
		if (Dist>0||ShaderLayersVertex.Count>0||ShellsOn.On||TessellationOn.On)
shaderCode+=@"//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
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
};";
		if (TessellationOn.On){
		if (TessellationType.Type==0){
			shaderCode+=@"		float4 tess (appdata_min v0, appdata_min v1, appdata_min v2)
		{
			return "+TessellationQuality.Get()+@";
		}
";
		}
		if (TessellationType.Type==1){
			shaderCode+=@"		float4 tess (appdata_min v0, appdata_min v1, appdata_min v2)
		{
			float3 pos0 = mul(_Object2World,v0.vertex).xyz;
			float3 pos1 = mul(_Object2World,v1.vertex).xyz;
			float3 pos2 = mul(_Object2World,v2.vertex).xyz;
			float4 tess;
			tess.x = distance(pos1, pos2)*"+TessellationQuality.Get()+@";
			tess.y = distance(pos2, pos0)*"+TessellationQuality.Get()+@";
			tess.z = distance(pos0, pos1)*"+TessellationQuality.Get()+@";
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
";
			if (TessellationFalloff.Get()!="1")
			shaderCode+="			return pow(tess/50,"+TessellationFalloff.Get()+")*50;\n";
			else
			shaderCode+="			return tess;\n";
			shaderCode+="		}\n";
		}
		if (TessellationType.Type==2){
			shaderCode+=@"		float4 tess (appdata_min v0, appdata_min v1, appdata_min v2)
		{
";
			if (TessellationFalloff.Get()!="1")
			shaderCode+="			return pow(UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, (51-"+TessellationQuality.Get()+")*2)/50,"+TessellationFalloff.Get()+")*50;\n";
			else
			shaderCode+="			return UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, (51-"+TessellationQuality.Get()+")*2);\n";
			shaderCode+="		}";
		}
		}
		
		shaderCode+="\n";
		//"void vert (inout appdata_full v, out Input o) {\n"+
		if (Dist>0||ShaderLayersVertex.Count>0||ShellsOn.On||TessellationOn.On){
			shaderCode+="//Generate the vertex shader\n"+
			"void vert (inout appdata_min v) {\n";
			//"	UNITY_INITIALIZE_OUTPUT(Input, o);\n";
			//if (SG.GeneralUV=="Texcoord"&&SG.UsedGenericUV){
			//	shaderCode+="	o.Texcoord = v.texcoord;\n";
			//}
			shaderCode+=GCVertexInternal(SG,SP,Dist);
			shaderCode+="}\n";
		}
		SG.InVertex = false;
		SG.UsedBases.Clear();
		return shaderCode;
	}
	public string GCVertexMask(ShaderGenerate SG, ShaderPass SP, float Dist){
		SG.InVertex = true;
		string shaderCode = "";
		shaderCode+="float4 vert(appdata_base v) : POSITION {\n";
		
		shaderCode+=GCVertexInternal(SG,SP,Dist);
		shaderCode+="	return mul (UNITY_MATRIX_MVP, v.vertex);\n";
		shaderCode+="}\n";
		SG.InVertex = false;
		SG.UsedBases.Clear();
		return shaderCode;
	}
	public string GCInputs(ShaderGenerate SG){
		string shaderCode = "";
		//string WPAdd = "";
		//string WNAdd = "";
		//string TNBAdd = "";//World Normal World Pos
		int TexCoord = -1;
		string In = "";
		if (SG.UsedWorldPos==true){TexCoord+=1;
			In+="		float3 worldPos;\n";}

		if (SG.UsedVertexColors==true)
		In+="		float4 color: Color;\n";

		if (SG.UsedWorldNormals==true)
		{TexCoord+=1;
			In+="		float3 worldNormal;\n";}
		if (SG.UsedWorldRefl==true)
		{TexCoord+=1;
			In+="		float3 worldRefl;\n";}

		if (SG.UsedScreenPos==true)
		{TexCoord+=1;
			In+="		float4 screenPos;\n";}
		if (SG.UsedViewDir==true)
		{TexCoord+=1;
			In+="		float3 viewDir;\n";}
		if (SG.UsedVertex==true)
		{In+="		float4 color : COLOR;\n";}
		//UnityEngine.Debug.Log("TOOMANY"+SG.TooManyTexcoords.ToString());
		if (!SG.TooManyTexcoords){
			foreach (ShaderInput SI in ShaderInputs){
				if (SI.Type==0)
				{
					TexCoord+=1;
					if (SI.UsedMapType0==true||SI.UsedMapType1==true)
					{
						/*if ("uv"+SI.Get()==SG.GeneralUV&&SG.UsedGenericUV){
							In+="	float2 uv"+SI.Get()+ad+";\n";
						}
						else{
							In+="	float2 uv"+SI.Get()+ad+";\n";
						}*/
						if (SI.UsedMapType0==true)
						In+="		float2 uv"+SI.Get()+";\n";
						if (SI.UsedMapType1==true)
						In+="		float2 uv2"+SI.Get()+";\n";
					}
				}				
			}
		}
		TexCoord+=1;
		if (SG.GeneralUV=="uvTexcoord"&&SG.UsedGenericUV){
			//string ad = " : TEXCOORD"+TexCoord.ToString()+"";
			In += "		float2 uvTexcoord"+";\n";
			TexCoord+=1;
		}
		/////////////////////////////////////
		shaderCode+="//Create an Input struct, which lets us read different things that the Surface Shader creates. These are things like the view direction, world position etc.\n"+
		"	struct Input {\n";
		shaderCode+=In;
		
		bool InternalData = false;
		if (SG.UsedWorldNormals)
		InternalData = true;
		
		#if PRE_UNITY_5
			if (DiffuseLightingType.Type==4){
				InternalData  = true;
			}
		#endif
		if (InternalData)
		shaderCode+="		INTERNAL_DATA\n";
		shaderCode+="	};\n";	

		return shaderCode;
	}	
	public string GCLighting(ShaderGenerate SG,ShaderPass SP){
		string shaderCode = "";
		string TempStr1;
		string TempStr2;
		string NormalizeNormals = "";
		//if (SG.UsedWorldNormals==false)
		//NormalizeNormals="	s.Normal = normalize(s.Normal);\n";
		string DiffusePart = "";
		string SpecularPart = "";
		string CustomPartD = "";
		string CustomPartS = "";
		string CustomPartA = "";
		string CustomPart = "";
		string Multiplier = "";
		ShaderLayersLightingAll.CodeName="123ASD$#%";
		ShaderLayersLightingAll.NameUnique.Text="LightingAllDiffuse";
		ShaderLayersLightingAll.EndTag.Text="rgba";
		ShaderLayersLightingDiffuse.CodeName="123ASD$#%";
		ShaderLayersLightingDiffuse.NameUnique.Text="LightingDiffuse";
		ShaderLayersLightingDiffuse.EndTag.Text="rgba";
		ShaderLayersLightingSpecular.CodeName="123ASD$#%";
		ShaderLayersLightingSpecular.NameUnique.Text="LightingSpecular";
		ShaderLayersLightingSpecular.EndTag.Text="rgba";
		CustomPartD = GCLayers(SG,"LightingDiffuse",ShaderLayersLightingAll,"c","rgba","",false,true).Replace("123ASD$#%","c");
		CustomPartD += GCLayers(SG,"LightingDiffuse",ShaderLayersLightingDiffuse,"c","rgba","",false,true).Replace("123ASD$#%","c");
		if (SpecularOn.On){
			//ShaderLayersLightingAll.CodeName="spec";
			ShaderLayersLightingSpecular.EndTag.Text="rgb";
			ShaderLayersLightingAll.EndTag.Text="rgb";
			ShaderLayersLightingAll.NameUnique.Text="LightingAllSpecular";
			CustomPartS = GCLayers(SG,"LightingAllSpecular",ShaderLayersLightingAll,"Spec","rgb","",false,true).Replace("123ASD$#%","Spec");
			CustomPartS += GCLayers(SG,"LightingSpecular",ShaderLayersLightingSpecular,"Spec","rgb","",false,true).Replace("123ASD$#%","Spec");
			if (DiffuseLightingType.Type!=5)
			CustomPart = CustomPartD+"\n"+CustomPartS+"\nc.rgb = c.rgb*s.Albedo+Spec;\n";
			else
			CustomPart = CustomPartD+"\n"+CustomPartS+"\n";
		}
		else{
			if (DiffuseLightingType.Type!=5)
			CustomPart = CustomPartD+"\n"+CustomPartS+"\nc.rgb = c.rgb*s.Albedo;\n";
			else
			CustomPart = CustomPartD+"\n"+CustomPartS+"\n\n";
		}
		ShaderLayersLightingAll.NameUnique.Text="LightingDirect";
		ShaderLayersLightingAmbient.EndTag.Text="rgb";
		ShaderLayersLightingAmbient.CodeName="gi.indirect.diffuse";
		ShaderLayersLightingAmbient.NameUnique.Text="LightingIndirect";
		CustomPartA = GCLayers(SG,"LightingIndirect",ShaderLayersLightingAmbient,"gi.indirect.diffuse","rgb","",false,true).Replace("123ASD$#%","gi.indirect.diffuse");
		if ((DiffuseLightingType.Type==0||DiffuseLightingType.Type==4)&&UsesShellLighting(SP))
		{
			DiffusePart = NormalizeNormals;
			if (DiffuseNormals.Get()=="1")
			DiffusePart += "	half NdotL = 1; //Disabled using normals, so just set this to 1.\n";
			else if (DiffuseNormals.Get()=="0")
			DiffusePart += "	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.\n";
			else
			DiffusePart += "	half NdotL = lerp(max (0, dot (s.Normal, lightDir)),1,"+DiffuseNormals.Get()+"); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source. Finally, we blend this with the default value of 1 (Due to no normals being turned up)\n";
			
			DiffusePart += "	half4 c;\n"+
			"	c.rgb = lightColor * atten * NdotL"+Multiplier+"; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.\n"+
			"	c.a = s.Alpha; //Set the output alpha to the surface Alpha.\n";
		}			//#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
		if ((DiffuseLightingType.Type==1)&&UsesShellLighting(SP))
		{

			TempStr1 = DiffuseSetting1.Get();
			DiffusePart=NormalizeNormals+
			"	\n"+
			"	half roughness2=("+TempStr1+"*2)*("+TempStr1+"*2);\n"+
			"	half2 AandB = roughness2/(roughness2 + float2(0.33,0.09));//Computing some constants\n"+
			"	half2 oren_nayar = float2(1, 0) + float2(-0.5, 0.45) * AandB;\n"+
			"	\n"+
			"	//Theta and phi\n"+
			"	half2 cos_theta = saturate(float2(dot(s.Normal,lightDir),dot(s.Normal,viewDir)));\n"+
			"	half2 cos_theta2 = cos_theta * cos_theta;\n"+
			"	half sin_theta = sqrt((1-cos_theta2.x)*(1-cos_theta2.y));\n"+
			"	half3 light_plane = normalize(lightDir - cos_theta.x*s.Normal);\n"+
			"	half3 view_plane = normalize(viewDir - cos_theta.y*s.Normal);\n"+
			"	half cos_phi = saturate(dot(light_plane, view_plane));\n"+
			"	 \n"+
			"	//composition\n"+
			"	half diffuse_oren_nayar = cos_phi * sin_theta / max(cos_theta.x, cos_theta.y);\n"+
			"	 \n"+
			"	half diffuse = cos_theta.x * (oren_nayar.x + oren_nayar.y * diffuse_oren_nayar);\n"+
			"	half4 c;\n"+
			"	c.rgb = lightColor.rgb*(max(0,diffuse) * atten"+Multiplier+");\n"+
			"	c.a = s.Alpha;\n";
		}		
		if (DiffuseLightingType.Type==2&&UsesShellLighting(SP))
		{
			DiffusePart=NormalizeNormals+
			"	half4 c;\n"+
			"	half3 Surf1 = lightColor.rgb * (max(0,dot (s.Normal, lightDir)) * atten"+Multiplier+");//Calculate lighting the standard way (See Diffuse lighting modes comments).\n";


			TempStr1 = DiffuseSetting1.Get();
			TempStr2 = DiffuseSetting2.Get();
			DiffusePart+="	half3 Surf2 = lightColor.rgb * (max(0,dot (-s.Normal, lightDir)* "+TempStr1+"/2.0 + "+TempStr1+"/2.0) * atten"+Multiplier+");//Calculate diffuse lighting with inverted normals while taking the Wrap Amount into consideration.\n"+
			"	c.rgb = Surf1+(Surf2*(0.8-abs(dot(normalize(s.Normal), normalize(lightDir))))*"+TempStr1+" * "+TempStr2+".rgb);//Combine the two lightings together, by adding the standard one with the inverted one.\n"+
			//"	c.rgb = 1-dot(normalize(s.Normal), normalize(lightDir));\n"+
			//"	c.rgb = Surf1+(Surf2*"+TempStr1+" * "+TempStr2+".rgb);\n"+
			"	c.a = s.Alpha;\n";
		}
		if (DiffuseLightingType.Type==5&&UsesShellLighting(SP))
		{
			DiffusePart=NormalizeNormals+
			"	half4 c;\n"+
			"	//Just pass the color and alpha without adding any lighting.\n"+
			"	c.rgb = float3(1,1,1);\n"+
			"	c.a = s.Alpha;\n";
		}	
		if (DiffuseLightingType.Type==3||!UsesShellLighting(SP))
		{
			DiffusePart=NormalizeNormals+
			"	half4 c;\n"+
			"	//Just pass the color and alpha without adding any lighting.\n"+
			"	c.rgb = float3(1,1,1);\n"+
			"	c.a = s.Alpha;\n";
		}		


		if (SpecularOn.On&&UsesShellLighting(SP))
		{
			TempStr1 = SpecularOffset.Get();
			if (TempStr1=="0")
			TempStr1 = "";
			else
			TempStr1 = "+float3(sin((float)"+TempStr1+"),cos((float)"+TempStr1+"),tan((float)"+TempStr1+"))";
			SpecularPart="	float3 Spec;\n";
			if (SpecularLightingType.Type==0)
			{
				SpecularPart+="	half3 h = normalize (lightDir + viewDir"+TempStr1+");	\n"+	
				"	float nh = max (0, dot (s.Normal, h));\n"+
				"	Spec = pow (nh, s.Smoothness*128.0) * s.Specular;\n";
			}		
			if (SpecularLightingType.Type==1)
			{
				SpecularPart+="	Spec = (dot(reflect(-lightDir, s.Normal),viewDir"+TempStr1+"));\n"+	
				"	Spec = pow(max(0.0,Spec),s.Smoothness*128.0) * s.Specular;\n";
			}		
			if (SpecularLightingType.Type==2)
			{
				SpecularPart+="	Spec = abs(dot(s.Normal,reflect(-lightDir, -viewDir"+TempStr1+")));\n"+
				//"Spec = Spec;\n"+
				"	Spec = (half3(1.0f,1.0f,1.0f)-(pow(sqrt(Spec),2 - s.Smoothness)));\n"+
				"	Spec = saturate(Spec)*s.Specular;";
			}					
			SpecularPart+="	Spec = Spec * atten * 2 * lightColor.rgb;\n";

			//TempStr1 = SpecularColor.Get();				
			//SpecularPart+="	Spec = Spec * "+TempStr1+".rgb;\n";
			if (SpecularEnergy.On==true)
			SpecularPart+="	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/("+(9.0f*3.14f).ToString()+"))/9.0f);\n";
			//SpecularPart+="	c.rgb+=Spec;\n";
		}
		bool U4 = false;
		#if PRE_UNITY_5
		U4 = true;
		#endif
		if (!(GCLightingName(SP)=="CLPBR_Standard"&&U4)){
			shaderCode+="//Generate simpler lighting code:\n"+
			"half4 Lighting"+GCLightingName(SP)+" (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {\n";
			shaderCode+="	half3 SSlightColor = _LightColor0.rgb;\n";
			shaderCode+="	half3 lightColor = _LightColor0.rgb;\n";
			shaderCode+="	half3 SSnormal = s.Normal;\n"+
			"	half3 SSalbedo = s.Albedo;\n"+
			"	half3 SSspecular = s.Specular;\n"+
			"	half3 SSemission = s.Emission;\n"+
			"	half SSalpha = s.Alpha;\n";
			
			foreach(ShaderLayerList SLL in ShaderLayersMasks)
			{
				if (SG.UsedMasks[SLL]>0&&SLL.IsLighting.On){
					shaderCode+=SLL.GCVariable();
					shaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
				}
			}
			shaderCode+=DiffusePart;
			shaderCode+=SpecularPart;
			shaderCode+=CustomPart;
			shaderCode += "	\n"+
			"	return c;\n"+
			"}\n";
		}
		shaderCode+="#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED\n";
			if (!U4)
			shaderCode+="#include \"UnityPBSLighting.cginc\" //Include some PBS stuff.\n";
		if (SG.GI&&GCLightingName(SP)!="CLUnlit"&&GCLightingName(SP)!="CLPBR_Standard"){
			shaderCode+="//Generate lighting code for each GI part:\n"+
			"half4 Lighting"+GCLightingName(SP)+"Light (CSurfaceOutput s, half3 viewDir, UnityLight light) {\n";
			shaderCode+="	half3 SSlightColor = _LightColor0;\n"+
			"	half3 lightColor = _LightColor0;\n"+
			"	half3 lightDir = light.dir;\n"+
			"	half3 atten = light.color/_LightColor0;\n"+
			"	half3 SSnormal = s.Normal;\n"+
			"	half3 SSalbedo = s.Albedo;\n"+
			"	half3 SSspecular = s.Specular;\n"+
			"	half3 SSemission = s.Emission;\n"+
			"	half SSalpha = s.Alpha;\n";
			//if (SG.Temp){
				foreach(ShaderLayerList SLL in ShaderLayersMasks)
				{
					if (SG.UsedMasks[SLL]>0&&SLL.IsLighting.On){
						shaderCode+=SLL.GCVariable();
						shaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
					}
				}
				shaderCode+=DiffusePart.Replace("* atten","");
				shaderCode+=SpecularPart.Replace("* atten","");
				shaderCode+=CustomPart.Replace("* atten","");
			//}
			/*else{
				shaderCode+=DiffusePart.Replace("* atten * 2","");
				shaderCode+=SpecularPart.Replace("* atten * 2","");
			}*/
			shaderCode += "\n"+
			"	return c;\n"+
			"}";
			shaderCode+="\n"+
			"//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.\n"+
			"half4 Lighting"+GCLightingName(SP)+" (CSurfaceOutput s, half3 viewDir, UnityGI gi) {\n"+
			"	half4 c;\n"+
			"	c = Lighting"+GCLightingName(SP)+"Light(s,viewDir,gi.light);\n"+
			"	#if defined(DIRLIGHTMAP_SEPARATE)\n"+
			"		#ifdef LIGHTMAP_ON\n"+
			"			c += Lighting"+GCLightingName(SP)+"Light(s,viewDir,gi.light2);\n"+
			"		#endif\n"+
			"		#ifdef DYNAMICLIGHTMAP_ON\n"+
			"			c += Lighting"+GCLightingName(SP)+"Light(s,viewDir,gi.light3);\n"+
			"		#endif\n"+
			"	#endif\n"+
			"	half3 SSlightColor = _LightColor0;\n"+
			"	half3 lightColor = _LightColor0;\n"+
			"	half3 lightDir = gi.light.dir;\n"+
			"	half3 atten = gi.light.color/_LightColor0;\n"+
			"	half3 SSnormal = s.Normal;\n"+
			"	half3 SSalbedo = s.Albedo;\n"+
			"	half3 SSspecular = s.Specular;\n"+
			"	half3 SSemission = s.Emission;\n"+
			"	half SSalpha = s.Alpha;\n";
			
			foreach(ShaderLayerList SLL in ShaderLayersMasks)
			{
				if (SG.UsedMasks[SLL]>0&&SLL.IsLighting.On){
					shaderCode+=SLL.GCVariable();
					shaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
				}
			}			
			
			//if (!SG.Temp)
			shaderCode += "\n"+
			"	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT\n"+
			CustomPartA+"\n"+
			"		c.rgb += s.Albedo * gi.indirect.diffuse;\n"+
			"	#endif\n";		
			shaderCode+="	return c;\n"+
			"}\n"+
			"";
			
			
			shaderCode+="\n//Some weird Unity stuff for GI calculation (I think?).\n"+
			"inline void Lighting"+GCLightingName(SP)+"_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){\n";
			if (SpecularOn.On){
				shaderCode+="#if UNITY_VERSION >= 520\n"+
							"	UNITY_GI(gi, s, data);\n"+
							"#else\n"+
							"	gi = UnityGlobalIllumination (data, 1.0, s.Smoothness, s.Normal,false);\n"+
							"#endif\n";
							//"}\n";
				
				
				
				//"	gi = UnityGlobalIllumination (data, 1.0, s.Smoothness, s.Normal, false);";
			}
			else{
				shaderCode+="#if UNITY_VERSION >= 520\n"+
							"	UNITY_GI(gi, s, data);\n"+
							"#else\n"+
							"	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);\n"+
							"#endif\n";
							//"}\n";
				
				
				//"	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal, false);";
			}
			shaderCode+="\n"+
			"}\n";
		}
		else
		if (GCLightingName(SP)=="CLPBR_Standard"){

			ShaderLayersLightingAll.CodeName="123ASD$#%";
			ShaderLayersLightingAll.EndTag.Text="rgb";
			ShaderLayersLightingAll.NameUnique.Text="LightingSpecular";
			ShaderLayersLightingDiffuse.CodeName="123ASD$#%";
			ShaderLayersLightingDiffuse.EndTag.Text="rgb";
			ShaderLayersLightingSpecular.CodeName="123ASD$#%";
			ShaderLayersLightingSpecular.EndTag.Text="rgb";
			ShaderLayersLightingAmbient.CodeName="123ASD$#%";
			ShaderLayersLightingAmbient.EndTag.Text="rgb";
			CustomPartS = GCLayers(SG,"LightingAllSpecular",ShaderLayersLightingAll,"specularColor","rgb","",false,true).Replace("123ASD$#%","specularColor")+GCLayers(SG,"LightingSpecular",ShaderLayersLightingSpecular,"specularColor","rgb","",false,true).Replace("123ASD$#%","specularColor");
			
			ShaderLayersLightingAll.NameUnique.Text="LightingDiffuse";
			CustomPartD = GCLayers(SG,"LightingAllDiffuse",ShaderLayersLightingAll,"lightColor","rgb","",false,true).Replace("123ASD$#%","lightColor")+GCLayers(SG,"LightingDiffuse",ShaderLayersLightingDiffuse,"lightColor","rgb","",false,true).Replace("123ASD$#%","lightColor");
			CustomPartA = GCLayers(SG,"LightingAmbient",ShaderLayersLightingAmbient,"gi.diffuse","rgb","",false,true).Replace("123ASD$#%","gi.diffuse");
			ShaderLayersLightingAll.NameUnique.Text="LightingDirect";
			shaderCode+="//Include a bunch of PBS Code from files UnityPBSLighting.cginc and UnityStandardBRDF.cginc for the purpose of custom lighting effects.\n";
			shaderCode+=@"
half4 BRDF1_Unity_PBSSS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{"+
			"\n	half3 SSlightColor = _LightColor0;\n"+
			"	half3 lightDir = light.dir;\n"+
			"	half3 atten = light.color/_LightColor0;\n"+
			"	half3 SSnormal = normal;\n"+
			"	half3 SSalbedo = diffColor;\n"+
			"	half3 SSspecular = specColor;\n"+
			"	half3 SSemission = float3(0,0,0);\n"+
			"	half SSalpha = 1;\n";
	foreach(ShaderLayerList SLL in ShaderLayersMasks)
	{
		if (SG.UsedMasks[SLL]>0&&SLL.IsLighting.On){
			shaderCode+=SLL.GCVariable();
			shaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
		}
	}
shaderCode+=@"
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
";
	if (U4)
	shaderCode+=@"half specularTerm = max(0, (V * D * nl));// Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)";
	else
	shaderCode+=@"half specularTerm = max(0, (V * D * nl) * unity_LightGammaCorrectionConsts_PIDiv4);// Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)";
	
	shaderCode+=@"
	half diffuseTerm = disneyDiffuse * nl;
	
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
	";
	
	if (U4)
	shaderCode+=@"half3 lightColor = (light.color * diffuseTerm);";
	else
	shaderCode+="\n"+CustomPartA+@"
	half3 lightColor = (gi.diffuse + light.color * diffuseTerm);";
	
	shaderCode+=@"
	half3 specularColor = specularTerm*light.color * FresnelTerm (specColor, lh)+(gi.specular * FresnelLerp (specColor, grazingTerm, nv));
"+CustomPartD+"\n"+CustomPartS+@"
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
	half3 halfDir = normalize (light.dir + viewDir);"+
			"\n	half3 SSlightColor = _LightColor0;\n"+
			"	half3 lightDir = light.dir;\n"+
			"	half3 atten = light.color/_LightColor0;\n"+
			"	half3 SSnormal = normal;\n"+
			"	half3 SSalbedo = diffColor;\n"+
			"	half3 SSspecular = specColor;\n"+
			"	half3 SSemission = float3(0,0,0);\n"+
			"	half SSalpha = 1;\n";
	foreach(ShaderLayerList SLL in ShaderLayersMasks)
	{
		if (SG.UsedMasks[SLL]>0&&SLL.IsLighting.On){
			shaderCode+=SLL.GCVariable();
			shaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
		}
	}
	shaderCode+=@"

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
	";
	
	if (!U4)
	shaderCode+="\n"+CustomPartA+@"
	lightColor += gi.diffuse;
	";
	
	shaderCode+=@"
"+CustomPartD+"\n"+CustomPartS+@"
	
	half3 color =	diffColor* lightColor + specularColor;

	return half4(color, 1);
}"+

  /*  half3 color =	diffColor * lightColor
                    + specularColor;
    half3 color =	(diffColor + specular * specColor) * light.color * nl
    				+ gi.diffuse * diffColor
					+ gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);*/
@"

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
	half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp"+
			"\n	half3 SSlightColor = _LightColor0;\n"+
			"	half3 lightDir = light.dir;\n"+
			"	half3 atten = light.color/_LightColor0;\n"+
			"	half3 SSnormal = normal;\n"+
			"	half3 SSalbedo = diffColor;\n"+
			"	half3 SSspecular = specColor;\n"+
			"	half3 SSemission = float3(0,0,0);\n"+
			"	half SSalpha = 1;\n";
	foreach(ShaderLayerList SLL in ShaderLayersMasks)
	{
		if (SG.UsedMasks[SLL]>0&&SLL.IsLighting.On){
			shaderCode+=SLL.GCVariable();
			shaderCode+=GCLayers(SG,SLL.Name.Text,SLL,SLL.CodeName,SLL.EndTag.Text,SLL.Function,false,true);
		}
	}
	shaderCode+=@"

	half3 reflDir = reflect (viewDir, normal);
	half3 halfDir = normalize (light.dir + viewDir);

	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);

	// Vectorize Pow4 to save instructions
	half2 rlPow4AndFresnelTerm = Pow4 (half2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
	half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
	half fresnelTerm = rlPow4AndFresnelTerm.y;
";
if (U4)
shaderCode+="#if 0 // Lookup texture to save instructions\n";
else
shaderCode+="#if 1 // Lookup texture to save instructions\n";

shaderCode+=@"
	half specular = tex2D(unity_NHxRoughness, half2(rlPow4, 1-oneMinusRoughness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;
#else
	half roughness = 1-oneMinusRoughness;
	half n = RoughnessToSpecPower (roughness) * .25;
	half specular = (n + 2.0) / (2.0 * UNITY_PI * UNITY_PI) * pow(dot(reflDir, light.dir), n) * nl;// / unity_LightGammaCorrectionConsts_PI;
	//half specular = (1.0/(UNITY_PI*roughness*roughness)) * pow(dot(reflDir, light.dir), n) * nl;// / unity_LightGammaCorrectionConsts_PI;
#endif
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));

	half3 lightColor = light.color * nl;
	half3 specularColor = specular * specColor * lightColor + gi.specular * lerp (specColor, grazingTerm, fresnelTerm);";
	
	if (!U4)
	shaderCode+="\n"+CustomPartA+"\nlightColor += gi.diffuse;	\n";
	
	shaderCode+=""+CustomPartD+"\n"+CustomPartS+@"
	
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
#endif";
if (U4)
shaderCode+="\n//Generate lighting code similar to the Unity Standard Shader. Not gonna deny, I have no clue how much of it works.\n"+
			"half4 LightingCLPBR_Standard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {\n"+@"
	UnityLight l;
	l.color = _LightColor0.rgb*atten;
	/*#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(_WorldSpaceLightPos0.xyz - s.worldPos);
		#else
			lightDir = _WorldSpaceLightPos0.xyz;
	#endif*/
	l.dir = (lightDir);
	l.ndotl = max (0, dot (s.Normal, l.dir));
	UnityIndirect l2;
	l2.diffuse = s.worldRefl*0.0001;
	float mip = pow(1-s.Smoothness,3.0/4.0) * UNITY_SPECCUBE_LOD_STEPS;
	half4 rgbm = texCUBElod(_Cube, float4(s.worldRefl,mip));
	l2.specular = rgbm;//texCUBELOD(_Cube,s.worldRefl);
	#if !defined(UNITY_PASS_FORWARDBASE)
		l2.specular = float3(0,0,0);
	#endif
	//l2.diffuse = 0;
	half3 lightColor = _LightColor0.rgb;//CSurfaceOutput s, half3 viewDir, UnityGI gi){
	s.Normal = normalize(s.Normal);
	// energy conservation
	half oneMinusReflectivity;
	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);
	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);
	half4 c = UNITY_BRDF_PBSSS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, l, l2);
	//c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
	c.a = outputAlpha;
	//c.rgb *= s.worldRefl;
	c.rgb+=l2.diffuse;
	return c;
	}
";
else
shaderCode+="\n//Generate lighting code similar to the Unity Standard Shader. Not gonna deny, I have no clue how much of it works.\n"+
			"half4 LightingCLPBR_Standard (CSurfaceOutput s, half3 viewDir, UnityGI gi){\n"+
			"	s.Normal = normalize(s.Normal);\n"+
			"	// energy conservation\n"+
			"	half oneMinusReflectivity;\n"+
			"	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);\n"+

			"	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)\n"+
			"	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha\n"+
			"	half outputAlpha;\n"+
			"	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);\n"+

			"	half4 c = UNITY_BRDF_PBSSS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);\n"+
			"	c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);\n"+
			"	c.a = outputAlpha;\n"+
			"	return c;\n"+
			"}\n";
		if (!U4){
		shaderCode+="\n"+
		"inline void Lighting"+GCLightingName(SP)+"_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){\n";
			if (SpecularOn.On){
				shaderCode+="#if UNITY_VERSION >= 520\n"+
							"	UNITY_GI(gi, s, data);\n"+
							"#else\n"+
							"	gi = UnityGlobalIllumination (data, 1.0, s.Smoothness, s.Normal);\n"+
							"#endif\n";
							//"}\n";
			}
			else{
				shaderCode+="#if UNITY_VERSION >= 520\n"+
							"	UNITY_GI(gi, s, data);\n"+
							"#else\n"+
							"	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal);\n"+
							"#endif\n";
							//"}\n";
			}
		shaderCode+="\n"+
		"}\n";
		}
		}
		shaderCode+="#endif\n";
		return shaderCode;
	}
	public void GCResetInputs(){
		foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
			foreach(ShaderVar SV in SL.ShaderVars){
				if (SV.UseInput==false&&(SV.CType==Types.Texture||SV.CType==Types.Cubemap)){
					SV.Input = null;
				}
			}
		}	

	}
	public void GenerateUsed(ShaderGenerate SG){
		//UsedScreenPos
		if (ParallaxHeight.Get()!="0"&&ParallaxOn.On){
			SG.UsedParallax = true;
			TechShaderTarget.Float = Mathf.Max(TechShaderTarget.Float,3);
			
			if (ParallaxSilhouetteClipping.On)
			SG.UsedGenericUV = true;
			
			SG.UsedNormals = true;
		}
		foreach (ShaderLayerList SL in ShaderLayersMasks){
			//SL.GCUses = 0;
			SG.UsedMasks.Add(SL,0);
		}
		foreach (ShaderLayerList SL in ShaderLayersMasks){
			foreach (ShaderLayerList SL2 in ShaderLayersMasks){
				if (SL!=SL2){
					if (ShaderUtil.CodeName(SL.Name.Text)==ShaderUtil.CodeName(SL2.Name.Text)){
						SL.Name.Text += " 2";
					}
				}
			}
		}
		foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
			if (SL.Stencil.Obj!=null){
				//((ShaderLayerList)SL.Stencil.Obj).GCUses+=1;
				//if (SG.UsedMasks.ContainsKey(((ShaderLayerList)SL.Stencil.Obj)))
				SG.UsedMasks[((ShaderLayerList)SL.Stencil.Obj)]++;
			}
			foreach(ShaderEffect SE in SL.LayerEffects)
			{
				if (ShaderEffect.GetMethod(SE.TypeS,"SetUsed")!=null&&SE.Visible){
					var meth = ShaderEffect.GetMethod(SE.TypeS,"SetUsed");
					
					if (meth.GetParameters().Length==1)
					meth.Invoke(null,new object[]{SG});
					else
					meth.Invoke(null,new object[]{SG,SE});
				}
			}
		}
		foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
		//UnityEngine.Debug.Log(SL.Parent);
		//if (SG.UsedMasks.ContainsKey(SL.Parent))
		//UnityEngine.Debug.Log(SG.UsedMasks[SL.Parent]);
		if (!SL.Parent.IsMask.On||SG.UsedMasks[SL.Parent]>0){
		//UnityEngine.Debug.Log("WentAhead");
			bool NameCollision = false;
			while (1==1){
				NameCollision = false;
				foreach (ShaderLayer SL2 in ShaderUtil.GetAllLayers()){
					if (ShaderUtil.CodeName(SL.Name.Text)==ShaderUtil.CodeName(SL2.Name.Text)&&SL!=SL2){
						NameCollision = true;
						SL.Name.Text += " 2";
					}
				}
				if (NameCollision == false)
				break;
			}
			
			SL.SampleCount = 0;
			
			if (SL.MapType.Type==(int)ShaderMapType.UVMap1)
			SG.UsedMapType0 = true;

			if (SL.MapType.Type==(int)ShaderMapType.UVMap2){
				SG.UsedMapType1 = true;
				if (SL.Image.Input!=null)
				SL.Image.Input.UsedMapType1 = true;
			}

			if (SL.MapType.Type==(int)ShaderMapType.Reflection)
			SG.UsedWorldRefl = true;
			if (SL.MapType.Type==(int)ShaderMapType.Position)
			SG.UsedWorldPos = true;
			if (SL.MapType.Type==(int)ShaderMapType.Position)
				SG.UsedWorldPos = true;
			if (SL.MapType.Type==(int)ShaderMapType.Direction){
				SG.UsedWorldRefl = true;
				SG.UsedWorldNormals = true;
			}

			if (SL.MapType.Type==(int)ShaderMapType.Generate){
				SG.UsedWorldPos = true;
				SG.UsedMapGenerate = true;
				SG.UsedWorldNormals = true;
			}

			if (SL.MapType.Type==(int)ShaderMapType.View||SL.UseFadeout.On)
			SG.UsedScreenPos = true;

			if (SL.MapType.Type==(int)ShaderMapType.RimLight)
			SG.UsedViewDir = true;

			if (SL.LayerType.Type==(int)LayerTypes.VertexColors)
			SG.UsedVertex = true;
			if (SL.LayerType.Type==(int)LayerTypes.Noise)
			SG.UsedNoise = true;
			if (SL.LayerType.Type==(int)LayerTypes.GrabDepth){
				if (SL.SpecialType.Type==0)
				SG.UsedGrabPass = true;
				else
				SG.UsedDepthTexture = true;
			}

			if ((SL.MapType.Type==(int)ShaderMapType.UVMap1||SL.MapType.Type==(int)ShaderMapType.UVMap2)&&SL.LayerType.Type!=(int)LayerTypes.Texture&&SL.UsesMap())
			SG.UsedGenericUV = true;


			foreach(ShaderEffect SE in SL.LayerEffects)
			{
				if (ShaderEffect.GetMethod(SE.TypeS,"SetUsed")!=null&&SE.Visible){
					var meth = ShaderEffect.GetMethod(SE.TypeS,"SetUsed");
					
					if (meth.GetParameters().Length==1)
					meth.Invoke(null,new object[]{SG});
					else
					meth.Invoke(null,new object[]{SG,SE});
				}
			}

			//if (SL.IsVertex&&(VertexMasks)(int)SL.VertexMask.Float==VertexMasks.View)
			//	SG.UsedWorldPos = true;
			if (SG.UsedParallax&&SL.GetDimensions()==3)
				SG.UsedWorldNormals = true;
				
		}
		}
		if (ShaderLayersShellNormal.Count>0)
		SG.UsedShellsNormals = true;
		if (ShaderLayersNormal.Count>0)
		SG.UsedNormals = true;
		
		if (SpecularOn.On)
		SG.UsedViewDir = true;

		SG.UsedViewDir = true;
		
		if (ShaderLayersNormal.Count>0&&SG.UsedWorldRefl)
		SG.UsedWorldNormals = true;
		string GenTex = "uvTexcoord";
		float CheckIfTexcoord = 0;
		foreach (ShaderInput SI in ShaderInputs){
			if (SI.Type==0)
				CheckIfTexcoord+=0.5f;
		}
		if (SG.UsedGenericUV)
		SG.GeneralUV = GenTex;
		
		
		if (SG.UsedWorldRefl==true)
			CheckIfTexcoord+=1;
		if (SG.UsedScreenPos==true)
			CheckIfTexcoord+=1;
		if (SG.UsedViewDir==true)
			CheckIfTexcoord+=1;
		#if PRE_UNITY_5
		if (DiffuseLightingType.Type==4){
			SG.UsedWorldRefl = true;
		}
		#endif
		
		if (CheckIfTexcoord>3){
			SG.TooManyTexcoords = true;
			SG.UsedGenericUV = true;
			SG.GeneralUV = GenTex;
		}
	}
	/*public string GenerateBlendMode(ShaderPass SP){
		string shaderCode = "";
		if (ShaderPassBase(SP)||(ShaderPassShells(SP)&&ShellsBlendMode.Type==0))
		{
			if (TransparencyOn.On==false)
			{
				if (ShaderPassBase(SP))
				shaderCode+="";	
				else
				shaderCode+="Blend One One";	
			}
			if (TransparencyType.Type == 1&&TransparencyOn.On)
			{
				if (ShaderPassBase(SP))
				shaderCode+="Blend SrcAlpha OneMinusSrcAlpha";	
				else
				shaderCode+="Blend SrcAlpha One";	
			}
		}
		if (ShaderPassBase(SP)||(ShaderPassShells(SP)&&ShellsBlendMode.Type==1))
		{
			if (ShaderPassLight(SP))
			shaderCode+="Blend One One";	
			else
			shaderCode+="Blend One One";
		}		
		return shaderCode;		
	}*/
	public string GCGrabPass(ShaderGenerate SG){
		if (SG.UsedGrabPass){
			//sampler2D _GrabTexture;
			//float4 _GrabTexture_TexelSize;
			return "\nGrabPass {}\n";
		}
		return "";
	}
	public string GenerateCode(){
		return GenerateCode(false);
	}
	//public string GenerateCode(bool Temp){
	//	return GenerateCode_Real(Temp);
	//}
	public string GenerateCode(bool Temp){
	return GenerateCode(Temp,false);
	}
	public string GenerateCode(bool Temp,bool Wireframe){
		/*Stopwatch sw = new Stopwatch();
sw.Start();*/
		ShaderSandwich.Instance.Status = "Compiling Shader...";//"Generating Shader Code...";
		RecalculateAutoInputs();
		TechCull.Update(new string[]{"All","Front","Back"},new string[]{"","",""},new string[]{"Off","Back","Front"}); 
		//TechCull.Type = 1;
		string ShaderCode = "";
		SG = new ShaderGenerate();
		SG.Temp = Temp;
		SG.Wireframe = Wireframe;
		GenerateUsed(SG);

		ShaderCode+=GCTop(SG);

		ShaderCode+=CGProperties(SG);
		
		ShaderCode+=GCSubShader(SG);

		if (TransparencyReceive.On&&TransparencyOn.On)
		ShaderCode+="\nFallback Off\n}";
		else
		ShaderCode+="\nFallback \"VertexLit\"\n}";
		/*sw.Stop();
string ExecutionTimeTaken = string.Format("Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}",sw.Elapsed.Minutes,sw.Elapsed.Seconds,sw.Elapsed.TotalMilliseconds);
UnityEngine.Debug.Log(ExecutionTimeTaken);*/
		while (ShaderCode != ShaderCode.Replace(".rgb.rgb",".rgb"))
		ShaderCode = ShaderCode.Replace(".rgb.rgb",".rgb");
		while (ShaderCode != ShaderCode.Replace(".a.a",".a"))
		ShaderCode = ShaderCode.Replace(".a.a",".a");
		while (ShaderCode != ShaderCode.Replace(".xy.xy",".xy"))
		ShaderCode = ShaderCode.Replace(".xy.xy",".xy");
		
		ShaderCode = ShaderCode.Replace("\r\n","\n");
		ShaderCode = ShaderCode.Replace("\r","\n");
		return ShaderCode;
	}
	public Dictionary<string,ShaderVar> GetSaveLoadDict(){
		Dictionary<string,ShaderVar> D = new Dictionary<string,ShaderVar>();

		D.Add(ShaderName.Name,ShaderName);
		D.Add(DiffMode.Name,DiffMode);

		D.Add(TechLOD.Name,TechLOD);
		D.Add(TechCull.Name,TechCull);
		D.Add(TechShaderTarget.Name,TechShaderTarget);

		D.Add(MiscVertexRecalculation.Name,MiscVertexRecalculation);
		D.Add(MiscFog.Name,MiscFog);
		D.Add(MiscAmbient.Name,MiscAmbient);
		D.Add(MiscVertexLights.Name,MiscVertexLights);
		D.Add(MiscLightmap.Name,MiscLightmap);
		D.Add(MiscFullShadows.Name,MiscFullShadows);
		D.Add(MiscForwardAdd.Name,MiscForwardAdd);
		D.Add(MiscShadows.Name,MiscShadows);
		D.Add(MiscInterpolateView.Name,MiscInterpolateView);
		D.Add(MiscHalfView.Name,MiscHalfView);

		D.Add(DiffuseOn.Name,DiffuseOn);
		D.Add(DiffuseLightingType.Name,DiffuseLightingType);
		D.Add(DiffuseColor.Name,DiffuseColor);
		D.Add(DiffuseSetting1.Name,DiffuseSetting1);
		D.Add(DiffuseSetting2.Name,DiffuseSetting2);
		D.Add(DiffuseNormals.Name,DiffuseNormals);

		D.Add(SpecularOn.Name,SpecularOn);
		D.Add(SpecularLightingType.Name,SpecularLightingType);
		D.Add(SpecularHardness.Name,SpecularHardness);
		D.Add(SpecularColor.Name,SpecularColor);
		D.Add(SpecularEnergy.Name,SpecularEnergy);
		D.Add(SpecularOffset.Name,SpecularOffset);

		D.Add(EmissionOn.Name,EmissionOn);
		D.Add(EmissionColor.Name,EmissionColor);
		D.Add(EmissionType.Name,EmissionType);

		D.Add(TransparencyOn.Name,TransparencyOn);
		D.Add(TransparencyType.Name,TransparencyType);
		D.Add(TransparencyZWrite.Name,TransparencyZWrite);
		D.Add(TransparencyPBR.Name,TransparencyPBR);
		D.Add(TransparencyAmount.Name,TransparencyAmount);
		D.Add(TransparencyReceive.Name,TransparencyReceive);
		D.Add(TransparencyZWriteType.Name,TransparencyZWriteType);
		D.Add(BlendMode.Name,BlendMode);

		D.Add(ShellsOn.Name,ShellsOn);
		D.Add(ShellsCount.Name,ShellsCount);
		D.Add(ShellsDistance.Name,ShellsDistance);
		D.Add(ShellsEase.Name,ShellsEase);
		D.Add(ShellsTransparencyType.Name,ShellsTransparencyType);
		D.Add(ShellsTransparencyZWrite.Name,ShellsTransparencyZWrite);
		D.Add(ShellsCull.Name,ShellsCull);
		D.Add(ShellsZWrite.Name,ShellsZWrite);
		D.Add(ShellsUseTransparency.Name,ShellsUseTransparency);
		D.Add(ShellsBlendMode.Name,ShellsBlendMode);
		
		D.Add(ShellsTransparencyAmount.Name,ShellsTransparencyAmount);
		D.Add(ShellsLighting.Name,ShellsLighting);
		D.Add(ShellsFront.Name,ShellsFront);

		D.Add(ParallaxOn.Name,ParallaxOn);
		D.Add(ParallaxHeight.Name,ParallaxHeight);
		D.Add(ParallaxBinaryQuality.Name,ParallaxBinaryQuality);
		D.Add(ParallaxSilhouetteClipping.Name,ParallaxSilhouetteClipping);

		D.Add(TessellationOn.Name,TessellationOn);
		D.Add(TessellationType.Name,TessellationType);
		D.Add(TessellationQuality.Name,TessellationQuality);
		D.Add(TessellationFalloff.Name,TessellationFalloff);
		D.Add(TessellationSmoothingAmount.Name,TessellationSmoothingAmount);

		return D;
	}
	public string Save(){
		/*Stopwatch sw = new Stopwatch();
sw.Start();*/
		string S = "BeginShaderParse\n1.0\nBeginShaderBase\n";
		/*
S += ShaderLayersMasks.Count.ToString()+"#? Shader Layer Masks Count\n";
foreach (ShaderLayerList SLL in ShaderLayersMasks){
S += SLL.Save();
}*/
		foreach (ShaderInput SI in ShaderInputs){
			S+=SI.Save();
		}
		S += ShaderUtil.SaveDict(GetSaveLoadDict());
		

		
		foreach (ShaderLayerList SLL in GetShaderLayerLists()){
			S+=SLL.Save();
		}
		S += "EndShaderBase\n";
		S += "EndShaderParse";
		/*sw.Stop();
string ExecutionTimeTaken = string.Format("Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}",sw.Elapsed.Minutes,sw.Elapsed.Seconds,sw.Elapsed.TotalMilliseconds);
UnityEngine.Debug.Log(ExecutionTimeTaken);*/

		return S;
	}
	public string FileVersion = "";
	static public ShaderBase Load(StringReader S){
		ShaderBase SB = ShaderBase.CreateInstance<ShaderBase>();
		S.ReadLine();
		SB.FileVersion = S.ReadLine();
		S.ReadLine();
		
		ShaderSandwich.Instance.OpenShader = SB;
		var D = SB.GetSaveLoadDict();
		while(1==1){
			string Line =  S.ReadLine();
			if (Line!=null){
				if(Line=="EndShaderBase")break;

				if (Line.Contains("#!"))
				ShaderUtil.LoadLine(D,Line);
				else
				if (Line=="BeginShaderLayerList")
				ShaderLayerList.Load(S,SB.GetShaderLayerLists(),SB.ShaderLayersMasks);
				else
				if (Line=="BeginShaderInput")
				SB.ShaderInputs.Add(ShaderInput.Load(S));
			}
			else
			break;
		}
		if (SB.TechShaderTarget.Float==1f)
		SB.TechShaderTarget.Float = 3f;
		return SB;
	}
	
	public void CleanUp(){
		foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
			UEObject.DestroyImmediate(SL);
		}
		UEObject.DestroyImmediate(this);
	}
	public bool UsesShellLighting(ShaderPass SP){
		if (ShaderPassShells(SP)){
			if (ShellsLighting.On)
			return true;
			
			return false;
		}
		return true;
	}
	
}