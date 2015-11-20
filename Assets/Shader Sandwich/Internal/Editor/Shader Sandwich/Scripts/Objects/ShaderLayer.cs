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
using System.Diagnostics;

public enum LayerTypes{
Color,
Gradient,
VertexColors,
Texture,
Cubemap,
Noise,
Previous,
Literal,
GrabDepth};

public enum VertexMasks{
None = 0,
Normal = 1,
Position = 2
//View = 3
};

public enum ShaderMapType{UVMap1,UVMap2,Reflection,Direction,RimLight,Generate,View,Position};

[System.Serializable]
public class ShaderLayer : ScriptableObject{// : ScriptableObject {
public List<ShaderVar> ShaderVars = new List<ShaderVar>();
public int LayerID;

//[NonSerialized]public ShaderLayerList Parent = null;//
[NonSerialized]private ShaderLayerList Parent2;//
public ShaderLayerList Parent{
		get{
			if (Parent2==null){
				foreach(ShaderLayerList SLL in ShaderSandwich.Instance.OpenShader.GetShaderLayerLists()){
					SLL.FixParents();
				}
			}
			
			return Parent2;
		}
		set{
			Parent2 = value;
		}
	}
public bool IsVertex{
	get{
		return (Parent.Name.Text=="Vertex"||(ShaderBase.Current.SG!=null&&ShaderBase.Current.SG.InVertex));
	}
	set{
	
	}
}
public bool IsLighting{
	get{
		return (Parent.IsLighting.On||(ShaderBase.Current.SG!=null&&ShaderBase.Current.SG.InVertex));
	}
	set{
	
	}
}
public ShaderVar LayerType = new ShaderVar();
public bool LayerTypeHelp;
static public string[] LayerTypeNames = {"Color","Gradient","Vertex Colors","Texture","Cubemap","Noise","Previous Texture","Literal","Grab/Depth"};
public ShaderVar Name = new ShaderVar();
public bool Selected = true;
public bool Enabled = true;
//Types
//Color 0
public ShaderVar Color = new ShaderVar();
//Gradient 1 
public ShaderVar Color2 = new ShaderVar();
[XmlIgnore,NonSerialized] public Texture2D GradTex;
[XmlIgnore,NonSerialized] public Texture2D ColTex;
//Vertex Colors 2
//Texture 3 
//public Texture2D Image;
//public int ImageInput = -1;
public ShaderVar Image = new ShaderVar();
public bool IsNormalMap = false;
public enum ImageTypes {Normal,Greyscale,NormalMap,ScaledNormalMap,PackedNormalMap}
public ImageTypes ImageType = ImageTypes.Normal;
public Texture2D ImageResize;

//Cubemap 4
//public Cubemap Cube;
public ShaderVar Cube = new ShaderVar();
public Texture2D CubeResize;
//Noise 5
public ShaderVar NoiseDim = new ShaderVar();
public ShaderVar NoiseType = new ShaderVar();
public ShaderVar NoiseA = new ShaderVar();
public ShaderVar NoiseB = new ShaderVar();
public ShaderVar NoiseC = new ShaderVar();


public ShaderVar LightData = new ShaderVar();
public bool NoiseClamp;
//Previous Texture 6

//GrabDepth
public ShaderVar SpecialType = new ShaderVar();
public ShaderVar LinearizeDepth = new ShaderVar();

public int Rotation = 0;
	
public ShaderVar UseAlpha = new ShaderVar();
public ShaderVar Stencil = new ShaderVar();
//Color Editing

public int ColorR = 0;
public int ColorG = 1;
public int ColorB = 2;
public int ColorA = 3;
[XmlIgnore,NonSerialized]static public string[] ColorChannelNames = {"Red","Green","Blue","Alpha"};
[XmlIgnore,NonSerialized]static public string[] ColorChannelCodeNames = {"r","g","b","a"};


//Mapping
public bool MapTypeOpen;
public bool MapTypeHelp;
public ShaderVar MapType;
public ShaderVar MapLocal;
//1"UVs/UV1",
//2"UVs/UV2",
//3"World/Normals/Normal",
//4"World/Normals/Reflection",
//5"World/Normals/Edge",
//6"World/Generated/From View","
//7World/Generated/World Position",
//8"World/Generated/Cube",
//9"Local/Normals/Normal",
//10"Local/Normals/Reflection",
//11"Local/Normals/Edge",/
//12"Local/Generated/From View",
//13"Local/Generated/Local Position",
//14"Local/Generated/Cube"
public ShaderInput UVTexture;

[XmlIgnore,NonSerialized]static public string[] MapTypeDescriptions = {"Uses the 1st UV map. This is used in almost every shader.","Uses the 2nd UV map.","Used with cubemaps to simulate reflections","Based on the direction between the view and the normal of the object, it will use the first row of the texture.","The texture will be the same from any angle, based on the view.","The texture will be placed based on the world position.","The texture will be placed on each side of the model separately to eliminate seams.","Used with cubemaps to simulate Image Based Lighting."};

public int MappingX = 0;
public int MappingY = 1;
public int MappingZ = 2;
static public string[] MappingNames = {"X","Y","Z"};
static public string[] MappingCodeNames = {"x","y","z"};

public ShaderVar MixAmount = new ShaderVar();

public ShaderVar UseFadeout = new ShaderVar();
public ShaderVar FadeoutMinL = new ShaderVar();
public ShaderVar FadeoutMaxL = new ShaderVar();
public ShaderVar FadeoutMin = new ShaderVar();
public ShaderVar FadeoutMax = new ShaderVar();

public string[] AStencilChannelNames = {"Red","Green","Blue","Alpha"};
public string[] AStencilChannelCodeNames = {"r","g","b","a"};

static public string[] AVertexTypeNames = {"Normal","Local","World","View Space"};
static public string[] AVertexAxisTypeNames = {"X","Y","Z"};

public ShaderVar MixType = new ShaderVar();

public bool MathsOpen;
public bool MathsHelp;
public string[] MathTypeNames = {"Result","Brackets/(","Brackets/)","Basic/Add","Basic/Subtract","Basic/Multiply","Basic/Divide","Basic/Round","Basic/Pow","Function/Min","Function/Max","Dot"};
public string[] MathNamesNice = {"Result","(",")","Add","Subtract","Multiply","Divide","Round","Pow","Min","Max","Dot"};
public string[] MathNamesCode = {"Result","(",")","+","-","*","/","Round","Pow","Min","Max","Dot"};
[XmlIgnore,NonSerialized]public int[] MathTypeNamesNumb = new int[]{0,1,2,3,4,5,6,7,8,9,11,10};
[XmlIgnore,NonSerialized]public int[] MathNumbsCode = {-2,-1,0,1,1,1,1,1,2,2,2,2};
[XmlIgnore,NonSerialized]public int[] MathTypeCode = {-2,-1,0,1,1,1,1,2,2,2,2,2};
//public MathPart[] MathParts = new MathPart[1000];
public int MathCount = 0;

public ShaderVar VertexMask = new ShaderVar();

public bool Initiated = false;

public int SampleCount = 0;
public string ShaderCodeSamplers;
public string ShaderCodeEffects;
public List<string> SpawnedBranches;

//Effects
public List<ShaderEffect> LayerEffects = new List<ShaderEffect>();
public int SelectedEffect = 0;

//Generation Vars
public int InfluenceCount;
///////////////////sd//////////////////dasdasd//////////////////
///////sdasdasda///////////asdasdasdasd///////////////////////////
//sdasd///////////sdasdasd//////////////////////////////////////////
/////////////sdasdasda///////////////dasdasd///////////////////////////
public void OnEnable(){
	Color.OnChange += UpdateColor;
	Color.OnChange += UpdateGradient;
	Color2.OnChange += UpdateGradient;
	Image.OnChange += UpdateImage;
}
public void OnDisable(){
	Color.OnChange -= UpdateColor;
	Color.OnChange -= UpdateGradient;
	Color2.OnChange -= UpdateGradient;
	Image.OnChange -= UpdateImage;
}
public override string ToString(){
	return Name.Text+" (ShaderLayer)";
}
public ShaderLayer(){
Name = new ShaderVar("Layer Name","");
LayerType = new ShaderVar( "Layer Type",new string[] {"Color", "Gradient", "Vertex Color","Texture","Cubemap","Noise","Previous","Literal","Grab/Depth"},new string[] {"Color - A plain color.", "Gradient - A color that fades.", "Vertex Colors - The colors interpolated between each vertex","Texture - A 2d texture.","Cubemap - A 3d reflection map.","Noise - Perlin Noise","Previous - The previous value.","Literal - Uses the value directly from the mapping.","Grab/Depth - Access the entire screen or the screen's depth buffer."},3);
ShaderVars.Add(LayerType);

Color = new ShaderVar("Main Color", new Vector4(160f/255f,204f/255f,225/255f,1f));
ShaderVars.Add(Color);
Color2 = new ShaderVar("Second Color", new Vector4(0.0f,0.0f,0.0f,1f));
ShaderVars.Add(Color2);
Image = new ShaderVar("Main Texture", "Texture2D");ShaderVars.Add(Image);

Cube = new ShaderVar( "Cubemap","Cubemap");
ShaderVars.Add(Cube);
UseAlpha = new ShaderVar("Use Alpha",false);
Stencil = new ShaderVar("Stencil","ListOfObjects");

MixAmount = new ShaderVar( "Mix Amount",1f);ShaderVars.Add(MixAmount);

UseFadeout = new ShaderVar("Use Fadeout",false);
FadeoutMinL = new ShaderVar("Fadeout Limit Min",0f);
FadeoutMaxL = new ShaderVar("Fadeout Limit Max",10f);
FadeoutMin = new ShaderVar("Fadeout Start",3f);
FadeoutMax = new ShaderVar("Fadeout End",5f);

MixType = new ShaderVar("Mix Type", new string[]{"Mix","Add","Subtract","Multiply","Divide","Lighten","Darken","Normal Map Combine","Dot"},new string[]{"","","","","","","","",""},3);ShaderVars.Add(MixType);

MapType = new ShaderVar("UV Map",new string[]{"UV Map","UV Map2","Reflection","Direction","Rim Light","Generate","View","Position"},new string[]{"The first UV Map","The second UV Map (Lightmapping)","The reflection based on the angle of the face.","Simply the angle of the face.","Maps from the edge to the center of the model.","Created the UVMap using tri-planar mapping.(Good for generated meses)","Plaster the layer onto it from whatever view you are facing from.","Maps using the world position."});
MapLocal = new ShaderVar("Map Local",false);

NoiseDim = new ShaderVar("Noise Dimensions",new string[]{"2D","3D"},new string[]{"",""});
NoiseType = new ShaderVar("Noise Type",new string[]{"Perlin","Cloud","Cubist","Cell","Dot"},new string[]{"","","","",""});
NoiseA = new ShaderVar("Noise A",0f);
NoiseB = new ShaderVar("Noise B",1f);
NoiseC = new ShaderVar("Noise C",false);

LightData = new ShaderVar("Light Data",new string[]{"Light/Direction","Light/Attenuation","View Direction","Channels/Albedo(Diffuse)","Channels/Normal","Channels/Specular","Channels/Emission","Channels/Alpha","Light/Color"},new string[]{"","","","","","","","",""});
//NoiseC.Range0 = 1;
//NoiseC.Range1 = 3;
NoiseC.NoInputs = true;
SpecialType = new ShaderVar("Special Type",new string[]{"Grab","Depth"},new string[]{"",""});
LinearizeDepth = new ShaderVar("Linearize Depth",false);
VertexMask = new ShaderVar("Vertex Mask",2f);

}
public void UpdateShaderVars(bool Link){
	ShaderVars.Clear();
	var fieldValues = this.GetType()
						 .GetFields()
						 .Select(field => field.GetValue(this))
						 .ToList();
	foreach(object obj in fieldValues)
	{
		ShaderVar SV = obj as ShaderVar;
		if (SV!=null)
		{
			SV.MyParent = this;
			ShaderVars.Add(SV);
		}
	}
	

	foreach(ShaderEffect SE in LayerEffects){
		foreach (ShaderVar SV in SE.Inputs)
		{
			SV.MyParent = this;
			ShaderVars.Add(SV);
		}
	}
}
public string GetLayerCatagory(){
return GetLayerCatagory_Real(true);
}
public string GetLayerCatagory(bool Add){
return GetLayerCatagory_Real(Add);
}
public string BetterLayerCatagory(){
string LC = Parent.LayerCatagory;

if (LC=="Diffuse")
LC = "Texture";
if (LC=="Normals")
LC = "Normal Map";

return LC;
}
public string GetLayerCatagory_Real(bool Add){
string LC = BetterLayerCatagory();

if (Add)
if (Parent.Inputs!=0)
LC+=""+(Parent.Inputs+1).ToString();

if (Add)
Parent.Inputs+=1;
return LC;
}

/*public void OnEnable()
{
	if (Initiated==false||Color==null)
	{
		//Color =  new Vector4(0.8f,0.8f,0.8f,1f);
		//Color2 =  new Vector4(0f,0f,0f,1f);
		GradTex = new Texture2D(20,10);
		UpdateGradient();
		Selected = true;
		//MathParts = new MathPart[1000];
		Initiated=true;
	}
}*/
public bool BugCheck(){
	Stencil.SetToMasks(Parent,0);
	if (IsLighting){
		LayerType.Update(new string[] {"Color", "Gradient", "Light Data","Texture","Cubemap","Noise","Previous","Literal","Grab/Depth"},new string[] {"Color - A plain color.", "Gradient - A color that fades.", "Light Data - Access Lighting layer specific data.","Texture - A 2d texture.","Cubemap - A 3d reflection map.","Noise - Perlin Noise","Previous - The previous value.","Literal - Uses the value directly from the mapping.","Grab/Depth - Access the entire screen or the screen's depth buffer."},3);	
	}
	MixType.Update(new string[]{"Mix","Add","Subtract","Multiply","Divide","Lighten","Darken","Normals Mix","Dot"},new string[]{"","","","","","","","",""},3);

	MapType.Update(new string[]{"UV Map","UV Map2","Reflection","Direction","Rim Light","Generate","View","Position"},new string[]{"The first UV Map","The second UV Map (Lightmapping)","The reflection based on the angle of the face.","Simply the angle of the face.","Maps from the edge to the center of the model, highlighting the rim.","Created the UVMap using tri-planar mapping.(Good for generated meshes)","Plaster the layer onto it from whatever view you are facing from.","Maps using the world position."},3);
	bool RetVal = false;
	if (Color.UpdateToInput())
	RetVal = true;
	if (Color2.UpdateToInput())
	RetVal = true;
	
	return RetVal;
	/*MathTypeNames = new string[]{"Result","Brackets/(","Brackets/)","Basic/Add","Basic/Subtract","Basic/Multiply","Basic/Divide","Basic/Round","Basic/Pow","Function/Min","Function/Max","Function/Frac","Function/Clamp","Dot","Trigonometry/Sin","Trigonometry/Cos","Trigonometry/Tan","Trigonometry/Distance"};
	MathTypeNamesNumb = new int[]{0,1,2,3,4,5,6,7,8,9,10,12,13,11,14,15,16,17};
	MathNamesNice = new string[]{"Result","(",")","Add","Subtract","Multiply","Divide","Round","Pow","Min","Max","Dot","Frac","Clamp","Sin","Cos","Tan","Distance"};
	MathNamesCode = new string[]{"Result","(",")","+","-","*","/","round","pow","min","max","dot","frac","clamp","sin","cos","tan","distance"};
	MathNumbsCode = new int[]{-2,-1,0,1,1,1,1,0,1,1,1,1,0,2,0,0,0,1};
	MathTypeCode = new int[]{-2,-1,0,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2};*/
	//if (MathParts==null||MathParts.Length<800)
	//{
	//MathParts = new MathPart[1000];
	//}
}

public int GetDimensions(){
	return GetDimensions_Real(true);
}
public int GetDimensions(bool O){
	return GetDimensions_Real(O);
}
public bool UsesMap(){
	return (GetDimensions_Real(false)!=0);
}
public int GetDimensions_Real(bool O){
	int Dim = 2;
	if (LayerType.Type==(int)LayerTypes.Color)
		Dim = 0;
	if(LayerType.Type==(int)LayerTypes.VertexColors)
		Dim = 0;
	if(LayerType.Type==(int)LayerTypes.Previous)
		Dim = 0;
	if (LayerType.Type==(int)LayerTypes.Gradient)
		Dim = 1;
	if  (LayerType.Type == (int)LayerTypes.Literal)
		Dim = 3;
	if  (LayerType.Type == (int)LayerTypes.GrabDepth)
		Dim = 2;		
	if (LayerType.Type==(int)LayerTypes.Texture)
		Dim = 2;	
	if(LayerType.Type==(int)LayerTypes.Noise)
	{
		if (NoiseDim.Type==0)
		Dim = 2;
		if (NoiseDim.Type==1)
		Dim = 3;		
	}
	if (LayerType.Type==(int)LayerTypes.Cubemap)
		Dim = 3;
	if (O)
	if (MapType.Type==(int)ShaderMapType.Generate)
		Dim = 3;
		
	return Dim;
}




public void UpdateGradient()
{
        int y = 0;
		if (GradTex==null||GradTex.width!=40)
		GradTex = new Texture2D(40,40,TextureFormat.ARGB32,false);
		if (MapType.Type==(int)ShaderMapType.RimLight)
        while (y < GradTex.height) {
            int x = 0;
            while (x <=GradTex.width) {
				Color color;
				float LERP = Mathf.Clamp(1f-Mathf.Pow(Vector2.Distance(new Vector2(x,y),new Vector2(20,20))/20f,4),0,1);
				color = Color.Vector.Add((Color2.Vector.Sub(Color.Vector)).Mul(LERP)).ToColor();
                GradTex.SetPixel(x, y, color);
                ++x;
            }
            ++y;
        }
		else
        while (y < GradTex.height) {
            int x = 0;
            while (x <=GradTex.width) {
				Color color;
				x-=1;
				color = Color.Vector.Add((Color2.Vector.Sub(Color.Vector)).Mul(((float)x/(GradTex.width)))).ToColor();
				
                GradTex.SetPixel(x, y, color);
				x+=1;
                ++x;
            }
            ++y;
        }
        GradTex.Apply();
		//Debug.Log("Gradient Update");
}
public void UpdateColor()
{
        int y = 0;
		if (ColTex==null)
		ColTex = new Texture2D(1,1);
        while (y < ColTex.height) {
            int x = 0;
            while (x <=ColTex.width) {
				Color color;
				x-=1;
				color = Color.Vector.ToColor();
                ColTex.SetPixel(x, y, color);
				x+=1;
                ++x;
            }
            ++y;
        }
        ColTex.Apply();
}
public bool DrawIcon(Rect rect, bool Down){
	return DrawIcon_Real(rect,Down,true);
}
public bool DrawIcon(Rect rect){
	return DrawIcon_Real(rect,false,false);
}
bool DrawIcon_Real(Rect rect,bool Down,bool Button){
	bool ret = false;
	if (Button==true)
	{
		GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
		ButtonStyle.stretchHeight = true;
		ButtonStyle.fixedHeight = 0;
		ButtonStyle.stretchWidth = true;
		ButtonStyle.fixedWidth = 0;
		//if (Down==false){
			if (GUI.Button(rect,"",ButtonStyle)){
				ret=true;
				if (Event.current.button == 1 ){
					GenericMenu toolsMenu = new GenericMenu();
					toolsMenu.AddItem(new GUIContent("Copy"), false, ShaderSandwich.Instance.LayerCopy,this);
					toolsMenu.AddItem(new GUIContent("Paste"), false, ShaderSandwich.Instance.LayerPaste,Parent);
					toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0));
					EditorGUIUtility.ExitGUI();					
				}
			}
		//}
		//else
		if (Down==true)
		if (Event.current.type == EventType.Repaint)
		//ret = GUI.Toggle(rect,Down,"",ButtonStyle);
		ButtonStyle.Draw(rect,false,true,true,true);
	}
	rect.x+=15;
	rect.y+=15;
	rect.width-=30;
	rect.height-=30;
	
	//string mT = MixType.Names[MixType.Type];
	Material mT;
	
	if (ShaderSandwich.Instance.BlendLayers)
	mT = ShaderUtil.GetMaterial(MixType.Names[MixType.Type], new Color(1f,1f,1f,MixAmount.Float),UseAlpha.On);
	else
	mT = ShaderUtil.GetMaterial("Mix", new Color(1f,1f,1f,1f),false);
	
	Texture2D Tex = null;
	Tex = GetTexture();
	if (Tex!=null)
	Graphics.DrawTexture( rect ,Tex ,mT);
	//GUI.DrawTexture(rect,Tex);
	GUI.color = new Color(1,1,1,1);
	return ret;
}
/*float Unique2D(float2 t){
	//float x = frac(sin(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453);
	float x = frac(frac(tan(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453)*7.35);
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
}*/
public float Frac(float F){
	return (F-Mathf.Floor(F));
}
public float Lerp2D(Vector2 P,float Col1, float Col2, float Col3, float Col4){
	Vector2 ft = P * 3.1415927f;
	Vector2 f = new Vector2(1f - Mathf.Cos(ft.x),1f - Mathf.Cos(ft.y)) * 0.5f;
	P = f;
	float S1 = Mathf.Lerp(Col1,Col2,P.x);
	float S2 = Mathf.Lerp(Col3,Col4,P.x);
	float L = Mathf.Lerp(S1,S2,P.y);
	return L;
}
public float BlockNoise(float X,float Y){
	float XX = Mathf.Floor(X);
	float YY = Mathf.Floor(Y);
	float SS = Frac(Frac(Mathf.Tan(Vector2.Dot(new Vector2(XX,YY),new Vector2(12.9898f,78.233f))) * 43758.5453f)*7.35f);
	float SE = Frac(Frac(Mathf.Tan(Vector2.Dot(new Vector2(XX+1,YY+0),new Vector2(12.9898f,78.233f))) * 43758.5453f)*7.35f);
	float ES = Frac(Frac(Mathf.Tan(Vector2.Dot(new Vector2(XX+0,YY+1),new Vector2(12.9898f,78.233f))) * 43758.5453f)*7.35f);
	float EE = Frac(Frac(Mathf.Tan(Vector2.Dot(new Vector2(XX+1,YY+1),new Vector2(12.9898f,78.233f))) * 43758.5453f)*7.35f);
	return Lerp2D(new Vector2(Frac(X),Frac(Y)),SS,SE,ES,EE);
}
Float4 sign(Float4 s,bool SpawnNew){
	if (SpawnNew)
	s = new Float4(s.x,s.y,s.z,s.w);
	if (s.x>0)s.x = 1;
	if (s.x<0)s.x = -1;
	if (s.x==0)s.x = 0;
	if (s.y>0)s.y = 1;
	if (s.y<0)s.y = -1;
	if (s.y==0)s.y = 0;
	if (s.z>0)s.z = 1;
	if (s.z<0)s.z = -1;
	if (s.z==0)s.z = 0;
	if (s.w>0)s.w = 1;
	if (s.w<0)s.w = -1;
	if (s.w==0)s.w = 0;
	return s;
}
public Float4 CellularWeightSamples( Float4 Samples )
{
	Samples = Samples.Mul(2.0f).Sub(1);
	//return (1.0 - Samples * Samples) * sign(Samples);
	return ((Samples+0).Square().Square()).Sub(sign(Samples,false));
}
public Float2 floor(Float2 s,bool SpawnNew){
	if (SpawnNew)
		s = new Float2(s.x,s.y);
	s.x = Mathf.Floor(s.x);
	s.y = Mathf.Floor(s.y);
	return s;
}
public Float4 floor(Float4 s,bool SpawnNew){
	if (SpawnNew)
		s = new Float4(s.x,s.y,s.z,s.w);
	s.x = Mathf.Floor(s.x);
	s.y = Mathf.Floor(s.y);
	s.z = Mathf.Floor(s.z);
	s.w = Mathf.Floor(s.w);
	return s;
}
public Float2 min(Float2 s,Float2 ss,bool SpawnNew){
	if (SpawnNew){
	s = new Float2(s.x,s.y);
	ss = new Float2(ss.x,ss.y);
	}
	s.x = Mathf.Min(s.x,ss.x);
	s.y = Mathf.Min(s.y,ss.y);
	return s;
}
public Float min(float s,float ss,bool SpawnNew){
	s = Mathf.Min(s,ss);
	return s;
}
public Float2 max(Float2 s,Float2 ss,bool SpawnNew){
	if (SpawnNew){
	s = new Float2(s.x,s.y);
	ss = new Float2(ss.x,ss.y);
	}
	s.x = Mathf.Max(s.x,ss.x);
	s.y = Mathf.Max(s.y,ss.y);
	return s;
}
public Float max(float s,float ss,bool SpawnNew){
	s = Mathf.Max(s,ss);
	return s;
}
public Float2 frac(Float2 s,bool SpawnNew){
	if (SpawnNew)
	return s-floor(s,true);
	else
	return s.Sub(floor(s,true));
}
public Float4 frac(Float4 s,bool SpawnNew){
	if (SpawnNew)
	return s-floor(s,true);
	else
	return s.Sub(floor(s,true));
}
public void FastHash2D(Float2 Pos,out Float4 hash_0, out Float4 hash_1){
	//Float2 Offset = new Float2(26,161);
	float Domain = 71;
	Float2 SomeLargeFloats = new Float2(951.135664f,642.9478304f);
	Float4 P = new Float4(Pos.x,Pos.y,Pos.x+1,Pos.y+1);
	P = P-floor(P*(1.0f/Domain),false)*Domain;
	P.x += 26;
	P.y += 161;
	P.z += 26;
	P.w += 161;
	P.Mul(P);
	P = P.xzxz.Mul(P.yyww);
	hash_0 = frac(P*(1/SomeLargeFloats.x),false);
	hash_1 = frac(P*(1/SomeLargeFloats.y),false);
}
public float CellNoise(float X,float Y,float Jitter){
	Float2 P = new Float2(X,Y);
	Float2 Pi = floor(P,true);
	Float2 Pf = P.Sub(Pi);
	Float4 HashX, HashY;
	FastHash2D(Pi,out HashX,out HashY);
	HashX = (CellularWeightSamples(HashX).Mul(Jitter)).Add(0,1,0,1);
	HashY = (CellularWeightSamples(HashY).Mul(Jitter)).Add(0,0,1,1);
	Float4 dx = Pf.xxxx.Sub(HashX);
	Float4 dy = Pf.yyyy.Sub(HashY);
	Float4 d = dx.Square()+dy.Square();
	d.xy = min(d.xy,d.zw,false);
	return min(d.x,d.y,false).Mul(1.0f/1.125f);
}
Float3 Interpolation_C2( Float3 x ) { return x * x * x * (x * (x * 6.0f - 15.0f) + 10.0f); }
//Float2 Interpolation_C2( Float2 x ) { return (x+0).Square().Square().Mul(x * ((x*6.0f).Sub(15.0f)).Add(10.0f)); }
//Float2 Interpolation_C2( Float2 x ) { return x * x * x * ((x * ((x * 6.0f) - 15.0f)) + 10.0f); }
//Float2 Interpolation_C2( Float2 x ) { return x*x*x*(x*(x*6f-15f)+10f); }
Float2 Interpolation_C2( Float2 x ) { return (x*x).Square()*((x*(x*6f-15f)).Add(10f)); }
//Float2 Interpolation_C2( Float2 x ) { return (x*x).Square().Mul((x * ((x * 6.0f) - 15.0f)).Add(10.0f)); }
void FastHash2D(Float2 Pos,out Float4 hash_0, out Float4 hash_1, out Float4 hash_2){
	Float2 Offset = new Float2(26,161);
	Float Domain = 71f;
	Float3 SomeLargeFloats = new Float3(951.135664f,642.9478304f,803.202459f);
	Float4 P = new Float4(Pos,Pos+1);
	P = P.Sub(floor(P.Mul((1.0f/Domain)),true).Mul(Domain));
	P.Add(Offset.xyxy);
	P.Square();
	P = P.xzxz*P.yyww;
	hash_0 = frac(P*(1f/SomeLargeFloats.x),true);
	hash_1 = frac(P*(1f/SomeLargeFloats.y),true);
	hash_2 = frac(P*(1f/SomeLargeFloats.z),true);
}
public Float4 rsqrt(Float4 asd){
	asd.x = Mathf.Pow(asd.x,-1f/2f);
	asd.y = Mathf.Pow(asd.y,-1f/2f);
	asd.z = Mathf.Pow(asd.z,-1f/2f);
	asd.w = Mathf.Pow(asd.w,-1f/2f);
	return asd;
}
public Float2 pow(Float2 asd,Float p){
	asd.x = Mathf.Pow(asd.x,p);
	asd.y = Mathf.Pow(asd.y,p);
	return asd;
}
public Float4 clamp(Float4 asd,float mi,float ma){
	asd.x = Mathf.Clamp(asd.x,mi,ma);
	asd.y = Mathf.Clamp(asd.y,mi,ma);
	asd.z = Mathf.Clamp(asd.z,mi,ma);
	asd.w = Mathf.Clamp(asd.w,mi,ma);
	return asd;
}
public float clamp(float asd,float mi,float ma){
	asd = Mathf.Clamp(asd,mi,ma);
	return asd;
}
public Float dot(Float4 asd,Float4 asd2){
	return asd.x*asd2.x+asd.y*asd2.y+asd.z*asd2.z+asd.w*asd2.w;
}
public Float dot(Float2 asd,Float2 asd2){
	return asd.x*asd2.x+asd.y*asd2.y;
}
Float CubistNoise(float X,float Y,float Val1,float Val2)
{
	Float2 P = new Float2(X,Y);
	Float2 Pi = floor(P,true);
	Float4 Pf_Pfmin1 = P.xyxy.Sub(new Float4(Pi,Pi+1));
	Float4 HashX, HashY, HashValue;
	FastHash2D(Pi,out HashX,out HashY,out HashValue);
	Float4 GradX = HashX.Sub(0.499999f);
	Float4 GradY = HashY.Sub(0.499999f);
	Float4 GradRes = rsqrt(GradX*GradX+GradY*GradY)*(GradX*Pf_Pfmin1.xzxz+GradY*Pf_Pfmin1.yyww);
	GradRes = ( HashValue - 0.5f ).Mul( 1.0f / GradRes );
	
	GradRes.Mul(1.4142135623730950488016887242097f);
	Float2 blend = Interpolation_C2(Pf_Pfmin1.xy);
	Float4 blend2 = new Float4(blend,new Float2(1.0f-blend));
	Float final = (dot(GradRes,blend2.zxzx*blend2.wwyy));
	//return Interpolation_C2(new Float2(0.6f));
	return clamp((final.Add(Val1)).Mul(Val2),0.0f,1.0f);
}

