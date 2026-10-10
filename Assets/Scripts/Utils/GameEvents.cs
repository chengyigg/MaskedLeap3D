using System;

public static class GameEvents
{
    // ³Ô½ð±Ò
    public static event Action OnCoinCollected;
    public static void CoinCollected() => OnCoinCollected?.Invoke();

    // Íæ¼ÒËÀÍö
    public static event Action OnPlayerDied;
    public static void PlayerDied() => OnPlayerDied?.Invoke();

    // Íæ¼Òµ½´ïÖÕµã
    public static event Action OnPlayerWin;
    public static void PlayerWin() => OnPlayerWin?.Invoke();

    // Íæ¼Ò±»µ¯Ìø£¨µ¯»É¡¢²ÈµÐÈË£©
    public static event Action OnPlayerBounced;
    public static void PlayerBounced() => OnPlayerBounced?.Invoke();
}