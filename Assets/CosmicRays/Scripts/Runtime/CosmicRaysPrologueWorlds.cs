using UnityEngine;
using UnityEngine.UI;

public static class CosmicRaysPrologueWorlds
{
    public sealed class ExplorationAnchors
    {
        public Transform Door;
        public Transform Terminal;
        public Transform Note;
        public Vector3 SpawnPosition;
        public float SpawnYaw;
    }

    private static readonly Color Cyan = new Color(0.42f, 0.9f, 0.92f, 1f);
    private static readonly Color Amber = new Color(1f, 0.65f, 0.2f, 1f);
    private static readonly Color Magenta = new Color(0.92f, 0.3f, 0.63f, 1f);
    private static readonly Color WarmWhite = new Color(1f, 0.82f, 0.62f, 1f);
    private static readonly Color Steel = new Color(0.16f, 0.21f, 0.24f, 1f);
    private static readonly Color DeepBlue = new Color(0.025f, 0.055f, 0.08f, 1f);

    public static void BuildTitle(Transform root)
    {
        CosmicRaysWorldBuilder.ApplySpaceSkybox();
        RenderSettings.fog = false;

        CosmicRaysWorldBuilder.AddDirectionalLight(
            root,
            "Solar Rim Light",
            new Vector3(24f, -35f, 0f),
            new Color(0.75f, 0.84f, 1f),
            1.25f);

        GameObject sun = Sphere(
            root,
            "Sun",
            new Vector3(-8.5f, 1.4f, -7f),
            3.4f,
            CosmicRaysWorldBuilder.CreateLit(
                "Solar Surface",
                new Color(1f, 0.45f, 0.12f),
                0f,
                0.25f,
                new Color(1f, 0.38f, 0.08f),
                7f));
        CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Solar Flare",
            sun.transform.position,
            new Color(1f, 0.46f, 0.16f),
            4.6f,
            22f);

        GameObject earth = TexturedSphere(
            root,
            "Earth",
            new Vector3(4.8f, 1.2f, -2.4f),
            3.1f,
            "EarthDay",
            Color.white);
        EarthAtmosphere(root, earth.transform.position, 3.35f);
        CosmicRaysSpin earthSpin = earth.AddComponent<CosmicRaysSpin>();
        earthSpin.degreesPerSecond = 2.2f;
        earthSpin.axis = new Vector3(0.12f, 1f, 0.04f);
        AddMoon(root, earth.transform.position + new Vector3(-2.1f, 0.6f, 0.7f));