Float DotFalloff( Float xsq ) { xsq = 1.0f - xsq; return xsq.Square().Square(); }
Float4 FastHash2D(Float2 Pos){
	Float2 Offset = new Float2(26,161);
	Float Domain = 71;
	Float SomeLargeFloat = 951.135664f;
	Float4 P = new Float4(Pos.xy,Pos.xy+1);
	//P = P-floor(P*(1.0f/Domain))*Domain;
	P = P-floor((P+0).Mul(1.0f/Domain),true).Mul(Domain);
	P.Add(Offset.xyxy);
	P.Square();
	return frac(P.xzxz.Mul(P.yyww).Mul(1.0f/SomeLargeFloat),false);
}
Float DotNoise(Float X,Float Y,Float Val1,Float Val2,Float Val3)
{
	Float3 Rad = new Float3(Val1,Val2,Val3);
	Float2 P = new Float2(X,Y);
	Float radius_low = Rad.x;
	Float radius_high = Rad.y;
	Float2 Pi = floor(P,true);
	Float2 Pf = P-Pi;

	Float4 Hash = FastHash2D(Pi);
	
	Float Radius = max(0.0f,radius_low+Hash.z*(radius_high-radius_low),true);
	Float Value = Radius/max(radius_high,radius_low,true);
	
	Radius = 2.0f/Radius;
	Pf *= Radius;
	Pf -= (Radius - 1.0f);
	Pf += Hash.xy*(Radius - 2f);
	Pf = pow(Pf,Rad.z);
	return DotFalloff(min(dot(Pf,Pf),1.0f,true))*Value;
}

