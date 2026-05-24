using UnityEngine;

public sealed class Collectible : MonoBehaviour
{
    [SerializeField] private int value = 1;

    public int Value => value;
}