        GameObject saturn = new GameObject("Saturn");
        saturn.transform.SetParent(root, false);
        saturn.transform.localPosition = new Vector3(-4.6f, 3.7f, 2.4f);
        GameObject saturnBall = TexturedSphere(
            saturn.transform,
            "Saturn Sphere",
            Vector3.zero,
            2.2f,
            "Saturn",
            new Color(0.96f, 0.88f, 0.72f, 1f));
        saturnBall.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);

        var rings = new GameObject("Saturn Rings");
        rings.transform.SetParent(saturn.transform, false);
        rings.transform.localRotation = Quaternion.Euler(19f, -12f, 0f);
        CosmicRaysWorldBuilder.AddRingBand(
            rings.transform,
            "Saturn Ring Inner",
            1.25f,
            1.72f,
            new Color(0.86f, 0.72f, 0.5f, 0.46f));
        CosmicRaysWorldBuilder.AddRingBand(
            rings.transform,
            "Saturn Ring Outer",
            1.84f,
            2.18f,
            new Color(0.67f, 0.58f, 0.46f, 0.28f));
        CosmicRaysSpin saturnSpin = saturnBall.AddComponent<CosmicRaysSpin>();
        saturnSpin.degreesPerSecond = 1.2f;

        CosmicRaysWorldBuilder.AddDust(
            root,
            "Solar Dust",
            new Vector3(0f, 1.6f, -4f),
            new Vector3(18f, 9f, 12f),
            new Color(0.45f, 0.75f, 1f, 0.18f),
            8f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.7f;
    }

    public static void BuildDinner(Transform root)
    {
        CosmicRaysWorldBuilder.ApplyInteriorAmbient(
            new Color(0.16f, 0.105f, 0.075f),
            new Color(0.08f, 0.055f, 0.045f),
            0.026f);

        BuildRoom(
            root,
            new Vector2(8.4f, 6.2f),
            3.15f,
            new Color(0.2f, 0.13f, 0.09f),
            new Color(0.12f, 0.085f, 0.075f),
            new Color(0.07f, 0.055f, 0.05f));

        CosmicRaysWorldBuilder.AddModel(
            root,
            "LivingRug",
            new Vector3(0f, 0.012f, 0f),
            0f,
            2.7f,
            false);
        GameObject table = CosmicRaysWorldBuilder.AddModel(
            root,
            "LaidTable",
            new Vector3(0f, 0f, 0f),
            0f,
            1.28f);
        if (table != null)
        {
            table.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }

        CosmicRaysWorldBuilder.AddModel(
            root,
            "DiningChair",
            new Vector3(0f, 0f, 1.22f),
            180f,
            1.08f);
        CosmicRaysWorldBuilder.AddModel(
            root,
            "DiningChair",
            new Vector3(0f, 0f, -1.25f),
            0f,
            1.08f);
        CosmicRaysWorldBuilder.AddModel(
            root,
            "DiningChair",
            new Vector3(-1.38f, 0f, 0f),
            90f,
            1.08f);
        CosmicRaysWorldBuilder.AddModel(
            root,
            "DiningChair",
            new Vector3(1.38f, 0f, 0f),
            -90f,
            1.08f);

        GameObject bookcase = CosmicRaysWorldBuilder.AddModel(
            root,
            "Bookcase",
            new Vector3(-3.72f, 0f, 1.1f),
            90f,
            1.4f);
        if (bookcase != null)
        {
            bookcase.transform.localPosition = new Vector3(-3.55f, 0f, 1.1f);
        }
        CosmicRaysWorldBuilder.AddModel(
            root,
            "PlantLarge",
            new Vector3(3.45f, 0f, 2.2f),
            0f,
            1.25f);

        GameObject wallArt = CosmicRaysWorldBuilder.AddModel(
            root,
            "WallArt",
            new Vector3(1.75f, 1.72f, 3.03f),
            180f,
            1.35f,
            false);
        if (wallArt != null)
        {
            wallArt.transform.localPosition = new Vector3(1.75f, 1.72f, 3.03f);
        }

        AddPendantLight(root, new Vector3(0f, 2.86f, 0f), WarmWhite, 4.2f);
        CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Candle Glow",
            new Vector3(0.18f, 1.18f, 0.08f),
            new Color(1f, 0.48f, 0.18f),
            2.6f,
            4.2f);
        CosmicRaysWorldBuilder.AddSpotLight(
            root,
            "Warm Window Wash",
            new Vector3(-3.3f, 2.65f, -1.8f),
            Quaternion.Euler(54f, 38f, 0f),
            new Color(1f, 0.7f, 0.46f),
            2.3f,
            8f,
            52f);
        AddWallStrips(root, 8.2f, 6.0f, 3.1f, new Color(1f, 0.58f, 0.28f, 0.55f));
        AddDustMotes(root, new Vector3(0f, 1.4f, 0f), new Vector3(7f, 2.5f, 5f), new Color(1f, 0.78f, 0.5f, 0.15f));
    }

    public static ExplorationAnchors BuildBedroom(Transform root)
    {
        CosmicRaysWorldBuilder.ApplyInteriorAmbient(
            new Color(0.075f, 0.105f, 0.135f),
            new Color(0.025f, 0.05f, 0.075f),
            0.035f);

        const float width = 7.4f;
        const float depth = 5.5f;
        const float height = 3.05f;
        BuildRoom(
            root,
            new Vector2(width, depth),
            height,
            new Color(0.16f, 0.13f, 0.11f),
            new Color(0.075f, 0.11f, 0.14f),
            new Color(0.045f, 0.065f, 0.09f));

        CosmicRaysWorldBuilder.AddModel(
            root,
            "BedroomRug",
            new Vector3(-0.1f, 0.021f, -0.5f),
            0f,
            2.15f,
            false);

        GameObject bed = CosmicRaysWorldBuilder.AddModel(
            root,
            "DoubleBed",
            new Vector3(-2.28f, 0f, 0.45f),
            90f,
            1.08f);
        if (bed != null)
        {
            CosmicRaysWorldBuilder.AddInvisibleCollider(
                root,
                "Bed Collision",
                new Vector3(-2.28f, 0.55f, 0.45f),
                new Vector3(2.25f, 1.1f, 1.7f));
        }

        CosmicRaysWorldBuilder.AddModel(
            root,
            "BedsideTable",
            new Vector3(-3.15f, 0f, 1.72f),
            90f,
            1f);
        GameObject lamp = CosmicRaysWorldBuilder.AddModel(
            root,
            "TableLamp",
            new Vector3(-3.05f, 0.51f, 1.72f),
            0f,
            1.15f,
            false);
        if (lamp != null)
        {
            lamp.transform.localPosition = new Vector3(-3.05f, 0.51f, 1.72f);
        }
        CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Bedside Lamp Glow",
            new Vector3(-3.02f, 0.92f, 1.72f),
            new Color(1f, 0.62f, 0.32f),
            2.8f,
            4.8f);

        CosmicRaysWorldBuilder.AddModel(
            root,
            "BedroomDesk",
            new Vector3(1.72f, 0f, 2.3f),
            180f,
            1.12f);
        CosmicRaysWorldBuilder.AddInvisibleCollider(
            root,
            "Desk Collision",
            new Vector3(1.72f, 0.38f, 2.3f),
            new Vector3(1.25f, 0.76f, 0.64f));
        GameObject monitor = CosmicRaysWorldBuilder.AddModel(
            root,
            "ComputerMonitor",
            new Vector3(1.72f, 0.82f, 2.18f),
            180f,
            1.05f,
            false);
        if (monitor != null)
        {
            monitor.transform.localPosition = new Vector3(1.72f, 0.82f, 2.18f);
        }
        CosmicRaysWorldBuilder.AddModel(
            root,
            "DeskChair",
            new Vector3(1.72f, 0f, 1.48f),
            0f,
            1.02f);

        GameObject bookcase = CosmicRaysWorldBuilder.AddModel(
            root,
            "Bookcase",
            new Vector3(3.42f, 0f, -0.8f),
            -90f,
            1f);
        if (bookcase != null)
        {
            CosmicRaysWorldBuilder.AddInvisibleCollider(
                root,
                "Bookcase Collision",
                new Vector3(3.42f, 0.86f, -0.8f),
                new Vector3(0.38f, 1.72f, 1.04f));
        }

        CosmicRaysWorldBuilder.AddModel(
            root,
            "PlantLarge",
            new Vector3(-3.12f, 0f, -2.1f),
            18f,
            1.2f);
        CosmicRaysWorldBuilder.AddModel(
            root,
            "ConsoleShelf",
            new Vector3(3.35f, 0f, -2.16f),
            -90f,
            1.1f);

        GameObject art = CosmicRaysWorldBuilder.AddModel(
            root,
            "WallArt",
            new Vector3(0.55f, 1.78f, 2.7f),
            180f,
            1.25f,
            false);
        if (art != null)
        {
            art.transform.localPosition = new Vector3(0.55f, 1.78f, 2.7f);
        }

        GameObject lights = CosmicRaysWorldBuilder.AddModel(
            root,
            "StringLights",
            new Vector3(-0.25f, 1.95f, 2.65f),
            180f,
            1.5f,
            false);
        if (lights != null)
        {
            lights.transform.localPosition = new Vector3(-0.25f, 1.95f, 2.65f);
        }
        CosmicRaysWorldBuilder.AddPointLight(
            root,
            "String Light Glow",
            new Vector3(-0.25f, 2.2f, 2.38f),
            new Color(1f, 0.56f, 0.28f),
            1.6f,
            3.7f);

        BuildWindow(root, new Vector3(-3.52f, 1.55f, -0.85f));
        CosmicRaysWorldBuilder.AddSpotLight(
            root,
            "Window Moonlight",
            new Vector3(-2.8f, 2.45f, -0.85f),
            Quaternion.Euler(48f, 90f, 0f),
            new Color(0.45f, 0.68f, 1f),
            2.7f,
            9f,
            58f);

        GameObject floorLamp = CosmicRaysWorldBuilder.AddModel(
            root,
            "FloorLamp",
            new Vector3(2.92f, 0f, 1.72f),
            0f,
            1.05f);
        if (floorLamp != null)
        {
            CosmicRaysWorldBuilder.AddPointLight(
                root,
                "Floor Lamp Glow",
                new Vector3(2.92f, 1.55f, 1.72f),
                new Color(1f, 0.72f, 0.45f),
                2.2f,
                4.5f);
        }

        AddWallStrips(root, width - 0.15f, depth - 0.15f, height - 0.18f, new Color(0.26f, 0.62f, 0.78f, 0.34f));
        AddDustMotes(root, new Vector3(0f, 1.5f, 0f), new Vector3(6.5f, 2.6f, 4.8f), new Color(0.5f, 0.75f, 1f, 0.1f));

        Transform door = BuildSealedDoor(root, new Vector3(3.47f, 0f, 1.05f));
        CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Lab Door Spill",
            new Vector3(3.2f, 1.15f, 1.05f),
            new Color(0.4f, 0.82f, 0.95f),
            1.8f,
            3.2f);

        return new ExplorationAnchors
        {
            Door = door,
            SpawnPosition = new Vector3(0.15f, 0.08f, -1.85f),
            SpawnYaw = 18f
        };
    }

    public static ExplorationAnchors BuildLab(Transform root)
    {
        CosmicRaysWorldBuilder.ApplyInteriorAmbient(
            new Color(0.045f, 0.075f, 0.095f),
            new Color(0.012f, 0.04f, 0.055f),
            0.028f);

        GameObject lab = CosmicRaysWorldBuilder.AddModel(
            root,
            "ScienceLab",
            new Vector3(-1f, 0f, 0f),
            0f,
            1f);
        if (lab != null)
        {
            Renderer[] labRenderers = lab.GetComponentsInChildren<Renderer>();
            for (int index = 0; index < labRenderers.Length; index++)
            {
                Renderer renderer = labRenderers[index];
                Vector3 size = renderer.bounds.size;
                bool large = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) > 1.1f;
                renderer.shadowCastingMode = large
                    ? UnityEngine.Rendering.ShadowCastingMode.On
                    : UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }
        }

        CosmicRaysWorldBuilder.AddInvisibleCollider(
            root,
            "Lab Floor Collision",
            new Vector3(0f, 0.025f, 0f),
            new Vector3(18f, 0.18f, 12f));
        AddInvisibleBoundary(root, "Lab West Boundary", new Vector3(-8.85f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 12f));
        AddInvisibleBoundary(root, "Lab East Boundary", new Vector3(8.85f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 12f));
        AddInvisibleBoundary(root, "Lab North Boundary", new Vector3(0f, 1.6f, 5.88f), new Vector3(18f, 3.2f, 0.2f));
        AddInvisibleBoundary(root, "Lab South Boundary", new Vector3(0f, 1.6f, -5.88f), new Vector3(18f, 3.2f, 0.2f));

        GameObject reactor = CosmicRaysWorldBuilder.AddModel(
            root,
            "ReactorCore",
            new Vector3(0.55f, 0.11f, -0.15f),
            0f,
            0.74f);
        if (reactor != null)
        {
            var reactorCollider = new GameObject("Containment Core Collision");
            reactorCollider.transform.SetParent(root, false);
            reactorCollider.transform.localPosition = new Vector3(0.55f, 1.38f, -0.15f);
            CapsuleCollider capsule = reactorCollider.AddComponent<CapsuleCollider>();
            capsule.radius = 0.92f;
            capsule.height = 2.55f;
            capsule.center = Vector3.zero;
        }

        Light coreLight = CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Containment Core Glow",
            new Vector3(0.55f, 1.4f, -0.15f),
            new Color(0.95f, 0.24f, 0.56f),
            5.2f,
            6.2f,
            true);
        CosmicRaysPulse corePulse = coreLight.gameObject.AddComponent<CosmicRaysPulse>();
        corePulse.baseIntensity = 5.2f;
        corePulse.amplitude = 1.15f;
        corePulse.speed = 1.55f;
        corePulse.phase = 0.4f;

        GameObject lowerRing = CosmicRaysWorldBuilder.AddRingBand(
            root,
            "Containment Ring Lower",
            0.98f,
            1.08f,
            new Color(0.35f, 0.92f, 0.95f, 0.32f));
        lowerRing.transform.localPosition = new Vector3(0.55f, 0.17f, -0.15f);
        GameObject upperRing = CosmicRaysWorldBuilder.AddRingBand(
            root,
            "Containment Ring Upper",
            0.51f,
            0.58f,
            new Color(0.95f, 0.32f, 0.65f, 0.52f));
        upperRing.transform.localPosition = new Vector3(0.55f, 2.43f, -0.15f);
        CosmicRaysSpin upperSpin = upperRing.AddComponent<CosmicRaysSpin>();
        upperSpin.degreesPerSecond = 9f;

        CosmicRaysWorldBuilder.AddVolumetricCone(
            root,
            "Containment Haze",
            new Vector3(0.55f, 2.65f, -0.15f),
            1.28f,
            2.5f,
            new Color(0.95f, 0.3f, 0.64f),
            0.12f);

        GameObject holo = CosmicRaysWorldBuilder.AddModel(
            root,
            "HoloTable",
            new Vector3(3.05f, 0.11f, 1.15f),
            0f,
            1f);
        CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Terminal Hologram Glow",
            new Vector3(3.05f, 1.62f, 1.15f),
            new Color(0.3f, 0.8f, 1f),
            3.4f,
            4.6f);
        CosmicRaysWorldBuilder.AddModel(
            root,
            "ServerRack",
            new Vector3(7.72f, 0.11f, -4.6f),
            180f,
            1.05f);
        CosmicRaysWorldBuilder.AddModel(
            root,
            "ConsoleShelf",
            new Vector3(-7.3f, 0.11f, 4.55f),
            90f,
            1.25f);

        AddLabTerminalPanel(root, new Vector3(3.05f, 1.67f, 1.15f));
        AddWorldLabel(
            root,
            "SPA-077 Wall Sign",
            new Vector3(0.55f, 2.12f, -5.68f),
            Quaternion.Euler(0f, 0f, 0f),
            "SPA-077",
            "SOLAR PARTICLE ABSORBER",
            1.95f,
            new Color(0.29f, 0.88f, 0.94f, 1f),
            new Color(0.015f, 0.055f, 0.07f, 0.88f),
            1.45f);
        AddWorldLabel(
            root,
            "Containment Warning",
            new Vector3(-1.9f, 2.05f, 0.12f),
            Quaternion.Euler(0f, 90f, 0f),
            "CONTAINMENT BAY",
            "MAGNETIC FIELD ACTIVE",
            1.5f,
            new Color(1f, 0.38f, 0.28f, 1f),
            new Color(0.07f, 0.025f, 0.02f, 0.9f),
            0.82f);
        AddWorldLabel(
            root,
            "Terminal Sign",
            new Vector3(3.05f, 2.25f, 1.15f),
            Quaternion.Euler(0f, 180f, 0f),
            "SPA-077",
            "MAGNETIC CALIBRATION",
            1.35f,
            new Color(0.35f, 0.9f, 1f, 1f),
            new Color(0.01f, 0.04f, 0.06f, 0.8f),
            0.68f);

        AddPendantLight(root, new Vector3(3.05f, 2.7f, 1.15f), new Color(0.38f, 0.8f, 1f), 2.5f);
        CosmicRaysWorldBuilder.AddSpotLight(
            root,
            "Reactor Overhead",
            new Vector3(0.55f, 2.68f, -0.15f),
            Quaternion.Euler(90f, 0f, 0f),
            new Color(0.52f, 0.76f, 1f),
            3.2f,
            6.5f,
            62f);
        CosmicRaysWorldBuilder.AddSpotLight(
            root,
            "Entry Pool",
            new Vector3(-1.2f, 2.62f, -2.2f),
            Quaternion.Euler(62f, 22f, 0f),
            new Color(0.42f, 0.7f, 0.9f),
            2.2f,
            7f,
            55f);
        CosmicRaysWorldBuilder.AddSpotLight(
            root,
            "Desk Pool",
            new Vector3(5.0f, 2.55f, 3.65f),
            Quaternion.Euler(56f, 4f, 0f),
            new Color(1f, 0.7f, 0.42f),
            2.6f,
            5.5f,
            48f);

        GameObject note = CosmicRaysWorldBuilder.CreateBox(
            root,
            "Alice's Note",
            new Vector3(5.0f, 0.22f, 3.72f),
            new Vector3(0.32f, 0.03f, 0.23f),
            new Color(0.98f, 0.76f, 0.34f),
            false,
            0f,
            0.7f);
        note.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
        Renderer noteRenderer = note.GetComponent<Renderer>();
        if (noteRenderer != null)
        {
            noteRenderer.sharedMaterial = CosmicRaysWorldBuilder.CreateLit(
                "Note Paper",
                new Color(1f, 0.79f, 0.36f),
                0f,
                0.8f,
                new Color(1f, 0.55f, 0.12f),
                1.15f);
            noteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Light noteGlow = CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Note Glow",
            new Vector3(5.0f, 0.44f, 3.72f),
            new Color(1f, 0.6f, 0.18f),
            0.8f,
            1.8f);
        CosmicRaysPulse notePulse = noteGlow.gameObject.AddComponent<CosmicRaysPulse>();
        notePulse.baseIntensity = 0.8f;
        notePulse.amplitude = 0.28f;
        notePulse.speed = 2.2f;
        note.SetActive(false);

        AddLabDust(root);
        AddWallStrips(root, 17.6f, 11.6f, 2.65f, new Color(0.34f, 0.76f, 0.95f, 0.25f));

        return new ExplorationAnchors
        {
            Terminal = holo != null ? holo.transform : null,
            Note = note.transform,
            SpawnPosition = new Vector3(-1.35f, 0.2f, -2.35f),
            SpawnYaw = 42f
        };
    }

    public static void BuildEnding(Transform root)
    {
        CosmicRaysWorldBuilder.ApplySpaceSkybox();
        RenderSettings.fog = false;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.65f;

        CosmicRaysWorldBuilder.AddDirectionalLight(
            root,
            "Saturn Backlight",
            new Vector3(28f, 188f, 0f),
            new Color(0.65f, 0.76f, 1f),
            0.9f);

        GameObject enceladus = TexturedSphere(
            root,
            "Enceladus",
            new Vector3(0f, 1.8f, 0f),
            5.2f,
            "EnceladusBase",
            new Color(0.8f, 0.92f, 1f, 1f));
        CosmicRaysSpin spin = enceladus.AddComponent<CosmicRaysSpin>();
        spin.degreesPerSecond = 1.1f;
        spin.axis = new Vector3(0.14f, 1f, 0.02f);

        GameObject atmosphere = Sphere(
            root,
            "Enceladus Atmosphere",
            enceladus.transform.position,
            5.46f,
            CosmicRaysWorldBuilder.CreateUnlit(
                "Enceladus Halo",
                new Color(0.38f, 0.74f, 1f, 0.055f)));
        if (atmosphere.GetComponent<Renderer>() != null)
        {
            CosmicRaysWorldBuilder.ConfigureTransparent(atmosphere.GetComponent<Renderer>().sharedMaterial, 2f);
            atmosphere.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        GameObject saturn = new GameObject("Distant Saturn");
        saturn.transform.SetParent(root, false);
        saturn.transform.localPosition = new Vector3(-7.2f, 5.2f, 5.8f);
        TexturedSphere(
            saturn.transform,
            "Distant Saturn Sphere",
            Vector3.zero,
            2.9f,
            "Saturn",
            new Color(0.97f, 0.9f, 0.74f, 1f));
        var rings = new GameObject("Distant Saturn Rings");
        rings.transform.SetParent(saturn.transform, false);
        rings.transform.localRotation = Quaternion.Euler(22f, -8f, 0f);
        CosmicRaysWorldBuilder.AddRingBand(
            rings.transform,
            "Distant Ring A",
            1.6f,
            2.15f,
            new Color(0.85f, 0.72f, 0.5f, 0.4f));
        CosmicRaysWorldBuilder.AddRingBand(
            rings.transform,
            "Distant Ring B",
            2.28f,
            2.68f,
            new Color(0.62f, 0.55f, 0.44f, 0.23f));

        AddBeacon(root, new Vector3(0f, 4.35f, 0f));
        AddGeyser(root, new Vector3(-0.8f, -0.52f, 1.12f));
        AddGeyser(root, new Vector3(0.22f, -0.62f, 1.42f));
        AddGeyser(root, new Vector3(1.02f, -0.38f, 0.92f));
        AddGeyser(root, new Vector3(1.52f, -0.14f, 0.12f));

        CosmicRaysWorldBuilder.AddDust(
            root,
            "Encelaadus Ice Dust",
            new Vector3(0f, 1.8f, -2.5f),
            new Vector3(16f, 8f, 10f),
            new Color(0.58f, 0.86f, 1f, 0.2f),
            18f);
        CosmicRaysWorldBuilder.AddPointLight(
            root,
            "Enceladus Rim",
            new Vector3(3.8f, 2.4f, -2.8f),
            new Color(0.46f, 0.76f, 1f),
            4.2f,
            12f);
    }

    public static void PositionTitleCamera(Camera camera, float time)
    {
        if (camera == null)
        {
            return;
        }

        camera.transform.position = new Vector3(
            Mathf.Sin(time * 0.037f) * 0.9f,
            2.3f + Mathf.Sin(time * 0.051f) * 0.12f,
            -11f + Mathf.Cos(time * 0.029f) * 0.35f);
        camera.transform.rotation = Quaternion.LookRotation(
            new Vector3(0f, 1.75f, 0f) - camera.transform.position,
            Vector3.up);
        camera.fieldOfView = 48f;
    }

    public static void PositionDinnerCamera(Camera camera, float time)
    {
        if (camera == null)
        {
            return;
        }

        camera.transform.position = new Vector3(
            2.5f + Mathf.Sin(time * 0.055f) * 0.22f,
            1.52f + Mathf.Sin(time * 0.07f) * 0.025f,
            -2.6f + Mathf.Cos(time * 0.043f) * 0.18f);
        camera.transform.rotation = Quaternion.LookRotation(
            new Vector3(0f, 1.02f, 0.12f) - camera.transform.position,
            Vector3.up);
        camera.fieldOfView = 44f;
    }

    public static void PositionEndingCamera(Camera camera, float time)
    {
        if (camera == null)
        {
            return;
        }

        camera.transform.position = new Vector3(
            Mathf.Sin(time * 0.035f) * 0.55f,
            2.5f + Mathf.Sin(time * 0.047f) * 0.09f,
            -8.8f + Mathf.Cos(time * 0.026f) * 0.22f);
        camera.transform.rotation = Quaternion.LookRotation(
            new Vector3(0f, 2.0f, 0f) - camera.transform.position,
            Vector3.up);
        camera.fieldOfView = 49f;
    }

    private static void BuildRoom(
        Transform parent,
        Vector2 size,
        float height,
        Color floorColor,
        Color wallColor,
        Color ceilingColor)
    {
        float halfWidth = size.x * 0.5f;
        float halfDepth = size.y * 0.5f;

        CosmicRaysWorldBuilder.CreateBox(
            parent,
            "Floor",
            new Vector3(0f, -0.1f, 0f),
            new Vector3(size.x, 0.2f, size.y),
            floorColor,
            true,
            0.05f,
            0.42f);
        CosmicRaysWorldBuilder.CreateBox(
            parent,
            "Ceiling",
            new Vector3(0f, height + 0.1f, 0f),
            new Vector3(size.x, 0.2f, size.y),
            ceilingColor,
            false,
            0f,
            0.15f);
        CosmicRaysWorldBuilder.CreateBox(
            parent,
            "North Wall",
            new Vector3(0f, height * 0.5f, halfDepth + 0.1f),
            new Vector3(size.x + 0.2f, height, 0.2f),
            wallColor,
            true,
            0.02f,
            0.35f);
        CosmicRaysWorldBuilder.CreateBox(
            parent,
            "South Wall",
            new Vector3(0f, height * 0.5f, -halfDepth - 0.1f),
            new Vector3(size.x + 0.2f, height, 0.2f),
            wallColor,
            true,
            0.02f,
            0.35f);
        CosmicRaysWorldBuilder.CreateBox(
            parent,
            "West Wall",
            new Vector3(-halfWidth - 0.1f, height * 0.5f, 0f),
            new Vector3(0.2f, height, size.y),
            wallColor,
            true,
            0.02f,
            0.35f);
        CosmicRaysWorldBuilder.CreateBox(
            parent,
            "East Wall",
            new Vector3(halfWidth + 0.1f, height * 0.5f, 0f),
            new Vector3(0.2f, height, size.y),
            wallColor,
            true,
            0.02f,
            0.35f);

        Color seam = new Color(0.14f, 0.3f, 0.38f, 0.32f);
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            parent,
            "Floor Light Seam North",
            new Vector3(0f, 0.015f, halfDepth - 0.08f),
            new Vector3(size.x - 0.3f, 0.012f, 0.024f),
            seam,
            0.75f);
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            parent,
            "Floor Light Seam South",
            new Vector3(0f, 0.015f, -halfDepth + 0.08f),
            new Vector3(size.x - 0.3f, 0.012f, 0.024f),
            seam,
            0.75f);
    }

    private static void BuildWindow(Transform parent, Vector3 position)
    {
        Material stars = CosmicRaysWorldBuilder.CreateUnlit(
            "Bedroom Star Window",
            Color.white,
            CosmicRaysWorldBuilder.WindowStars);
        GameObject view = CosmicRaysWorldBuilder.CreateQuad(
            parent,
            "Star Window View",
            new Vector3(position.x - 0.04f, position.y, position.z),
            new Vector3(2.15f, 2.45f, 1f),
            Quaternion.Euler(0f, -90f, 0f),
            stars);
        if (view != null)
        {
            view.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        GameObject window = CosmicRaysWorldBuilder.AddModel(
            parent,
            "WallWindow",
            position,
            -90f,
            1.05f,
            false);
        if (window != null)
        {
            window.transform.localPosition = position;
        }

        CosmicRaysWorldBuilder.CreateEmissiveBox(
            parent,
            "Window Sill Light",
            new Vector3(position.x + 0.02f, 0.38f, position.z),
            new Vector3(0.05f, 0.025f, 1.95f),
            new Color(0.32f, 0.68f, 1f, 0.72f),
            1.2f);
    }

    private static Transform BuildSealedDoor(Transform parent, Vector3 position)
    {
        var door = new GameObject("Mom's Private Lab Door");
        door.transform.SetParent(parent, false);
        door.transform.localPosition = position;
        door.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        CosmicRaysWorldBuilder.CreateBox(
            door.transform,
            "Door Frame",
            new Vector3(0f, 1.18f, 0.06f),
            new Vector3(1.62f, 2.4f, 0.16f),
            new Color(0.06f, 0.09f, 0.11f),
            false,
            0.55f,
            0.42f);
        CosmicRaysWorldBuilder.CreateBox(
            door.transform,
            "Door Panel",
            new Vector3(0f, 1.14f, -0.03f),
            new Vector3(1.28f, 2.18f, 0.12f),
            new Color(0.1f, 0.16f, 0.18f),
            false,
            0.45f,
            0.55f);
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            door.transform,
            "Door Seam Left",
            new Vector3(-0.58f, 1.14f, -0.1f),
            new Vector3(0.025f, 2.02f, 0.022f),
            new Color(0.25f, 0.82f, 0.95f, 0.9f),
            1.5f);
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            door.transform,
            "Door Seam Right",
            new Vector3(0.58f, 1.14f, -0.1f),
            new Vector3(0.025f, 2.02f, 0.022f),
            new Color(0.25f, 0.82f, 0.95f, 0.9f),
            1.5f);
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            door.transform,
            "Door Lock Bar",
            new Vector3(0f, 1.54f, -0.11f),
            new Vector3(0.88f, 0.04f, 0.026f),
            new Color(1f, 0.43f, 0.18f, 1f),
            1.35f);
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            door.transform,
            "Door Handle",
            new Vector3(0.43f, 1.02f, -0.12f),
            new Vector3(0.08f, 0.13f, 0.06f),
            new Color(0.4f, 0.85f, 0.95f),
            0.8f);

        AddWorldLabel(
            door.transform,
            "Door Sign",
            new Vector3(0f, 2.62f, -0.02f),
            Quaternion.identity,
            "PRIVATE LAB",
            "AUTHORIZED PERSONNEL ONLY",
            1.42f,
            new Color(1f, 0.48f, 0.24f, 1f),
            new Color(0.035f, 0.03f, 0.03f, 0.88f),
            0.54f);

        return door.transform;
    }

    private static void AddWorldLabel(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        string title,
        string subtitle,
        float worldWidth,
        Color accent,
        Color background,
        float worldHeight)
    {
        var canvasObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(Image));
        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localPosition = position;
        rect.localRotation = rotation;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(760f, 280f);
        rect.localScale = Vector3.one * (worldWidth / 760f);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 4;

        Image image = canvasObject.GetComponent<Image>();
        image.color = background;
        image.raycastTarget = false;

        Image edge = CosmicRaysUIFactory.CreateEdgeLine(
            canvasObject.transform,
            new Color(accent.r, accent.g, accent.b, 0.9f),
            0.012f);

        Text titleLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            canvasObject.transform,
            "Title",
            title,
            new Vector2(0.06f, 0.35f),
            new Vector2(0.94f, 0.88f),
            58,
            TextAnchor.MiddleLeft);
        titleLabel.color = accent;

        Text subtitleLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            canvasObject.transform,
            "Subtitle",
            subtitle,
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.36f),
            28,
            TextAnchor.MiddleLeft);
        subtitleLabel.color = new Color(0.72f, 0.82f, 0.86f, 1f);

        if (worldHeight > 0f)
        {
            rect.sizeDelta = new Vector2(760f, Mathf.Max(190f, 760f * worldHeight / worldWidth));
        }
    }

    private static void AddLabTerminalPanel(Transform parent, Vector3 position)
    {
        Material screenMaterial = CosmicRaysWorldBuilder.CreateUnlit(
            "Terminal Screen",
            new Color(0.25f, 0.76f, 0.92f, 0.78f));
        GameObject panel = CosmicRaysWorldBuilder.CreateQuad(
            parent,
            "Terminal Screen Panel",
            position,
            new Vector3(1.18f, 0.7f, 1f),
            Quaternion.Euler(0f, 90f, 0f),
            screenMaterial);
        if (panel != null)
        {
            panel.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        AddWorldLabel(
            parent,
            "Terminal Readout",
            position + new Vector3(0f, 0f, 0.012f),
            Quaternion.Euler(0f, 90f, 0f),
            "MAGNETIC CALIBRATION",
            "SPA-077 // AWAITING INPUT",
            1.22f,
            new Color(0.56f, 0.96f, 1f, 1f),
            new Color(0.01f, 0.045f, 0.07f, 0.72f),
            0.58f);
    }

    private static void AddInvisibleBoundary(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size)
    {
        CosmicRaysWorldBuilder.AddInvisibleCollider(parent, name, position, size);
    }

    private static void AddWallStrips(
        Transform parent,
        float width,
        float depth,
        float height,
        Color color)
    {
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            parent,
            "Wall Strip North",
            new Vector3(0f, height, depth * 0.5f - 0.1f),
            new Vector3(width * 0.72f, 0.03f, 0.02f),
            color,
            1.1f);
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            parent,
            "Wall Strip South",
            new Vector3(0f, height, -depth * 0.5f + 0.1f),
            new Vector3(width * 0.72f, 0.03f, 0.02f),
            color,
            1.1f);
    }

    private static void AddDustMotes(Transform parent, Vector3 center, Vector3 size, Color color)
    {
        CosmicRaysWorldBuilder.AddDust(parent, "Floating Dust", center, size, color, 22f);
    }

    private static void AddLabDust(Transform root)
    {
        CosmicRaysWorldBuilder.AddDust(
            root,
            "Lab Dust Lower",
            new Vector3(0f, 1.35f, 0f),
            new Vector3(16f, 2.4f, 10f),
            new Color(0.48f, 0.8f, 0.92f, 0.12f),
            28f);
        CosmicRaysWorldBuilder.AddDust(
            root,
            "Core Motes",
            new Vector3(0.55f, 1.8f, -0.15f),
            new Vector3(3.4f, 2.2f, 3.4f),
            new Color(1f, 0.46f, 0.74f, 0.22f),
            18f);
    }

    private static void AddPendantLight(
        Transform parent,
        Vector3 position,
        Color color,
        float intensity)
    {
        CosmicRaysWorldBuilder.CreateEmissiveBox(
            parent,
            "Pendant Housing",
            position,
            new Vector3(0.75f, 0.055f, 0.22f),
            color,
            2.6f);
        CosmicRaysWorldBuilder.AddSpotLight(
            parent,
            "Pendant Downlight",
            position - new Vector3(0f, 0.05f, 0f),
            Quaternion.Euler(90f, 0f, 0f),
            color,
            intensity,
            7.5f,
            52f,
            true);
        CosmicRaysWorldBuilder.AddPointLight(
            parent,
            "Pendant Bloom",
            position,
            new Color(color.r, color.g, color.b),
            intensity * 0.38f,
            4.2f);
    }

    private static void AddBeacon(Transform parent, Vector3 position)
    {
        var root = new GameObject("Enceladus Signal Beacon");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = position;

        Sphere(
            root.transform,
            "Beacon Core",
            Vector3.zero,
            0.32f,
            CosmicRaysWorldBuilder.CreateLit(
                "Beacon Core Material",
                new Color(0.44f, 0.9f, 1f),
                0f,
                0.5f,
                new Color(0.38f, 0.9f, 1f),
                6f));

        CosmicRaysWorldBuilder.AddRingBand(
            root.transform,
            "Beacon Ring A",
            0.52f,
            0.57f,
            new Color(0.4f, 0.9f, 1f, 0.64f));
        GameObject ringB = CosmicRaysWorldBuilder.AddRingBand(
            root.transform,
            "Beacon Ring B",
            0.76f,
            0.81f,
            new Color(0.38f, 0.76f, 1f, 0.38f));
        ringB.transform.localRotation = Quaternion.Euler(68f, 24f, 0f);
        CosmicRaysSpin spin = ringB.AddComponent<CosmicRaysSpin>();
        spin.degreesPerSecond = 11f;

        Light beaconLight = CosmicRaysWorldBuilder.AddPointLight(
            root.transform,
            "Beacon Light",
            Vector3.zero,
            new Color(0.4f, 0.88f, 1f),
            7f,
            8f,
            true);
        CosmicRaysPulse pulse = beaconLight.gameObject.AddComponent<CosmicRaysPulse>();
        pulse.baseIntensity = 7f;
        pulse.amplitude = 1.8f;
        pulse.speed = 1.8f;

        CosmicRaysWorldBuilder.AddVolumetricCone(
            root.transform,
            "Signal Column",
            new Vector3(0f, 4.1f, 0f),
            0.36f,
            7f,
            new Color(0.38f, 0.78f, 1f),
            0.08f);
    }

    private static void AddGeyser(Transform parent, Vector3 position)
    {
        CosmicRaysWorldBuilder.AddGeyser(
            parent,
            "Enceladus Geyser",
            position,
            new Color(0.62f, 0.9f, 1f, 1f));
        Light glow = CosmicRaysWorldBuilder.AddPointLight(
            parent,
            "Geyser Glow",
            position + new Vector3(0f, 0.25f, 0f),
            new Color(0.45f, 0.78f, 1f),
            1.2f,
            3f);
        CosmicRaysPulse pulse = glow.gameObject.AddComponent<CosmicRaysPulse>();
        pulse.baseIntensity = 1.2f;
        pulse.amplitude = 0.35f;
        pulse.speed = 2.7f;
    }

    private static GameObject Sphere(
        Transform parent,
        string name,
        Vector3 position,
        float diameter,
        Material material)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(parent, false);
        sphere.transform.localPosition = position;
        sphere.transform.localScale = Vector3.one * diameter;
        Object.Destroy(sphere.GetComponent<Collider>());
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
        return sphere;
    }

    private static GameObject TexturedSphere(
        Transform parent,
        string name,
        Vector3 position,
        float diameter,
        string textureName,
        Color tint)
    {
        Texture2D texture = Resources.Load<Texture2D>("CosmicRays/Textures/" + textureName);
        Material material = CosmicRaysWorldBuilder.CreateTexturedLit(
            name + " Material",
            texture,
            tint,
            0f,
            0.2f);
        if (material != null && material.HasProperty("_BaseMap"))
        {
            material.SetTextureScale("_BaseMap", new Vector2(2f, 1f));
        }
        return Sphere(parent, name, position, diameter, material);
    }

    private static void EarthAtmosphere(Transform parent, Vector3 position, float diameter)
    {
        GameObject atmosphere = Sphere(
            parent,
            "Earth Atmosphere",
            position,
            diameter,
            CosmicRaysWorldBuilder.CreateUnlit(
                "Earth Atmosphere Material",
                new Color(0.3f, 0.66f, 1f, 0.07f)));
        Renderer renderer = atmosphere.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            CosmicRaysWorldBuilder.ConfigureTransparent(renderer.sharedMaterial, 2f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    private static void AddMoon(Transform parent, Vector3 position)
    {
        GameObject moon = Sphere(
            parent,
            "Moon",
            position,
            0.42f,
            CosmicRaysWorldBuilder.CreateLit(
                "Moon Surface",
                new Color(0.5f, 0.53f, 0.56f),
                0f,
                0.16f));
        CosmicRaysSpin spin = moon.AddComponent<CosmicRaysSpin>();
        spin.degreesPerSecond = 3f;
    }
}
