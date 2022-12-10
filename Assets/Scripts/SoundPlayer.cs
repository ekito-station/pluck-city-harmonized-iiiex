using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioClip d2;
    public AudioClip e2;
    public AudioClip f2;
    public AudioClip g2;
    public AudioClip a2;
    public AudioClip c3;
    public AudioClip e3;
    public AudioClip f3;
    public AudioClip g3;
    public AudioClip a3;
    public AudioClip b3;

    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Collided with string.");
        StringController stringController = other.gameObject.GetComponent<StringController>();
        float strPitch = stringController.pitch;

        switch (strPitch)
        {
            case 0.36f:
                audioSource.PlayOneShot(b3);
                Debug.Log("Played b3.");
                break; 
            case 0.44f:
                audioSource.PlayOneShot(a3);
                Debug.Log("Played a3.");
                break;                
            case 0.56f:
                audioSource.PlayOneShot(g3);
                Debug.Log("Played g3.");
                break;
            case 0.64f:
                audioSource.PlayOneShot(f3);
                Debug.Log("Played f3.");
                break;
            case 1.0f:
                audioSource.PlayOneShot(e3);
                Debug.Log("Played e3.");
                break;
            case 1.44f:
                audioSource.PlayOneShot(c3);
                Debug.Log("Played c3.");
                break;
            case 1.78f:
                audioSource.PlayOneShot(a2);
                Debug.Log("Played a2.");
                break;
            case 2.25f:
                audioSource.PlayOneShot(g2);
                Debug.Log("Played g2.");
                break;
            case 2.56f:
                audioSource.PlayOneShot(f2);
                Debug.Log("Played f2.");
                break;
            case 3.16f:
                audioSource.PlayOneShot(e2);
                Debug.Log("Played e2.");
                break;
            case 100.0f:
                audioSource.PlayOneShot(d2);
                Debug.Log("Played d2.");
                break;                                                                                                                                            
            default:
                Debug.Log("Played none.");
                break;
        }
    }
}
