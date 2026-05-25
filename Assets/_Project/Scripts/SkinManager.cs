using TMPro;
using UnityEngine;

public sealed class SkinManager : MonoBehaviour
{
    [System.Serializable]
    public sealed class MaterialSkin
    {
        public string displayName;
        public int price;
        public Material material;
    }

    [System.Serializable]
    public sealed class SkySkin
    {
        public string displayName;
        public int price;
        public Color backgroundColor;
    }

    private const string PlayerCategory = "PLAYER";
    private const string RoadCategory = "ROAD";
    private const string ObstacleCategory = "OBSTACLE";
    private const string SkyCategory = "SKY";

    [Header("References")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CoinManager coinManager;

    [Header("Shop UI")]
    [SerializeField] private TMP_Text shopCoinsText;
    [SerializeField] private TMP_Text shopStatusText;

    [Header("Player Skins")]
    [SerializeField] private MaterialSkin[] playerSkins;
    [SerializeField] private TMP_Text[] playerButtonTexts;

    [Header("Road Skins")]
    [SerializeField] private MaterialSkin[] roadSkins;
    [SerializeField] private TMP_Text[] roadButtonTexts;

    [Header("Obstacle Skins")]
    [SerializeField] private MaterialSkin[] obstacleSkins;
    [SerializeField] private TMP_Text[] obstacleButtonTexts;

    [Header("Sky Skins")]
    [SerializeField] private SkySkin[] skySkins;
    [SerializeField] private TMP_Text[] skyButtonTexts;

    private int selectedPlayerSkin;
    private int selectedRoadSkin;
    private int selectedObstacleSkin;
    private int selectedSkySkin;

    private void Awake()
    {
        selectedPlayerSkin = LoadSelectedSkin(PlayerCategory);
        selectedRoadSkin = LoadSelectedSkin(RoadCategory);
        selectedObstacleSkin = LoadSelectedSkin(ObstacleCategory);
        selectedSkySkin = LoadSelectedSkin(SkyCategory);

        UnlockFreeMaterialSkins(PlayerCategory, playerSkins);
        UnlockFreeMaterialSkins(RoadCategory, roadSkins);
        UnlockFreeMaterialSkins(ObstacleCategory, obstacleSkins);
        UnlockFreeSkySkins();
    }

    private void Start()
    {
        ApplyAllSelectedSkins();
        RefreshShopUi();
    }

    public void TryUsePlayerSkin(int index)
    {
        if (!TryUnlockMaterialSkin(PlayerCategory, index, playerSkins))
        {
            return;
        }

        selectedPlayerSkin = index;
        SaveSelectedSkin(PlayerCategory, index);
        ApplyPlayerSkin();

        SetStatus($"Selected: {playerSkins[index].displayName}");
        RefreshShopUi();
    }

    public void TryUseRoadSkin(int index)
    {
        if (!TryUnlockMaterialSkin(RoadCategory, index, roadSkins))
        {
            return;
        }

        selectedRoadSkin = index;
        SaveSelectedSkin(RoadCategory, index);
        ApplyExistingRoadSkins();

        SetStatus($"Selected: {roadSkins[index].displayName}");
        RefreshShopUi();
    }

    public void TryUseObstacleSkin(int index)
    {
        if (!TryUnlockMaterialSkin(ObstacleCategory, index, obstacleSkins))
        {
            return;
        }

        selectedObstacleSkin = index;
        SaveSelectedSkin(ObstacleCategory, index);
        ApplyExistingObstacleSkins();

        SetStatus($"Selected: {obstacleSkins[index].displayName}");
        RefreshShopUi();
    }

    public void TryUseSkySkin(int index)
    {
        if (!IsValidIndex(index, skySkins))
        {
            SetStatus("Invalid sky skin.");
            return;
        }

        SkySkin skin = skySkins[index];

        if (!IsUnlocked(SkyCategory, index))
        {
            if (!TryBuySkin(SkyCategory, index, skin.displayName, skin.price))
            {
                return;
            }
        }

        selectedSkySkin = index;
        SaveSelectedSkin(SkyCategory, index);
        ApplySkySkin();

        SetStatus($"Selected: {skin.displayName}");
        RefreshShopUi();
    }

    public void ApplyRoadSkin(GameObject roadObject)
    {
        if (!IsValidIndex(selectedRoadSkin, roadSkins))
        {
            return;
        }

        ApplyMaterial(roadObject, roadSkins[selectedRoadSkin].material);
    }

    public void ApplyObstacleSkin(GameObject obstacleObject)
    {
        if (obstacleObject == null)
        {
            return;
        }

        if (!IsValidIndex(selectedObstacleSkin, obstacleSkins))
        {
            Debug.LogWarning($"Invalid obstacle skin index: {selectedObstacleSkin}.");
            return;
        }

        Material material = obstacleSkins[selectedObstacleSkin].material;

        if (material == null)
        {
            Debug.LogWarning($"Obstacle skin material is missing for index {selectedObstacleSkin}.");
            return;
        }

        Renderer[] renderers = obstacleObject.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"No Renderer found on obstacle object: {obstacleObject.name}.");
            return;
        }

