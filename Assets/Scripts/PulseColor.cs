using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class PulseColor : MonoBehaviour
{
    private float PulseTimeInterval = 0.3f;
    private Color VariationColor;
    private Color InitialColor;
    private int count;
    public int PulseCount = 500;
    private bool isRed = false;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.3f);
        Pulse();
    }

    public void Pulse()
    {
        if (IsInvoking("DoPulse"))
        {
            return;
        }
        Color col = GetComponent<RawImage>().color;
        if (isRed){
            InitialColor = col;
            VariationColor = new Color(255,255,255,255);
            count = PulseCount;
            InvokeRepeating("DoPulse", 0.001f, PulseTimeInterval);
        }
    }

    void DoPulse()
    {
        Color c = count % 2 == 0 ? VariationColor : InitialColor;
        GetComponent<RawImage>().color = c;
        if (--count == 0) CancelInvoke("DoPulse");
        if (!isRed) { GetComponent<RawImage>().color = VariationColor; CancelInvoke("DoPulse"); }
    }

    public void setRed(bool set)
    {
        isRed = set;
    }
}