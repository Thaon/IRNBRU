using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using SU = ShaderUtil;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
//Image Color Cubemap Float Range

public enum InputMainTypes {None,MainColor,MainTexture,BumpMap,MainCubemap,SpecularColor,Shininess,ReflectColor,Parallax,ParallaxMap,Cutoff,ShellDistance,TerrainControl,TerrainSplat0,TerrainSplat1,TerrainSplat2,TerrainSplat3,TerrainNormal0,TerrainNormal1,TerrainNormal2,TerrainNormal3};
public enum InputSpecialTypes {None,Time,TimeFast,TimeSlow,TimeVerySlow,SinTime,SinTimeFast,SinTimeSlow,CosTime,CosTimeFast,CosTimeSlow,ShellDepth,ParallaxDepth,ClampedSinTime,ClampedSinTimeFast,ClampedSinTimeSlow,ClampedCosTime,ClampedCosTimeFast,ClampedCosTimeSlow};
//[System.Serializable]
public class ShaderInput : ScriptableObject{// : UnityEngine.Object{
	public int Type;
	public string Name;
	public string VisName = "";
	//public ShaderVar Image = new ShaderVar("Texture2D");
	[XmlIgnore,NonSerialized] public Texture2D Image;
	public int ImageDefault;
	public string[] ImageDefaultNames = {"white","bump"};
	[DataMember]
	public ShaderColor Color =  new ShaderColor(0.8f,0.8f,0.8f,1f);
	//public ShaderVar Cube = new ShaderVar("Cubemap");
	[XmlIgnore,NonSerialized] public Cubemap Cube;
	
	public string ImageGUID = "";
	public bool NormalMap = false;
	public string CubeGUID = "";
	public float Number; 
	public float Range0; 
	public float Range1; 
	public bool UsedMapType6;
	public bool UsedMapType1;
	public bool UsedMapType0 = true;
	public bool InEditor = true;
	public InputMainTypes MainType = InputMainTypes.None;
	public InputSpecialTypes SpecialType = InputSpecialTypes.None;

	public bool AutoCreated = false;
	public int UsedCount = 0;
	
	public void Update(){
		float time = (float)EditorApplication.timeSinceStartup;   
		Vector4 vTime = new Vector4(( time / 20f)%1000f, time%1000f, (time*2f)%1000f, (time*3f)%1000f);
		Vector4 vCosTime = new Vector4( Mathf.Cos(time / 8f), Mathf.Cos(time/4f), Mathf.Cos(time/2f), Mathf.Cos(time));
		Vector4 vSinTime = new Vector4( Mathf.Sin(time / 8f), Mathf.Sin(time/4f), Mathf.Sin(time/2f), Mathf.Sin(time));
		float NewNumber = -97.5f;
		switch(SpecialType){
			case InputSpecialTypes.Time:
				NewNumber = vTime.y;break;
			case InputSpecialTypes.TimeFast:
				NewNumber = vTime.z;break;
			case InputSpecialTypes.TimeSlow:
				NewNumber = vTime.y/2f;break;
			case InputSpecialTypes.TimeVerySlow:
				NewNumber = vTime.y/6f;break;
			case InputSpecialTypes.SinTime:
				NewNumber = vSinTime.w;break;
			case InputSpecialTypes.SinTimeFast:
				NewNumber = Mathf.Sin(vTime.z);break;
			case InputSpecialTypes.SinTimeSlow:
				NewNumber = vSinTime.z;break;
			case InputSpecialTypes.CosTime:
				NewNumber = vCosTime.w;break;
			case InputSpecialTypes.CosTimeFast:
				NewNumber = Mathf.Cos(vTime.z);break;
			case InputSpecialTypes.CosTimeSlow:
				NewNumber = vCosTime.z;break;
				
			case InputSpecialTypes.ClampedSinTime:
				NewNumber = (vSinTime.w+1f)/2f;break;
			case InputSpecialTypes.ClampedSinTimeFast:
				NewNumber = (Mathf.Sin(vTime.z)+1f)/2f;break;
			case InputSpecialTypes.ClampedSinTimeSlow:
				NewNumber = (vSinTime.z+1f)/2f;break;
			case InputSpecialTypes.ClampedCosTime:
				NewNumber = (vCosTime.w+1f)/2f;break;
			case InputSpecialTypes.ClampedCosTimeFast:
				NewNumber = (Mathf.Cos(vTime.z)+1f)/2f;break;
			case InputSpecialTypes.ClampedCosTimeSlow:
				NewNumber = (vCosTime.z+1f)/2f;break;
		}
		if (NewNumber!=-97.5f){
			Number = NewNumber;
			
		}
		
	}
	
