using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using SU = ShaderUtil;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
#pragma warning disable 0618
[System.Serializable]
public class ShaderObjectField : EditorWindow {

	public static ShaderObjectField Instance {
	get { return (ShaderObjectField)GetWindow(typeof (ShaderObjectField),false,"",false); }
	}
	public GUISkin SSSkin;
	public Vector2 Scroll;
	
	public string SearchText = "";
	public int Selected = -1;
	public List<object> ObjFieldObject;
	public List<Texture2D> ObjFieldImage;
	public List<bool> ObjFieldEnabled;
	public ShaderVar Caller = null;
	public bool SomethingOtherThanAMask = false;
	static public ShaderObjectField Show(ShaderVar SV,string Name,List<object> Objects,List<Texture2D> Images,List<bool> Bools,int Sel){
		ShaderObjectField windowG = (ShaderObjectField)ScriptableObject.CreateInstance(typeof(ShaderObjectField));//new ShaderObjectField();
		windowG.ShowAuxWindow();
		windowG.Caller = SV;
		//windowG.ShowUtility();
		windowG.title = Name;
		windowG.ObjFieldObject = Objects;
		windowG.ObjFieldImage = Images;
		windowG.ObjFieldEnabled = Bools;
		
		/*int X = -1;
		foreach(object obj in windowG.ObjFieldObject){
			X+=1;
			if (obj==Sel){
			windowG.Selected = X;
			Debug.Log(obj);
			}
		}*/
		windowG.Selected = Sel;
		return windowG;
	}
	//[MenuItem ("Window/ObjFieldTest")]
	public static void Init () {
		//ShaderObjectField windowG = (ShaderObjectField)EditorWindow.GetWindow (typeof (ShaderObjectField));
		ShaderObjectField windowG = new ShaderObjectField();
		windowG.ShowAuxWindow();
		//windowG.wantsMouseMove = true;
		//windowG.minSize = new Vector2(750,360);
	}
		//Color TextColor = new Color(0.8f,0.8f,0.8f,1f);
		//Color TextColorA = new Color(1f,1f,1f,1f);
		//Color BackgroundColor = new Color(0.18f,0.18f,0.18f,1);
    void OnGUI() {

		//ShaderObjectField windowG = (ShaderObjectField)EditorWindow.GetWindow (typeof (ShaderObjectField),true,"",false);

		Vector2 WinSize = new Vector2(position.width,position.height);
		//if (SSSkin==null){
		//	SSSkin = (GUISkin)GUISkin.Instantiate(GUI.skin);
			//SSSkin.GetStyle("SearchTextField").normal.background = EditorGUIUtility.Load("Movement/SearchStart.png") as Texture2D;
			//SSSkin.GetStyle("SearchTextField").active.background = EditorGUIUtility.Load("Movement/SearchStart.png") as Texture2D;
			//SSSkin.GetStyle("SearchTextField").hover.background = EditorGUIUtility.Load("Movement/SearchStart.png") as Texture2D;
			//SSSkin.GetStyle("SearchTextField").focused.background = EditorGUIUtility.Load("Movement/SearchStart.png") as Texture2D;
		//}
		//GUI.skin = SSSkin;
	
		//GUI.backgroundColor = new Color(0.4f,0.4f,0.4f,1f);
		GUI.Box(new Rect(0,0,WinSize.x,30),"");
		//GUI.backgroundColor = new Color(1f,1f,1f,1f);
		SearchText = GUI.TextField(new Rect(10,8,WinSize.x-20,18),SearchText,GUI.skin.GetStyle("SearchTextField"));
		//GUI.backgroundColor = new Color(0.25f,0.25f,0.25f,1f);
		if (Event.current.type == EventType.Repaint)
		GUI.skin.GetStyle("ObjectPickerTab").Draw(new Rect(0,30,56,1), false, true, true,false);
		
		GUI.Label(new Rect(6,30,56,16),"Assets");
		Scroll = GUI.BeginScrollView(new Rect(0,32+16,WinSize.x,WinSize.y-32-24-16),Scroll,new Rect(0,0,WinSize.x-15,WinSize.y*2),false,true);
		
		int X = -1;
		//ObjectPickerGroupHeader
		//Draw(Rect position, bool isHover, bool isActive, bool on, bool hasKeyboardFocus); 
		if (ObjFieldObject!=null){
		foreach(object obj in ObjFieldObject){
			X+=1;
			//Debug.Log(obj.ToString()+","+ObjFieldEnabled[X].ToString());
			if (ObjFieldEnabled[X]==false)
			GUI.enabled = false;
			if (Event.current.type == EventType.Repaint){
				if (Selected==X){
					GUI.skin.GetStyle("ProjectBrowserTextureIconDropShadow").Draw(new Rect(20+X*115,20,100,100),false,false,true,false);
					GUI.skin.GetStyle("ProjectBrowserGridLabel").Draw(new Rect(20+X*115,120,100,20),false,false,true,true);
				}
				else{
					GUI.Box(new Rect(20+X*115,20,100,100),"","ProjectBrowserTextureIconDropShadow");
				}
				GUI.DrawTexture(new Rect(23+X*115,22,94,94),ObjFieldImage[X]);
				GUI.Label(new Rect(20+X*115,120,100,20),obj.ToString(),"ProjectBrowserGridLabel");
			}
			else
			{
				//if (GUI.Button(new Rect(20+X*100,20,100,100),""))
				if (ShaderUtil.MouseDownIn(new Rect(20+X*115,20,100,100))){
					
					GUI.changed = true;
					Caller.ForceGUIChange = true;
					ShaderSandwich.Instance.Repaint();
					Selected = X;
					//Debug.Log(Event.current.clickCount);
					if (Event.current.clickCount==2)
					Close();
				}
			}
			GUI.enabled = true;
		}
		}
		else{
			GUI.Label(new Rect(100,100,200,400),"Sorry, there's been a weird bug :(. Try saving and reloading this file, and if that doesn't work please send a bug report :).");
		}
			//if (Event.current.type != EventType.Repaint){
			if (ShaderUtil.MouseDownIn(new Rect(0,0,WinSize.x,WinSize.y))){
				//if (GUI.Button(new Rect(0,0,WinSize.x,800),""))
				Selected = -1;
					GUI.changed = true;
					Caller.ForceGUIChange = true;
					ShaderSandwich.Instance.Repaint();
					//Debug.Log(Event.current.clickCount);
					if (Event.current.clickCount==2)
					Close();				
			}		
		Caller.Selected = Selected;
		GUI.EndScrollView();
		GUI.Box(new Rect(0,WinSize.y-24,WinSize.x,24),"");
		if (!SomethingOtherThanAMask&&Caller.Selected!=-1&&Caller.Obj!=null){
			if (((ShaderLayerList)Caller.Obj).EndTag.Text.Length>1){
				EditorGUI.BeginChangeCheck();
				Caller.MaskColorComponent = GUI.SelectionGrid(new Rect(0,WinSize.y-24,150,24),Caller.MaskColorComponent,new string[]{"R","G","B","A"},4);
				if (EditorGUI.EndChangeCheck()){
					GUI.changed = true;
					Caller.ForceGUIChange = true;
				}
			}
		}	
    }
	void Nothing(){}
}