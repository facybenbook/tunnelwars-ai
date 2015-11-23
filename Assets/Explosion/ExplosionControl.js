#pragma strict

function Start ()
{
    var angle : float = Random.Range(0, 360);
    transform.Rotate (Vector3.forward * angle);
}

function Update ()
{
    Destroy(gameObject, 0.2);
}