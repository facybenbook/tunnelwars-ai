
var markedForDestroy = false;

function Start ()
{
    var angle : float = Random.Range(0, 360);
    transform.Rotate (Vector3.forward * angle);
    markedForDestroy = false;
}

function Update ()
{
    if (!markedForDestroy) {
    	Destroy(gameObject, 0.2);
    }
}