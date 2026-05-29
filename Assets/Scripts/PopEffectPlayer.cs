using UnityEngine;

public class PopEffectPlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem popEffectPrefab;

    public void Play(Vector3 position, Color color)
    {
        if (popEffectPrefab == null)
            return;

        ParticleSystem effect = Instantiate(popEffectPrefab, position, Quaternion.identity, transform);

        ParticleSystem.MainModule main = effect.main;
        main.startColor = color;

        effect.Play();

        Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax);
    }
}