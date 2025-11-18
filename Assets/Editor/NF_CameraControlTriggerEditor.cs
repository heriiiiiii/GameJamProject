using Cinemachine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NF_CameraControlTrigger))]
public class NF_CameraControlTriggerEditor : Editor
{
    NF_CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (NF_CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObjects.swapCameras)
        {
            cameraControlTrigger.customInspectorObjects.cameraOnLeft =
                (CinemachineVirtualCamera)EditorGUILayout.ObjectField(
                    "Camera on Left",
                    cameraControlTrigger.customInspectorObjects.cameraOnLeft,
                    typeof(CinemachineVirtualCamera),
                    true
                );

            cameraControlTrigger.customInspectorObjects.cameraOnRight =
                (CinemachineVirtualCamera)EditorGUILayout.ObjectField(
                    "Camera on Right",
                    cameraControlTrigger.customInspectorObjects.cameraOnRight,
                    typeof(CinemachineVirtualCamera),
                    true
                );
        }

        if (cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspectorObjects.panDirection =
                (PanDirection)EditorGUILayout.EnumPopup(
                    "Camera Pan Direction",
                    cameraControlTrigger.customInspectorObjects.panDirection
                );

            cameraControlTrigger.customInspectorObjects.panDistance =
                EditorGUILayout.FloatField(
                    "Pan Distance",
                    cameraControlTrigger.customInspectorObjects.panDistance
                );

            cameraControlTrigger.customInspectorObjects.panTime =
                EditorGUILayout.FloatField(
                    "Pan Time",
                    cameraControlTrigger.customInspectorObjects.panTime
                );
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}