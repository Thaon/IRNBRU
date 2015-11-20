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
[System.Serializable]
public enum Types {
	Vec,
	Float,
	Type,
	Toggle,
	Texture,
	Cubemap,
	ObjectArray,
	Text
};
public enum DrawTypes {Color,Slider01,Int,Float,Type,Toggle,Texture,Cubemap,ObjectArray,Text};

[System.Serializable]
public class ShaderVar{

	public delegate void ChangeDelegate();
	public ChangeDelegate OnChange;
	
	[DataMember]
	public ShaderColor Vector;//{ get; set; }
	public string Text = "";
	[XmlIgnore,NonSerialized] public Texture2D Image;
	public string ImageGUID = "";
	[XmlIgnore,NonSerialized] public Cubemap Cube;
	public string CubeGUID = "";
	public float Float;
	public ShaderInput Input;
	public int ColorComponent = 0;
	public bool RGBAMasks = false;
	public bool LightingMasks = false;
	public int MaskColorComponent = 0;
	public string MaskColorComponentS{
	get{
		if (((ShaderLayerList)Obj).EndTag.Text.Length>1){
			return (new string[]{".r",".g",".b",".a"})[MaskColorComponent];
		}
		return "";
	}
	}
	public bool UseInput = false;
	public bool Hidden = false;
	public bool Use = true;
	public bool Editing = false;
	public bool NoSlider = false;
	public bool NoArrows = false;
	public bool ForceGUIChange = false;
	public float EditingPopup = 0f;
	public double EditingPopupStartTime = 0f;
	public ShaderObjectField ObjField;
	public List<object> ObjFieldObject;
	public List<Texture2D> ObjFieldImage;
	public List<bool> ObjFieldEnabled;
	public object RealObj;
	public object Obj{
		set {RealObj = value;}
		get {
			if (RealObj!=null){
				if (ObjFieldObject.IndexOf(RealObj)!=-1)
				return RealObj;
			}
			if (ObjFieldObject!=null&&ObjFieldObject.Count>RealSelected&&RealSelected>=0)
			return ObjFieldObject[RealSelected];
			return null;
		}
	}
	
	public object OldObj = null;
	public int RealSelected = -1;
	public int Selected{
		get {if (ObjFieldObject!=null){RealSelected = ObjFieldObject.IndexOf(Obj);return ObjFieldObject.IndexOf(Obj);}return -1;}
		set {
			if (ObjFieldObject!=null&&ObjFieldObject.Count>value){
				if (value>=0)
				Obj = ObjFieldObject[value];
				else
				Obj = null;
			}
			RealSelected = value;
		}
	}
	public bool ObjFieldOn = false;
	public int Type;
	public Rect LastUsedRect;
	[XmlArrayAttribute]public string[] Names;
	[XmlArrayAttribute]public string[] CodeNames;
	[XmlArrayAttribute]public string[] Descriptions;
	
	
	[XmlArrayAttribute]public string[] ImagePaths;
	[XmlArrayAttribute,XmlIgnore,NonSerialized]public Texture2D[] Images;
	public bool On;
	
	public int TypeDispL = 4;
	
	public float Range0 = 0;
	public float Range1 = 1;
	
	public Types CType;
	
	public bool NoInputs = false;
	
	public string Name = "";
	
	
	public string WarningTitle = "";
	public string WarningMessage = "";
	public string WarningOption1 = "";
	public string WarningOption2 = "";
	public string WarningOption3 = "";	
	public delegate void WarningDelegateReal(int Option, ShaderVar SV);
	public WarningDelegateReal WarningDelegate;	
	
	public void WarningSetup(string Title, string Message, string Option1,string Option2,string Option3,WarningDelegateReal Delegate){
		WarningTitle = Title;
		WarningMessage = Message;
		WarningOption1 = Option1;
		WarningOption2 = Option2;
		WarningOption3 = Option3;
		WarningDelegate = Delegate;
	}
	public void WarningSetup(string Title, string Message, string Option1,string Option2,WarningDelegateReal Delegate){
		WarningTitle = Title;
		WarningMessage = Message;
		WarningOption1 = Option1;
		WarningOption2 = Option2;
		WarningDelegate = Delegate;
	}
	
	
	[XmlIgnore,NonSerialized]public ShaderLayer MyParent_Real;
	public ShaderLayer MyParent{
		get{
			if (MyParent_Real==null){
				foreach(ShaderLayer SL in ShaderUtil.GetAllLayers())
				SL.UpdateShaderVars(true);
			}
			
			return MyParent_Real;
		}
		set{
			MyParent_Real = value;
		}
	
	}
	
	public bool Safe(){
		if (Get()!="0"&&Get()!="1")
		return false;
		
		return true;
	}
	
