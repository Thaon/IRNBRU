﻿// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful.Editors
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(GaussianBlur))]
	public class GaussianBlurEditor : BaseEffectEditor
	{
		SerializedProperty p_Passes;
		SerializedProperty p_Downscaling;

		void OnEnable()
		{
			p_Passes = serializedObject.FindProperty("Passes");
			p_Downscaling = serializedObject.FindProperty("Downscaling");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(p_Passes);
			EditorGUILayout.PropertyField(p_Downscaling);

			serializedObject.ApplyModifiedProperties();
		}
	}
}