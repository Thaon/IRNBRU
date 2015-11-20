using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
public class ShaderTransition{
	public enum TransDir{Forward,Backward,None,Reverse};
	public TransDir Transitioning = TransDir.None;
	public float TransitionTime = 0f;
	public double TransitionStart = 0f;
	public float TransitionSpeed = 0.3f;
	
	public void Reset(){
		TransitionTime = 0f;
		Transitioning = TransDir.None;
	}
	public void Start(TransDir dir){
		Start_Real(dir,0.3f);
	}
	public void Start(TransDir dir,float speed){
		Start_Real(dir,speed);
	}	
	
	void Start_Real(TransDir dir,float speed){
		TransitionSpeed = speed;
		TransitionStart = EditorApplication.timeSinceStartup;
		if (dir!=TransDir.Reverse)
		{
			Transitioning = dir;
		}
		else
		{
			if (Transitioning == TransDir.None||Transitioning == TransDir.Backward)
			Transitioning = TransDir.Forward;
			else
			Transitioning = TransDir.Backward;
		}
		TransitionTime = 0f;
	}
	bool Done2(){
		if(Transitioning==TransDir.Forward)
			if(Get()>0.999)
				return true;
		if(Transitioning==TransDir.Backward)
			if(Get()<0.001)
				return true;
		return false;
	}
	public bool DoneHit(){
		if(Done2())
		{
			TransitionTime = 0f;
			Transitioning = TransDir.None;
			return true;
		}
		return false;
	}
	public bool Done(){
		if(Done2())
		{
			//TransitionTime = 0f;
			//Transitioning = TransDir.None;
		}
		if (Transitioning == TransDir.None)
		return true;
		else
		return false;
	}	
	public float Get(){
		if (Transitioning==TransDir.Forward)
			return Mathf.Min(1f,(1f-Mathf.Pow(1f-(float)(EditorApplication.timeSinceStartup-TransitionStart)/TransitionSpeed,3f)));
			
		if (Transitioning==TransDir.Backward)
			return Mathf.Max(0,1f-(1f-Mathf.Pow(1f-(float)(EditorApplication.timeSinceStartup-TransitionStart)/TransitionSpeed,3f)));
		return 0f;
	}
}