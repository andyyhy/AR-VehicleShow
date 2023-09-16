using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AROpenURL : MonoBehaviour
{
    public void OpenUrl()
    {
        Application.OpenURL("https://en.wikipedia.org/wiki/Car");
    }
}