public ShaderColor GetSample(float X,float Y){
	if (LayerType.Type==(int)LayerTypes.Gradient)
		return ShaderColor.Lerp(Color.Vector,Color2.Vector,X);
	if (LayerType.Type==(int)LayerTypes.Literal)
		return new ShaderColor(X,Y,0,1);
	if (LayerType.Type==(int)LayerTypes.Noise&&NoiseType.Type==0)
		return new ShaderColor(Mathf.PerlinNoise(X*3f,Y*3f));
	if (LayerType.Type==(int)LayerTypes.Noise&&NoiseType.Type==1)
		return new ShaderColor(BlockNoise(X*3f,Y*3f));
	if (LayerType.Type==(int)LayerTypes.Noise&&NoiseType.Type==2)
		return new ShaderColor(CubistNoise(X*3f,Y*3f,NoiseA.Float,NoiseB.Float));
	if (LayerType.Type==(int)LayerTypes.Noise&&NoiseType.Type==3)
		return new ShaderColor(CellNoise(X*3f,Y*3f,NoiseA.Float));
	if (LayerType.Type==(int)LayerTypes.Noise&&NoiseType.Type==4)
		return new ShaderColor(DotNoise(X*3f,Y*3f,NoiseA.Float,NoiseB.Float,NoiseC.On?2f:1f));
	if (LayerType.Type==(int)LayerTypes.Texture){
		if (Image.Input!=null&&Image.Input.NormalMap)
		return new ShaderColor(0.5f,0.5f,1,0.5f);
		
		return new ShaderColor(1,1,1,1);
	}
	return null;
}
public bool GetSample(){
	if (LayerType.Type==(int)LayerTypes.Gradient)
		return true;
	if (LayerType.Type==(int)LayerTypes.Noise)
		return true;
	if (LayerType.Type==(int)LayerTypes.Literal)
		return true;
	if (LayerType.Type==(int)LayerTypes.Texture&&Image.Image==null)
		return true;
	return false;
}
public Texture2D GetTexture(){
	Image.ImageS();
	Cube.CubeS();
	if (LayerType.Type==(int)LayerTypes.Color){
		if (ColTex ==null)
			UpdateColor();		
		return ColTex;
	}
	if (LayerType.Type==(int)LayerTypes.Texture){
		if (ImageResize ==null)
			UpdateImage();
		if (ImageResize !=null&&Image.ImageS()!=null){
			return ImageResize;
		}
	}
	if (LayerType.Type==(int)LayerTypes.Cubemap){
		if (CubeResize == null)
			UpdateCubemap();
		if (CubeResize !=null&&Cube.CubeS()!=null){
			return CubeResize;
		}
	}
	if (LayerType.Type==(int)LayerTypes.Noise){
		if (ShaderSandwich.PerlNoise !=null&&NoiseType.Type==0){
			return ShaderSandwich.PerlNoise;
		}
		if (ShaderSandwich.BlockNoise !=null&&NoiseType.Type==1){
			return ShaderSandwich.BlockNoise;
		}
		if (ShaderSandwich.BlockNoise !=null&&NoiseType.Type==2){
			return ShaderSandwich.CubistNoise;
		}
		if (ShaderSandwich.BlockNoise !=null&&NoiseType.Type==3){
			return ShaderSandwich.CellNoise;
		}
		if (ShaderSandwich.BlockNoise !=null&&NoiseType.Type==4){
			return ShaderSandwich.DotNoise;
		}
	}
	if (LayerType.Type==(int)LayerTypes.Gradient){
		if (GradTex ==null)
			UpdateGradient();
		return GradTex;
	}
	if (LayerType.Type==(int)LayerTypes.Literal){
		if (ShaderSandwich.Literal!=null)
			return ShaderSandwich.Literal;
	}
	if (LayerType.Type==(int)LayerTypes.GrabDepth){
		if (SpecialType.Type==0){
			if (ShaderSandwich.Literal!=null)
				return ShaderSandwich.GrabPass;
		}
		if (SpecialType.Type==1){
			if (ShaderSandwich.Literal!=null)
				return ShaderSandwich.DepthPass;
		}
	}
	return null;
}
bool MoreOrLess(float A1, float A2, float D){
	if ((A1<A2+D)&&(A1>A2-D))
	return true;
	
	return false;
}
bool MoreOrLess(int A1, int A2, int D){
	if ((A1<A2+D)&&(A1>A2-D))
	return true;
	
	return false;
}
void UpdateImage(){
	//Stopwatch sw = new Stopwatch();
	//sw.Start();
	string path = AssetDatabase.GetAssetPath(Image.ImageS());
	TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath(path);
	bool OldIsReadable = false;
	
	if (ti!=null){
		OldIsReadable = ti.isReadable;
		int OldMaxSize = ti.maxTextureSize;
		ti.isReadable = true;
		ti.maxTextureSize = 128;
		AssetDatabase.ImportAsset(path);
		//ShaderUtil.TimerDebug(sw,"Reimported1");
		ImageResize = new Texture2D(70,70,TextureFormat.ARGB32,false);
		bool IsGreyscale = true;
		bool IsNormalMap = true;
		int BadNormalBlues = 0;
		int BadNormalBlues2 = 0;
		bool IsNormalMapScaled = true;
		Color32[] colors = new Color32[(int)(ImageResize.width*ImageResize.height)];
		/*
		for(int x = 0;x<ImageResize.width;x++){
			for(int y = 0;y<(int)ImageResize.height;y++){
				Color LayerPixel = Image.Image.GetPixelBilinear((float)x/ImageResize.width,(float)y/ImageResize.height);
				if (LayerPixel.b<0.45||(MoreOrLess(LayerPixel.b,LayerPixel.g,0.0001f)))
					IsNormalMap = false;
				
				colors[x+(y*ImageResize.width)] = LayerPixel;
			}
		}*/
		//int mipLevel = SmallArea/(70*2);
		int mipLevel = 0;
		/*for(int i = 0;i<5;i++){
			if ((Image.Image.width>>(mipLevel+1)>70))
			mipLevel+=1;
		}*/
		Color32[] ImageColors = Image.Image.GetPixels32(mipLevel);
		int MipmapWidth = (int)Mathf.Max(1,Image.Image.width>>mipLevel);///(mipLevel*2));//>>mipLevel);
		int MipmapHeight = (int)Mathf.Max(1,Image.Image.height>>mipLevel);
		//UnityEngine.Debug.Log(MoreOrLess(100,150,2));
		for(int x = 0;x<ImageResize.width;x++){
			for(int y = 0;y<(int)ImageResize.height;y++){
				Color32 LayerPixel = ImageColors[ShaderUtil.FlatArray((int)  ((float)x/ImageResize.width*MipmapWidth),(int)((float)y/ImageResize.height*MipmapHeight),MipmapWidth,MipmapHeight)];
				
				if ((!MoreOrLess(LayerPixel.r,LayerPixel.b,30))||(!MoreOrLess(LayerPixel.r,LayerPixel.g,10))||(!MoreOrLess(LayerPixel.g,LayerPixel.b,30)))
					IsGreyscale = false;
				if (LayerPixel.r<114)
					BadNormalBlues+=1;
				//if (LayerPixel.b<240)
				//UnityEngine.Debug.Log(LayerPixel.b.ToString());
				if (LayerPixel.b<245)
					BadNormalBlues+=1;
					
				
				colors[x+(y*ImageResize.width)] = LayerPixel;
			}
		}
		if (BadNormalBlues>((ImageResize.width*ImageResize.height)/2))
			IsNormalMap = false;
		if (BadNormalBlues2>((ImageResize.width*ImageResize.height)/2))
			IsNormalMapScaled = false;
//		UnityEngine.Debug.Log(IsGreyscale);
		if (ti.normalmap)
		ImageType = ImageTypes.PackedNormalMap;
		else
		if (IsGreyscale)
		ImageType = ImageTypes.Greyscale;
		else
		if (IsNormalMapScaled)
		ImageType = ImageTypes.ScaledNormalMap;
		else
		if (IsNormalMap)
		ImageType = ImageTypes.NormalMap;
		else
		ImageType = ImageTypes.Normal;
		
//		UnityEngine.Debug.Log(ImageType);
		ImageResize.SetPixels32(colors);
		ImageResize.Apply(false,false);
		//ShaderUtil.TimerDebug(sw,"Shrunk");
		ti.isReadable = OldIsReadable;
		ti.maxTextureSize = OldMaxSize;
		AssetDatabase.ImportAsset(path);
		//ShaderUtil.TimerDebug(sw,"Reimported2");
		//sw.Stop();
		//ShaderUtil.TimerDebug(sw);
	}	
}
void UpdateCubemap(){
	string path = AssetDatabase.GetAssetPath(Cube.CubeS());
	TextureImporter ti =  TextureImporter.GetAtPath(path) as TextureImporter;
	bool OldIsReadable = false;
	if (ti!=null){
			OldIsReadable = ti.isReadable;
			ti.isReadable = true;
			AssetDatabase.ImportAsset(path);
	}
	CubeResize = new Texture2D(70,70,TextureFormat.ARGB32,false);
	Color[] colors = new Color[(int)(CubeResize.width*CubeResize.height)];
	Cubemap UseCube = Cube.Cube;
	try {
		Cube.Cube.GetPixel(CubemapFace.PositiveX,0,0);
	}
	catch {
		UseCube = ShaderSandwich.KitchenCube;
	}
	for(int x = 0;x<CubeResize.width;x++){
		for(int y = 0;y<(int)CubeResize.height;y++){
			Color LayerPixel;
			LayerPixel = UseCube.GetPixel(CubemapFace.PositiveX,(int)((float)x/CubeResize.width*UseCube.width),(int)((1-(float)y/CubeResize.height)*UseCube.height));
			colors[x+(y*CubeResize.width)] = LayerPixel;
		}
	}	
	
	CubeResize.SetPixels(colors);
	CubeResize.Apply(false,false);
	if (ti!=null){
		ti.isReadable = OldIsReadable;
		AssetDatabase.ImportAsset(path);
	}	
}
public void DrawGUI(){
	Color oldCol = GUI.color;
	Color oldColb = GUI.backgroundColor;
	
	Name.Text = GUI.TextField(new Rect(0,0,250,20),Name.Text);
	int YOffset = 20;
	
	if (IsVertex){
		VertexMask.Float = (float)(int)(VertexMasks)EditorGUI.EnumPopup(new Rect(0,YOffset,250,20)," ",(VertexMasks)(int)VertexMask.Float,ShaderUtil.EditorPopup);
		GUI.Label(new Rect(0,YOffset,250,20),"Vertex Disp Type: ");
		YOffset+=20;
	}
	
	LayerType.Draw(new Rect(0,YOffset,250,20));
	if (LayerType.Type==(int)LayerTypes.Color)
	Color.Draw(new Rect(0,90+YOffset,250,20),"Color: ");
	if (LayerType.Type==(int)LayerTypes.Gradient){
	Color.Draw(new Rect(0,90+YOffset,250,20),"Start Color: ");
	Color2.Draw(new Rect(0,110+YOffset,250,20),"End Color");};
	
	Image.Use = false;
	Cube.Use = false;
	if (LayerType.Type==(int)LayerTypes.Texture){
		if (Image.Input==null)
		Image.WarningSetup("No Input","Textures and cubemaps require an input to function correctly. Would you like to add one automatically?","Yes","No",AddTextureOrCubemapInput);
		else
		Image.WarningReset();
		Image.Use = true;
		if(Image.Draw(new Rect(0,90+YOffset,250,20),"Image: ")){
			GUI.changed = true;
			UpdateImage();
			
			bool AlreadyFixed = false;
			bool AlreadyFixed2 = false;
			bool AlreadyFixed3 = false;
			foreach(ShaderEffect SE in LayerEffects)
			{
				if(SE.TypeS=="SSEUnpackNormal")
				AlreadyFixed = true;
				if(SE.TypeS=="SSENormalMap")
				AlreadyFixed2 = true;
				if(SE.TypeS=="SSESwitchNormalScale2")
				AlreadyFixed3 = true;
			}
			
			if (Parent.CodeName=="o.Normal"){
				TextureImporter ti = (TextureImporter) TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(Image.ImageS()));
				if (ti!=null){
					/*if (ti.normalmap==false){
						if (AlreadyFixed == true){
							for(int i=LayerEffects.Count - 1; i > -1; i--)
							{
								if(LayerEffects[i].TypeS=="SSEUnpackNormal")
								if (LayerEffects[i].AutoCreated == true)
								LayerEffects.RemoveAt(i);
							}
						}
						if (AlreadyFixed2==false&&!IsNormalMap){
							AddLayerEffect("SSENormalMap");
							LayerEffects[LayerEffects.Count-1].Inputs[2].Type = 0;
							LayerEffects[LayerEffects.Count-1].AutoCreated = true;
						}
					}
					else{
						if (AlreadyFixed == false){
							AddLayerEffect("SSEUnpackNormal");
							LayerEffects[LayerEffects.Count-1].AutoCreated = true;
						}
						if (AlreadyFixed2 == true){
							for(int i=LayerEffects.Count - 1; i > -1; i--)
							{
								if(LayerEffects[i].TypeS=="SSENormalMap")
								if (LayerEffects[i].AutoCreated == true)
								LayerEffects.RemoveAt(i);
							}
						}						
						Image.WarningReset();
					}*/
					int Added = 0;
					if (ImageType == ImageTypes.PackedNormalMap){
						if (AlreadyFixed == false){
							AddLayerEffect("SSEUnpackNormal");
							LayerEffects[LayerEffects.Count-1].AutoCreated = true;
							
						}
						Added = 1;
						//Image.WarningReset();
					}
					if (ImageType == ImageTypes.ScaledNormalMap){
						if (!AlreadyFixed3){
							AddLayerEffect("SSESwitchNormalScale2");
							LayerEffects[LayerEffects.Count-1].AutoCreated = true;
							
						}
						Added = 3;
					}
					if (ImageType == ImageTypes.Greyscale||ImageType == ImageTypes.Normal){
						if (!AlreadyFixed2){
							AddLayerEffect("SSENormalMap");
							LayerEffects[LayerEffects.Count-1].Inputs[2].Type = 0;
							LayerEffects[LayerEffects.Count-1].AutoCreated = true;
						}
						Added = 2;
					}

				
					//if (AlreadyFixed == true||AlreadyFixed2 == true||AlreadyFixed3 == true){
					for(int i = LayerEffects.Count - 1; i > -1; i--){
						if(LayerEffects[i].TypeS=="SSENormalMap"&&Added!=2){
							if (LayerEffects[i].AutoCreated == true)
							LayerEffects.RemoveAt(i);
						}
						else
						if(LayerEffects[i].TypeS=="SSESwitchNormalScale2"&&Added!=3){
							if (LayerEffects[i].AutoCreated == true)
							LayerEffects.RemoveAt(i);
						}
						else
						if(LayerEffects[i].TypeS=="SSEUnpackNormal"&&Added!=1){
							if (LayerEffects[i].AutoCreated == true)
							LayerEffects.RemoveAt(i);
						}
					}
					//}
				}
			}
		}
	}
	if (LayerType.Type==(int)LayerTypes.Cubemap){
		if (Cube.Input==null)
			Cube.WarningSetup("No Input","Textures and cubemaps require an input to function correctly. Would you like to add one automatically?","Yes","No",AddTextureOrCubemapInput);
		else
			Cube.WarningReset();
		Cube.Use = true;
		if(Cube.Draw(new Rect(0,90+YOffset,250,20),"Cubemap: "))
			UpdateCubemap();
	}
	
	if (LayerType.Type==(int)LayerTypes.VertexColors&&IsLighting){
		//LightData.Draw(new Rect(0,90+YOffset,250,20),"Type: ");
		GUI.Label(new Rect(0,90+YOffset,250,20),"Data:");
		LightData.Type=EditorGUI.Popup(new Rect(100,90+YOffset,150,20),LightData.Type,LightData.Names,ShaderUtil.EditorPopup);
	}
	if (LayerType.Type==(int)LayerTypes.Noise){
		NoiseType.Draw(new Rect(0,90+YOffset,250,20),"Type: ");
		NoiseDim.Draw(new Rect(0,115+YOffset,250,20),"Dimensions: ");
		if (NoiseType.Type==2){
			NoiseA.Range1 = 1;
			NoiseA.Draw(new Rect(0,135+YOffset,250,20),"Value A: ");
			NoiseB.Draw(new Rect(0,155+YOffset,250,20),"Value B: ");
			YOffset+=40;
		}
		if (NoiseType.Type==3){
			NoiseA.Range1 = 0.5f;
			NoiseA.Draw(new Rect(0,135+YOffset,250,20),"Jitter: ");
			YOffset+=40;
		}
		if (NoiseType.Type==4){
			NoiseA.Range1 = 1;
			NoiseA.Draw(new Rect(0,135+YOffset,250,20),"Min Size: ");
			NoiseB.Draw(new Rect(0,155+YOffset,250,20),"Max Size: ");
			NoiseC.Draw(new Rect(0,175+YOffset,250,20),"Square: ");
			YOffset+=60;
		}
	}
	if (LayerType.Type==(int)LayerTypes.GrabDepth){
		SpecialType.Draw(new Rect(0,90+YOffset,250,20),"Type: ");
		if (SpecialType.Type==1)
		LinearizeDepth.Draw(new Rect(0,115+YOffset,250,20),"Linearize Depth: ");
	}
	
	YOffset-=20;
	if (Event.current.type == EventType.Repaint)
	{
		GUI.skin.GetStyle("Button").Draw(new Rect(0,170+YOffset,250,30),"",false,true,false,false);
		GUI.skin.label.wordWrap = false;
		//GUI.skin.label.alignment = TextAnchor.UpperCenter;
		
		GUI.color = new Color(0.3f,0.8f,0.7f,1);
		GUI.backgroundColor = new Color(1,1,1,1);
			GUI.skin.GetStyle("ProgressBarBar").Draw(new Rect(0,170+YOffset,250*MixAmount.Float,30),"",false,true,false,false);
			GUI.color = new Color(1,1,1,1);
			GUI.Label(new Rect(80,170+YOffset,250,30),MixType.Names[MixType.Type]+" Amount");
			GUI.color = new Color(0,0,0,1);
			GUI.Label(new Rect(80,170+YOffset,250*MixAmount.Float-80,30),MixType.Names[MixType.Type]+" Amount");
			
		GUI.skin.label.alignment = TextAnchor.UpperLeft;	
		GUI.skin.label.wordWrap = true;	
		//EditorGUI.ProgressBar(new Rect(0,200,250,30),MixAmount.Float,"Mix Amount");	
	}
	MixAmount.DrawGear(new Rect(-290/2+26,170+YOffset+10,290,20));
	GUI.color = new Color(1,1,1,0);
	GUI.backgroundColor = oldColb;
	//	MixAmount.NoInputs = true;
		MixAmount.Draw(new Rect(-5,170+YOffset,295,30),"");
		MixAmount.LastUsedRect.x-=110;
		MixAmount.LastUsedRect.y+=10;
	GUI.color = oldCol;
	//MixAmount.NoInputs = false;
		MixAmount.DrawGear(new Rect(-290/2+26,170+YOffset+10,20,20));
	
	
	MixType.Draw(new Rect(0,200+YOffset,250,20));
	
	//Fadeout
	if (!IsLighting){
	UseFadeout.Draw(new Rect(0,260+YOffset,210,20),"Fadeout: ");
	YOffset+=20;
	if (UseFadeout.On){
		EditorGUI.BeginChangeCheck();
		EditorGUI.MinMaxSlider(new Rect(40,280+YOffset,180,20), ref FadeoutMin.Float, ref FadeoutMax.Float, FadeoutMinL.Float, FadeoutMaxL.Float);
		if (EditorGUI.EndChangeCheck()){
			GUI.changed = true;
			EditorGUIUtility.editingTextField = false;
			FadeoutMin.Float = Mathf.Max(FadeoutMinL.Float,Mathf.Round(FadeoutMin.Float*100)/100);
			FadeoutMax.Float = Mathf.Min(FadeoutMaxL.Float,Mathf.Round(FadeoutMax.Float*100)/100);
			FadeoutMin.UpdateToVar();
			FadeoutMax.UpdateToVar();
		}
		//MinL = EditorGUI.FloatField(new Rect(142,130,40,20),MinL);
		//MaxL = EditorGUI.FloatField(new Rect(BoxSize.x-40,130,40,20),MaxL);
		//FadeoutMin.Float = EditorGUI.FloatField(new Rect(180,240+YOffset,40,20),FadeoutMin.Float);
		FadeoutMax.NoSlider = true;
		FadeoutMax.NoArrows = true;
		
		FadeoutMin.NoSlider = true;
		FadeoutMin.NoArrows = true;
		
		FadeoutMinL.NoSlider = true;
		FadeoutMinL.NoArrows = true;
		
		FadeoutMaxL.NoSlider = true;
		FadeoutMaxL.NoArrows = true;
		
		FadeoutMin.LabelOffset = 70;
		FadeoutMax.LabelOffset = 70;
		
		//EditorGUI.BeginChangeCheck();
		FadeoutMin.Draw(new Rect(0,260+YOffset,120,17),"Start:");
		FadeoutMax.Draw(new Rect(130,260+YOffset,120,17),"End:");
		
		FadeoutMinL.Draw(new Rect(10,280+YOffset,20,17),"");
		FadeoutMaxL.Draw(new Rect(230,280+YOffset,20,17),"");
		//FadeoutMax.Float = EditorGUI.FloatField(new Rect(180,240+YOffset,40,20),FadeoutMax.Float);
		YOffset+=40;
	}
	}else{UseFadeout.On=false;}
	UseAlpha.Draw(new Rect(0,260+YOffset,210,20),"Use Alpha: ");
	
	/*if (Stencil.ObjFieldObject==null)Stencil.ObjFieldObject=new List<object>();
	if (Stencil.ObjFieldImage==null)Stencil.ObjFieldImage=new List<Texture2D>();
	Stencil.ObjFieldObject.Clear();
	Stencil.ObjFieldImage.Clear();
	foreach(ShaderLayerList SLL in ShaderSandwich.Instance.OpenShader.ShaderLayersMasks){
		Stencil.ObjFieldObject.Add(SLL);
		Stencil.ObjFieldImage.Add(SLL.GetIcon());
	}*/
	if (Parent.Parallax||Parent.Name.Text=="Height"){
	GUI.enabled = false;Stencil.Selected = -1;}
	
	
	Stencil.LightingMasks = Parent.IsLighting.On;
	Stencil.Draw(new Rect(0,280+YOffset,250,20),"Mask: ");
	Stencil.NoInputs = true;
	
	GUI.enabled = true;
	
	YOffset+=20;
	if (LayerType.Type==(int)LayerTypes.VertexColors||LayerType.Type==(int)LayerTypes.Color)
	GUI.enabled = false;
	
	MapLocal.LabelOffset = 90;
	MapLocal.Draw(new Rect(169,342+YOffset,120,20),"Local:");
	//UnityEngine.Debug.Log(IsLighting);
	if (IsLighting){
		if (GUI.enabled){
			GUI.enabled = false;
			MapType.Draw(new Rect(0,300+YOffset,250,20));
			GUI.enabled = true;
			if (MapType.Type!=3)
			MapType.Type=3;
			EditorGUI.BeginChangeCheck();
			GUI.Toggle(new Rect(0,300+YOffset-1+22,81,18),MapType.Type==3,"Direction","button");
			if (EditorGUI.EndChangeCheck())
			MapType.Type=3;
		}else{MapType.Draw(new Rect(0,300+YOffset,250,20));}
	}
	else
	if (MapType.Draw(new Rect(0,300+YOffset,250,20)))UpdateGradient();
	GUI.enabled = true;
	
	YOffset+=380;
	YOffset+=15;
	ShaderUtil.DrawEffects(new Rect(10,YOffset,230,10),this,LayerEffects,ref SelectedEffect);
}
public void WarningFixDelegate(int Option,ShaderVar SV){
	if (Option==1){
		AddLayerEffect("SSENormalMap");
		LayerEffects[LayerEffects.Count-1].Inputs[2].Type = 0;
		LayerEffects[LayerEffects.Count-1].AutoCreated = true;
	}
	if (Option==0){
		AddLayerEffect("SSESwizzle");
		LayerEffects[LayerEffects.Count-1].AutoCreated = true;
		LayerEffects[LayerEffects.Count-1].Inputs[0].Type = 3;
		LayerEffects[LayerEffects.Count-1].Inputs[1].Type = 1;
		LayerEffects[LayerEffects.Count-1].Inputs[2].Type = 0;
		LayerEffects[LayerEffects.Count-1].Inputs[3].Type = 0;
	}
	//Image.WarningReset();
}
public void AddTextureOrCubemapInput(int Option,ShaderVar SV){
	if (Option==0){
		if (LayerType.Type==(int)LayerTypes.Texture){
			Image.AddInput();
		}
		if (LayerType.Type==(int)LayerTypes.Cubemap){
			Cube.AddInput();
		}
		Image.WarningReset();
		Cube.WarningReset();
	}
}
public void AddLayerEffect(object TypeName){
		ShaderEffect NewEffect = ShaderEffect.CreateInstance<ShaderEffect>();
		NewEffect.ShaderEffectIn((string)TypeName);//ShaderSandwich.EffectsList[0]);
		LayerEffects.Add(NewEffect);
		Parent.UpdateIcon(new Vector2(70,70));
}
static public void DrawGUIGen(bool Text){
	//Color oldCol = GUI.color;
	//Color oldColb = GUI.backgroundColor;
	
	if (Text)
	GUI.Box(new Rect(0,0,250,21),"The layers name");
	else
	GUI.Box(new Rect(0,0,250,21),"");
	
	int YOffset = 20;
	if (Text)
	GUI.Box(new Rect(0,0+YOffset,250,81),"The layer's type. This changes what the layer itself looks like.");
	else
	GUI.Box(new Rect(0,0+YOffset,250,81),"");
	if (Text)
	GUI.Box(new Rect(0,80+YOffset,250,71),"Some layer type specific properties, such as the color of the layer, the texture it uses, etc.");else
	GUI.Box(new Rect(0,80+YOffset,250,71),"");
	
	if (Text)
	GUI.Box(new Rect(0,150+YOffset,250,31),"The layers mix amount, how much the layer overwrites previous layers.");
	else
	GUI.Box(new Rect(0,150+YOffset,250,31),"");
	
	if (Text)
	GUI.Box(new Rect(0,180+YOffset,250,61),"How the layers blend. Mix is standard blending. Add on the other hand, adds the colors different components (Red, Green, Blue, Alpha) seperately.");
	else
	GUI.Box(new Rect(0,180+YOffset,250,61),"");	
	
	
	if (Text)
	GUI.Box(new Rect(0,240+YOffset,250,21),"Fadeout the layer by distance.");
	else
	GUI.Box(new Rect(0,240+YOffset,250,21),"");		
	
	YOffset+=20;
	if (Text)
	GUI.Box(new Rect(0,240+YOffset,250,21),"Whether or not the layer uses it's alpha into account when combining with other layers.");
	else
	GUI.Box(new Rect(0,240+YOffset,250,21),"");		
	
	if (Text)
	GUI.Box(new Rect(0,260+YOffset,250,21),"The mask the layer uses.");
	else
	GUI.Box(new Rect(0,260+YOffset,250,21),"");		
	
	if (Text)
	GUI.Box(new Rect(0,300+YOffset,250,91),"How the layer is placed on the object. The layer needs to know how to choose what colour from it's texture or gradient to put at what point on the model.");
	else
	GUI.Box(new Rect(0,300+YOffset,250,91),"");
	
	if (Text)
	GUI.Box(new Rect(0,390+YOffset,250,91),"Layer effects which can alter what the layer looks like and how it's mapped.");
	else
	GUI.Box(new Rect(0,300+YOffset,250,91),"");
	
	/*GUI.color = new Color(1,1,1,0);
	GUI.backgroundColor = oldColb;
		MixAmount.Draw(new Rect(0,220+YOffset,250,30),"");
	GUI.color = oldCol;
	MixType.Draw(new Rect(0,250+YOffset,250,20));
	if (MapType.Draw(new Rect(0,330+YOffset,250,20)))UpdateGradient(Color.Vector,Color2.Vector,false);*/

}










