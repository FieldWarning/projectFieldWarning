using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MSG_Scrollbar : MonoBehaviour
{
    private Scrollbar _scrollbar;
    // Start is called before the first frame update
    void Start()
    {
        _scrollbar = GetComponent<Scrollbar>();
    }

    // Update is called once per frame
    void Update()
    {
        _scrollbar.value = 0;
    }
}
