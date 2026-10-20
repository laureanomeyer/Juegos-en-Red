        using UnityEngine;

public class EncryptionTester : MonoBehaviour
{
    [SerializeField] private int valorOriginal = 10;
    [SerializeField] private int valorEncriptado = 0;
    [SerializeField] private int valorDesencriptado = 0;
    [SerializeField] private bool esValido = false;

    [Header("Botones de prueba")]
    [SerializeField] private bool ejecutarEncriptacion = false;
    [SerializeField] private bool ejecutarDesencriptacion = false;
    [SerializeField] private bool ejecutarVerificacion = false;
    [SerializeField] private bool limpiarResultados = false;

    private void OnValidate()
    {
        if (ejecutarEncriptacion)
        {
            ejecutarEncriptacion = false;
            valorEncriptado = ArithmeticEncryption.Encrypt(valorOriginal);
        }

        if (ejecutarDesencriptacion)
        {
            ejecutarDesencriptacion = false;
            valorDesencriptado = ArithmeticEncryption.Decrypt(valorEncriptado);
        }

        if (ejecutarVerificacion)
        {
            ejecutarVerificacion = false;
            esValido = ArithmeticEncryption.IsValidEncrypted(valorEncriptado);
            Debug.Log($"¿Es válido {valorEncriptado}? {esValido}");
        }

        if (limpiarResultados)
        {
            limpiarResultados = false;
            valorEncriptado = 0;
            valorDesencriptado = 0;
            esValido = false;
        }
    }

    private void Start()
    {
        Debug.Log("=== PRUEBA DE ENCRIPTACIÓN ===");
        Debug.Log("Fórmula encriptación: (valor * 9) + 14");
        Debug.Log("Fórmula desencriptación: (valor - 14) / 9");
    }
}