	public string Get(){
		return Get(0);
	}
	public string Get(float Wrap){
		ShaderGenerate SG = ShaderBase.Current.SG;
		if (Input!=null)
		{
			if (Input.SpecialType!=InputSpecialTypes.None&&!Input.InEditor){
				UpdateToInput(false);
				string[] SInputTypes = new string[]{"None","_SSTime.y","_SSTime.z","_SSTime.y/2","_SSTime.y/6","_SSSinTime.w","sin(_SSTime.z)","_SSSinTime.z","_SSCosTime.w","cos(_SSTime.z)","_SSCosTime.z","SSShellDepth","SSParallaxDepth","((_SSSinTime.w+1)/2)","((sin(_SSTime.z)+1)/2)","((_SSSinTime.z+1)/2)","((_SSCosTime.w+1)/2)","((cos(_SSTime.z)+1)/2)","((_SSCosTime.z+1)/2))"};
				if (SG!=null&&!SG.Temp)
				SInputTypes = new string[]{"None","_Time.y","_Time.z","_Time.y/2","_Time.y/6","_SinTime.w","sin(_Time.z)","_SinTime.z","_CosTime.w","cos(_Time.z)","_CosTime.z","SSShellDepth","SSParallaxDepth","((_SinTime.w+1)/2)","((sin(_Time.z)+1)/2)","((_SinTime.z+1)/2)","((_CosTime.w+1)/2)","((cos(_Time.z)+1)/2)","((_CosTime.z+1)/2)"};
				
				InputSpecialTypes[] EInputTypes2 = new InputSpecialTypes[]{InputSpecialTypes.None,InputSpecialTypes.Time,InputSpecialTypes.TimeFast,InputSpecialTypes.TimeSlow,InputSpecialTypes.TimeVerySlow,InputSpecialTypes.SinTime,InputSpecialTypes.SinTimeFast,InputSpecialTypes.SinTimeSlow,InputSpecialTypes.CosTime,InputSpecialTypes.CosTimeFast,InputSpecialTypes.CosTimeSlow,InputSpecialTypes.ShellDepth,InputSpecialTypes.ParallaxDepth,InputSpecialTypes.ClampedSinTime,InputSpecialTypes.ClampedSinTimeFast,InputSpecialTypes.ClampedSinTimeSlow,InputSpecialTypes.ClampedCosTime,InputSpecialTypes.ClampedCosTimeFast,InputSpecialTypes.ClampedCosTimeSlow};
				int Index = Array.IndexOf(EInputTypes2,Input.SpecialType);
				if (Index!=0)
				return SInputTypes[Index];
			}
			else
			if (Input.InEditor||ShaderBase.Current.SG.Temp){
				if (!(CType==Types.Float&&Input.Type==1))
				return Input.Get();
				else
				return Input.Get()+"."+(new string[]{"r","g","b","a"}[ColorComponent]);
			}
		}

		if (CType==Types.Vec)
		return "float4("+Vector.r.ToString()+", "+Vector.g.ToString()+", "+Vector.b.ToString()+", "+Vector.a.ToString()+")";
		
		if (CType==Types.Float&&Wrap==0)
		return Float.ToString();
		else
		if (CType==Types.Float)
		return (Float%Wrap).ToString();
		
		return "Error";
	}
	
