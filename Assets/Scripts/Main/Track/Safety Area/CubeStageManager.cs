using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class CubeStageManager : MonoBehaviour
{
    private CubeMovementController[,,] _cubes; // i : i층 큐브
    private Vector3[,,] _cubesPos;

    private bool[,,] _isCubesMoving;
    private bool[,,] _isCubesRotating;
    private bool[,,] _isCubesDisappearing;

    private static float i_interval = 70; // y
    private static float j_interval = 120; // z
    private static float k_interval = 120; // x

    private const float MOVE_TIME = 3f;
    private const float ROTATE_TIME = 2f;
    private const float DISAPPEAR_TIME = 3f;

    private WaitForSeconds _moveWait;
    private WaitForSeconds _rotateWait;
    private WaitForSeconds _disappearWait;


    public GameObject[] cubeObjs;

    private void OnEnable()
    {
        cubeObjs = new GameObject[64];
        for (int i = 0; i < 4; i++)
        {
            Transform cubeParent = transform.GetChild(i);
            for (int j = 0; j < 16; j++)
                cubeObjs[16 * i + j] = cubeParent.GetChild(j).gameObject;
        }

        Vector3 origin = cubeObjs[0].transform.position;
        _cubes = new CubeMovementController[4, 4, 4];
        _cubesPos = new Vector3[4, 4, 4];
        // 전부 false로 초기화됨
        _isCubesDisappearing = new bool[4, 4, 4];
        _isCubesMoving = new bool[4, 4, 4];
        _isCubesRotating = new bool[4, 4, 4];
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                {
                    int idx = 16 * i + 4 * j + k;
                    _cubes[i, j, k] = cubeObjs[idx].GetComponent<CubeMovementController>();

                    Vector3 cubePos = origin + new Vector3(k * k_interval,
                                                           i * i_interval,
                                                           j * j_interval);
                    _cubesPos[i, j, k] = cubePos;
                    cubeObjs[idx].transform.position = cubePos;
                }

        _moveWait = new WaitForSeconds(MOVE_TIME + 0.5f);
        _rotateWait = new WaitForSeconds(ROTATE_TIME + 0.5f);
        _disappearWait = new WaitForSeconds(DISAPPEAR_TIME + 0.5f);


        for (int i = 0; i < 8; i++)
            StartCoroutine(Move());

        for (int i = 0; i < 12; i++)
            StartCoroutine(Rotate());

        for (int i = 0; i < 12; i++)
            StartCoroutine(Disappear());
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    IEnumerator Move()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        while (true)
        {
            StartCoroutine(MoveCube());
            yield return new WaitForSeconds(MOVE_TIME + Random.Range(0.7f, 3f));
        }
    }

    private IEnumerator MoveCube()
    {
        int axis = Random.Range(0, 3);
        bool rotateRight = Random.Range(0, 2) == 0;
        int plane1 = Random.Range(0, 3);
        int plane2 = Random.Range(0, 3);
        int axisNum = Random.Range(0, 4);

        int x = axis == 0 ? axisNum : plane1;
        int y = axis == 1 ? axisNum : (axis == 0 ? plane1 : plane2);
        int z = axis == 2 ? axisNum : plane2;

        // 회전할 네 개의 좌표
        Vector3Int[] points = new Vector3Int[4];

        if (axis == 0)
        {
            points[0] = new Vector3Int(x, y, z);
            points[1] = new Vector3Int(x, y + 1, z);
            points[2] = new Vector3Int(x, y + 1, z + 1);
            points[3] = new Vector3Int(x, y, z + 1);
        }
        else if (axis == 1)
        {
            points[0] = new Vector3Int(x, y, z);
            points[1] = new Vector3Int(x, y, z + 1);
            points[2] = new Vector3Int(x + 1, y, z + 1);
            points[3] = new Vector3Int(x + 1, y, z);
        }
        else // axis == 2
        {
            points[0] = new Vector3Int(x, y, z);
            points[1] = new Vector3Int(x + 1, y, z);
            points[2] = new Vector3Int(x + 1, y + 1, z);
            points[3] = new Vector3Int(x, y + 1, z);
        }

        bool canMove = true;
        for (int i = 0; i < 4; i++)
        {
            Vector3Int to = points[i];
            if (_isCubesMoving[to.x, to.y, to.z] || _isCubesRotating[to.x, to.y, to.z])
            {
                canMove = false;
                break;
            }
        }

        // 이동시킬 큐브가 이미 이동 중이라면 종료
        if (!canMove)
            yield break;

        // 현재 큐브들을 임시로 저장
        CubeMovementController[] tempCubes = new CubeMovementController[4];
        for (int i = 0; i < 4; i++)
        {
            Vector3Int point = points[i];
            tempCubes[i] = _cubes[point.x, point.y, point.z];
        }

        // 이동 시작과 동시에 배열 참조 교체
        for (int i = 0; i < 4; i++)
        {
            Vector3Int fromPoint = points[i];
            Vector3Int toPoint;

            if (rotateRight)
            {
                toPoint = points[(i + 1) % 4];
            }
            else
            {
                toPoint = points[(i + 3) % 4]; // (i - 1 + 4) % 4와 같음
            }

            // 큐브 이동 시작
            tempCubes[i].MoveTo(_cubesPos[toPoint.x, toPoint.y, toPoint.z], MOVE_TIME);

            // 배열 참조 교체
            _cubes[toPoint.x, toPoint.y, toPoint.z] = tempCubes[i];

            // 이동 상태 설정
            _isCubesMoving[fromPoint.x, fromPoint.y, fromPoint.z] = true;
        }



        yield return _moveWait;

        for (int i = 0; i < 4; i++)
        {
            Vector3Int to = points[i];
            _isCubesMoving[to.x, to.y, to.z] = false;
        }
    }


    IEnumerator Rotate()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        while (true)
        {
            StartCoroutine(RotateCube());
            yield return new WaitForSeconds(ROTATE_TIME + Random.Range(0.7f, 2.5f));
        }
    }

    IEnumerator RotateCube()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 4);
        int z = Random.Range(0, 4);

        int sign = -1 + 2 * Random.Range(0, 2);
        int axisNum = Random.Range(0, 3);
        Vector3 axis = axisNum == 0 ? Vector3.right :
                       (axisNum == 1 ? Vector3.up : Vector3.forward);

        if (_isCubesMoving[x, y, z] || _isCubesRotating[x, y, z])
            yield break;

        _cubes[x, y, z].Rotate(axis * sign, ROTATE_TIME);
        _isCubesRotating[x, y, z] = true;

        yield return _rotateWait;

        _isCubesRotating[x, y, z] = false;
    }
    

    IEnumerator Disappear()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        while (true)
        {
            StartCoroutine(DisappearCube());
            yield return new WaitForSeconds(DISAPPEAR_TIME + Random.Range(0.7f, 2.5f));
        }
    }

    IEnumerator DisappearCube()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 4);
        int z = Random.Range(0, 4);

        if (_isCubesDisappearing[x, y, z])
            yield break;

        _cubes[x, y, z].Disappear(DISAPPEAR_TIME);
        _isCubesDisappearing[x, y, z] = true;

        yield return _disappearWait;

        _isCubesDisappearing[x, y, z] = false;
    }
}
