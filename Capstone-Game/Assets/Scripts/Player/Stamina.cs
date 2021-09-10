using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    public UnityEngine.UI.Image staminaBar;

    [Header("Settings")]
    public float maxStamina = 100f;
    public float minStaminaToStartUse = 10f;
    public float staminaRegenPerSecond = 10f;
    public float staminaRegenDelay = 1f;

    [Space]
    public bool allowNegativeStamina = false;
    public bool zeroStopsRegen = false;
    public bool failStopsRegen = false;
    
    /// <summary> Current Stamina of the player. </summary>
    private float stamina;
    /// <summary> Whether the player is using Stamina or not. Prevents recharging when true. </summary>
    private bool usingStamina;
    /// <summary> Time of the last stamina use. Allows calculation of regen delay. </summary>
    private float lastStaminaUse;
    /// <summary> True if the player stopped using Stamina recently, and the value is below the regen-use threshold. Prevents Stamina use. </summary>
    private bool belowStaminaRegenThreshold;

    // Start is called before the first frame update
    void Awake()
    {

        stamina = maxStamina;
    }

    public bool StaminaAvailable()
    {
        if (stamina > 0 && belowStaminaRegenThreshold == false)
            return true;
        return false;
    }

    public void UseAllStamina()
    {
        stamina = 0;
        usingStamina = true;
        belowStaminaRegenThreshold = true;
    }

    /// <summary> Call when an action requires Stamina. Returns true if there is enough stamina for the action (based on stamina settings), and then deducts the specified amount. </summary>
    public bool UseStamina(float value)
    {
        bool succeeded = false;

        //Warn about negative stamina uses (a seperate function should be used to regen quicker).
        if (value < 0)
            Debug.LogError("Reducing stamina by negative value. Is this correct?");

        if (value <= 0 && !zeroStopsRegen)
            return true;

        // Check if the stamina is above 0, and is not locked by regeneration.
        if (belowStaminaRegenThreshold == false && stamina > 0)
        {
            // Return true, use up the specified stamina, and signal that stamina was used this update.
            succeeded = true;
            stamina -= value;
            usingStamina = true;

            // Refund the negative stamina if negative stamina has been disabled.
            if (!allowNegativeStamina && stamina < 0)
                stamina = 0;
        }

        // Lock stamina use until it exceeds the threshold when it reaches 0.
        if (stamina <= 0)
            belowStaminaRegenThreshold = true;

        // Signal that stamina was used regardless of success if the setting is enabled.
        if (failStopsRegen)
            usingStamina = true;

        return succeeded;
    }

    // Update is called once per frame
    void Update()
    {
        UpdStamina();
    }

    /// <summary> Regenerate Stamina based on settings, and update Stamina graphic. </summary>
    private void UpdStamina()
    {
        if (usingStamina)
        {
            lastStaminaUse = Time.time;
        }
        else if (Time.time > lastStaminaUse + staminaRegenDelay)
        {
            stamina += staminaRegenPerSecond * Time.deltaTime;

            if (stamina >= maxStamina)
            {
                belowStaminaRegenThreshold = false;
                stamina = maxStamina;
            }
            else if (stamina >= minStaminaToStartUse)
            {
                belowStaminaRegenThreshold = false;
            }
        }

        if (staminaBar != null)
            staminaBar.fillAmount = stamina / maxStamina;

        usingStamina = false;
    }
}
