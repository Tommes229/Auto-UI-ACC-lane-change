using TurnTheGameOn.SimpleTrafficSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DensityController : MonoBehaviour
{
    public Text poolSizeText;
    public Text densityText;
    public Text activeText;
    public Slider densitySlider;

    private void Start()
    {
        poolSizeText.text = "Cars In Pool: " + AITrafficController.Instance.carsInPool.ToString();
        densitySlider.maxValue = AITrafficController.Instance.carsInPool;
        densitySlider.value = AITrafficController.Instance.density;
        SetDensity();
        StartCoroutine("UpdateText");
    }

    public void SetDensity()
    {
        AITrafficController.Instance.density = (int) densitySlider.value;
        densityText.text = "Target Density: " + densitySlider.value.ToString();
    }

    public IEnumerator UpdateText()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            activeText.text = "Cars Active: " +  AITrafficController.Instance.currentDensity.ToString();
        }
    }
}
