using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 照準をコントロールする
/// </summary>
public class CrosshairController : MonoBehaviour
{
    /// <summary>照準の画像</summary>
    [SerializeField] Image _sprite = default;
    /// <summary>照準のパネル</summary>
    [SerializeField] RectTransform _crosshair = default;
    /// <summary>プレイヤー</summary>
    [SerializeField] PlayerController _player = default;
    /// <summary>ロックオンしていない時の照準の色</summary>
    [SerializeField] Color _defaultColor = new Color(0, 0, 0, 0);
    /// <summary>ロックオンしている時の照準の色</summary>
    [SerializeField] Color _lockOnColor = Color.red;

    void Update()
    {
        if (_player.LockedTarget)
        {
            // ロックオンしている時は照準をターゲットに重ねる
            _crosshair.position = Camera.main.WorldToScreenPoint(_player.LockedTarget.position);
            _sprite.color = _lockOnColor;
        }
        else
        {
            _sprite.color = _defaultColor;
        }
    }
}
