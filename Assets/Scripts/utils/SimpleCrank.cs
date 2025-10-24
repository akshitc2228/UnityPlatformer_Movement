using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//consider moving this to a global location
public interface IActivatable
{
    void Activate();
}

public abstract class SimpleCrank : MonoBehaviour
{
    [SerializeField] protected Sprite toggledSprite;
    [SerializeField] protected bool isToggle = false;

    protected bool activated = false;
    protected SpriteRenderer sr;
    protected Sprite originalSprite;
    protected bool playerInRange = false;

    protected virtual void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSprite = sr != null ? sr.sprite : null;
    }

    protected virtual void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.O))
        {
            if (!isToggle && activated) return;

            activated = isToggle ? !activated : true;
            sr.sprite = activated ? toggledSprite : originalSprite;

            ActivationAction();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    protected virtual void ActivationAction() { }
}
