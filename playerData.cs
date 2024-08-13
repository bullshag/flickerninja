using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerData : MonoBehaviour
{
    public int hpCurrent;
    public int hpMax;
    public int stCurrent;
    public int stMax;
    public float regenRate;
    public int regenAmount;
    // Start is called before the first frame update
    void Start()
    {
        hpMax = 100;
        stMax = 100;
        resetStHP(); 
        StartCoroutine(RegenerateStamina()); // Start the regeneration coroutine

    }

    public void resetStHP()
    {
        hpCurrent = hpMax;
        stCurrent = stMax;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator RegenerateStamina()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenRate); // Wait for 1 second

            if (stCurrent < stMax) // Check if stamina is not full
            {
                stCurrent += Mathf.FloorToInt(regenAmount); // Increase stamina
                stCurrent = Mathf.Clamp(stCurrent, 0, stMax); // Ensure stamina doesn't exceed max
            }
        }
    }

}
