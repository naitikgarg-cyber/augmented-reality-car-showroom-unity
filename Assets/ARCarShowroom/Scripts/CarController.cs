using UnityEngine;

public class CarController : MonoBehaviour
{
    public Renderer bodyRenderer;
    public Transform leftDoor, rightDoor, wheelGroup;
    public Light[] headlights;
    bool doors, engine;
    float lastDistance, wheelSpin;
    Vector3 initialScale;

    void Start() { initialScale = transform.localScale; }
    void Update()
    {
        if (engine && wheelGroup) { wheelSpin += 250f * Time.deltaTime; wheelGroup.localRotation = Quaternion.Euler(wheelSpin,0,0); }
        if (Input.touchCount == 1)
        {
            Touch t=Input.GetTouch(0);
            if(t.phase==TouchPhase.Moved) transform.Rotate(Vector3.up,-t.deltaPosition.x*.15f,Space.World);
        }
        if(Input.touchCount>=2)
        {
            Touch a=Input.GetTouch(0), b=Input.GetTouch(1); float d=Vector2.Distance(a.position,b.position);
            if(a.phase==TouchPhase.Began||b.phase==TouchPhase.Began) lastDistance=d;
            if(lastDistance>0) { float f=d/lastDistance; transform.localScale=Vector3.ClampMagnitude(transform.localScale*f,1.8f); transform.localScale=Vector3.Max(transform.localScale,Vector3.one*.12f); lastDistance=d; }
        }
    }
    public void ToggleDoors(){doors=!doors;if(leftDoor)leftDoor.localRotation=Quaternion.Euler(0,doors?-55:0,0);if(rightDoor)rightDoor.localRotation=Quaternion.Euler(0,doors?55:0,0);}
    public void ToggleLights(){bool on=headlights!=null&&headlights.Length>0&&!headlights[0].enabled;if(headlights!=null)foreach(var l in headlights)if(l)l.enabled=on;}
    public void ToggleEngine(){engine=!engine;}
    public void SetColor(Color c){if(bodyRenderer)bodyRenderer.material.color=c;}
}
