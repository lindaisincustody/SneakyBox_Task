using UnityEngine;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    public BuildManager buildManager; // Reference to your BuildManager script
    public RawImage buttonRawImage; // Reference to the RawImage component of the button

    private Material defaultMaterial;

    private void Start()
    {
        defaultMaterial = buttonRawImage.material;
        UpdateButtonMaterial();
    }

    public void UpdateButtonMaterial()
    {
        if (buildManager != null && buttonRawImage != null)
        {
            int materialIndex = buildManager.currentMaterialIndex;

            if (materialIndex >= 0 && materialIndex < buildManager.materials.Count)
            {
                buttonRawImage.material = buildManager.materials[materialIndex];
            }
            else
            {
                buttonRawImage.material = defaultMaterial;
            }
        }
    }
}
