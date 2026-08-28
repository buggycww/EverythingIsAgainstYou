using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    public string Name;
    public Sprite Portrait;
    public Color PortraitTint = Color.white;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public string[] options;
    public string[] playerInteractionDialogueLines;
    public AudioClip voiceSound;
    public float voicePitch = 1f;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
}
