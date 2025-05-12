using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = 9.8f;


    [Header("Camera Settings")]
    [SerializeField] private bool invertYAxis = false; 
    
 
}
