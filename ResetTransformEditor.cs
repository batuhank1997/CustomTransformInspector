using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform), true)]
[CanEditMultipleObjects]
public class CustomTransformInspector : Editor
{
	//Unity's built-in editor
	Editor defaultEditor;
	Transform _transform;

	void OnEnable()
	{
		//When this inspector is created, also create the built-in inspector
		defaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
		_transform = target as Transform;
	}

	void OnDisable()
	{
		//When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
		//Also, make sure to call any required methods like OnDisable
		MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		if (disableMethod != null)
			disableMethod.Invoke(defaultEditor, null);
		DestroyImmediate(defaultEditor);
	}

	public override void OnInspectorGUI()
	{
		StandardTransformInspector();
	}

	private void StandardTransformInspector()
	{
		bool didPositionChange = false;
		bool didRotationChange = false;
		bool didScaleChange = false;

		GUIStyle style = EditorStyles.miniButton;

		style = SetStyles(style);

		// Store current values for checking later
		Vector3 initialLocalPosition = _transform.localPosition;
		Vector3 initialLocalEuler = _transform.localEulerAngles;
		Vector3 initialLocalScale = _transform.localScale;

		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		Vector3 localPosition = EditorGUILayout.Vector3Field("Position", _transform.localPosition);
		if (EditorGUI.EndChangeCheck())
			didPositionChange = true;
		if (GUILayout.Button("R", style))
		{
			ResetPosition();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();

		EditorGUI.BeginChangeCheck();

		Vector3 localEulerAngles;

		localEulerAngles = EditorGUILayout.Vector3Field("Rotation", TransformUtils.GetInspectorRotation(_transform));

		if (EditorGUI.EndChangeCheck())
			didRotationChange = true;

		if (GUILayout.Button("R", style))
		{
			ResetRotation();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();

		EditorGUI.BeginChangeCheck();
		Vector3 localScale = EditorGUILayout.Vector3Field("Scale", _transform.localScale);
		if (EditorGUI.EndChangeCheck())
			didScaleChange = true;

		if (GUILayout.Button("R", style))
		{
			ResetScale();
		}
		EditorGUILayout.EndHorizontal();

		// Apply changes with record undo
		if (didPositionChange || didRotationChange || didScaleChange)
		{
			Undo.RecordObject(_transform, _transform.name);

			if (didPositionChange)
				_transform.localPosition = localPosition;

			if (didRotationChange)
				_transform.localEulerAngles = localEulerAngles;

			if (didScaleChange)
				_transform.localScale = localScale;

		}

		// Since BeginChangeCheck only works on the selected object
		// we need to manually apply transform changes to all selected objects.
		Transform[] selectedTransforms = Selection.transforms;
		if (selectedTransforms.Length > 1)
		{
			foreach (var item in selectedTransforms)
			{
				if (didPositionChange || didRotationChange || didScaleChange)
					Undo.RecordObject(item, item.name);

				if (didPositionChange)
				{
					item.localPosition = ApplyChangesOnly(
						item.localPosition, initialLocalPosition, _transform.localPosition);
				}

				if (didRotationChange)
				{
					item.localEulerAngles = ApplyChangesOnly(
						item.localEulerAngles, initialLocalEuler, _transform.localEulerAngles);
				}

				if (didScaleChange)
				{
					item.localScale = ApplyChangesOnly(
						item.localScale, initialLocalScale, _transform.localScale);
				}

			}
		}
	}

	private Vector3 ApplyChangesOnly(Vector3 toApply, Vector3 initial, Vector3 changed)
	{
		if (!Mathf.Approximately(initial.x, changed.x))
			toApply.x = _transform.localPosition.x;

		if (!Mathf.Approximately(initial.y, changed.y))
			toApply.y = _transform.localPosition.y;

		if (!Mathf.Approximately(initial.z, changed.z))
			toApply.z = _transform.localPosition.z;

		return toApply;
	}

	void ResetPosition()
	{
		_transform.localPosition = Vector3.zero;
	}

	void ResetRotation()
	{
		_transform.localRotation = Quaternion.identity;
		TransformUtils.SetInspectorRotation(_transform, Vector3.zero);
	}

	void ResetScale()
	{
		_transform.localScale = Vector3.one;
	}

	public static GUIStyle SetStyles(GUIStyle style)
	{
		//style = new GUIStyle(GUI.skin.window);
		var IconStyle = new GUIStyle();
		style = new GUIStyle(GUI.skin.button);
		style.normal.textColor = Color.white;
		style.normal.background = style.onActive.background;
		style.alignment = TextAnchor.MiddleLeft;
		style.fixedWidth = 20;

		return style;
	}
}
