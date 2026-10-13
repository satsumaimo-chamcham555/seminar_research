using UnityEngine;

/// <summary>
/// カーソルのロック/解除をまとめた共通ヘルパー（新規追加）。
/// 以前は各クラスで lockState と visible を毎回2行セットで書いていた。
/// </summary>
public static class CursorUtil
{
    public static void SetLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
