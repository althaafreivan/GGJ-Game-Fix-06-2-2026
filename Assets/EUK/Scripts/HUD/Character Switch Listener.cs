using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterSwitchListener : MonoBehaviour
{
    public Image parent;
    public Sprite s_BNW, s_WNB;
    public TextMeshProUGUI text;
    public RectTransform BNW, WNB;

    [Header("Animation Settings")]
    public float duration = 0.4f;
    public Vector3 offset = new Vector3(30, 30, 0);

    private Vector3 topPos;
    private Vector3 bottomPos;
    private bool isFirstChange = true;

    private void Awake()
    {
        // Capture initial positions. 
        // We assume the one at the back (first sibling) is bottom, 
        // and the one at the front (last sibling) is top.
        if (BNW.GetSiblingIndex() > WNB.GetSiblingIndex())
        {
            topPos = BNW.localPosition;
            bottomPos = WNB.localPosition;
        }
        else
        {
            topPos = WNB.localPosition;
            bottomPos = BNW.localPosition;
        }
    }

    private void Start()
    {
        Player.onPlayerChange += changed;
        if (Player.ActivePlayer != null)
        {
            changed(Player.ActivePlayer);
        }
    }

    private void OnDestroy()
    {
        Player.onPlayerChange -= changed;
    }

    private void changed(Player player)
    {
        bool isMainPlayer = player.GetComponent<M_SwapPlayer>().MainPlayer;
        RectTransform newActive = isMainPlayer ? BNW : WNB;
        RectTransform newInactive = isMainPlayer ? WNB : BNW;

        if (isFirstChange)
        {
            isFirstChange = false;
            BNW.DOKill();
            WNB.DOKill();
            newActive.localPosition = topPos;
            newInactive.localPosition = bottomPos;
            newActive.SetAsLastSibling();
            UpdateUI(isMainPlayer);
            return;
        }

        // Kill existing tweens to prevent overlapping and position issues
        BNW.DOKill();
        WNB.DOKill();

        // Animate switch
        // 1. Move inactive (currently top) to top-right
        // 2. Move active (currently bottom) to top
        // 3. Move inactive to bottom and set as back sibling

        Sequence seq = DOTween.Sequence();

        // New active moves to top over the full duration
        seq.Append(newActive.DOLocalMove(topPos, duration).SetEase(Ease.InOutQuad));
        
        // New inactive moves out, switches sibling, then moves in
        seq.Insert(0, newInactive.DOLocalMove(topPos + offset, duration * 0.5f).SetEase(Ease.OutQuad));
        seq.InsertCallback(duration * 0.5f, () => newInactive.SetAsFirstSibling());
        seq.Insert(duration * 0.5f, newInactive.DOLocalMove(bottomPos, duration * 0.5f).SetEase(Ease.InQuad));

        UpdateUI(isMainPlayer);
    }

    private void UpdateUI(bool isMainPlayer)
    {
        if (isMainPlayer)
        {
            text.text = "Nothing";
            text.color = Color.black;
            parent.sprite = s_WNB;
        }
        else
        {
            text.text = "Expected";
            text.color = Color.white;
            parent.sprite = s_BNW;
        }
    }
}