	public Texture2D ImageS(){
		if (Image==null)
			Image = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(ImageGUID),typeof(Texture2D));
		//Debug.Log(AssetDatabase.GUIDToAssetPath(ImageGUID));
		return Image;
	}
	public Cubemap CubeS(){
		if (Cube==null)
			Cube = (Cubemap)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(CubeGUID),typeof(Cubemap));
		return Cube;
	}
	public void OnEnable()
	{

	}
	public ShaderInput(){

	}
	public ShaderInput(bool A){
		AutoCreated = A;
	}
	public string Get(){
		string NewName = "SSS"+VisName;
		NewName = ShaderUtil.CodeName(NewName);
		if (MainType==InputMainTypes.MainColor)
		NewName = "Color";
		if (MainType==InputMainTypes.MainTexture)
		NewName = "MainTex";
		if (MainType==InputMainTypes.BumpMap)
		NewName = "BumpMap";
		if (MainType==InputMainTypes.MainCubemap)
		NewName = "Cube";
		if (MainType==InputMainTypes.SpecularColor)
		NewName = "SpecColor";
		if (MainType==InputMainTypes.Shininess)
		NewName = "Shininess";
		if (MainType==InputMainTypes.Cutoff)
		NewName = "Cutoff";
		if (MainType==InputMainTypes.ShellDistance)
		NewName = "ShellDistance";
		if (MainType==InputMainTypes.ReflectColor)
		NewName = "ReflectColor";
		if (MainType==InputMainTypes.Parallax)
		NewName = "Parallax";
		if (MainType==InputMainTypes.ParallaxMap)
		NewName = "ParallaxMap";
		if (MainType==InputMainTypes.TerrainControl)
		NewName = "Control";
		if (MainType==InputMainTypes.TerrainSplat0)
		NewName = "Splat0";
		if (MainType==InputMainTypes.TerrainSplat1)
		NewName = "Splat1";
		if (MainType==InputMainTypes.TerrainSplat2)
		NewName = "Splat2";
		if (MainType==InputMainTypes.TerrainSplat3)
		NewName = "Splat3";
		if (MainType==InputMainTypes.TerrainNormal0)
		NewName = "Normal0";
		if (MainType==InputMainTypes.TerrainNormal1)
		NewName = "Normal1";
		if (MainType==InputMainTypes.TerrainNormal2)
		NewName = "Normal2";
		if (MainType==InputMainTypes.TerrainNormal3)
		NewName = "Normal3";
		return "_"+NewName;
	}
	public Dictionary<string,ShaderVar> GetSaveLoadDict(){
		//Create some temp Shader Vars
		ShaderVar SVType = new ShaderVar("Type",(float)Type);
		ShaderVar SVVisName = new ShaderVar("VisName",VisName);
		ShaderVar SVImageDefault = new ShaderVar("ImageDefault",ImageDefault);
		ShaderVar SVImage = new ShaderVar("Image",AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Image)));
		ShaderVar SVCube = new ShaderVar("Cube",AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Cube)));
		ShaderVar SVColor = new ShaderVar("Color",new Color(0.8f,0.8f,0.8f,1f));
		if (Color!=null)
		SVColor = new ShaderVar("Color",Color.ToColor());
		ShaderVar SVNumber = new ShaderVar("Number",Number);
		ShaderVar SVRange0 = new ShaderVar("Range0",Range0);
		ShaderVar SVRange1 = new ShaderVar("Range1",Range1);
		ShaderVar SVMainType = new ShaderVar("MainType",(float)(int)MainType);
		ShaderVar SVSpecialType = new ShaderVar("SpecialType",(float)(int)SpecialType);
		ShaderVar SVInEditor = new ShaderVar("InEditor",InEditor?1f:0f);
		ShaderVar SVNormalMap = new ShaderVar("NormalMap",NormalMap?1f:0f);
		
		Dictionary<string,ShaderVar> D = new Dictionary<string,ShaderVar>();

		D.Add(SVType.Name,SVType);
		D.Add(SVVisName.Name,SVVisName);
		D.Add(SVImageDefault.Name,SVImageDefault);
		D.Add(SVImage.Name,SVImage);
		D.Add(SVCube.Name,SVCube);
		D.Add(SVColor.Name,SVColor);
		D.Add(SVNumber.Name,SVNumber);
		D.Add(SVRange0.Name,SVRange0);
		D.Add(SVRange1.Name,SVRange1);
		D.Add(SVMainType.Name,SVMainType);
		D.Add(SVSpecialType.Name,SVSpecialType);
		D.Add(SVInEditor.Name,SVInEditor);
		D.Add(SVNormalMap.Name,SVNormalMap);
		return D;
	}
	public string Save(){
		string S = "BeginShaderInput\n";

		S += ShaderUtil.SaveDict(GetSaveLoadDict());
		S += "EndShaderInput\n";
		return S;
	}
	static public ShaderInput Load(StringReader S){
		ShaderInput SI = ShaderInput.CreateInstance<ShaderInput>();//UpdateGradient
		var D = SI.GetSaveLoadDict();
		while(1==1){
			string Line =  ShaderUtil.Sanitize(S.ReadLine());

			if (Line!=null){
				if(Line=="EndShaderInput")break;
				
				if (Line.Contains("#!"))
				ShaderUtil.LoadLine(D,Line);
			}
			else
			break;
		}
		
/*		ShaderVar SVType = new ShaderVar("Type",(float)Type);
		ShaderVar SVVisName = new ShaderVar("VisName",VisName);
		ShaderVar SVImageDefault = new ShaderVar("ImageDefault",ImageDefault);
		ShaderVar SVColor = new ShaderVar("Color",Color.ToColor());
		ShaderVar SVNumber = new ShaderVar("Number",Number);
		ShaderVar SVRange0 = new ShaderVar("Range0",Range0);
		ShaderVar SVRange1 = new ShaderVar("Range1",Range1);
		ShaderVar SVMainType = new ShaderVar("MainType",(float)(int)MainType);
		ShaderVar SVSpecialType = new ShaderVar("SpecialType",(float)(int)SpecialType);*/
		SI.Type = (int)D["Type"].Float;
		SI.VisName = D["VisName"].Text;
		SI.ImageDefault = (int)D["ImageDefault"].Float;
		SI.Image = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(D["Image"].Text),typeof(Texture2D));
		SI.Cube = (Cubemap)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(D["Cube"].Text),typeof(Cubemap));
		SI.Color = D["Color"].Vector;
		SI.Number = D["Number"].Float;
		SI.Range0 = D["Range0"].Float;
		SI.Range1 = D["Range1"].Float;
		SI.MainType = (InputMainTypes)(int)D["MainType"].Float;
		SI.SpecialType = (InputSpecialTypes)(int)D["SpecialType"].Float;
		//if (SI.SpecialType!=InputSpecialTypes.None)
		if (D.ContainsKey("InEditor")&&D["InEditor"].Float==0f)
		SI.InEditor = false;
		else
		SI.InEditor = true;
		
		if (D.ContainsKey("NormalMap")&&D["NormalMap"].Float==0f)
		SI.NormalMap = false;
		else
		SI.NormalMap = true;
		
		//Debug.Log(SI.VisName);
		return SI;
	}
	/*_Color
	_SpecColor
	_Shininess
	_ReflectColor
	_Parallax
	_MainTex
	_Cube
	_BumpMap
	_ParallaxMap*/	
}