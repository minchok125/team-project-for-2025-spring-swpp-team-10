using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INextCheckpointObserver
{

    /// <summary>
    /// 플레이어의 체크포인트 진행 상황이 업데이트될 때 호출됩니다. (예: "3 / 10 완료")
    /// </summary>
    /// <param name="activatedIndex">마지막으로 활성화된 체크포인트의 순서 인덱스입니다. (0부터 시작, 아직 하나도 안 지났으면 -1)</param>
    /// <param name="totalCheckpoints">순서 목록에 등록된 전체 체크포인트의 개수입니다.</param>
    void OnCheckpointProgressUpdated(int activatedIndex, int totalCheckpoints);


    /// <summary>
    /// 다음 목표 체크포인트의 위치가 변경되었을 때 호출됩니다.
    /// </summary>
    /// <param name="nextPosition">다음 체크포인트의 월드 좌표입니다. 마지막 체크포인트이거나 목표가 없으면 null이 전달됩니다.</param>
    void OnNextCheckpointChanged(Vector3? nextPosition);
}