////////////////Code Gen Stuff
public string GCUVs(ShaderGenerate SG,string OffsetX,string OffsetY, string OffsetZ){
return GCUVs_Real(SG,OffsetX,OffsetY,OffsetZ,true);
}
public string GCUVs(ShaderGenerate SG){
return GCUVs_Real(SG,"","","",true);
}
public string GCUVs(ShaderGenerate SG,bool UseEffects){
return GCUVs_Real(SG,"","","",UseEffects);
}
public string GCUVs_Real(ShaderGenerate SG,string OffsetX,string OffsetY, string OffsetZ,bool UseEffects){
	//string ad = "IN";
	//string Map1D = "0";
	//string Map2D = "float2(0,0)";
	//string Map3D = "float3(0,0,0)";
	string Map = "float3(0,0,0)";
	
	string LocalStart = "";
	string LocalEnd = "";
	if (MapLocal.On){
		LocalStart = "mul(_World2Object, float4(";
		LocalEnd = ",1)).xyz";
	}
	//string MapZ = "0";
	
	int TypeDimensions = GCTypeDimensions();
	int UVDimensions = GCUVDimensions();
	if (MapType.Type==(int)ShaderMapType.UVMap1){
		string UV = "";
		
		if (UVTexture==null)
		UV = "IN."+SG.GeneralUV;
		else
		UV = "uv"+UVTexture.Get();
		
		if (IsVertex)
		UV = "v.texcoord.xyz";
		
		Map = UV+".xy";
		//Map2D = UV+".xy";
		if (LayerType.Type == (int)LayerTypes.Cubemap){
			string MapX = "sin("+UV+".x)";
			string MapY = "cos("+UV+".y)";			
			string MapZ = "sin("+UV+".y)";
			Map = "float3("+MapX+","+MapY+","+MapZ+")";
		}
	}
	if (MapType.Type==(int)ShaderMapType.UVMap2){
		string UV = "";
		
		if (UVTexture==null)
		UV = "IN."+SG.GeneralUV;
		else
		UV = "IN.uv2"+UVTexture.Get();
		
		if (IsVertex)
		UV = "v.texcoord2.xyz";
		
		Map = UV+".xy";
		if (LayerType.Type == (int)LayerTypes.Cubemap){
			string MapX = "sin("+UV+".x)";
			string MapY = "cos("+UV+".y)";			
			string MapZ = "sin("+UV+".y)";
			Map = "float3("+MapX+","+MapY+","+MapZ+")";
		}
	}
	if (MapType.Type==(int)ShaderMapType.Reflection){
		string oNameWorldRefType = "IN.worldRefl";//oNameWorldRef;
		if (SG.UsedWorldNormals==false)
		oNameWorldRefType = LocalStart+"IN.worldRefl"+LocalEnd;
		else
		oNameWorldRefType = LocalStart+"WorldReflectionVector(IN,o.Normal)"+LocalEnd;
		
		if (IsVertex){
			if (MapLocal.On)
			oNameWorldRefType = "reflect(-UnityWorldSpaceViewDir(v.vertex.xyz), v.normal)";
			else
			oNameWorldRefType = "reflect(-UnityWorldSpaceViewDir(mul(_Object2World, v.vertex).xyz), UnityObjectToWorldNormal(v.normal))";
		}
		
		Map = oNameWorldRefType;
	}
	if (MapType.Type==(int)ShaderMapType.Direction)
	{
		string oNameNormalType;//oNameNormal;
		oNameNormalType = LocalStart+"IN.worldNormal"+LocalEnd;
		if (SG.UsedNormals)
		oNameNormalType = LocalStart+"WorldNormalVector(IN, o.Normal)"+LocalEnd;
		if (IsVertex){
			if (MapLocal.On)
			oNameNormalType = "v.normal";
			else
			oNameNormalType = "UnityObjectToWorldNormal(v.normal)";
		}
		if (IsLighting){
			oNameNormalType = LocalStart+"SSnormal"+LocalEnd;
		}
		//Debug.Log(MapType.ToString());		
		Map = oNameNormalType;
	}		
	if (MapType.Type==(int)ShaderMapType.RimLight)
	{
		string oNameViewDir="IN.viewDir";
		//oNameWorldRefType = "reflect(-UnityWorldSpaceViewDir(mul(_Object2World, v.vertex).xyz), UnityObjectToWorldNormal(v.normal))";
		if (IsVertex)
		Map = "(1-dot(normalize(UnityWorldSpaceViewDir(mul(_Object2World, v.vertex).xyz)), normalize(UnityObjectToWorldNormal(v.normal))))";
		else
		Map = "(1-dot(o.Normal, "+oNameViewDir+"))";
	}
	if (MapType.Type==(int)ShaderMapType.View)
	{
		string ScreenPosVar = "IN.screenPos";
		if (IsVertex)
		ScreenPosVar = "(ComputeScreenPos(mul (UNITY_MATRIX_MVP, v.vertex))/ComputeScreenPos(mul (UNITY_MATRIX_MVP, v.vertex)).w)";
		Map = "("+ScreenPosVar+".xyw)";
		if (LayerType.Type == (int)LayerTypes.Cubemap){
			string MapX = "sin("+ScreenPosVar+".x)";
			string MapY = "cos("+ScreenPosVar+".y)";			
			string MapZ = "sin("+ScreenPosVar+".y)";
			Map = "float3("+MapX+","+MapY+","+MapZ+")";
		}		
	}	
	if (MapType.Type==(int)ShaderMapType.Position||MapType.Type==(int)ShaderMapType.Generate)
	{
		if (IsVertex){
			if (MapLocal.On)
			Map = "v.vertex.xyz";
			else
			Map = "mul(_Object2World, v.vertex).xyz";
		}
		else
			Map = LocalStart+"IN.worldPos"+LocalEnd;
	}		
	
	
	if (UseEffects){
		LayerEffects.Reverse();
		foreach(ShaderEffect SE in LayerEffects){
			if (SE.Visible){
				var Meth = ShaderEffect.GetMethod(SE.TypeS,"GenerateMap");
				if (Meth!=null){
					object[] Vars = new object[]{SG,SE,this,Map,UVDimensions,TypeDimensions};
					Map = (string)Meth.Invoke(null,Vars);
					UVDimensions = (int)Vars[4];
					TypeDimensions = (int)Vars[5];
				}
			}	
		}
		LayerEffects.Reverse();
	}
	Map  = "("+Map+")";
	
	
	if (TypeDimensions==0)
	{
		Map = "0";
	}
	if (TypeDimensions==1)
	{
		if (UVDimensions == 2)
			Map = Map+".x";//xy";
		if (UVDimensions == 3)
			Map = Map+".x";//xy";
	}
	if (TypeDimensions==2)
	{
		if (UVDimensions == 1)
			Map = "float2("+Map+",0)";//xy";
		if (UVDimensions == 3)
			Map = Map+".xy";//xy";
	}
	if (TypeDimensions==3)
	{
		if (UVDimensions == 1)
			Map = "float3("+Map+",0,0)";//xy";
		if (UVDimensions == 2)
			Map = "float3("+Map+",0)";//xy";
	}
	Map  = "("+Map+")";
	

	
	if (TypeDimensions==1)
	{
		if (OffsetX!="")
		Map+=" + "+OffsetX;
		if (SG.MapDispOn==true)
		Map="("+Map+"+MapDisp.r)";
	}
	if (TypeDimensions==2)
	{
		if (OffsetX!="")
		Map+=" + float2("+OffsetX+", "+OffsetY+")";
		if (SG.MapDispOn==true)
		Map="("+Map+"+MapDisp.rg)";
	}
	if (TypeDimensions==3)
	{
		if (OffsetX!="")
		Map+=" + float3("+OffsetX+", "+OffsetY+", "+OffsetZ+")";
		if (SG.MapDispOn==true)
		Map="("+Map+"+MapDisp.rgb)";			
	}
	Map  = "("+Map+")";
	if (LayerType.Type == (int)LayerTypes.Noise)
	Map  = "("+Map+"*3)";
	if (Parent.Parallax||Parent.Name.Text=="Height"){
		if (GetDimensions()==3)
		Map+="+(worldView*(depth))";
		else
		if (GetDimensions()==2)
		Map+="+((view*(depth)).xy)";
		else
		Map+="+((view*(depth)).x)";
	}	
	return Map;
}

