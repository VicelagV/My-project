using UnityEngine;

namespace AllAboard.Movement
{
    public abstract class PrototypeInteractable : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public virtual bool AllowedWhileSeated => false;
        public abstract bool Interact(PlayerInteraction actor);
    }
}
