using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    [SerializeField] private List<Image> heartImages;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += UpdateHearts;
    }

    private void Start()
    {
        UpdateHearts();
    }

    private void OnDisable()
    {
        if(playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts()
    {
        int full = playerHealth.FullHearts;
        int half = playerHealth.HalfHearts;

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (full > 0)
            {
                heartImages[i].sprite = fullHeart;
                full--;
            }
            else if (half > 0)
            {
                heartImages[i].sprite = halfHeart;
                half--;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }
}