	public ShaderVar()
	{
		
	}
	public void Update(string[] N,string[] D)
	{
		Names = N;
		Descriptions = D;
		TypeDispL = Names.Length;
	}
	public void Update(string[] N,string[] D,string[] CN)
	{
		Names = N;
		CodeNames = CN;
		Descriptions = D;
		TypeDispL = Names.Length;
	}
	public void Update(string[] N,string[] D,int S)
	{
		Names = N;
		Descriptions = D;
		TypeDispL = S;
	}
	///////////////////
	public ShaderVar(string Nam,Vector4 Vec)
	{
		Vector = new ShaderColor(Vec);
		CType = Types.Vec;
		Name = Nam;
	}
	public ShaderVar(string Nam,string s)
	{
		if (s=="Texture2D")
			CType = Types.Texture;
		else
		if (s=="Cubemap")
			CType = Types.Cubemap;
		else
		if (s=="ListOfObjects")
			CType = Types.ObjectArray;
		else{
			CType = Types.Text;
			Text = s;
		}
		
		Name = Nam;
		
	}
	public ShaderVar(string Nam,bool b)
	{
		On = b;
		CType = Types.Toggle;
		Name = Nam;
		
	}
	public ShaderVar(string Nam,float flo,float R0,float R1)
	{
		Float = flo;
		CType = Types.Float;
		Name = Nam;
		Range0 = R0;
		Range1 = R1;
	}
	public ShaderVar(string Nam,float flo)
	{
		Float = flo;
		CType = Types.Float;
		Name = Nam;
	}
	public ShaderVar(string Nam,string[] N,string[] D)
	{
		Names = N;
		Descriptions = D;
		CType = Types.Type;
		TypeDispL = Names.Length;
		Name = Nam;
	}	
	public ShaderVar(string Nam,string[] N,string[] IP,string ASD)
	{
		Names = N;
		ImagePaths = IP;
		CType = Types.Type;
		TypeDispL = Names.Length;
		Name = Nam;
	}		
	public ShaderVar(string Nam,string[] N,string[] IP,string ASD,string[] D)
	{
		Names = N;
		ImagePaths = IP;
		CType = Types.Type;
		TypeDispL = Names.Length;
		Descriptions = D;
		Name = Nam;
	}	
	public ShaderVar(string Nam,string[] N,string[] D,string[] CN)
	{
		Names = N;
		CodeNames = CN;
		Descriptions = D;
		CType = Types.Type;
		TypeDispL = Names.Length;
		Name = Nam;
	}	
	public ShaderVar(string Nam,string[] N,string[] D,int S)
	{
		Names = N;
		Descriptions = D;
		CType = Types.Type;
		TypeDispL = S;
		Name = Nam;
		
	}			
	
	
	
	
	public bool Draw(Rect rect)
	{
		if (CType==Types.Vec)
		return Draw_Real(rect,DrawTypes.Color,"");
		if (CType==Types.Float)
		return Draw_Real(rect,DrawTypes.Slider01,"");
		if (CType==Types.Type)
		return Draw_Real(rect,DrawTypes.Type,"");
		if (CType==Types.Toggle)
		return Draw_Real(rect,DrawTypes.Toggle,"");
		if (CType==Types.Texture)
		return Draw_Real(rect,DrawTypes.Texture,"");
		if (CType==Types.Cubemap)
		return Draw_Real(rect,DrawTypes.Cubemap,"");
		return false;
	}	
	public bool Draw(Rect rect,string S)
	{
		if (CType==Types.Vec)
		return Draw_Real(rect,DrawTypes.Color,S);
		if (CType==Types.Float)
		return Draw_Real(rect,DrawTypes.Slider01,S);
		if (CType==Types.Type)
		return Draw_Real(rect,DrawTypes.Type,S);
		if (CType==Types.Toggle)
		return Draw_Real(rect,DrawTypes.Toggle,S);
		if (CType==Types.Texture)
		return Draw_Real(rect,DrawTypes.Texture,S);
		if (CType==Types.Cubemap)
		return Draw_Real(rect,DrawTypes.Cubemap,S);
		if (CType==Types.ObjectArray)
		return Draw_Real(rect,DrawTypes.ObjectArray,S);
		
		return false;
	}
	public bool Draw(Rect rect,string S,DrawTypes d)
	{
		return Draw_Real(rect,d,S);
	}
	public bool DrawPicType(Rect rect,Texture2D Tex,string Na){
	return DrawPicType_Real(rect,Tex,Na,true);
	}
	public bool DrawPicType(Rect rect,Texture2D Tex,string Na,bool Alp){
	return DrawPicType_Real(rect,Tex,Na,Alp);
	}
	public bool DrawPicType_Real(Rect rect,Texture2D Tex,string Na,bool Alp){
	if (NoInputs==true)
	UseInput = false;
		Rect InitialRect = rect;
		GUI.Box(rect,"",GUI.skin.button);
		rect.x+=1;
		rect.y+=1;
		rect.width-=2;
		rect.height-=2;
		GUI.DrawTexture( rect ,Tex);
		if (Alp)
		{
		GUI.DrawTexture( rect ,Tex);
		GUI.DrawTexture( rect ,Tex);
		GUI.DrawTexture( rect ,Tex);
		}
		rect = InitialRect;
		rect.y+=rect.height-20;
		rect.height = 20;
		GUI.Box(rect,"",GUI.skin.button);
		GUI.skin.label.alignment = TextAnchor.UpperCenter;
		SU.Label(rect,Na,12);
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		
		rect.y+=20;//rect.height;
		rect.height = 30;
		rect.width = 40;
		if(GUI.Button( rect ,ShaderSandwich.LeftArrow))Type-=1;
		
		rect.x+=InitialRect.width-40;
		if(GUI.Button( rect ,ShaderSandwich.RightArrow))Type+=1;
		
		return false;
	}
	public void UseEditingMouse(bool Paint){
		Rect rect = LastUsedRect;
		rect.x-=150-LabelOffset;
		ShaderInput OrigInput = Input;
		int OrigComponent = ColorComponent;
		if (EditingPopup>0f){
			
			if ((Event.current.type == EventType.MouseDown) &&(
			//!(new Rect(rect.x+110,rect.y,20,20).Contains(Event.current.mousePosition))&&
			!(new Rect(rect.x+110,rect.y-(20*EditingPopup)-20,110,40).Contains(Event.current.mousePosition))
			)){
				ShaderSandwich.ShaderVarEditing = null;
				GUI.changed = true;
			}
		
			if (Paint==true){
				if (EditingPopup>0f){
				//Debug.Log(rect);
					GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,EditingPopup);
					ShaderUtil.BeginGroup(new Rect(rect.x+110,rect.y-(20*EditingPopup)-20,120,40),GUI.skin.button);

					GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
					ButtonStyle.padding = new RectOffset(0,0,0,0);
					ButtonStyle.margin = new RectOffset(0,0,0,0);

					List<string> InputNamesList = new List<string>();
					List<int> InputIntsList = new List<int>();
					List<ShaderInput> InputInputsList = new List<ShaderInput>();
					List<int> InputComponentsList = new List<int>();
					InputNamesList.Add("-");
					InputIntsList.Add(0);
					InputInputsList.Add(null);
					InputComponentsList.Add(0);
					int ii = 1;
					foreach(ShaderInput SI in ShaderBase.Current.ShaderInputs){
						string SIType = "";
						if (SI.Type==0)
						SIType = "Tex";
						if (SI.Type==1)
						SIType = "Color";
						if (SI.Type==2)
						SIType = "Cube";
						if (SI.Type==3)
						SIType = "Float";
						if (SI.Type==4)
						SIType = "Range";
						if (
						(CType == Types.Float&&(SI.Type==3||SI.Type==4||SI.Type==1))||
						(CType == Types.Vec&&SI.Type==1)||
						(CType == Types.Texture&&SI.Type==0)||
						(CType == Types.Cubemap&&SI.Type==2)
						){
							if (CType == Types.Float&&SI.Type==1){
								InputNamesList.Add(SIType+": "+SI.VisName.Replace("-","/")+"/R");
								InputIntsList.Add(ii);
								InputInputsList.Add(SI);
								InputComponentsList.Add(0);
								ii++;
								InputNamesList.Add(SIType+": "+SI.VisName.Replace("-","/")+"/G");
								InputIntsList.Add(ii);
								InputInputsList.Add(SI);
								InputComponentsList.Add(1);
								ii++;
								InputNamesList.Add(SIType+": "+SI.VisName.Replace("-","/")+"/B");
								InputIntsList.Add(ii);
								InputInputsList.Add(SI);
								InputComponentsList.Add(2);
								ii++;
								InputNamesList.Add(SIType+": "+SI.VisName.Replace("-","/")+"/A");
								InputIntsList.Add(ii);
								InputInputsList.Add(SI);
								InputComponentsList.Add(3);
								ii++;
							}
							else{
								InputNamesList.Add(SIType+": "+SI.VisName.Replace("-","/"));
								InputIntsList.Add(ii);
								InputInputsList.Add(SI);
								InputComponentsList.Add(0);
								ii++;
							}
						}
						
					}
					string[] InputNames = InputNamesList.ToArray();
					int[] InputInts = InputIntsList.ToArray();
					//InputNames[ii] = "Add New Input";
					
					int IndexOfInput = 0;
					if (InputInputsList.IndexOf(Input)!=-1)
					IndexOfInput = InputInputsList.IndexOf(Input)+ColorComponent;
					//IndexOfInput = ShaderBase.Current.ShaderInputs.IndexOf(Input)+1+ColorComponent;
					//Debug.Log(IndexOfInput);
					SU.Label(new Rect(5,5,110,15),"Inputs:",11);
					//IndexOfInput = 5;
					//int SIS = InputInts[EditorGUI.Popup(new Rect(5,20,75,15), Array.IndexOf(InputInts,IndexOfInput), InputNames,GUI.skin.GetStyle("MiniPopup"))];
					//IntInput = EditorGUI.Popup(new Rect(5,20,75,15),IntInput, InputNames,GUI.skin.GetStyle("MiniPopup"));
					//int SIS = IntInput;
					int SIS = EditorGUI.IntPopup(new Rect(5,20,75,15), IndexOfInput, InputNames,InputInts,GUI.skin.GetStyle("MiniPopup"));
					
					if (SIS>=InputInputsList.Count)
					SIS = 0;
					
					if (SIS==0)
					Input = null;
					else{
						Input = InputInputsList[SIS];
						ColorComponent = InputComponentsList[SIS];
					}
					//Debug.Log(SIS);
					//Debug.Log(ColorComponent);
					//Input = ShaderBase.Current.ShaderInputs[SIS-1];
					if (GUI.Button(new Rect(80,20,15,15),ShaderSandwich.Plus,ButtonStyle)||ShaderUtil.MouseUpIn(new Rect(80,20,15,15))){
						AddInput();
						ShaderSandwich.ShaderVarEditing = null;
						ShaderSandwich.ValueChanged = true;
						GUI.changed = true;
						ShaderSandwich.Instance.RegenShaderPreview();
						ShaderSandwich.Instance.UpdateShaderPreview();
					}
					if (GUI.Button(new Rect(100,20,15,15),ShaderSandwich.CrossRed,ButtonStyle)||ShaderUtil.MouseUpIn(new Rect(100,20,15,15))){
						/*ShaderBase.Current.ShaderInputs.Remove(Input);
						List<ShaderVar> SVs = new List<ShaderVar>();
						foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
							SL.UpdateShaderVars(true);
							SVs.AddRange(SL.ShaderVars);
						}
						SVs.AddRange(ShaderBase.Current.GetMyShaderVars());
						ShaderInput OldInput = Input;
						foreach(ShaderVar SV in SVs){
							if (SV.Input==OldInput)
							SV.Input = null;
						}*/
						if (Input!=null){
						ShaderInput OldInput = Input;
						Input = null;
						List<ShaderVar> SVs = new List<ShaderVar>();
						foreach (ShaderLayer SL in ShaderUtil.GetAllLayers()){
							SL.UpdateShaderVars(true);
							SVs.AddRange(SL.ShaderVars);
						}
						SVs.AddRange(ShaderBase.Current.GetMyShaderVars());
						OldInput.UsedCount = 0;
						foreach(ShaderVar SV in SVs){
							if (SV.Input==OldInput)
								OldInput.UsedCount+=1;
						}
						if (OldInput.UsedCount==0){
							ShaderBase.Current.ShaderInputs.Remove(OldInput);
							foreach(ShaderVar SV in SVs){
								if (SV.Input==OldInput)
									SV.Input = null;
							}
						}
						//UEObject.DestroyImmediate(SL,false);
						ShaderSandwich.ShaderVarEditing = null;
						EditorGUIUtility.ExitGUI();
						}
					}
					if (OrigInput!=Input||OrigComponent!=ColorComponent){
						UpdateToInput();
						GUI.changed = true;
						ShaderSandwich.Instance.ChangeSaveTemp(MyParent);
					}
					ShaderUtil.EndGroup();
				}

			}
		}
	}
	public ShaderInput AddInput(){
		Input = ShaderInput.CreateInstance<ShaderInput>();
		Input.AutoCreated = true;
		//Debug.Log(MyParent);
		if (MyParent!=null)
		Input.VisName = MyParent.Name.Text+" - "+Name;
		else
		Input.VisName = Name;
		ShaderBase.Current.ShaderInputs.Add(Input);
		ShaderBase.Current.RecalculateAutoInputs();
		Input.Number = Float;
		Input.Range0 = Range0;
		Input.Range1 = Range1;
		Input.Color = Vector;

		if (CType==Types.Vec)
		Input.Type=1;
		if (CType==Types.Float)
		if (NoSlider==false)
		Input.Type=4;
		else
		Input.Type=3;
		

		if (CType==Types.Texture){
			Input.Image = Image;
			Input.ImageGUID = ImageGUID;
			Input.Type = 0;
		}
		if (CType==Types.Cubemap){
			Input.Cube = Cube;
			Input.CubeGUID = CubeGUID;
			Input.Type = 2;
		}
		return Input;
	}
	void DrawEditingInterface(){
	
	}
	public void DrawGear(Rect rect){
		GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
		ButtonStyle.padding = new RectOffset(0,0,0,0);
		ButtonStyle.margin = new RectOffset(0,0,0,0);
		if (NoInputs==false){
			//if (Editing==false)
			//	Editing = GUI.Toggle(new Rect(rect.x+110,rect.y,20,20),Editing,new GUIContent(ShaderSandwich.Gear),ButtonStyle);
			//else
				if ((Event.current.type == EventType.MouseDown) &&(new Rect(rect.x+LabelOffset-30,rect.y,rect.height,rect.height).Contains(Event.current.mousePosition))){
				if (ShaderSandwich.ShaderVarEditing!=this){
				ShaderSandwich.ShaderVarEditing = this;
				EditingPopupStartTime = EditorApplication.timeSinceStartup;
				}
				else
				ShaderSandwich.ShaderVarEditing = null;
				}
				if (Input!=null)
				GUI.Toggle(new Rect(rect.x+LabelOffset-30,rect.y,rect.height,rect.height),ShaderSandwich.ShaderVarEditing==this,new GUIContent(ShaderSandwich.GearLinked),ButtonStyle);
				else
				GUI.Toggle(new Rect(rect.x+LabelOffset-30,rect.y,rect.height,rect.height),ShaderSandwich.ShaderVarEditing==this,new GUIContent(ShaderSandwich.Gear),ButtonStyle);
				

				
				if (Event.current.type==EventType.Repaint){
					if (ShaderSandwich.ShaderVarEditing==this){
						//EditingPopup+=0.03f;
						EditingPopup = Mathf.Min(1,(float)(EditorApplication.timeSinceStartup-EditingPopupStartTime)*6f);
					}
					else{
						//EditingPopup-=0.03f;
						EditingPopup = Mathf.Max(0,(float)(1-(EditorApplication.timeSinceStartup-EditingPopupStartTime))*6f);
					}
				}
				
				if (WarningTitle!=""&&WarningDelegate!=null)
				if (GUI.Button(new Rect(rect.x+LabelOffset-50-5,rect.y,20,20),new GUIContent(ShaderSandwich.Warning),ButtonStyle)){
				
				if (WarningOption3!="")
				WarningDelegate(EditorUtility.DisplayDialogComplex(WarningTitle,WarningMessage,WarningOption1,WarningOption2,WarningOption3),this);
				else
				WarningDelegate(EditorUtility.DisplayDialog(WarningTitle,WarningMessage,WarningOption1,WarningOption2) ? 0 : 1,this);
				}
				
				//GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1);
		}

	}
	public string GetMaskName(){
		if (Obj!=null)
		return ((ShaderLayerList)Obj).CodeName;
		return "float4(0.5f,0.5f,0.5f,0.5f)";
	}
	public int LabelOffset = 150;
	bool Draw_Real(Rect rect,DrawTypes d,string S)
	{
		if (ForceGUIChange==true){
			ForceGUIChange = false;
			GUI.changed = true;
		}	
		LastUsedRect = SU.AddRectVector(rect,ShaderUtil.GetGroupVector());
		bool RetVal = false;
		Rect InitialRect = rect;
		Color oldCol = GUI.backgroundColor;
		ShaderColor OldVector = Vector;
		RetVal = UpdateToInput(false);
		
		if (d == DrawTypes.Type)
		{
			if (ImagePaths==null||ImagePaths.Length==0){
				rect.height*=Mathf.Ceil((float)Names.Length/(float)TypeDispL);
				int oldType =  Type;
				Type = GUI.SelectionGrid(rect,Type,Names,TypeDispL);
				if (Type!=oldType)
				RetVal = true;
				rect.y+=rect.height;
				rect.height+=20;
				SU.Label(rect,Descriptions[Type],12);
			}
			else
			{
				//GUI.backgroundColor = new Color(0,0,0,1);
				
				if (Type>=Names.Length)
				Type = 0;				
				if (Type<0)
				Type = Names.Length-1;
				
				GUI.Box(rect,"",GUI.skin.button);
				if (Images==null)Images = new Texture2D[ImagePaths.Length];
				if (Images[Type]==null)Images[Type] = EditorGUIUtility.Load("Shader Sandwich/"+ImagePaths[Type]) as Texture2D;
				if (ImagePaths[Type]!=""){
					GUI.DrawTexture( rect ,Images[Type]);
					GUI.DrawTexture( rect ,Images[Type]);
					GUI.DrawTexture( rect ,Images[Type]);
					GUI.DrawTexture( rect ,Images[Type]);
				}
				
				rect.y+=rect.height-20;
				rect.height = 20;
				GUI.Box(rect,"",GUI.skin.button);
				GUI.skin.label.alignment = TextAnchor.UpperCenter;
				SU.Label(rect,Names[Type],12);
				GUI.skin.label.alignment = TextAnchor.UpperLeft;
				
				rect.y+=20;//rect.height;
				rect.height = 30;
				rect.width = 40;
				if(GUI.Button( rect ,ShaderSandwich.LeftArrow))Type-=1;
				
				rect.x+=InitialRect.width-40;
				if(GUI.Button( rect ,ShaderSandwich.RightArrow))Type+=1;
				
				if (Type>=Names.Length)
				Type = 0;				
				if (Type<0)
				Type = Names.Length-1;				
			}
		}
		else
		{
			if (S!="")
			{
				if (d != DrawTypes.Toggle)
				DrawGear(rect);
				
				GUI.backgroundColor = Color.white;
				SU.Label(rect,S,12);
				//UseInput = GUI.Toggle(new Rect(rect.x+120,rect.y+2,20,20),UseInput,"");
				rect.x+=LabelOffset;
				rect.width-=LabelOffset;
			}
			GUI.backgroundColor = Color.white;
			if (d == DrawTypes.Slider01){
				GUI.backgroundColor = new Color(0.2f,0.2f,0.2f,1f);

				
				if (NoSlider==false){
					//if (S!="")
					rect.width-=60-20;
					EditorGUI.BeginChangeCheck();
					Float = GUI.HorizontalSlider (rect,Float, Range0, Range1);
					if (EditorGUI.EndChangeCheck())
						EditorGUIUtility.editingTextField = false;
					//if (S!="")
					{
					//rect.width+=10;
					Float = EditorGUI.FloatField( new Rect(rect.x+rect.width+10,rect.y,30,20),Float,ShaderUtil.EditorFloatInput);
					}
				}
				else
				{
					rect.x-=5;
					rect.width+=6;
					EditorGUIUtility.labelWidth = 30;
					if (NoArrows)
					Float = EditorGUI.FloatField( rect,Float,ShaderUtil.EditorFloatInput);
					else
					Float = EditorGUI.FloatField( rect,"<>",Float,ShaderUtil.EditorFloatInput);
					EditorGUIUtility.labelWidth = 0;
					//GUI.Box(rect,"");
				}
			}
			if (d == DrawTypes.Color){
				
				Vector = new ShaderColor(EditorGUI.ColorField (rect,Vector.ToColor()));
				if (!OldVector.Cmp(Vector))
				RetVal = true;
			
			}
			if (d == DrawTypes.Texture){
				if (Image==null)
				Image = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(ImageGUID),typeof(Texture2D));
				
				Texture2D oldImage = Image;
				
				Image = (Texture2D) EditorGUI.ObjectField (rect,Image, typeof (Texture2D),false);
				ImageGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Image));
				if (oldImage!=Image)
				RetVal = true;
			}
			if (d == DrawTypes.Cubemap){
				if (Cube==null)
				Cube = (Cubemap)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(CubeGUID),typeof(Cubemap));
				
				Cubemap oldCube = Cube;
				
				Cube = (Cubemap) EditorGUI.ObjectField (rect,Cube, typeof (Cubemap),false);
				CubeGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Cube));
				if (oldCube!=Cube)
				RetVal = true;
			}
			if (d == DrawTypes.Toggle){
				//Debug.Log("Toggle!");
				rect.x-=30;//+=(rect.width-50);
				rect.width = 17;
				rect.height = 17;

				GUI.backgroundColor = new Color(0.2f,0.2f,0.2f,1f);
				
				GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
				ButtonStyle.padding = new RectOffset(0,0,0,0);
				ButtonStyle.margin = new RectOffset(0,0,0,0);
				
				if (On)
				On = GUI.Toggle(rect,On,ShaderSandwich.Tick,ButtonStyle);
				else
				On = GUI.Toggle(rect,On,ShaderSandwich.Cross,ButtonStyle);				
				//On = EditorGUI.Toggle (rect,On);
			
			}
			if (d == DrawTypes.ObjectArray){
				if (Event.current.type == EventType.Repaint)
				GUI.skin.GetStyle("ObjectFieldThumb").Draw(rect, false, false, false,ObjFieldOn); 
				if ((ObjField!=null)){
				//GUIUtility.hotControl = 0;
				ObjFieldOn = true;
				}
				if (ShaderSandwich.GUIMouseDown&&(rect.Contains(Event.current.mousePosition)))
				ObjFieldOn = true;
				if (GUIUtility.hotControl != 0&&ObjField==null)
				ObjFieldOn = false;
				//GUI.skin.GetStyle("ObjectFieldThumb").Draw(rect, bool isHover, bool isActive, bool on, bool hasKeyboardFocus); 
				Rect newRect = rect;
				newRect.width-=2;
				newRect.height-=2;
				newRect.y+=1;
				newRect.x+=1;
				if (Obj!=null)
				GUI.DrawTexture(newRect,ObjFieldImage[Selected]);
				rect.x+=rect.width-32;
				rect.width = 32;
				rect.y+=rect.height-8;
				rect.height = 8;
				if (GUI.Button(rect,"Select",GUI.skin.GetStyle("ObjectFieldThumbOverlay2"))){
					if (RGBAMasks){
					ObjField = ShaderObjectField.Show(this,"Select Mask (RGBA Masks Only!)",ObjFieldObject,ObjFieldImage,ObjFieldEnabled,Selected);
					ObjField.SomethingOtherThanAMask = true;
					}
					else
					ObjField = ShaderObjectField.Show(this,"Select Mask",ObjFieldObject,ObjFieldImage,ObjFieldEnabled,Selected);
				}
			}
			
		}
		UpdateToVar();
		
		GUI.backgroundColor = oldCol;
		
		if (RetVal&&OnChange!=null)
		OnChange();
		
		return RetVal;
	}

	public bool UpdateToInput(){
		return UpdateToInput(true);
	}
	public bool UpdateToInput(bool Execute){
		bool RetVal = false;
		if (Input!=null){
			
			float OldFloat =  Float;
			if (Input.Type==1&&CType==Types.Float){
				if (ColorComponent==0)
				Float = Input.Color.r;
				if (ColorComponent==1)
				Float = Input.Color.g;
				if (ColorComponent==2)
				Float = Input.Color.b;
				if (ColorComponent==3)
				Float = Input.Color.a;
			}
			else
			Float = Input.Number;
			
			if (!Mathf.Approximately(OldFloat,Float))
			RetVal = true;
			
			Range0 = Input.Range0;
			Range1 = Input.Range1;
			
			if (Vector!=null&&Input.Color!=null&&!Input.Color.Cmp(Vector)){
			RetVal = true;}//Debug.Log("Color");}
			Vector = Input.Color;
			
			if (Input.ImageS()!=Image)//Need to fix blahblah
			RetVal = true;
			Image = Input.Image;
			
			if (Input.CubeS()!=Cube){
			RetVal = true;}//Debug.Log("Cubemap");}
			Cube = Input.Cube;
		}
		if (RetVal&&OnChange!=null&&Execute){
		OnChange();}
		if (RetVal&&Execute){ShaderSandwich.ValueChanged = true;}
		return RetVal;
	}
	public void UpdateToVar(){
		if (Input!=null){
			
			if (Input.Type==1&&CType==Types.Float){
				//Vector = Vector.Copy();
				if (ColorComponent==0)
				Vector.r = Float;
				if (ColorComponent==1)
				Vector.g = Float;
				if (ColorComponent==2)
				Vector.b = Float;
				if (ColorComponent==3)
				Vector.a = Float;
			}
			else
			Input.Number = Float;
			
			Input.Range0 = Range0;
			Input.Range1 = Range1;
			Input.Color = Vector;
			Input.Image = Image;
			Input.Cube = Cube;
			Input.ImageGUID = ImageGUID;
			Input.CubeGUID = CubeGUID;
		}
	}
	public Texture2D ImageS(){
		if (Image==null&&(Input==null||(Input.Image!=null)))
			Image = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(ImageGUID),typeof(Texture2D));
		//Debug.Log(AssetDatabase.GUIDToAssetPath(ImageGUID));
		return Image;
	}
	public Cubemap CubeS(){
		if (Cube==null&&(Input==null||(Input.Cube!=null)))
			Cube = (Cubemap)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(CubeGUID),typeof(Cubemap));
		return Cube;
	}
	public bool NeedLinked(){
		if ((CType==Types.Texture&&Image!=null)||(CType==Types.Cubemap&&Cube!=null)){
			return true;
		}
		return false;
	}
	public Texture2D GetImage(int n){
		if (Images==null)Images = new Texture2D[ImagePaths.Length];
		if (Images[n]==null)
			Images[n] = EditorGUIUtility.Load("Shader Sandwich/"+ImagePaths[n]) as Texture2D;
		return Images[n];	
		
	}
