using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float playerMaxHealth = 150f;
    [SerializeField] private float invincibleTimer = 0.6f;
    private float currentHealth;

    [SerializeField] private SceneBoundsSO sceneBoundsSO;

    //getter for currentHealth
    public float GetCurrentHealth() => currentHealth;

    [SerializeField] private FreezeInputEventSO freezeInputs;

    private bool isInvincible = false;
    private Coroutine invincibleCoroutine;

    private const float healthPerHeart = 50f;

    public int FullHearts { get; private set; }
    public int HalfHearts { get; private set; }

    //EVENTS:
    public event Action OnHealthChanged;
    public event Action OnPlayerHurt;
    public event Action OnPlayerDeath;

    void Start()
    {
        currentHealth = playerMaxHealth;
        RecalculateHearts();
        OnHealthChanged?.Invoke();
    }

    private void Update()
    {
        if (currentHealth <= 0f) return;
        Vector3 pos = this.transform.position;
        //we hit rock bottom
        if(Mathf.Approximately(pos.y, sceneBoundsSO.minY) )
        {
            ReduceCurrentHealth(currentHealth);
        }
    }

    public void ReduceCurrentHealth(float damageAmount)
    {
        if(!isInvincible)
        {
            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, playerMaxHealth);
            RecalculateHearts();
            OnHealthChanged?.Invoke();
            OnPlayerHurt?.Invoke();

            if(currentHealth <= 0f)
            {
                OnPlayerDeath?.Invoke();
                //WARNING: do check this cause we're not lifting this lock anywhere
                freezeInputs.Raise(true);
            }

            isInvincible = true;

            if(invincibleCoroutine == null)
                StartCoroutine(StartInvincibilityFrames());
        }
    }

    IEnumerator StartInvincibilityFrames()
    {
        yield return new WaitForSeconds(invincibleTimer);
        isInvincible = false;
        invincibleCoroutine = null;
    }

    public void IncreaseCurrentHealth(float healthIncrement)
    {
        currentHealth += healthIncrement;
        currentHealth = Mathf.Clamp(currentHealth, 0, playerMaxHealth);
        RecalculateHearts();
        OnHealthChanged?.Invoke();
    }

    public void IncreaseMaxHealth(float extra)
    {
        playerMaxHealth += extra;
        currentHealth = playerMaxHealth;
        RecalculateHearts();
        OnHealthChanged?.Invoke();
    }

    private void RecalculateHearts()
    {
        float health = currentHealth;

        FullHearts = 0;
        HalfHearts = 0;

        if (health <= 0f) return;

        FullHearts = (int)(health / healthPerHeart);
        health %= healthPerHeart;

        if (health >= healthPerHeart / 2f)
        {
            HalfHearts = 1;
        }
    }
}

