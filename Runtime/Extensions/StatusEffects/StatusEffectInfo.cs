using UnityEngine;

[System.Serializable]
public class StatusEffectInfo
{
    //Time
    public float duration;
    public float currentTime;

    public Coroutine update;

    //State
    public State currentState;
    public enum State
    {
        Pending,
        Active,
        Expired
    }
}
