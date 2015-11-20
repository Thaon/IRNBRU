#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
#define PRE_UNITY_5
#else
#define UNITY_5
#endif
#pragma warning disable 0618
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using SU = ShaderUtil;
using UnityEngine.Rendering;
[System.Serializable]
public class ShaderBugReport : EditorWindow {
	bool Feature = false;
	
	enum ErrorTypes {Pink,NotWorking,Other};
	ErrorTypes Error = ErrorTypes.Pink;
	bool IncludeCurrentShader = true;
	bool Private = false;
	string BContext = "";
	string BName = "A bug!";
	string BEmail = "So I can contact you for more info :).";
	
	enum FeatureTypes {Low,Medium,High};
	FeatureTypes FeatureImportance = FeatureTypes.Medium;
	string FContext = "";
	string FName = "A feature!";
	//Include Unity Version, OS, Graphics Card, 
	[MenuItem ("Help/Shader Sandwich/Report Bug or Suggest Feature!")]
	public static void Init () {
		//ShaderBugReport windowG = (ShaderBugReport)EditorWindow.GetWindow (typeof (ShaderBugReport));
		ShaderBugReport windowG = ScriptableObject.CreateInstance<ShaderBugReport>();
		windowG.ShowUtility();
		windowG.wantsMouseMove = true;
		windowG.minSize = new Vector2(320,450);
		//windowG.maxSize = new Vector2(541,441);
		//windowG.maxSize = new Vector2(400,400);
		windowG.title = "Submit Bug or Feature!";
		//windowG.Meshes = null;
		
	}
//	int InArea = 0;
	bool EditingContext = false;
	bool Refocused = false;
	void OnGUI(){
		EditorStyles.textField.wordWrap = true;
		Repaint();
		Rect OldRect = position;
		OldRect.width=320;
		OldRect.height=450;
		if (position.width>320||position.height>450)
		position = OldRect;
		maximized = false;
		GUISkin oldskin = GUI.skin;
		Vector2 WinSize = new Vector2(position.width,position.height);
		if(Event.current.type==EventType.Repaint)
		ShaderUtil.AddProSkin(WinSize);
		
		if (!Feature)
		GUI.Toggle(new Rect(10,10,(WinSize.x-20)/2,30),true,"Report Bug",GUI.skin.GetStyle("ButtonLeft"));
		else
		if (GUI.Button(new Rect(10,10,(WinSize.x-20)/2,30),"Report Bug",GUI.skin.GetStyle("ButtonLeft")))
			Feature = false;
			
		if (Feature)
		GUI.Toggle(new Rect(10+(WinSize.x-20)/2,10,(WinSize.x-20)/2,30),true,"Suggest Feature",GUI.skin.GetStyle("ButtonLeft"));
		else
		if (GUI.Button(new Rect(10+(WinSize.x-20)/2,10,(WinSize.x-20)/2,30),"Suggest Feature",GUI.skin.GetStyle("ButtonLeft")))
			Feature = true;
		
		int YOffset = 40;
		GUI.Box(new Rect(5,YOffset,WinSize.x-10,WinSize.y-20),"","Button");
		if (!Feature){
			
			GUI.Label(new Rect(10,YOffset+0,WinSize.x-20,100),"Oh no, you've found a bug! Sorry about this, but here's how you can help. Just check a few options and hit send to help Shader Sandwich become even more awesome :).");
			
			GUI.Label(new Rect(10,YOffset+70,WinSize.x-20,20),"Name of the bug:");
			BName = GUI.TextField(new Rect(10,YOffset+90,WinSize.x-20,20),BName);
			YOffset+=50;
			GUI.Label(new Rect(10,YOffset+70,WinSize.x-20,20),"Whats wrong?");
			Error = (ErrorTypes)EditorGUI.IntPopup(new Rect(110,YOffset+70,WinSize.x-120,15),(int)Error,new string[]{"The shaders pink!","Something isn't doing what I expect.","Something else."},new int[]{0,1,2},ShaderUtil.EditorPopup);
			//EditorGUI.TextField(new Rect(0,60,WinSize.x-10,20),"ASD");
			GUI.Label(new Rect(10,YOffset+95,WinSize.x-20,20),"Got any details you think are important?");
			GUI.SetNextControlName("Context");
			//Debug.Log((GUI.GetNameOfFocusedControl()!="Context"&&Context==" ")?("Then type them here!"+UnityEngine.Random.value.ToString()):Context);
			string Con = EditorGUI.TextArea(new Rect(10, YOffset+120, WinSize.x-20, 90), (!EditingContext)?("Then type them here!"):BContext, GUI.skin.textArea);
			if (GUI.GetNameOfFocusedControl()!="Context"&&BContext==""){
			//	EditorGUI.TextArea(new Rect(0, 110, WinSize.x-20, 100), "Then type them here!", GUI.skin.textField);
//				InArea = 0;
			}
			else{
				if (!EditingContext){
					GUI.FocusControl("");
				}
				EditingContext = true;
			}
			if (EditingContext){
				if (!Refocused&&Event.current.type==EventType.Repaint){
					Refocused = true;
					GUI.FocusControl("Context");
				}
				if (Refocused)
				BContext = Con;
			}
			YOffset-=10;
			GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
			ButtonStyle.padding = new RectOffset(0,0,0,0);
			ButtonStyle.margin = new RectOffset(0,0,0,0);
			GUI.Label(new Rect(10, YOffset+220, WinSize.x-20, 20),"Your email address:");
			YOffset+=20;
			BEmail = GUI.TextField(new Rect(10, YOffset+220, WinSize.x-20, 20),BEmail);
			YOffset+=30;
			GUI.Label(new Rect(10, YOffset+220, WinSize.x-20, 20),"Include current shader (Please do!)");
			Rect rect = new Rect(WinSize.x-30, YOffset+220, 20, 20);
			if (IncludeCurrentShader)
				IncludeCurrentShader = GUI.Toggle(rect,IncludeCurrentShader,ShaderSandwich.Tick,ButtonStyle);
				else
				IncludeCurrentShader = GUI.Toggle(rect,IncludeCurrentShader,ShaderSandwich.Cross,ButtonStyle);
				
			YOffset+=25;
			GUI.Label(new Rect(10, YOffset+220, WinSize.x-20, 20),"Make the bug private");
			rect = new Rect(WinSize.x-30, YOffset+220, 20, 20);
			if (Private)
				Private = GUI.Toggle(rect,Private,ShaderSandwich.Tick,ButtonStyle);
				else
				Private = GUI.Toggle(rect,Private,ShaderSandwich.Cross,ButtonStyle);
			//Private=false;
			
		}
		if (Feature){
			
			GUI.Label(new Rect(10,YOffset+0,WinSize.x-20,100),"So, you think you've got a cool idea for a feature? Well tell us about it, and we might add it in!");
			GUI.Label(new Rect(10,YOffset+70,WinSize.x-20,20),"Name of the feature:");
			FName = GUI.TextField(new Rect(10,YOffset+90,WinSize.x-20,20),FName);
			YOffset+=50;
			
			GUI.Label(new Rect(10,YOffset+70,WinSize.x-20,20),"How important is this feature to you?");
			YOffset+=20;
			FeatureImportance = (FeatureTypes)EditorGUI.IntPopup(new Rect(10,YOffset+70,WinSize.x-80,15),(int)FeatureImportance,new string[]{"I can wait ages for it.","It would come in handy soon.","I really need it for the shader I'm making!"},new int[]{0,1,2},ShaderUtil.EditorPopup);
			//EditorGUI.TextField(new Rect(0,60,WinSize.x-10,20),"ASD");
			GUI.Label(new Rect(10,YOffset+95,WinSize.x-20,20),"So whats your cool idea? Please use detail :).");
			//Debug.Log((GUI.GetNameOfFocusedControl()!="Context"&&Context==" ")?("Then type them here!"+UnityEngine.Random.value.ToString()):Context);
			string Con = EditorGUI.TextArea(new Rect(10, YOffset+120, WinSize.x-20, 100), FContext, GUI.skin.textArea);
			FContext = Con;
			
			GUI.Label(new Rect(10, YOffset+220, WinSize.x-20, 20),"Your email address:");
			YOffset+=20;
			BEmail = GUI.TextField(new Rect(10, YOffset+220, WinSize.x-20, 20),BEmail);
			YOffset+=30;			
		}
		if (GUI.Button(new Rect(10,WinSize.y-50,100,40),"Submit!")){
			int Option = 0;
			if (!Feature)
			Option = EditorUtility.DisplayDialog("Ready?","Cool, your bug report is about to be submitted. Along with it will be some system info like \nUnity "+Application.unityVersion+"\n"
			+SystemInfo.operatingSystem+"\n"
			+SystemInfo.deviceModel+"\n"
+SystemInfo.graphicsDeviceName+"\nSM"+
(SystemInfo.graphicsShaderLevel/10).ToString()+"\n\n"+
"This stuff is important for fixing the bug, so if that's ok just click \"Go Ahead\"!","Go ahead!","Sorry, not yet.")?0:1;
			if (Feature)
			Option = EditorUtility.DisplayDialogComplex("Ready?","Cool, your feature idea is about to be submitted. If you want you can also submit some system info like \nUnity "+Application.unityVersion+"\n"
			+SystemInfo.operatingSystem+"\n"
			+SystemInfo.deviceModel+"\n"
+SystemInfo.graphicsDeviceName+"\nSM"+
(SystemInfo.graphicsShaderLevel/10).ToString()+"\n\n"+
"This is useful to me for statistical purposes and stuff, but if you want you can choose not to submit it.","Submit with data!","Don't submit yet.","Submit without data.");
			if (Option==0||Option==2){
				WWWForm form = new WWWForm();
				WWW www;
				if (!Feature){
					form.AddField("SSV", "1.2");//ztechnologies.com@gmail.com
					form.AddField("Email", BEmail);//ztechnologies.com@gmail.com
					form.AddField("Unity", Application.unityVersion);//Unity 5.02f
					form.AddField("OS", SystemInfo.operatingSystem);//Windows 7
					form.AddField("CPU", SystemInfo.deviceModel);//Intel(R)
					form.AddField("CName", "Nup");//SystemInfo.deviceName);//IMACATLOL-HP
					form.AddField("Name", BName);
					form.AddField("GPU", SystemInfo.graphicsDeviceName);//NVIDIA
					form.AddField("CID", SystemInfo.deviceUniqueIdentifier);//1fe8504986mgf003sfbtrt
					form.AddField("ShaderModel", (SystemInfo.graphicsShaderLevel/10).ToString());//3
					if (IncludeCurrentShader&&ShaderSandwich.Instance!=null&&ShaderSandwich.Instance.OpenShader!=null){
						string GenStr = "";
						try {
							GenStr = ShaderSandwich.Instance.OpenShader.GenerateCode(true);
						}
						catch{}
						form.AddField("File",GenStr+"\n\n/*\n"+ShaderSandwich.Instance.OpenShader.Save()+"\n*/");
					}
					else
					if (ShaderSandwich.Instance==null||ShaderSandwich.Instance.OpenShader==null){
						form.AddField("File", "Couldn't");
					}
					else
					if (ShaderSandwich.Instance!=null&&ShaderSandwich.Instance.OpenShader!=null){
						form.AddField("File", "Nup");
					}
					form.AddField("Private", Private ? "1":"0");
					form.AddField("Attr", Error.ToString());
					form.AddField("Context", BContext);
					form.AddField("Type", "Bug");
				}
				else{
					if (Option==0){
						form.AddField("SSV", "1.2");
						form.AddField("Email", BEmail);//ztechnologies.com@gmail.com
						form.AddField("Unity", Application.unityVersion);//Unity 5.02f
						form.AddField("OS", SystemInfo.operatingSystem);//Windows 7
						form.AddField("CPU", SystemInfo.deviceModel);//Intel(R)
						form.AddField("CName", "Nup");//SystemInfo.deviceName);//IMACATLOL-HP
						form.AddField("Name", FName);
						form.AddField("GPU", SystemInfo.graphicsDeviceName);//NVIDIA
						form.AddField("CID", SystemInfo.deviceUniqueIdentifier);//1fe8504986mgf003sfbtrt
						form.AddField("ShaderModel", (SystemInfo.graphicsShaderLevel/10).ToString());//3
						//form.AddField("Private", Private ? "1":"0");
						form.AddField("Attr", FeatureImportance.ToString());
						form.AddField("Context", FContext);
						form.AddField("Type", "Feature");
					}
					if (Option==2){
						form.AddField("SSV", "1.2");
						form.AddField("Email", BEmail);//ztechnologies.com@gmail.com
						form.AddField("Unity", "Nup");//Unity 5.02f
						form.AddField("OS", "Nup");//Unity 5.02f
						form.AddField("CPU", "Nup");//Intel(R)
						form.AddField("CName", "Nup");//SystemInfo.deviceName);//IMACATLOL-HP
						form.AddField("Name", FName);
						form.AddField("GPU", "Nup");//NVIDIA
						form.AddField("CID", "Nup");//1fe8504986mgf003sfbtrt
						form.AddField("ShaderModel", "Nup");//3
						//form.AddField("Private", Private ? "1":"0");
						form.AddField("Attr", FeatureImportance.ToString());
						form.AddField("Context", FContext);
						form.AddField("Type", "Feature");
					}
					
				}
				www = new WWW("http://electronic-mind.org/scripts/SSFeedbackReport.php", form);
				
				double Start = EditorApplication.timeSinceStartup;
				while (!(www.isDone==true||(EditorApplication.timeSinceStartup-Start>15)))
				{
					EditorUtility.DisplayProgressBar("Submitting!","Your "+(Feature?"feature":"bug")+" is now being uploaded!",((float)(EditorApplication.timeSinceStartup-Start))/15f);
				}
				EditorUtility.ClearProgressBar();
				if (www.isDone==true){
					EditorUtility.DisplayDialog("Success!","Ok, I'll take a look at it shortly, you can watch the progress of it on electronic-mind.org in the Shader Sandwich bug/feature area.","Cool!");
					Help.BrowseURL("http://electronic-mind.org/Pages/ShaderSandwichFeedbackView.php?id="+www.text);
				}
				else
				EditorUtility.DisplayDialog("Oops!","Sorry, something went wrong :(. You can try submitting again, or try a bit later. If it's urgent just email me at ztechnologies.com@gmail.com :).","Ok.");
				//Debug.Log( www.text);
			}
		}
		GUI.skin = oldskin;
	}
}