using System;
using UnityEngine;

public class NPCStats {
    private float _maxHealth;
    private float _currentHealth;
    private float _movementSpeed;
    private float _attackSpeed;
    private float _invincibilityTime = 0f;

    // Declare events for when the properties change
    public event EventHandler<float> OnMovementSpeedChanged;
    public event EventHandler<float> OnMaxHealthChanged;
    public event EventHandler<float> OnCurrentHealthChanged;
    public event EventHandler<float> OnAttackSpeedChanged;

    public event EventHandler<bool> OnInvincibilityChanged;

    public float movementSpeed {
        get => _movementSpeed;
        set {
            if (_movementSpeed != value) {
                _movementSpeed = value;
                OnMovementSpeedChanged?.Invoke(this,_movementSpeed);
            }
        }
    }

    public float maxHealth {
        get => _maxHealth;
        set {
            if (_maxHealth != value) {
                _maxHealth = value;
                OnMaxHealthChanged?.Invoke(this,_maxHealth);
            }
        }
    }

    public float currentHealth {
        get => _currentHealth;
        set {
            if (_currentHealth != value) {
                _currentHealth = value;
                OnCurrentHealthChanged?.Invoke(this,_currentHealth);
            }
        }
    }

    public float attackSpeed {
        get => _attackSpeed;
        set {
            if (_attackSpeed != value) {
                _attackSpeed = value;
                OnAttackSpeedChanged?.Invoke(this,_attackSpeed);
            }
        }
    }

    public float invincibilityTime {
        get => _invincibilityTime;
        set {
            if (_invincibilityTime != value) {
                _invincibilityTime = value;
                OnInvincibilityChanged?.Invoke(this,_invincibilityTime > 0);
            }
        }
    }

    /// <summary>
    /// If the NPC still has invincibility frames.
    /// </summary>
    public bool isInvincible => invincibilityTime > 0;

    public NPCStats(NPCBaseStats NPCBaseStats) {
        movementSpeed = NPCBaseStats.movementSpeed;
        maxHealth = NPCBaseStats.maxHealth;
        currentHealth = maxHealth;
        attackSpeed = NPCBaseStats.attackSpeed;
    }

    public void UpdateStats() {
        UpdateInvincibilityTime();
    }

    private void UpdateInvincibilityTime() {
        if (invincibilityTime > 0) {
            invincibilityTime -= Time.deltaTime;
            if (invincibilityTime <= 0) {
                invincibilityTime = 0;
                OnInvincibilityChanged?.Invoke(this,false);
            }
        } 
    }

    public float GetHealthPercentage() {
        return currentHealth / maxHealth;
    }
}