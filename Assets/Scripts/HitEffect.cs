using UnityEngine;
using UnityEngine.UI;

// This version uses UI Images instead of SpriteRenderers,
// so it works inside a Canvas (which is what your game uses).

public class HitEffect : MonoBehaviour
{
    public int particleCount = 12;
    public float speed = 300f;
    public float lifetime = 0.4f;
    public Color color = new Color(0.6f, 0.2f, 0.9f, 1f);
    public float particleSize = 20f;

    private RectTransform[] particles;

    private Vector2[] directions;
    private Image[] images;
  
    private float timer = 0f;

    void Start()
    {
        // We need to find the Canvas so our particles appear
        // in the same UI layer as everything else.
        Canvas canvas = GetComponentInParent<Canvas>();

        for (int i = 0; i < particleCount; i++)
        {

            GameObject p = new GameObject("Particle");

            p.transform.SetParent(transform, false);

            RectTransform rt = p.AddComponent<RectTransform>();

            // Set the particle's size
            rt.sizeDelta = new Vector2(particleSize, particleSize);
            rt.anchoredPosition = Vector2.zero;

            // Add an Image component so it's visible
            Image img = p.AddComponent<Image>();
            img.color = color;
            particles[i] = rt;
            images[i] = img;

            // Random direction for this particle to fly
            float angle = Random.Range(0f, Mathf.PI * 2f);
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    void Awake()
    {
        particles = new RectTransform[particleCount];
        directions = new Vector2[particleCount];
        images = new Image[particleCount];
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] == null) continue;

            // Move outward using anchoredPosition (UI coordinates)
            particles[i].anchoredPosition +=
                directions[i] * speed * Time.deltaTime;

            // Fade out
            Color c = images[i].color;
            c.a = 1f - progress;
            images[i].color = c;

            // Shrink
            float scale = 1f - progress;
            particles[i].localScale = Vector3.one * scale;
        }
    }
}