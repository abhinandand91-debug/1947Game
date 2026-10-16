// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// Spawns short-lived particle bursts built entirely in code, so no extra assets are needed.
/// </summary>
public static class Explosion
{
    private static Material _material;

    /// <summary>
    /// Spawns a burst of particles that destroys itself when finished.
    /// </summary>
    /// <param name="position">World position of the burst</param>
    /// <param name="color">Main particle color</param>
    /// <param name="scale">1 is a ship-sized explosion, smaller values are sparks</param>
    public static void Spawn(Vector3 position, Color color, float scale)
    {
        // Nudge towards the camera so the particles are never hidden behind the background
        Camera cam = Camera.main;
        if (cam != null)
        {
            position += (cam.transform.position - position).normalized;
        }

        GameObject go = new GameObject("Explosion");
        go.transform.position = position;

        ParticleSystem system = go.AddComponent<ParticleSystem>();
        system.Stop();

        ParticleSystem.MainModule main = system.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f * scale, 5f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f * scale, 0.3f * scale);
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(6, Mathf.RoundToInt(30f * scale))) });

        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f * scale;

        ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystem.ColorOverLifetimeModule fade = system.colorOverLifetime;
        fade.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        fade.color = new ParticleSystem.MinMaxGradient(gradient);

        go.GetComponent<ParticleSystemRenderer>().material = GetMaterial();
        system.Play();
    }

    private static Material GetMaterial()
    {
        if (_material == null)
        {
            _material = new Material(Shader.Find("Sprites/Default"));
        }
        return _material;
    }
}
