using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*[CustomEditor(typeof(GridMovement))]
public class GridMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);

        GridMovement gm = target as GridMovement;

        gm.timeToMove = EditorGUILayout.FloatField("Time To Move", gm.timeToMove);
        gm.distance = EditorGUILayout.FloatField("Distance", gm.distance);
        gm.BlockReverse = EditorGUILayout.Toggle("Block Reverse", gm.BlockReverse);
        
        EditorGUILayout.Space();

        gm.UpBound = EditorGUILayout.FloatField("Up Bound", gm.UpBound);
        gm.DownBound = EditorGUILayout.FloatField("Down Bound", gm.DownBound);
        gm.LeftBound = EditorGUILayout.FloatField("Left Bound", gm.LeftBound);
        gm.RightBound = EditorGUILayout.FloatField("Right Bound", gm.RightBound);

        EditorGUILayout.Space();

        gm.inputMethod = (GridMovement.InputMethod)
            EditorGUILayout.EnumPopup("Input Method", gm.inputMethod);
        if (gm.inputMethod == GridMovement.InputMethod.Custom)
        {
            gm.Up = (KeyCode)EditorGUILayout.EnumPopup("Up", gm.Up);
            gm.Down = (KeyCode)EditorGUILayout.EnumPopup("Down", gm.Down);
            gm.Left = (KeyCode)EditorGUILayout.EnumPopup("Left", gm.Left);
            gm.Right = (KeyCode)EditorGUILayout.EnumPopup("Right", gm.Right);
        }

    }
}*/
