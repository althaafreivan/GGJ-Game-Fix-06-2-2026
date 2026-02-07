using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSwitchListener : MonoBehaviour
{
    public Image parent;
    public Sprite s_BNW, s_WNB;
    public TextMeshProUGUI text;
    public Transform BNW, WNB;

    private void Start()
    {
        Player.onPlayerChange += changed;
    }

    private void changed(Player player)
    {
        if(player.GetComponent<M_SwapPlayer>().MainPlayer == true)
        {
           BNW.SetAsLastSibling();
            text.text = "Nothing";
            text.color = Color.black;
            parent.sprite = s_WNB;
        }
        else
        {
            WNB.SetAsLastSibling();
            text.text = "Expectation";
            text.color = Color.white;
            parent.sprite = s_BNW;
        }
    }

}
