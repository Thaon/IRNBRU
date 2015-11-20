using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
[Serializable]
public class ShaderSandwichSettings : ScriptableObject{
	public string[] RecentFiles = new string[5];
	public string CurPath;
	public List<string> RecentFilesList{
		get{
			var lst = new List<string>();
			lst.AddRange(RecentFiles);
			return lst;
		}
		set{
		
		}
	}
	public bool AutoUpdate;
	public bool AutoRotate;
	public bool HasAny(){
		if (RecentFiles==null)
		return false;
		for(int i = 0;i<RecentFiles.Length;i++){
			if (RecentFiles[i]!=""&&RecentFiles[i]!=null){
				Shader TheActualFile = ((Shader)AssetDatabase.LoadAssetAtPath(RecentFiles[i],typeof(Shader)));
				if (TheActualFile!=null)
					return true;
			}
		}
		return false;
	}
	public void AddNew(string file){
		var lst = new List<string>();
		if (RecentFiles!=null)
		lst.AddRange(RecentFiles);
		RecentFiles = new string[5];
		//if (!lst.Contains(file)){
			lst.Remove(file);
			lst.Insert(0,file);
		//}
		
		for(int i = 0;i<Mathf.Min(5,lst.Count);i++){
			RecentFiles[i] = lst[i];
		}
		EditorUtility.SetDirty( this );
		AssetDatabase.SaveAssets();
	}
}
