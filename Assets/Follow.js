#pragma strict

public var target : Transform;

function Start () {

}

function Update ()
{
    transform.position.x = Mathf.Min(Mathf.Max(target.transform.position.x, 256), 2688);
    transform.position.y = Mathf.Min(Mathf.Max(target.transform.position.y, 256), 1664);
}