using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TempShopController : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI jumpText;
    public TextMeshProUGUI maxWireText;
    public TextMeshProUGUI boosterUsageText;
    public TextMeshProUGUI boosterRecoveryText;

    private int _speedLevel;
    private int _jumpLevel;
    private int _maxWireLevel;
    private int _boosterUsageLevel;
    private int _boosterRecoveryLevel;

    // 스킬 최대 레벨
    private const int SPEED_MAX_LEVEL = 12;
    private const int JUMP_MAX_LEVEL = 12;
    private const int MAX_WIRE_MAX_LEVEL = 10;
    private const int BOOSTER_USAGE_MAX_LEVEL = 5;
    private const int BOOSTER_RECOVERY_MAX_LEVEL = 8;

    // 강화 증가폭
    private const float SPEED_INC = 0.05f;
    private const float JUMP_INC = 0.05f;
    private const int MAX_WIRE_INC = 2;
    private const float BOOSTER_USAGE_INC = -0.02f; // 1초 당 사용 비율 감소
    private const float BOOSTER_RECOVERY_INC = -0.5f; // n초 후 전부 회복
    // 디폴트값
    private const float SPEED_DEFAULT = 1f;
    private const float JUMP_DEFAULT = 1f;
    private const int MAX_WIRE_DEFAULT = 40;
    private const float BOOSTER_USAGE_DEFAULT = 0.3f;
    private const float BOOSTER_RECOVERY_DEFAULT = 8f;

    private void Start()
    {
        _speedLevel = _jumpLevel = _maxWireLevel = 0;
        _boosterUsageLevel = _boosterRecoveryLevel = 0;
        SetSpeedRate();
        SetJumpRate();
        SetMaxWireLength();
        SetBoosterUsageRate();
        SetBoosterRecoveryRate();
    }

    public void SpeedUpgrade()
    {
        if (_speedLevel >= SPEED_MAX_LEVEL)
            return;
        _speedLevel++;
        SetSpeedRate();
    }
    public void SpeedDowngrade()
    {
        if (_speedLevel <= 0)
            return;
        _speedLevel--;
        SetSpeedRate();
    }
    private void SetSpeedRate()
    {
        float speedRate = SPEED_DEFAULT + _speedLevel * SPEED_INC;
        float nextSpeedRate = SPEED_DEFAULT + (_speedLevel + 1) * SPEED_INC;
        speedText.text = $"{speedRate * 100}% -> {nextSpeedRate * 100}%";
        if (_speedLevel == SPEED_MAX_LEVEL) speedText.text = $"MAX : {speedRate * 100}%";
        PlayerManager.Instance.skill.SetSpeedRate(speedRate);
    }



    public void JumpUpgrade()
    {
        if (_jumpLevel >= JUMP_MAX_LEVEL)
            return;
        _jumpLevel++;
        SetJumpRate();
    }
    public void JumpDowngrade()
    {
        if (_jumpLevel <= 0)
            return;
        _jumpLevel--;
        SetJumpRate();
    }
    private void SetJumpRate()
    {
        float jumpHeightRate = JUMP_DEFAULT + _jumpLevel * JUMP_INC;
        float nextJumpHeightRate = SPEED_DEFAULT + (_jumpLevel + 1) * SPEED_INC;
        jumpText.text = $"{jumpHeightRate * 100}% -> {nextJumpHeightRate * 100}%";
        if (_jumpLevel == JUMP_MAX_LEVEL) jumpText.text = $"MAX : {jumpHeightRate * 100}%";
        PlayerManager.Instance.skill.SetJumpHeightRate(jumpHeightRate);
    }



    public void MaxWireUpgrade()
    {
        if (_maxWireLevel >= MAX_WIRE_MAX_LEVEL)
            return;
        _maxWireLevel++;
        SetMaxWireLength();
    }
    public void MaxWireDowngrade()
    {
        if (_maxWireLevel <= 0)
            return;
        _maxWireLevel--;
        SetMaxWireLength();
    }
    private void SetMaxWireLength()
    {
        float maxWireLength = MAX_WIRE_DEFAULT + _maxWireLevel * MAX_WIRE_INC;
        float nextMaxWireLength = MAX_WIRE_DEFAULT + (_maxWireLevel + 1) * MAX_WIRE_INC;
        maxWireText.text = $"{maxWireLength}m -> {nextMaxWireLength}m";
        if (_maxWireLevel == MAX_WIRE_MAX_LEVEL)
            maxWireText.text = $"MAX : {maxWireLength}m";
        PlayerManager.Instance.skill.SetMaxWireLength(maxWireLength);
    }



    public void BoosterUsageUpgrade()
    {
        if (_boosterUsageLevel >= BOOSTER_USAGE_MAX_LEVEL)
            return;
        _boosterUsageLevel++;
        SetBoosterUsageRate();
    }
    public void BoosterUsageDowngrade()
    {
        if (_boosterUsageLevel <= 0)
            return;
        _boosterUsageLevel--;
        SetBoosterUsageRate();
    }
    private void SetBoosterUsageRate()
    {
        float boosterUsageRate = BOOSTER_USAGE_DEFAULT + _boosterUsageLevel * BOOSTER_USAGE_INC;
        Debug.Log(boosterUsageRate);
        float nextBoosterUsageRate = BOOSTER_USAGE_DEFAULT + (_boosterUsageLevel + 1) * BOOSTER_USAGE_INC;
        boosterUsageText.text = $"1초 당 {boosterUsageRate * 100}% 소모 -> {nextBoosterUsageRate * 100}% 소모";
        if (_boosterUsageLevel == BOOSTER_USAGE_MAX_LEVEL)
            boosterUsageText.text = $"MAX : 1초 당\n{boosterUsageRate * 100}% 소모";
        PlayerManager.Instance.skill.SetBoosterUsageRate(boosterUsageRate);
    }



    public void BoosterRecoveryUpgrade()
    {
        if (_boosterRecoveryLevel >= BOOSTER_RECOVERY_MAX_LEVEL)
            return;
        _boosterRecoveryLevel++;
        SetBoosterRecoveryRate();
    }
    public void BoosterRecoveryDowngrade()
    {
        if (_boosterRecoveryLevel <= 0)
            return;
        _boosterRecoveryLevel--;
        SetBoosterRecoveryRate();
    }
    private void SetBoosterRecoveryRate()
    {
        float boosterRecoveryRate = BOOSTER_RECOVERY_DEFAULT + _boosterRecoveryLevel * BOOSTER_RECOVERY_INC;
        float nextBoosterRecoveryRate = BOOSTER_RECOVERY_DEFAULT + (_boosterRecoveryLevel + 1) * BOOSTER_RECOVERY_INC;
        boosterRecoveryText.text = $"{boosterRecoveryRate}초 -> {nextBoosterRecoveryRate}초 후\n완전 회복";
        if (_boosterRecoveryLevel == BOOSTER_RECOVERY_MAX_LEVEL)
            boosterRecoveryText.text = $"MAX : {boosterRecoveryRate}초 후\n완전 회복";
        PlayerManager.Instance.skill.SetBoosterRecoveryRate(1f / boosterRecoveryRate);
    }



    public void Close()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
