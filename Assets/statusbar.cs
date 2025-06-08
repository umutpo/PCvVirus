using UnityEngine;

public class statusbar : MonoBehaviour
{

    public Transform bar;
    
    public void setState(int current, int max) {
        float currentPercentage = (float)current / (float)max;
        if (currentPercentage < 0f) {
            currentPercentage = 0.0f;
        }
        Debug.Log(currentPercentage);

        bar.transform.localScale = new Vector3(currentPercentage, bar.transform.localScale.y, bar.transform.localScale.z);
        bar.transform.localPosition = new Vector3(-1 * (1.0f - currentPercentage) / 2.0f, bar.transform.localPosition.y, bar.transform.localPosition.z);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