/*	public ShaderColor Vector;//{ get; set; }
	public string ImageGUID = "";
	public string CubeGUID = "";
	public float Float;
	public ShaderInput Input;
	public ShaderObjectField ObjField;
	public int Type;
	
	public bool On;
	
	public int TypeDispL = 4;
	
	public float Range0 = 0;
	public float Range1 = 1;
	
	public Types CType;
	
public enum Types {
Vec,
Float,
Type,
Toggle,
Texture,
Cubemap,
ObjectArray};	
	*/	
	public string Save(){
	
		string S = "";
		//S+="";//+CType.ToString()+" #^ ";
		if (CType == Types.Vec)
		S += Vector.Save();
		if (CType == Types.Float)
		S += Float.ToString();
		if (CType == Types.Type)
		S += Type.ToString();
		if (CType == Types.Toggle)
		S += On.ToString();
		if (CType == Types.Texture)
		S += ImageGUID;
		if (CType == Types.Cubemap)
		S += CubeGUID;
		if (CType == Types.Text)
		S += Text;
		if (CType == Types.ObjectArray)
		S += Selected.ToString();
		//if (CType == Types.ObjectArray)
		//S += CubeGUID;
		
		S += " #^ CC"+ColorComponent.ToString();
		
		if (Input!=null)
		S += " #^ "+ShaderBase.Current.ShaderInputs.IndexOf(Input).ToString();
		
		S += " #?"+Name+"\n";

		return S;
	}
	public void Load (string S){
		S = ShaderUtil.Sanitize(S);
		string[] parts = S.Split(new string[] { "#^" }, StringSplitOptions.None);
//		Debug.Log(parts[0]);
		if (CType == Types.Vec)
		Vector.Load(parts[0]);
		if (CType == Types.Float)
		float.TryParse(parts[0],out Float);
		if (CType == Types.Type)
		Type = int.Parse(parts[0]);
		if (CType == Types.Toggle)
		On = bool.Parse(parts[0]);
		if (CType == Types.Text)
		Text = parts[0];
		if (CType == Types.Texture){
			ImageGUID = parts[0];
			Image = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(ImageGUID),typeof(Texture2D));
		}
		if (CType == Types.Cubemap){
			CubeGUID = parts[0];
			Cube = (Cubemap)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(CubeGUID),typeof(Cubemap));
		}
		//int ExpectedLength = 2;
		if (CType == Types.ObjectArray){
			Selected = int.Parse(parts[0]);
		}
		if (parts.Length>1){
			if (parts[1].Trim()!="AUTO"&&!parts[1].Trim().StartsWith("CC")){
				if (ShaderBase.Current.ShaderInputs.Count>int.Parse(parts[1].Trim()))
					Input = ShaderBase.Current.ShaderInputs[int.Parse(parts[1].Trim())];
			}
			//Debug.Log(parts[1].Trim());
			if (parts[1].Trim().StartsWith("CC")){
				ColorComponent = int.Parse(parts[1].Trim().Replace("CC",""));
			}
		}
		if (parts.Length==3){
			if (parts[2].Trim()!="AUTO"&&!parts[2].Trim().StartsWith("CC")){
				if (ShaderBase.Current.ShaderInputs.Count>int.Parse(parts[2].Trim()))
					Input = ShaderBase.Current.ShaderInputs[int.Parse(parts[2].Trim())];
			}
		}
		UpdateToInput();
	}
	public ShaderVar Copy(){
		ShaderVar SV = new ShaderVar();
		SV.Vector = Vector;
		SV.Image = Image;
		SV.ImageGUID = ImageGUID;
		SV.Cube = Cube;
		SV.CubeGUID = CubeGUID;
		SV.Float = Float;
		SV.Input = Input;
		SV.UseInput = UseInput;
		SV.Editing = Editing;
		SV.EditingPopup = EditingPopup;
		SV.ObjField = ObjField;
		SV.ObjFieldOn = ObjFieldOn;
		SV.Type = Type;
		SV.LastUsedRect = LastUsedRect;
		SV.Names = Names;
		SV.CodeNames = CodeNames;
		SV.Descriptions = Descriptions;
		
		
		SV.ImagePaths = ImagePaths;
		SV.Images = Images;
		SV.On = On;
		
		SV.TypeDispL = TypeDispL;
		
		SV.Range0 = Range0;
		SV.Range1 = Range1;
		
		SV.CType = CType;
		
		SV.NoInputs = NoInputs;
		
		SV.Name = Name;
		return SV;
	}
	public void WarningReset(){
	WarningTitle = "";
	WarningMessage = "";
	WarningOption1 = "";
	WarningOption2 = "";
	WarningOption3 = "";		
	}
	public void SetToMasks(ShaderLayerList SLL, int AllowRGB){
		if (ObjFieldObject==null)ObjFieldObject=new List<object>();
		if (ObjFieldImage==null)ObjFieldImage=new List<Texture2D>();
		if (ObjFieldEnabled==null)ObjFieldEnabled=new List<bool>();
		ObjFieldObject.Clear();
		ObjFieldImage.Clear();
		ObjFieldEnabled.Clear();
		bool Enab = true;
		foreach(ShaderLayerList SLL2 in ShaderSandwich.Instance.OpenShader.ShaderLayersMasks){
			if (SLL2==SLL)
			Enab = false;
			ObjFieldObject.Add(SLL2);
			ObjFieldImage.Add(SLL2.GetIcon());
			ObjFieldEnabled.Add(!(!Enab||((SLL2.EndTag.Text.Length!=4&&RGBAMasks))||(!LightingMasks&&SLL2.IsLighting.On)||(LightingMasks&&!SLL2.IsLighting.On)));
		}
	}
}