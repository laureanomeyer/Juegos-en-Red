using UnityEngine;

public static class ArithmeticEncryption
{
    private const int MULTIPLIER = 9;
    private const int OFFSET = 14;

    /// <summary>
    /// Encripta un resultado aritmetico multiplicandolo por 9 y sumandole 14.
    /// Formula: (valor * 9) + 14
    /// </summary>
    public static int Encrypt(int value)
    {
        int encrypted = (value * MULTIPLIER) + OFFSET;
        Debug.Log($"[ENCRYPT] Valor original: {value} -> Valor encriptado: {encrypted}");
        return encrypted;
    }

    /// <summary>
    /// Desencripta un valor restaurando el resultado aritmetico original.
    /// Formula inversa: (valor - 14) / 9
    /// </summary>
    public static int Decrypt(int encryptedValue)
    {
        if ((encryptedValue - OFFSET) % MULTIPLIER != 0)
        {
            Debug.LogWarning($"[DECRYPT] El valor {encryptedValue} no es valido (no es multiplo valido)");
            return -1;
        }

        int decrypted = (encryptedValue - OFFSET) / MULTIPLIER;
        Debug.Log($"[DECRYPT] Valor encriptado: {encryptedValue} -> Valor original: {decrypted}");
        return decrypted;
    }

    /// <summary>
    /// Verifica si un valor encriptado es valido.
    /// </summary>
    public static bool IsValidEncrypted(int encryptedValue)
    {
        return (encryptedValue - OFFSET) % MULTIPLIER == 0;
    }
}