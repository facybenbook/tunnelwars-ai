#pragma strict

var visible : boolean = false;

function Start ()
{
    transform.GetChild(0).gameObject.GetComponent.<Renderer>().enabled = false;
    transform.GetChild(1).gameObject.GetComponent.<Renderer>().enabled = false;
}

function Update ()
{

}

function SetMaster(mode : boolean)
{
    visible = true;
    if (mode == true)
    {
        transform.GetChild(0).gameObject.GetComponent.<Renderer>().enabled = true;
        transform.GetChild(1).gameObject.GetComponent.<Renderer>().enabled = false;
    }
    else
    {
        transform.GetChild(0).gameObject.GetComponent.<Renderer>().enabled = false;
        transform.GetChild(1).gameObject.GetComponent.<Renderer>().enabled = true;
    }
}