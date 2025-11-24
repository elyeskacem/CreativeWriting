using UnityEngine;

public class FloatingUpDown : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatDistance = 0.2f;

    private Vector3 startPos;
    private ParticleSystem ps;

    void Start()
    {
        startPos = transform.position;

        // --- Create Particle System ---
        ps = gameObject.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 3f;
        main.startSpeed = 0.2f;
        main.startSize = 0.2f;
        main.loop = true;

        // IMPORTANT: make particles move with the book
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 500f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;

        // make particles cover the whole book
        shape.radius = 20.0f;   // increase this to spread over the book

        // Red color over lifetime
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = grad;

        // IMPORTANT: give the particles a real material so they're not purple
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", Color.white);

        ps.Play();
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatDistance;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
