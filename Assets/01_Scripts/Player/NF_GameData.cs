using UnityEngine;

public static class NF_GameData
{
    // 🔸 Nombre del spawn donde aparecerá el jugador al cargar
    public static string nextSpawnName;

    // 🔹 Habilidades desbloqueadas
    public static bool dashUnlocked = false;
    public static bool wallJumpUnlocked = false;
    public static bool doubleJumpUnlocked = false;

    public static void SavePlayerState()
    {
        if (CA_PlayerController.Instance == null) return;

        dashUnlocked = CA_PlayerController.Instance.canUseDash;
        wallJumpUnlocked = CA_PlayerController.Instance.canUseWallJump;
        doubleJumpUnlocked = CA_PlayerController.Instance.canUseDoubleJump;
    }

    public static void LoadPlayerState(CA_PlayerController player)
    {
        if (player == null) return;

        player.canUseDash = dashUnlocked;
        player.canUseWallJump = wallJumpUnlocked;
        player.canUseDoubleJump = doubleJumpUnlocked;
    }
}
