using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class LaserMovementManager : MonoBehaviour
{
    
    public enum MovementType { SinWave, Rotate1, Move3, Move4 }
    [Header("여러 레이저의 움직임을 총괄하는 스크립트.\nmoveType별로 다른 움직임 애니메이션")]
    [SerializeField] private MovementType moveType;
    [Tooltip("해당 오브젝트와 플레이어 사이의 거리가 일정 거리 이하일 때 움직임 애니메이션 시작")]
    [SerializeField] public float activationDist = 300;
    [Tooltip("움직일 레이저들 선택")]
    [SerializeField] private GameObject[] laserObjs;


    #region SinWave
    [Header("localPosition 기준으로 움직입니다.\n"
          + "y축으로 [minY, minY + 2amplitude] 사이 왕복")]
    [Tooltip("y축 사인파 움직임의 왕복 주기")]
    [SerializeField] private float oscillationSpeed = 2f;
    [Tooltip("레이저 간 사인파 위상 분할 비율")]
    [SerializeField] private int waveFrequency = 6;
    [Tooltip("y축 이동 진폭")]
    [SerializeField] private float amplitude = 5;
    [Tooltip("y축 최저 높이")]
    [SerializeField] private float minY = 1;
    [Tooltip("레이저 간 사인파의 진행 방향을 반대로 조절")]
    [SerializeField] private bool reverseDir;
    #endregion


    #region Rotate1
    [Header("초록선(transform.forward) 방향을 기준으로\n"
          + "레이저가 회전합니다.")]
    [Tooltip("초당 회전 속도 (도 단위). 음수면 반대로 회전")]
    [SerializeField] private float rotationSpeed = 60f;
    [Tooltip("인접한 두 레이저 간의 각도 차이 (도 단위)")]
    [SerializeField] private float neighborLaserAngleOffset = 30f;

    private float angle = 0f;
    #endregion

    private Transform player;


    private void Start()
    {
        player = PlayerManager.Instance.transform;
    }


    private void Update()
    {
        if (!IsPlayerNear())
            return;

        if (moveType == MovementType.SinWave) SinWave();
        else if (moveType == MovementType.Rotate1) Rotate1();
        else if (moveType == MovementType.Move3) Move3();
        else if (moveType == MovementType.Move4) Move4();
    }


    // 레이저 중심지점과 플레이어 사이의 거리가 일정 거리 이하인지 확인
    private bool IsPlayerNear()
    {
        float sqrDist = (transform.position - player.position).sqrMagnitude;
        return sqrDist < activationDist * activationDist;
    }




    private void SinWave()
    {
        Vector3 localPos;
        // Sin 파형을 따라 레이저들을 y축으로 위아래 왕복 이동
        for (int i = 0; i < laserObjs.Length; i++)
        {
            int idx = reverseDir ? laserObjs.Length - 1 - i : i;
            // 레이저마다 사인파 위상 오프셋을 달리하여 물결처럼 움직이게 함
            float phaseOffset = (Time.time / oscillationSpeed) + ((float)idx / waveFrequency);
            localPos = laserObjs[i].transform.localPosition;
            localPos.y = minY + amplitude * (1 + Mathf.Sin(2 * Mathf.PI * phaseOffset));
            laserObjs[i].transform.localPosition = localPos;
        }
    }

    private void Rotate1()
    {
        angle += rotationSpeed * Time.deltaTime;
        angle %= 360;

        for (int i = 0; i < laserObjs.Length; i++)
        {
            float laserAngle = angle - neighborLaserAngleOffset * i;
            Vector3 lookVec = GetRotatedPerpendicularVector(transform.forward, laserAngle);
            laserObjs[i].transform.rotation = Quaternion.LookRotation(lookVec);
        }
    }


    // 벡터 v와 수직인 벡터 중 하나를 v를 축으로 angle도 만큼 회전한 벡터를 반환
    private static Vector3 GetRotatedPerpendicularVector(Vector3 v, float angle)
    {
        if (v == Vector3.zero)
            return Vector3.zero;

        // v와 수직인 임의의 벡터 구함
        Vector3 perpendicular = GetPerpendicularVector(v);

        // v를 축으로 angle도 만큼 회전
        Quaternion rotation = Quaternion.AngleAxis(angle, v.normalized);
        return rotation * perpendicular;
    }

    // 주어진 벡터와 수직인 특정한 단위 벡터를 반환
    private static Vector3 GetPerpendicularVector(Vector3 v)
    {
        // v가 0이면 처리 불가
        if (v == Vector3.zero)
            return Vector3.zero;

        // v와 수직인 벡터를 구하기 위해 v와 특정 축과 외적
        Vector3 axis = (Mathf.Abs(v.x) < 0.99f) ? Vector3.right : Vector3.up;
        return Vector3.Cross(v, axis).normalized;
    }

    private void Move3()
    {

    }

    private void Move4()
    {

    }
}



