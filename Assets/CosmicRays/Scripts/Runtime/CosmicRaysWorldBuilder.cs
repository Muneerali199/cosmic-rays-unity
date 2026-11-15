using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Builds the runtime art for COSMIC RAYS with imported GLB props and cinematic lighting.</summary>
public static class CosmicRaysWorldBuilder
{
    private static Shader _litShader;
    private static Shader _unlitShader;
    private static Shader _particleShader;
    private static Texture2D _spaceStars;
    private static Texture2D _windowStars;
    private static Texture2D _softDot;

    public static Shader LitShader
    {
        get
        {
            if (_litShader == null)
            {
                Material bundled = Resources.Load<Material>(
                    "CosmicRays/Materials/RuntimeLitOpaque");
                _litShader = bundled != null ? bundled.shader : null;
            }
            if (_litShader == null)
            {
                _litShader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (_litShader == null)
            {
                _litShader = Shader.Find("Standard");
            }
            return _litShader;
        }
    }

    public static Shader UnlitShader
    {
        get
        {
            if (_unlitShader == null)
            {
                Material bundled = Resources.Load<Material>(
                    "CosmicRays/Materials/RuntimeUnlitOpaque");
                _unlitShader = bundled != null ? bundled.shader : null;
            }
            if (_unlitShader == null)
            {
                _unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            if (_unlitShader == null)
            {
                _unlitShader = Shader.Find("Unlit/Color");
            }
            return _unlitShader;
        }
    }

    public static Shader ParticleShader
    {
        get
        {
            if (_particleShader == null)
            {
                Material bundled = Resources.Load<Material>(
                    "CosmicRays/Materials/RuntimeParticlesAlpha");
                _particleShader = bundled != null ? bundled.shader : null;
            }
            if (_particleShader == null)
            {
                _particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            }
            if (_particleShader == null)
            {
                _particleShader = UnlitShader;
            }
            return _particleShader;
        }
    }

    public static Material CreateLit(
        string name,
        Color color,
        float metallic = 0f,
        float smoothness = 0.35f,
        Color? emission = null,
        float emissionIntensity = 0f)
    {
        Shader shader = LitShader;
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);
        material.name = name;
        SetColor(material, color);
        SetFloat(material, "_Metallic", metallic);
        SetFloat(material, "_Smoothness", Mathf.Clamp01(smoothness));
        SetFloat(material, "_Glossiness", Mathf.Clamp01(smoothness));

        if (emission.HasValue && emissionIntensity > 0f)
        {
            Color emissive = emission.Value * emissionIntensity;
            SetColorProperty(material, "_EmissionColor", emissive);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        return material;
    }

    public static Material CreateTexturedLit(
        string name,
        Texture2D texture,
        Color tint,
        float metallic = 0f,
        float smoothness = 0.25f)
    {
        Material material = CreateLit(name, tint, metallic, smoothness);
        if (material == null)
        {
            return null;
        }

        ApplyTexture(material, texture);
        return material;
    }

    public static Material CreateTexturedUnlit(string name, Color tint, Texture2D texture)
    {
        Material material = CreateUnlit(name, tint, texture);
        return material;
    }

    public static Material CreateUnlit(string name, Color color, Texture2D texture = null)
    {
        Shader shader = UnlitShader;
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);
        material.name = name;
        SetColor(material, color);

        ApplyTexture(material, texture);
        return material;
    }

    private static void ApplyTexture(Material material, Texture2D texture)
    {
        if (material == null || texture == null)
        {
            return;
        }

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }
        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
        }
        material.mainTexture = texture;
    }

    public static void ConfigureTransparent(Material material, float blendMode = 0f)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", blendMode);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat(
                "_DstBlend",
                blendMode == 2f ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_AlphaClip", 0f);
            material.SetFloat("_Cull", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        else
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;
    }

    public static Material CreateGlass(string name, Color color)
    {
        Material material = CreateLit(name, color, 0.1f, 0.95f);
        ConfigureTransparent(material, 0f);
        SetColor(material, new Color(color.r, color.g, color.b, color.a));
        return material;
    }

    public static Material CreateParticleMaterial(string name, Color color, bool additive)
    {
        Shader shader = ParticleShader;
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);
        material.name = name;
        SetColor(material, color);
        Texture2D dot = SoftDot;
        if (dot != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", dot);
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", dot);
            }
            material.mainTexture = dot;
        }
        ConfigureTransparent(material, additive ? 2f : 0f);
        return material;
    }

    public static void ApplySpaceSkybox()
    {
        Material bundled = Resources.Load<Material>(
            "CosmicRays/Materials/RuntimeSkyboxPanoramic");
        Shader shader = bundled != null ? bundled.shader : Shader.Find("Skybox/Panoramic");
        if (shader == null)
        {
            shader = Shader.Find("Skybox/Procedural");
        }

        if (shader == null)
        {
            RenderSettings.skybox = null;
            return;
        }

        var skybox = new Material(shader);
        skybox.name = "COSMIC RAYS Starfield";
        if (SpaceStars != null)
        {
            skybox.SetTexture("_MainTex", SpaceStars);
        }
        SetFloat(skybox, "_Mapping", 1f);
        SetFloat(skybox, "_ImageType", 0f);
        SetFloat(skybox, "_Exposure", 1.25f);
        SetFloat(skybox, "_Rotation", 0f);
        SetFloat(skybox, "_AtmosphereThickness", 0f);

        RenderSettings.skybox = skybox;
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.fog = false;
        DynamicGI.UpdateEnvironment();
    }

    public static void ApplyInteriorAmbient(Color ambient, Color fogColor, float fogDensity)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }

    public static Texture2D SpaceStars
    {
        get
        {
            if (_spaceStars == null)
            {
                _spaceStars = CreateStarfieldTexture(2048, 1024, 7200, true, 11);
            }
            return _spaceStars;
        }
    }

    public static Texture2D WindowStars
    {
        get
        {
            if (_windowStars == null)
            {
                _windowStars = CreateStarfieldTexture(1024, 1024, 1800, true, 29);
            }
            return _windowStars;
        }
    }

    public static Texture2D SoftDot
    {
        get
        {
            if (_softDot == null)
            {
                _softDot = CreateSoftDotTexture(64);
            }
            return _softDot;
        }
    }

    public static GameObject CreateBox(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        Color color,
        bool collider = false,
        float metallic = 0f,
        float smoothness = 0.3f)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = position;
        box.transform.localScale = size;

        if (!collider)
        {
            Collider existing = box.GetComponent<Collider>();
            if (existing != null)
            {
                Object.Destroy(existing);
            }
        }

        ApplyMaterial(box, CreateLit(name + " Material", color, metallic, smoothness));
        return box;
    }

    public static GameObject CreateEmissiveBox(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        Color color,
        float intensity = 2f)
    {
        GameObject box = CreateBox(parent, name, position, size, color, false);
        Renderer renderer = box.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = CreateLit(name + " Emissive", color, 0f, 0.5f, color, intensity);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
        return box;
    }

    public static GameObject CreateQuad(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 scale,
        Quaternion rotation,
        Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.SetParent(parent, false);
        quad.transform.localPosition = position;
        quad.transform.localRotation = rotation;
        quad.transform.localScale = scale;

        Collider collider = quad.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        Renderer renderer = quad.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
        return quad;
    }

    public static GameObject AddModel(
        Transform parent,
        string resourceName,
        Vector3 position,
        float yaw,
        float scale = 1f,
        bool sitOnFloor = true)
    {
        GameObject prefab = Resources.Load<GameObject>("CosmicRays/Models/" + resourceName);
        if (prefab == null)
        {
            Debug.LogWarning("Missing GLB resource: CosmicRays/Models/" + resourceName);
            return null;
        }

        GameObject model = Object.Instantiate(prefab, parent);
        model.name = resourceName;
        model.transform.localPosition = position;
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        model.transform.localScale = Vector3.one * scale;
        ConfigureRenderers(model);

        if (sitOnFloor)
        {
            Bounds bounds = GetBounds(model);
            Vector3 adjusted = model.transform.position;
            adjusted.y += position.y - bounds.min.y;
            model.transform.position = adjusted;
        }

        return model;
    }

    public static GameObject AddModel(
        Transform parent,
        string resourceName,
        Vector3 position,
        Quaternion rotation,
        float scale = 1f,
        bool sitOnFloor = true)
    {
        GameObject model = AddModel(parent, resourceName, position, 0f, scale, false);
        if (model == null)
        {
            return null;
        }

        model.transform.localRotation = rotation;
        if (sitOnFloor)
        {
            Bounds bounds = GetBounds(model);
            Vector3 adjusted = model.transform.position;
            adjusted.y += position.y - bounds.min.y;
            model.transform.position = adjusted;
        }
        return model;
    }

    public static Bounds GetBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int index = 1; index < renderers.Length; index++)
        {
            bounds.Encapsulate(renderers[index].bounds);
        }
        return bounds;
    }

    public static void ConfigureRenderers(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        for (int index = 0; index < renderers.Length; index++)
        {
            Renderer renderer = renderers[index];
            if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer)
            {
                continue;
            }

            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Simple;
        }
    }

    public static int AddApproximateColliders(GameObject root, float minimumVolume = 0.35f, int maximum = 120)
    {
        if (root == null)
        {
            return 0;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        int added = 0;
        for (int index = 0; index < renderers.Length && added < maximum; index++)
        {
            Renderer renderer = renderers[index];
            if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer)
            {
                continue;
            }

            string lowerName = renderer.gameObject.name.ToLowerInvariant();
            if (lowerName.Contains("floor") || lowerName.Contains("ceiling") ||
                lowerName.Contains("glass") || lowerName.Contains("light") ||
                lowerName.Contains("screen") || lowerName.Contains("sign"))
            {
                continue;
            }

            Bounds bounds = renderer.bounds;
            Vector3 size = bounds.size;
            if (size.y < 0.35f || size.x * size.y * size.z < minimumVolume)
            {
                continue;
            }

            var collisionObject = new GameObject(root.name + " Collision");
            collisionObject.transform.SetParent(root.transform, true);
            collisionObject.transform.position = bounds.center;
            collisionObject.transform.rotation = Quaternion.identity;
            BoxCollider box = collisionObject.AddComponent<BoxCollider>();
            box.size = size;
            added++;
        }
        return added;
    }

    public static BoxCollider AddInvisibleCollider(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size)
    {
        var colliderObject = new GameObject(name);
        colliderObject.transform.SetParent(parent, false);
        colliderObject.transform.localPosition = position;
        BoxCollider box = colliderObject.AddComponent<BoxCollider>();
        box.size = size;
        return box;
    }

    public static Light AddPointLight(
        Transform parent,
        string name,
        Vector3 position,
        Color color,
        float intensity,
        float range,
        bool shadows = false)
    {
        var lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        light.shadowStrength = 0.7f;
        light.shadowBias = 0.04f;
        light.shadowNormalBias = 0.35f;
        light.bounceIntensity = 0.45f;
        return light;
    }

    public static Light AddSpotLight(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Color color,
        float intensity,
        float range,
        float angle,
        bool shadows = false)
    {
        var lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = position;
        lightObject.transform.localRotation = rotation;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = angle;
        light.innerSpotAngle = angle * 0.45f;
        light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        light.shadowStrength = 0.65f;
        light.shadowBias = 0.04f;
        light.shadowNormalBias = 0.35f;
        light.bounceIntensity = 0.35f;
        return light;
    }

    public static Light AddDirectionalLight(
        Transform parent,
        string name,
        Vector3 eulerAngles,
        Color color,
        float intensity,
        bool shadows = false)
    {
        var lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localRotation = Quaternion.Euler(eulerAngles);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = color;
        light.intensity = intensity;
        light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        light.shadowStrength = 0.55f;
        light.shadowBias = 0.05f;
        light.shadowNormalBias = 0.4f;
        light.bounceIntensity = 0.3f;
        RenderSettings.sun = light;
        return light;
    }

    public static GameObject AddVolumetricCone(
        Transform parent,
        string name,
        Vector3 topPosition,
        float radius,
        float height,
        Color color,
        float alpha)
    {
        Mesh mesh = CreateConeMesh(24);
        var coneObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        coneObject.transform.SetParent(parent, false);
        coneObject.transform.localPosition = topPosition;
        coneObject.transform.localScale = new Vector3(radius, height, radius);

        MeshFilter filter = coneObject.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = coneObject.GetComponent<MeshRenderer>();
        Material material = CreateParticleMaterial(name + " Volume", new Color(color.r, color.g, color.b, alpha), true);
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return coneObject;
    }

    public static ParticleSystem AddDust(
        Transform parent,
        string name,
        Vector3 center,
        Vector3 size,
        Color color,
        float rate = 18f)
    {
        var dustObject = new GameObject(name);
        dustObject.transform.SetParent(parent, false);
        dustObject.transform.localPosition = center;

        ParticleSystem particles = dustObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.05f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.04f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = color;
        main.maxParticles = 260;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = rate;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = size;

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.025f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.06f;
        noise.frequency = 0.16f;
        noise.scrollSpeed = 0.02f;
        noise.quality = ParticleSystemNoiseQuality.Low;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.5f, 0.18f),
                new GradientAlphaKey(0.5f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystemRenderer renderer = dustObject.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = CreateParticleMaterial(name + " Material", color, true);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        particles.Play();
        return particles;
    }

    public static ParticleSystem AddGeyser(
        Transform parent,
        string name,
        Vector3 position,
        Color color)
    {
        var geyserObject = new GameObject(name);
        geyserObject.transform.SetParent(parent, false);
        geyserObject.transform.localPosition = position;
        geyserObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem particles = geyserObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.55f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.055f);
        main.startColor = color;
        main.maxParticles = 500;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.02f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 72f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 13f;
        shape.radius = 0.03f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.4f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystemRenderer renderer = geyserObject.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = CreateParticleMaterial(name + " Material", color, false);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        particles.Play();
        return particles;
    }

    public static GameObject AddRingBand(
        Transform parent,
        string name,
        float innerRadius,
        float outerRadius,
        Color color)
    {
        var bandObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        bandObject.transform.SetParent(parent, false);
        MeshFilter filter = bandObject.GetComponent<MeshFilter>();
        filter.sharedMesh = CreateRingMesh(innerRadius, outerRadius, 128);

        MeshRenderer renderer = bandObject.GetComponent<MeshRenderer>();
        Material material = CreateUnlit(name + " Material", color);
        if (color.a < 0.999f)
        {
            ConfigureTransparent(material, 0f);
        }
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return bandObject;
    }

    public static GameObject AddSign(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        string text,
        float width,
        float height,
        Color textColor,
        Color backingColor)
    {
        var signObject = new GameObject(name);
        signObject.transform.SetParent(parent, false);
        signObject.transform.localPosition = position;
        signObject.transform.localRotation = rotation;

        GameObject backing = CreateBox(
            signObject.transform,
            "Backing",
            new Vector3(0f, 0f, 0.035f),
            new Vector3(width, height, 0.06f),
            backingColor,
            false,
            0.1f,
            0.5f);
        Renderer backingRenderer = backing.GetComponent<Renderer>();
        if (backingRenderer != null)
        {
            backingRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        GameObject underline = CreateEmissiveBox(
            signObject.transform,
            "Accent",
            new Vector3(0f, -height * 0.43f, -0.002f),
            new Vector3(width * 0.72f, 0.012f, 0.008f),
            textColor,
            1.6f);
        underline.transform.localRotation = Quaternion.identity;

        var textObject = new GameObject("Text", typeof(TextMesh));
        textObject.transform.SetParent(signObject.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0f, -0.005f);
        TextMesh textMesh = textObject.GetComponent<TextMesh>();
        Font font = Resources.Load<Font>("CosmicRays/Fonts/Rajdhani-SemiBold");
        if (font != null)
        {
            textMesh.font = font;
        }
        textMesh.text = text;
        textMesh.fontSize = 72;
        textMesh.characterSize = 0.32f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = textColor;
        textMesh.richText = false;

        MeshRenderer textRenderer = textObject.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            if (font != null)
            {
                textRenderer.sharedMaterial = font.material;
            }
            textRenderer.shadowCastingMode = ShadowCastingMode.Off;
            textRenderer.receiveShadows = false;
        }
        return signObject;
    }

    public static GameObject AddHologram(Transform parent, string name, Vector3 position, Color color)
    {
        var hologramRoot = new GameObject(name);
        hologramRoot.transform.SetParent(parent, false);
        hologramRoot.transform.localPosition = position;

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "Projection Beam";
        beam.transform.SetParent(hologramRoot.transform, false);
        beam.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        beam.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
        Object.Destroy(beam.GetComponent<Collider>());
        Material beamMaterial = CreateParticleMaterial("Hologram Beam", new Color(color.r, color.g, color.b, 0.09f), true);
        Renderer beamRenderer = beam.GetComponent<Renderer>();
        beamRenderer.sharedMaterial = beamMaterial;
        beamRenderer.shadowCastingMode = ShadowCastingMode.Off;

        GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planet.name = "Hologram Saturn";
        planet.transform.SetParent(hologramRoot.transform, false);
        planet.transform.localPosition = new Vector3(0f, 0.92f, 0f);
        planet.transform.localScale = Vector3.one * 0.34f;
        Object.Destroy(planet.GetComponent<Collider>());
        Material planetMaterial = CreateUnlit(
            "Hologram Planet",
            new Color(color.r, color.g, color.b, 0.9f),
            Resources.Load<Texture2D>("CosmicRays/Textures/Saturn"));
        if (planetMaterial != null)
        {
            ConfigureTransparent(planetMaterial, 0f);
        }
        Renderer planetRenderer = planet.GetComponent<Renderer>();
        planetRenderer.sharedMaterial = planetMaterial;
        planetRenderer.shadowCastingMode = ShadowCastingMode.Off;

        GameObject rings = new GameObject("Hologram Rings");
        rings.transform.SetParent(hologramRoot.transform, false);
        rings.transform.localPosition = planet.transform.localPosition;
        rings.transform.localRotation = Quaternion.Euler(19f, 0f, 0f);
        AddRingBand(rings.transform, "Hologram Ring A", 0.46f, 0.54f, new Color(color.r, color.g, color.b, 0.75f));
        AddRingBand(rings.transform, "Hologram Ring B", 0.59f, 0.64f, new Color(color.r, color.g, color.b, 0.45f));

        CosmicRaysSpin spin = planet.AddComponent<CosmicRaysSpin>();
        spin.degreesPerSecond = 14f;
        CosmicRaysSpin ringSpin = rings.AddComponent<CosmicRaysSpin>();
        ringSpin.degreesPerSecond = 8f;

        Light glow = AddPointLight(
            hologramRoot.transform,
            "Hologram Glow",
            new Vector3(0f, 0.9f, 0f),
            color,
            1.3f,
            3.4f);
        CosmicRaysPulse pulse = glow.gameObject.AddComponent<CosmicRaysPulse>();
        pulse.baseIntensity = 1.3f;
        pulse.amplitude = 0.25f;
        pulse.speed = 1.7f;
        return hologramRoot;
    }

    private static void ApplyMaterial(GameObject target, Material material)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null || material == null)
        {
            return;
        }

        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }

    private static void SetColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        material.color = color;
    }

    private static void SetColorProperty(Material material, string property, Color color)
    {
        if (material.HasProperty(property))
        {
            material.SetColor(property, color);
        }
    }

    private static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }

    private static Mesh CreateRingMesh(float innerRadius, float outerRadius, int segments)
    {
        var vertices = new Vector3[(segments + 1) * 2];
        var triangles = new int[segments * 6];
        var normals = new Vector3[vertices.Length];

        for (int index = 0; index <= segments; index++)
        {
            float angle = Mathf.PI * 2f * index / segments;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            vertices[index * 2] = direction * innerRadius;
            vertices[index * 2 + 1] = direction * outerRadius;
            normals[index * 2] = Vector3.up;
            normals[index * 2 + 1] = Vector3.up;
        }

        int triangleIndex = 0;
        for (int index = 0; index < segments; index++)
        {
            int inner = index * 2;
            int outer = inner + 1;
            int nextInner = (index + 1) * 2;
            int nextOuter = nextInner + 1;

            triangles[triangleIndex++] = inner;
            triangles[triangleIndex++] = nextInner;
            triangles[triangleIndex++] = nextOuter;
            triangles[triangleIndex++] = inner;
            triangles[triangleIndex++] = nextOuter;
            triangles[triangleIndex++] = outer;
        }

        var mesh = new Mesh();
        mesh.name = "COSMIC RAYS Ring";
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateConeMesh(int segments)
    {
        int vertexCount = 1 + (segments + 1) * 2;
        var vertices = new Vector3[vertexCount];
        var colors = new Color[vertexCount];
        var triangles = new int[segments * 6];

        vertices[0] = Vector3.zero;
        colors[0] = new Color(1f, 1f, 1f, 1f);

        for (int index = 0; index <= segments; index++)
        {
            float angle = Mathf.PI * 2f * index / segments;
            Vector3 edge = new Vector3(Mathf.Cos(angle), -1f, Mathf.Sin(angle));
            int first = 1 + index * 2;
            int second = first + 1;
            vertices[first] = edge;
            vertices[second] = edge;
            colors[first] = new Color(1f, 1f, 1f, 0f);
            colors[second] = new Color(1f, 1f, 1f, 0f);
        }

        int triangleIndex = 0;
        for (int index = 0; index < segments; index++)
        {
            int first = 1 + index * 2;
            int next = 1 + (index + 1) * 2;
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = first;
            triangles[triangleIndex++] = next;
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = next;
            triangles[triangleIndex++] = first;
        }

        var mesh = new Mesh();
        mesh.name = "COSMIC RAYS Volumetric Cone";
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Texture2D CreateStarfieldTexture(
        int width,
        int height,
        int starCount,
        bool nebula,
        int seed)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "COSMIC RAYS Starfield " + width + "x" + height;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            float v = y / (float)(height - 1);
            float band = Mathf.Exp(-Mathf.Pow((v - 0.52f) / 0.14f, 2f));

            for (int x = 0; x < width; x++)
            {
                byte r = 2;
                byte g = 4;
                byte b = 10;

                if (nebula && band > 0.01f)
                {
                    float noise =
                        Mathf.PerlinNoise(x * 0.006f + 2.7f, y * 0.006f + 8.1f) * 0.65f +
                        Mathf.PerlinNoise(x * 0.013f + 12.4f, y * 0.013f + 1.9f) * 0.35f;
                    float strength = band * Mathf.Clamp01(noise * 1.3f);
                    r = (byte)Mathf.Clamp(r + strength * 20f, 0f, 255f);
                    g = (byte)Mathf.Clamp(g + strength * 12f, 0f, 255f);
                    b = (byte)Mathf.Clamp(b + strength * 42f, 0f, 255f);
                }

                pixels[y * width + x] = new Color32(r, g, b, 255);
            }
        }

        var random = new System.Random(seed);
        for (int index = 0; index < starCount; index++)
        {
            int x = random.Next(0, width);
            int y = random.Next(0, height);
            float brightness = Mathf.Lerp(0.25f, 1f, (float)random.NextDouble());
            float warmth = (float)random.NextDouble();
            byte r = (byte)Mathf.Clamp(255f * brightness, 0f, 255f);
            byte g = (byte)Mathf.Clamp(255f * brightness * Mathf.Lerp(0.86f, 1f, warmth), 0f, 255f);
            byte b = (byte)Mathf.Clamp(255f * brightness * Mathf.Lerp(1f, 0.82f, warmth), 0f, 255f);
            int pixelIndex = y * width + x;
            pixels[pixelIndex] = new Color32(r, g, b, 255);

            if (brightness > 0.82f)
            {
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        if (offsetX == 0 && offsetY == 0)
                        {
                            continue;
                        }
                        int px = (x + offsetX + width) % width;
                        int py = Mathf.Clamp(y + offsetY, 0, height - 1);
                        int glowIndex = py * width + px;
                        Color32 existing = pixels[glowIndex];
                        pixels[glowIndex] = new Color32(
                            (byte)Mathf.Max(existing.r, (byte)(r * 0.35f)),
                            (byte)Mathf.Max(existing.g, (byte)(g * 0.35f)),
                            (byte)Mathf.Max(existing.b, (byte)(b * 0.35f)),
                            255);
                    }
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D CreateSoftDotTexture(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "COSMIC RAYS Soft Dot";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color32[size * size];
        float center = (size - 1) * 0.5f;
        float radius = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.2f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
