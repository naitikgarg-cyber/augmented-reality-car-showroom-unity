using UnityEngine;

public class CarFactory : MonoBehaviour
{
    [ContextMenu("Create Car Prefab")]
    public void CreatePrefab(){ var car=Build(); UnityEditor.PrefabUtility.SaveAsPrefabAsset(car,"Assets/ARCarShowroom/Prefabs/ARShowroomCar.prefab"); DestroyImmediate(car); }
    public static GameObject Build()
    {
        GameObject r=new("AR Showroom Car");
        Material body=Mat(new Color(.04f,.18f,.85f),.8f,.75f), glass=Mat(new Color(.02f,.05f,.08f),.2f,.35f), tire=Mat(Color.black,.1f,.2f), chrome=Mat(new Color(.65f,.68f,.72f),.9f,.7f), lamp=Mat(Color.white,.1f,.2f); lamp.EnableKeyword("_EMISSION"); lamp.SetColor("_EmissionColor",Color.white*2f);
        GameObject b=Cube("Body",r.transform,new Vector3(0,.55f,0),new Vector3(3.8f,.65f,1.8f),body); Cube("Cabin",r.transform,new Vector3(-.05f,1.02f,0),new Vector3(2.1f,.8f,1.55f),glass); Cube("Hood",r.transform,new Vector3(1.15f,.83f,0),new Vector3(1.25f,.25f,1.7f),body); Cube("Trunk",r.transform,new Vector3(-1.45f,.82f,0),new Vector3(.75f,.3f,1.7f),body);
        GameObject wg=new("Wheels");wg.transform.SetParent(r.transform,false);foreach(float x in new[]{-1.25f,1.25f})foreach(float z in new[]{-.98f,.98f}){var w=Cyl("Wheel",wg.transform,new Vector3(x,.42f,z),new Vector3(.42f,.16f,.42f),tire);w.transform.localRotation=Quaternion.Euler(90,0,0);var rim=Cyl("Rim",w.transform,Vector3.zero,new Vector3(.2f,.17f,.2f),chrome);rim.transform.localRotation=Quaternion.Euler(90,0,0);}
        var ld=Cube("Left Door",r.transform,new Vector3(.05f,.63f,-.94f),new Vector3(1.35f,.65f,.08f),body);var rd=Cube("Right Door",r.transform,new Vector3(.05f,.63f,.94f),new Vector3(1.35f,.65f,.08f),body);Cube("Front Bumper",r.transform,new Vector3(2.02f,.43f,0),new Vector3(.18f,.25f,1.82f),chrome);Cube("Rear Bumper",r.transform,new Vector3(-2.02f,.43f,0),new Vector3(.18f,.25f,1.82f),chrome);
        var h1=Cube("Headlight L",r.transform,new Vector3(2.08f,.68f,-.55f),new Vector3(.08f,.18f,.42f),lamp);var h2=Cube("Headlight R",r.transform,new Vector3(2.08f,.68f,.55f),new Vector3(.08f,.18f,.42f),lamp);Light l1=LightAt(h1.transform),l2=LightAt(h2.transform);l1.enabled=l2.enabled=false;
        var c=r.AddComponent<CarController>();c.bodyRenderer=b.GetComponent<Renderer>();c.leftDoor=ld.transform;c.rightDoor=rd.transform;c.headlights=new[]{l1,l2};c.wheelGroup=wg.transform;return r;
    }
    static Material Mat(Color c,float m,float s){var x=new Material(Shader.Find("Standard"));x.color=c;x.SetFloat("_Metallic",m);x.SetFloat("_Glossiness",s);return x;}
    static GameObject Cube(string n,Transform p,Vector3 pos,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().material=m;DestroyImmediate(g.GetComponent<Collider>());return g;}
    static GameObject Cyl(string n,Transform p,Vector3 pos,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().material=m;DestroyImmediate(g.GetComponent<Collider>());return g;}
    static Light LightAt(Transform p){var g=new GameObject("Beam");g.transform.SetParent(p,false);g.transform.localPosition=new Vector3(.15f,0,0);var l=g.AddComponent<Light>();l.type=LightType.Spot;l.range=8;l.spotAngle=35;l.intensity=3;return l;}
}