#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(LaserMovementManager))]
[CanEditMultipleObjects]
class LaserMovementManagerEditor : Editor
{
    LaserMovementManager _target;

    SerializedProperty moveTypeProp;
    SerializedProperty activationDistProp;
    SerializedProperty laserObjsProp;

    SerializedProperty oscillationSpeedProp;
    SerializedProperty waveFrequencyProp;
    SerializedProperty amplitudeProp;
    SerializedProperty reverseDirProp;
    SerializedProperty minYProp;

    SerializedProperty rotationSpeedProp;
    SerializedProperty neighborLaserAngleOffsetProp;

    private void OnEnable()
    {
        moveTypeProp = serializedObject.FindProperty("moveType");
        activationDistProp = serializedObject.FindProperty("activationDist");
        laserObjsProp = serializedObject.FindProperty("laserObjs");

        oscillationSpeedProp = serializedObject.FindProperty("oscillationSpeed");
        waveFrequencyProp = serializedObject.FindProperty("waveFrequency");
        amplitudeProp = serializedObject.FindProperty("amplitude");
        reverseDirProp = serializedObject.FindProperty("reverseDir");
        minYProp = serializedObject.FindProperty("minY");

        rotationSpeedProp = serializedObject.FindProperty("rotationSpeed");
        neighborLaserAngleOffsetProp = serializedObject.FindProperty("neighborLaserAngleOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(moveTypeProp);
        EditorGUILayout.PropertyField(activationDistProp);
        EditorGUILayout.PropertyField(laserObjsProp);

        if (moveTypeProp.enumValueIndex == (int)LaserMovementManager.MovementType.SinWave)
        {
            EditorGUILayout.PropertyField(oscillationSpeedProp);
            EditorGUILayout.PropertyField(waveFrequencyProp);
            EditorGUILayout.PropertyField(amplitudeProp);
            EditorGUILayout.PropertyField(minYProp);
            EditorGUILayout.PropertyField(reverseDirProp);
        }
        else if (moveTypeProp.enumValueIndex == (int)LaserMovementManager.MovementType.Rotate1)
        {
            EditorGUILayout.PropertyField(rotationSpeedProp);
            EditorGUILayout.PropertyField(neighborLaserAngleOffsetProp);
        }

        serializedObject.ApplyModifiedProperties();
    }



    public void OnSceneGUI()
    {
        _target = target as LaserMovementManager;

        // Scene 뷰에서 조절 가능한 구형 영역 표시
        HandleDetectPointDist();

        if (moveTypeProp.enumValueIndex == (int)LaserMovementManager.MovementType.Rotate1)
            DrawLineTowardsTransformForward();

        // 값이 바뀌었다면 객체를 Undo에 기록하고 dirty 상태로 만들어 저장되도록 함
        if (GUI.changed)
        {
            Undo.RecordObject(_target, "Modify Laser Detection Range");
            EditorUtility.SetDirty(_target);
        }
    }

    private void HandleDetectPointDist()
    {
        Handles.color = Color.red;

        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            _target.transform.position,
            _target.activationDist
        );

        if (newRadius != _target.activationDist)
        {
            _target.activationDist = newRadius;
        }
    }

    private void DrawLineTowardsTransformForward()
    {
        Transform t = _target.transform;
        Vector3 start = t.position - t.forward * 50f;
        Vector3 end = t.position + t.forward * 50f;
        Handles.color = Color.green;
        Handles.DrawLine(start, end);
    }
}
#endif
#endregion