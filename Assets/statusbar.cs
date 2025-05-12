using UnityEngine;

public class statusbar : MonoBehaviour
{

    public Transform bar;
    
    public void setState(int current, int max) {
        float state = (float)current;
        state /= max;

        if (state < 0f) {
            state = 0.0f;
        }

        Vector3 currentState = bar.transform.localScale;

        bar.transform.localScale = new Vector3(state, currentState.y, currentState.z);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
