using UnityEngine;
using UnityEngine.UI;

public class FinalHouseWinTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string winMessage = "YOU MADE IT HOME";
    [SerializeField] private bool pauseGameOnWin = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 1f, 0.35f, 0.85f);

    public static bool HasWon { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetWinState()
    {
        HasWon = false;
        Time.timeScale = 1f;
    }

    public void Initialize(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            winMessage = message;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HasWon || !IsPlayer(other))
        {
            return;
        }

        Win();
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag);
    }

    private void Win()
    {
        HasWon = true;
        ShowWinOverlay();

        if (pauseGameOnWin)
        {
            Time.timeScale = 0f;
        }
    }

    private void ShowWinOverlay()
    {
        if (GameObject.Find("WinCanvas") != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("WinCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject backdropObject = new GameObject("Backdrop", typeof(Image));
        backdropObject.transform.SetParent(canvasObject.transform, false);

        Image backdrop = backdropObject.GetComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform backdropRect = backdropObject.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("WinText", typeof(Text));
        textObject.transform.SetParent(canvasObject.transform, false);

        Text text = textObject.GetComponent<Text>();
        text.text = winMessage;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 96;
        text.fontStyle = FontStyle.Bold;
        text.font = GetDefaultFont();

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.35f);
        textRect.anchorMax = new Vector2(0.9f, 0.65f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private static Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    private void OnDrawGizmos()
    {
        BoxCollider triggerCollider = GetComponent<BoxCollider>();

        if (triggerCollider == null)
        {
            return;
        }

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.12f);
        Gizmos.DrawCube(triggerCollider.center, triggerCollider.size);

        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }
}
