using UnityEngine;

public class LuckManager : MonoBehaviour
{
    static LuckManager instance = null;
    static int luck = 0;
    private void Awake()
    {
        instance = this;
    }
    
    public static bool Calculate(float probability, bool isPositive)
    {
        if (luck >= 0)
        {
            for (int i = 0; i <= luck; i++)
            {
                if (probability > Random.value)
                {
                    if (isPositive)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else
        {
            for (int i = 0; i >= luck; i--)
            {
                if (probability < Random.value)
                {
                    return false;
                }
            }
            return true;
        }
    }
    public static void AddLuck() => luck++;
    public static void SetLuck(int value) => luck = value;
}
