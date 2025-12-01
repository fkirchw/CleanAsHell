using System;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponHandling : MonoBehaviour
{
    [FormerlySerializedAs("_boneHandL")] [SerializeField] private Transform boneHandL;
    [FormerlySerializedAs("_boneHandR")] [SerializeField] private Transform boneHandR;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (boneHandL == null || boneHandR == null)
        {
            Console.WriteLine("NO BONES 😭");
        }
        else
        {
            Console.WriteLine("BONES 😏");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Console.WriteLine(boneHandL.position);
    }
}