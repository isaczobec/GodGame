using System;

public class NPCStats {
    private float _movementSpeed;
    private float _maxHealth;
    private float _currentHealth;

    // Declare events for when the properties change
    public event EventHandler<float> OnMovementSpeedChanged;
    public event EventHandler<float> OnMaxHealthChanged;
    public event EventHandler<float> OnCurrentHealthChanged;

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

    public NPCStats(NPCBaseStats NPCBaseStats) {
        movementSpeed = NPCBaseStats.movementSpeed;
        maxHealth = NPCBaseStats.maxHealth;
        currentHealth = maxHealth;
    }
}