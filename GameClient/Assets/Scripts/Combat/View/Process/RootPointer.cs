using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootPointer : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Transform m_Root;
#pragma warning restore 0649
    public Transform root { get { return m_Root; } }
}