        foreach (Renderer currentRenderer in renderers)
        {
            currentRenderer.material = material;
        }
    }

    private bool TryUnlockMaterialSkin(string category, int index, MaterialSkin[] skins)
    {
        if (!IsValidIndex(index, skins))
        {
            SetStatus("Invalid skin.");
            return false;
        }

        MaterialSkin skin = skins[index];

        if (IsUnlocked(category, index))
        {
            return true;
        }

        return TryBuySkin(category, index, skin.displayName, skin.price);
    }

    private bool TryBuySkin(string category, int index, string skinName, int price)
    {
        if (price <= 0)
        {
            UnlockSkin(category, index);
            return true;
        }

        if (coinManager == null)
        {
            SetStatus("Coin manager is missing.");
            return false;
        }

        if (!coinManager.TrySpendCoins(price))
        {
            SetStatus($"Not enough coins for {skinName}.");
            return false;
        }

        UnlockSkin(category, index);
        SetStatus($"Bought: {skinName}");
        return true;
    }

    private void ApplyAllSelectedSkins()
    {
        ApplyPlayerSkin();
        ApplyExistingRoadSkins();
        ApplyExistingObstacleSkins();
        ApplySkySkin();
    }

    private void ApplyPlayerSkin()
    {
        if (playerRenderer == null)
        {
            Debug.LogWarning("Player Renderer is not assigned in SkinManager.");
            return;
        }

        if (!IsValidIndex(selectedPlayerSkin, playerSkins))
        {
            Debug.LogWarning($"Invalid player skin index: {selectedPlayerSkin}.");
            return;
        }

        Material material = playerSkins[selectedPlayerSkin].material;

        if (material == null)
        {
            Debug.LogWarning($"Player skin material is missing for index {selectedPlayerSkin}.");
            return;
        }

        playerRenderer.material = material;
    }

    private void ApplyExistingRoadSkins()
    {
        if (!IsValidIndex(selectedRoadSkin, roadSkins))
        {
            Debug.LogWarning($"Invalid road skin index: {selectedRoadSkin}.");
            return;
        }

        Material material = roadSkins[selectedRoadSkin].material;

        if (material == null)
        {
            Debug.LogWarning($"Road skin material is missing for index {selectedRoadSkin}.");
            return;
        }

        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer currentRenderer in renderers)
        {
            if (currentRenderer.gameObject.name.StartsWith("RoadSegment"))
            {
                currentRenderer.material = material;
            }
        }
    }

    private void ApplyExistingObstacleSkins()
    {
        if (!IsValidIndex(selectedObstacleSkin, obstacleSkins))
        {
            Debug.LogWarning($"Invalid obstacle skin index: {selectedObstacleSkin}.");
            return;
        }

        Material material = obstacleSkins[selectedObstacleSkin].material;

        if (material == null)
        {
            Debug.LogWarning($"Obstacle skin material is missing for index {selectedObstacleSkin}.");
            return;
        }

        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer currentRenderer in renderers)
        {
            GameObject currentObject = currentRenderer.gameObject;
            Transform root = currentObject.transform.root;

            bool isObstacle =
                currentObject.CompareTag("Obstacle") ||
                root.CompareTag("Obstacle") ||
                currentObject.name.StartsWith("Obstacle") ||
                root.name.StartsWith("Obstacle");

            if (!isObstacle)
            {
                continue;
            }

            currentRenderer.material = material;
        }
    }

    private void ApplySkySkin()
    {
        if (mainCamera == null || !IsValidIndex(selectedSkySkin, skySkins))
        {
            return;
        }

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = skySkins[selectedSkySkin].backgroundColor;
    }

    private static void ApplyMaterial(GameObject targetObject, Material material)
    {
        if (targetObject == null || material == null)
        {
            return;
        }

        Renderer objectRenderer = targetObject.GetComponentInChildren<Renderer>(true);

        if (objectRenderer == null)
        {
            Debug.LogWarning($"No Renderer found on {targetObject.name}.");
            return;
        }

        objectRenderer.material = material;
    }

    private void RefreshShopUi()
    {
        if (shopCoinsText != null && coinManager != null)
        {
            shopCoinsText.text = $"Total coins: {coinManager.TotalCoins}";
        }

        RefreshMaterialSkinButtons(PlayerCategory, playerSkins, playerButtonTexts, selectedPlayerSkin);
        RefreshMaterialSkinButtons(RoadCategory, roadSkins, roadButtonTexts, selectedRoadSkin);
        RefreshMaterialSkinButtons(ObstacleCategory, obstacleSkins, obstacleButtonTexts, selectedObstacleSkin);
        RefreshSkySkinButtons();
    }

    private void RefreshMaterialSkinButtons(
        string category,
        MaterialSkin[] skins,
        TMP_Text[] buttonTexts,
        int selectedIndex
    )
    {
        if (skins == null || buttonTexts == null)
        {
            return;
        }

        int count = Mathf.Min(skins.Length, buttonTexts.Length);

        for (int i = 0; i < count; i++)
        {
            if (buttonTexts[i] == null)
            {
                continue;
            }

            buttonTexts[i].text = BuildButtonText(
                skins[i].displayName,
                skins[i].price,
                IsUnlocked(category, i),
                i == selectedIndex
            );
        }
    }

    private void RefreshSkySkinButtons()
    {
        if (skySkins == null || skyButtonTexts == null)
        {
            return;
        }

        int count = Mathf.Min(skySkins.Length, skyButtonTexts.Length);

        for (int i = 0; i < count; i++)
        {
            if (skyButtonTexts[i] == null)
            {
                continue;
            }

            skyButtonTexts[i].text = BuildButtonText(
                skySkins[i].displayName,
                skySkins[i].price,
                IsUnlocked(SkyCategory, i),
                i == selectedSkySkin
            );
        }
    }

    private static string BuildButtonText(
        string displayName,
        int price,
        bool isUnlocked,
        bool isSelected
    )
    {
        if (isSelected)
        {
            return $"{displayName}\nSELECTED";
        }

        if (isUnlocked)
        {
            return $"{displayName}\nSELECT";
        }

        return $"{displayName}\nBUY {price}";
    }

    private void SetStatus(string message)
    {
        if (shopStatusText != null)
        {
            shopStatusText.text = message;
        }

        Debug.Log(message);
    }

    private static bool IsValidIndex(int index, MaterialSkin[] skins)
    {
        return skins != null && index >= 0 && index < skins.Length;
    }

    private static bool IsValidIndex(int index, SkySkin[] skins)
    {
        return skins != null && index >= 0 && index < skins.Length;
    }

    private static int LoadSelectedSkin(string category)
    {
        return PlayerPrefs.GetInt($"SKIN_SELECTED_{category}", 0);
    }

    private static void SaveSelectedSkin(string category, int index)
    {
        PlayerPrefs.SetInt($"SKIN_SELECTED_{category}", index);
        PlayerPrefs.Save();
    }

    private static bool IsUnlocked(string category, int index)
    {
        return PlayerPrefs.GetInt($"SKIN_UNLOCKED_{category}_{index}", 0) == 1;
    }

    private static void UnlockSkin(string category, int index)
    {
        PlayerPrefs.SetInt($"SKIN_UNLOCKED_{category}_{index}", 1);
        PlayerPrefs.Save();
    }

    private static void UnlockFreeMaterialSkins(string category, MaterialSkin[] skins)
    {
        if (skins == null)
        {
            return;
        }

        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].price <= 0)
            {
                UnlockSkin(category, i);
            }
        }
    }

    private void UnlockFreeSkySkins()
    {
        if (skySkins == null)
        {
            return;
        }

        for (int i = 0; i < skySkins.Length; i++)
        {
            if (skySkins[i].price <= 0)
            {
                UnlockSkin(SkyCategory, i);
            }
        }
    }
}