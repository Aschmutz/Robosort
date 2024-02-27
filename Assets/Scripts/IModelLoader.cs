using UnityEngine;

public abstract class IModelLoader : MonoBehaviour
{
    /**
     * Loads and Instatiates a Gameobject with a Meshcollider, Rigidbody, Meshrender, and Meshfilter of the Given alembic file at the @path variable
     * The Mesh Collider is set to Convex Mode for Compatability with the Rigidbody.
     * @path to an alembic file that is to be loaded.
     */
    /**
     * Returns a copy of the loaded model as a gameobject with a rigidbody and a convexe meshcollider as well as renderer both using the loaded mesh
     */
    public abstract GameObject Load(string path);
}