public int GCDimensions(){
	return GetDimensions();
}
public int GCTypeDimensions(){
	return GetDimensions();
}
public int GCUVDimensions(){
	int Dim = 2;
	if (MapType.Type==(int)ShaderMapType.UVMap1){
		Dim = 2;
		if (LayerType.Type == (int)LayerTypes.Cubemap)
			Dim = 3;
	}
	if (MapType.Type==(int)ShaderMapType.UVMap2){
		Dim = 2;
		if (LayerType.Type == (int)LayerTypes.Cubemap)
			Dim = 3;
	}
	if (MapType.Type==(int)ShaderMapType.Reflection)
		Dim = 3;
	if (MapType.Type==(int)ShaderMapType.Direction)
		Dim = 3;
	if (MapType.Type==(int)ShaderMapType.RimLight)
		Dim = 1;
	if (MapType.Type==(int)ShaderMapType.View){
		Dim = 3;
		if (LayerType.Type == (int)LayerTypes.Cubemap)
			Dim = 3;
	}	
	if (MapType.Type==(int)ShaderMapType.Position||MapType.Type==(int)ShaderMapType.Generate)
		Dim = 3;
	return Dim;
}
public string GCPixelBase(ShaderGenerate SG,string Map){
	if (MapType.Type==(int)ShaderMapType.Generate){
	if (GetDimensions(false)==1)
	return "("+GCPixelBase_Real(SG,"("+Map+").z")+"*blend.x + "+GCPixelBase_Real(SG,"("+Map+").z")+"*blend.y + "+GCPixelBase_Real(SG,"("+Map+").x")+"*blend.z)";
	if (GetDimensions(false)==2)
	return "("+GCPixelBase_Real(SG,"("+Map+").zy")+"*blend.x + "+GCPixelBase_Real(SG,"("+Map+").zx")+"*blend.y + "+GCPixelBase_Real(SG,"("+Map+").xy")+"*blend.z)";
	}
	
	return GCPixelBase_Real(SG,Map);
}
public string GCPixelBase_Real(ShaderGenerate SG,string Map){
	string PixCol = "";
	//if (Map.Length>40000)
	if (MixType.Type == 4){
		if (Color.Vector.r==0)Color.Vector.r = 0.001f;
		if (Color.Vector.g==0)Color.Vector.g = 0.001f;
		if (Color.Vector.b==0)Color.Vector.b = 0.001f;
		if (Color.Vector.a==0)Color.Vector.a = 0.001f;
		if (Color2.Vector.r==0)Color2.Vector.r = 0.001f;
		if (Color2.Vector.g==0)Color2.Vector.g = 0.001f;
		if (Color2.Vector.b==0)Color2.Vector.b = 0.001f;
		if (Color2.Vector.a==0)Color2.Vector.a = 0.001f;
	}
	if (LayerType.Type == (int)LayerTypes.Color){
		PixCol = Color.Get();
	}
	if (LayerType.Type == (int)LayerTypes.Gradient){
		PixCol = "lerp("+Color2.Get()+", "+Color.Get()+", "+Map+")";
		//(Color+((Color2-Color)*x)
	}	
	if (LayerType.Type == (int)LayerTypes.VertexColors){
		if (!IsLighting){
			if (IsVertex)
			PixCol = "v.color";
			else
			PixCol = "IN.color";
		}else{//Dir Atten ViewDir Diff Nor Spec Emm
			if (LightData.Type==0)
			PixCol = "lightDir";
			if (LightData.Type==1)
			PixCol = "atten.rrr";
			if (LightData.Type==2)
			PixCol = "viewDir";
			if (LightData.Type==3)
			PixCol = "SSalbedo";
			if (LightData.Type==4)
			PixCol = "SSnormal";
			if (LightData.Type==5)
			PixCol = "SSspecular";
			if (LightData.Type==6)
			PixCol = "SSemission";
			if (LightData.Type==7)
			PixCol = "SSalpha.rrr";
			if (LightData.Type==8)
			PixCol = "SSlightColor";
			PixCol = "float4("+PixCol+",0)";
			
		}
		
	}
	if (LayerType.Type == (int)LayerTypes.Texture){
		if (Image.Input!=null){
			if (IsVertex)
			PixCol = "tex2Dlod("+Image.Input.Get()+",float4("+Map+",0,0))";
			else
			PixCol = "tex2D("+Image.Input.Get()+","+Map+")";
		}
		else
		PixCol = "float4(1,1,1,1)";
		//(Color+((Color2-Color)*x)
	}
	if (LayerType.Type == (int)LayerTypes.Cubemap){
		//if (Map=="0"){
		//	PixCol = "texCUBE("+Cube.Input.Get()+",float3("+Map+"))";
		//}
		//else{
		if (Cube.Input!=null){
			if (IsVertex)
			PixCol = "texCUBElod("+Cube.Input.Get()+",float4("+Map+",0))";
			else
			PixCol = "texCUBE("+Cube.Input.Get()+","+Map+")";
		}
		else
		PixCol = "float4(1,1,1,1)";
		//}		
	}
	if (LayerType.Type == (int)LayerTypes.Noise){
		if (NoiseType.Type==0){
			if (NoiseDim.Type==0)
			PixCol = "Noise2D("+Map+")";
			if (NoiseDim.Type==1)
			PixCol = "Noise3D("+Map+")";	

			//if (NoiseClamp==true)
			PixCol = "("+PixCol+"+1)/2";
		}
		if (NoiseType.Type==1){
			if (NoiseDim.Type==0)
			PixCol = "NoiseB2D("+Map+")";
			if (NoiseDim.Type==1)
			PixCol = "NoiseB3D("+Map+")";	

			//if (NoiseClamp==true)
			//PixCol = "("+PixCol+"+1)/2";
		}
		if (NoiseType.Type==2){
			if (NoiseDim.Type==0)
			PixCol = "NoiseC2D("+Map+",float2("+NoiseA.Get()+","+NoiseB.Get()+"))";
			if (NoiseDim.Type==1)
			PixCol = "NoiseC3D("+Map+",float2("+NoiseA.Get()+","+NoiseB.Get()+"))";	

			//if (NoiseClamp==true)
			//PixCol = "("+PixCol+"+1)/2";
		}
		if (NoiseType.Type==3){
			if (NoiseDim.Type==0)
			PixCol = "NoiseD2D("+Map+","+NoiseA.Get()+")";
			if (NoiseDim.Type==1)
			PixCol = "NoiseD3D("+Map+","+NoiseA.Get()+")";	
		}
		if (NoiseType.Type==4){
			string f = "1";
			if (NoiseC.On)
			f = "2";
			if (NoiseDim.Type==0)
			PixCol = "NoiseE2D("+Map+",float3("+NoiseA.Get()+","+NoiseB.Get()+","+f+"))";
			if (NoiseDim.Type==1)
			PixCol = "NoiseE3D("+Map+",float3("+NoiseA.Get()+","+NoiseB.Get()+","+f+"))";	
		}
		
		PixCol = "(float("+PixCol+").rrrr)";
	}
	if (LayerType.Type == (int)LayerTypes.Literal){
		//PixCol = "float4("+Map+","+Map+","+Map+","+Map+")";
		PixCol = "float4("+Map+",1)";
	}
	if (LayerType.Type == (int)LayerTypes.GrabDepth){
		if (SpecialType.Type==0)
		PixCol = "tex2D( _GrabTexture, "+Map+")";
		else{
			if (LinearizeDepth.On)
			PixCol = "(LinearEyeDepth(tex2D(_CameraDepthTexture, "+Map+").r).rrrr)";
			else
			PixCol = "(tex2D(_CameraDepthTexture, "+Map+").rrrr)";
		}
	}
	if (LayerType.Type == (int)LayerTypes.Previous){
		if (Parent.EndTag.Text.Length==1)
		PixCol = "float4("+Parent.CodeName+".rrr,1)";
		else
		if (Parent.EndTag.Text.Length==4)
		PixCol = Parent.CodeName;
		else
		PixCol = "float4("+Parent.CodeName+",0)";
	}
	if ((LayerType.Type!=(int)LayerTypes.Previous)&&(!Map.Contains("*(depth)"))){
		if (!SG.UsedBases.ContainsKey(PixCol))
		SG.UsedBases.Add(PixCol,1);
		else
		SG.UsedBases[PixCol]+=1;
	}
	foreach(ShaderEffect SE2 in LayerEffects){
		if (SE2.Visible){
			if (ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase")!=null){
				if (ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase").GetParameters().Length==4)
				PixCol = (string)ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase").Invoke(null,new object[]{SE2,this,PixCol,Map});
				else
				PixCol = (string)ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase").Invoke(null,new object[]{SG,SE2,this,PixCol,Map});
			}
		}
	}
	return PixCol;
}
public string StartNewBranch(ShaderGenerate SG,string Map,int Effect){
	SampleCount+=1;
	int Branch = SampleCount;
	//UnityEngine.Debug.Log(Effect.ToString()+": " + Map);
	for(int i = Effect;i<LayerEffects.Count;i+=1){
		
		ShaderEffect SE = LayerEffects[i];
		//UnityEngine.Debug.Log(Effect.ToString()+": "+SE.TypeS);
		if (!SE.Visible)
		continue;
		
		var Meth = ShaderEffect.GetMethod(SE.TypeS,"Generate");
		var Meth2 = ShaderEffect.GetMethod(SE.TypeS,"GenerateAlpha");
		var Meth3 = ShaderEffect.GetMethod(SE.TypeS,"GenerateWAlpha");
		string EffectString = "";
		int OldShaderCodeEffectsLength = ShaderCodeEffects.Length;
		if (SE.UseAlpha.Float==0&&Meth!=null)
			EffectString = GetSampleName(Branch)+".rgb = "+(string)Meth.Invoke(null,new object[]{SG,SE,this,GetSampleName(Branch)+".rgb",i+1})+";\n";
		if (SE.UseAlpha.Float==1&&Meth3!=null)
			EffectString = GetSampleName(Branch)+" = "+(string)Meth3.Invoke(null,new object[]{SG,SE,this,GetSampleName(Branch),i+1})+";\n";
		if (SE.UseAlpha.Float==2&&Meth2!=null)
			EffectString = GetSampleName(Branch)+".a = "+(string)Meth2.Invoke(null,new object[]{SG,SE,this,GetSampleName(Branch)+".a",i+1})+";\n";
		ShaderCodeEffects=ShaderCodeEffects.Insert(ShaderCodeEffects.Length-OldShaderCodeEffectsLength,EffectString);
	}

	string PixCol = GCPixelBase(SG,Map);

	ShaderCodeSamplers += "				half4 "+GetSampleName(Branch)+" = "+PixCol+";\n";
	return GetSampleName(Branch);
}
public string GetSubPixel(ShaderGenerate SG,string Map,int Effect,int Branch){
	//UnityEngine.Debug.Log(ShaderCode);
	/*int AllowedToUseFloat = 3;
	bool AlphaChannel = (Parent.EndTag.Text.Length==1&&Parent.EndTag.Text=="a");
	if (UseAlpha.On)
	AllowedToUseFloat = 4;
	foreach(ShaderEffect SE2 in LayerEffects){
		if (SE2.UseAlpha.Float!=0)
		AllowedToUseFloat = 4;
	}
	if (AlphaChannel)
	AllowedToUseFloat = 4;
	if (LayerType.Type==(int)LayerTypes.Previous)
	AllowedToUseFloat = Parent.EndTag.Text.Length;*/
	
	ShaderEffect SE = null;
	if (LayerEffects.Count>Effect){
		SE = LayerEffects[Effect];
		if (!SE.Visible)
		return GetSubPixel(SG,Map,Effect+1,Branch);
		
		var Meth = ShaderEffect.GetMethod(SE.TypeS,"Generate");
		var Meth2 = ShaderEffect.GetMethod(SE.TypeS,"GenerateAlpha");
		var Meth3 = ShaderEffect.GetMethod(SE.TypeS,"GenerateWAlpha");
		string EffectString = "";
		if (SE.UseAlpha.Float==0)
			EffectString = GetSampleName(Branch)+".rgb = "+(string)Meth.Invoke(null,new object[]{SG,SE,this,GetSampleName(Branch)+".rgb",Effect+1})+";\n";
		if (SE.UseAlpha.Float==1)
			EffectString = GetSampleName(Branch)+" = "+(string)Meth3.Invoke(null,new object[]{SG,SE,this,GetSampleName(Branch),Effect+1})+";\n";
		if (SE.UseAlpha.Float==2)
			EffectString = GetSampleName(Branch)+".a = "+(string)Meth2.Invoke(null,new object[]{SG,SE,this,GetSampleName(Branch)+".a",Effect+1})+";\n";
		ShaderCodeEffects=ShaderCodeEffects+EffectString;
		return GetSubPixel(SG,Map,Effect+1,Branch);
	}
	if (!SpawnedBranches.Contains(GetSampleName(Branch))){
		//SpawnedBranches.Add(GetSampleName(Branch));

		
		string PixCol = GCPixelBase(SG,Map);
		foreach(ShaderEffect SE2 in LayerEffects){
			if (ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase")!=null){
				if (ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase").GetParameters().Length==4)
					return (string)ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase").Invoke(null,new object[]{SE2,this,PixCol,Map});
				else
					return (string)ShaderEffect.GetMethod(SE2.TypeS,"GenerateBase").Invoke(null,new object[]{SG,SE2,this,PixCol,Map});
			}
		}
		ShaderCodeSamplers = "				half4 "+GetSampleName(Branch)+" = "+PixCol+";\n"+ShaderCodeSamplers;
	}
	string RetName = GetSampleName(Branch);
	return RetName;
}
public string GetSampleName(int Branch){
	return ShaderUtil.CodeName(Name.Text+Parent.NameUnique.Text)+"_Sample"+Branch.ToString();//SampleCount.ToString();
}
public string GetSampleNameFirst(){
	return ShaderUtil.CodeName(Name.Text+Parent.NameUnique.Text)+"_Sample1";
}
public string GCPixel(ShaderGenerate SG,string Map){
	ShaderCodeSamplers = "";
	ShaderCodeEffects = "";
	SampleCount = 0;
	SpawnedBranches = new List<string>();
//	string PixCol;
	//if (UseEffects){
		LayerEffects.Reverse();
		StartNewBranch(SG,Map,0);
		LayerEffects.Reverse();
	//}
	bool NoEffects = true;
	foreach(ShaderEffect SE in LayerEffects){
		if (SE.Visible&&!SE.IsUVEffect)
		NoEffects = false;
	}
	return "		//Generate Layer: "+Name.Text+"\n			//Sample parts of the layer:\n"+ShaderCodeSamplers+(NoEffects?"":"\n			//Apply Effects:\n")+ReplaceLastOccurrence("				"+ShaderCodeEffects.Replace("\n","\n				"),"\n				","\n");
}
public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
{
        int place = Source.LastIndexOf(Find);

        if(place == -1)
           return string.Empty;

        string result = Source.Remove(place, Find.Length).Insert(place, Replace);
        return result;
}
public string GCCalculateMix(ShaderGenerate SG, string CodeName,string PixelColor,string Function,int ShaderNumber){//NormalDot
	string MixColor = "";
	string NormalsMixAddon1 = "3";
	string NormalsMixAddon2 = "";
	if (Parent.EndTag.Text.Length==4){
	NormalsMixAddon1 = "4";
	NormalsMixAddon2 = ",FSPC.ETFE.w";
	}
	
	//normalize(float3(Tex1.xy + Tex2.xy, Tex1.z*Tex2.z))
	//o.Albedo= ((normalize(float3(((Texture2_Sample1.rgb.xy+o.Albedo.xy)-1)*2,Texture2_Sample1.rgb.z*o.Albedo.z))/2)+0.5);
	string[] UseAlphaFalse = new string[]{"= FSPC.ETFE","+= FSPC.ETFE","-= FSPC.ETFE","*= FSPC.ETFE","/= FSPC.ETFE","= max(CN,FSPC.ETFE)","= min(CN,FSPC.ETFE)","= normalize(float"+NormalsMixAddon1+"(FSPC.ETFE.xy+CN.xy,FSPC.ETFE.z"+NormalsMixAddon2+"))","= dot(CN,FSPC.ETFE)"};//Unfinished
	string[] UseAlphaTrue = new string[]{"= lerp(CN,FSPC.ETFE,PCL)","+= FSPC.ETFE*PCL","-= FSPC.ETFE*PCL","= lerp(CN,CN*FSPC.ETFE,PCL)","= lerp(CN,CN/FSPC.ETFE,PCL)","= lerp(CN,max(CN,FSPC.ETFE),PCL)","= lerp(CN,min(CN,FSPC.ETFE),PCL)","= lerp(CN,normalize(float"+NormalsMixAddon1+"(FSPC.ETFE.xy+CN.xy,FSPC.ETFE.z"+NormalsMixAddon2+")),PCL)","= lerp(CN,dot(CN,FSPC.ETFE),PCL)"};//Unfinished	
	//string[] UseAlphaTrue = new string[]{"= lerp(CN,FSPC.ETFE,PCL)","+= FSPC.ETFE*PCL","-= FSPC.ETFE*PCL","= lerp(CN,CN*FSPC.ETFE,PCL)","/= FEPC.ETFS*PCL","= lerp(CN,max(CN,FSPC.ETFE),PCL)","= lerp(CN,min(CN,FSPC.ETFE),PCL)","= FSPCFE","= FSPCFE"};//Unfinished	
	if (IsVertex&&!Parent.IsMask.On){
		if ((VertexMasks)(int)VertexMask.Float==VertexMasks.Normal)
		PixelColor = "(("+PixelColor+")*float4(v.normal.rgb,1))";
		if ((VertexMasks)(int)VertexMask.Float==VertexMasks.Position)
		PixelColor = "(("+PixelColor+")*v.vertex)";
		//if ((VertexMasks)(int)VertexMask.Float==VertexMasks.View)
		//PixelColor = "(("+PixelColor+")*float4(normalize(UnityWorldSpaceViewDir(mul(_Object2World, v.vertex).xyz)),0))";
	}	
	bool Lerp = false;
	if (MixAmount.Get()!="0")
	{
		if (UseAlpha.On)//&&ShaderNumber!=1)
		Lerp = true;
		if (!MixAmount.Safe())
		Lerp = true;
		if (Stencil.Obj!=null)
		Lerp = true;
		if (UseFadeout.On)
		Lerp = true;
		
		
		if (Lerp == false)
			MixColor = UseAlphaFalse[MixType.Type];
		if (Lerp == true)
			MixColor = UseAlphaTrue[MixType.Type];
		
		/*if (MixAmount.Safe()&&UseAlpha.On)
		MixColor = MixColor.Replace("PCLI",PixelColor+".a");
		if (!MixAmount.Safe()&&UseAlpha.On)
		MixColor = MixColor.Replace("PCLI",PixelColor+".a*(1-"+MixAmount.Get()+")");
		if (MixAmount.Safe()&&!UseAlpha.On)
		MixColor = MixColor.Replace("PCLI","(1-"+MixAmount.Get()+")");	*/
		
		string Repl = "";
		string MulAdd = "";
		if (UseAlpha.On){
		Repl+=MulAdd+PixelColor+".a";MulAdd="*";}
		if (!MixAmount.Safe()){
		Repl+=MulAdd+MixAmount.Get();MulAdd="*";}
		if (Stencil.Obj!=null){
		Repl+=MulAdd+(((ShaderLayerList)Stencil.Obj).CodeName+Stencil.MaskColorComponentS);MulAdd="*";}
		if (UseFadeout.On){
		Repl+=MulAdd+("(1-saturate((IN.screenPos.z-("+FadeoutMin.Get()+"))/("+FadeoutMax.Get()+"-"+FadeoutMin.Get()+")))");MulAdd="*";}
		
		MixColor = MixColor.Replace("PCL",Repl);
		
		MixColor = MixColor.Replace("CN",CodeName);
		MixColor = MixColor.Replace("PC",PixelColor);	
//		UnityEngine.Debug.Log(Parent.Name.Text+":"+Parent.EndTag.Text);
		if (Parent.EndTag.Text!="")
		MixColor = MixColor.Replace("ET",Parent.EndTag.Text);
		else
		MixColor = MixColor.Replace(".ET","");
		
		if (Function!="")
		MixColor = MixColor.Replace("FS",Function+"(").Replace("FE",")");
		else
		MixColor = MixColor.Replace("FS","").Replace("FE","");
		
		MixColor = CodeName+" "+MixColor;

	}
	else
	return MixColor = "			//The layer has a Mix Amount of 0, which means forget about it :)\n";
	

	if (MixType.Type==0&&!Lerp)
	return "			//Set the "+(Parent.IsMask.On?"mask":"channel")+" to the new color\n				"+MixColor+";";
	return "			//Blend the layer into the channel using the "+MixType.Names[MixType.Type]+" blend mode\n				"+MixColor+";";

}
public Dictionary<string,ShaderVar> GetSaveLoadDict(){
	Dictionary<string,ShaderVar> D = new Dictionary<string,ShaderVar>();

	D.Add(Name.Name,Name);
	D.Add(LayerType.Name,LayerType);
	D.Add(Color.Name,Color);
	D.Add(Color2.Name,Color2);
	D.Add(Image.Name,Image);
	D.Add(Cube.Name,Cube);
	D.Add(NoiseType.Name,NoiseType);
	D.Add(NoiseDim.Name,NoiseDim);
	D.Add(NoiseA.Name,NoiseA);
	D.Add(NoiseB.Name,NoiseB);
	D.Add(NoiseC.Name,NoiseC);
	D.Add(LightData.Name,LightData);
	D.Add(SpecialType.Name,SpecialType);
	D.Add(LinearizeDepth.Name,LinearizeDepth);
	
	D.Add(MapType.Name,MapType);
	D.Add(MapLocal.Name,MapLocal);
	
	D.Add(UseAlpha.Name,UseAlpha);
	D.Add(MixAmount.Name,MixAmount);

	D.Add(UseFadeout.Name,UseFadeout);
	D.Add(FadeoutMinL.Name,FadeoutMinL);
	D.Add(FadeoutMaxL.Name,FadeoutMaxL);
	D.Add(FadeoutMin.Name,FadeoutMin);
	D.Add(FadeoutMax.Name,FadeoutMax);
	
	D.Add(MixType.Name,MixType);
	D.Add(Stencil.Name,Stencil);
	Stencil.SetToMasks(Parent,0);
	D.Add(VertexMask.Name,VertexMask);
	return D;
}
public ShaderLayer Copy(){
	return Load(new StringReader(Save()));
}
public string Save(){
	string S = "BeginShaderLayer\n";

	S += ShaderUtil.SaveDict(GetSaveLoadDict());
	foreach(ShaderEffect SE in LayerEffects){
		S += SE.Save();
	}
	S += "EndShaderLayer\n";
	return S;
}
static public ShaderLayer Load(StringReader S){
	ShaderLayer SL = ShaderLayer.CreateInstance<ShaderLayer>();//UpdateGradient
	var D = SL.GetSaveLoadDict();
	while(1==1){
		string Line =  ShaderUtil.Sanitize(S.ReadLine());

		if (Line!=null){
			if(Line=="EndShaderLayer")break;
			
			if (Line.Contains("#!"))
			ShaderUtil.LoadLine(D,Line);
			else
			if (Line=="BeginShaderEffect")
			SL.LayerEffects.Add(ShaderEffect.Load(S));
		}
		else
		break;
	}
	SL.GetTexture();
	return SL;
}
}