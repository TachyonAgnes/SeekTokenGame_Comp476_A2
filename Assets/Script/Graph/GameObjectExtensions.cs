using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// Using GameObject.DestroyImmediate when the game is being edited in the Unity Editor is necessary because the editor operates in a different lifecycle from the game. 
    /// In the editor, changes made to the scene are immediately reflected in the editor, so objects need to be destroyed immediately to accurately reflect the state of the scene.
    /// Additionally, the DestroyImmediate method can be useful in cases where you want to destroy an object and free up memory immediately, rather than waiting for the end of the frame or a specified delay.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="delay"></param>
    public static void DestroyChildren(this GameObject gameObject, float delay = 0) {
        for(int i = 0; i < gameObject.transform.childCount; ++i) {
            if (Application.isPlaying) {
                if(delay <= 0) {
                    GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
                }
                else {
                    GameObject.Destroy(gameObject.transform.GetChild(i).gameObject, delay);
                }
            }
            else {
                GameObject.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
                --i;
            }
        }
    }
}
