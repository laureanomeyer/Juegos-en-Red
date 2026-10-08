using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PokeApiRequest : MonoBehaviour
{
    private int idMinimo = 1;
    private int idMaximo = 151;
    private float escalaImagen = 4f;
    private Vector3 posicionImagen = Vector3.zero;
    private float esperaSiguiente = 3f;
    private const string BaseUrl = "https://pokeapi.co/api/v2/pokemon/";
    private SpriteRenderer imagenPokemon;
    private PokemonData pokemonActual;
    private int pistasDadas = 0;
    private bool jugando = false;
    private string textoIngresado = "";
    private bool pedidoAdivinar = false;

    private void Start()
    {
        GameObject go = new GameObject("ImagenPokemon");
        go.transform.position = posicionImagen;
        go.transform.localScale = Vector3.one * escalaImagen;
        imagenPokemon = go.AddComponent<SpriteRenderer>();

        StartCoroutine(NuevoPokemon());
    }

    private void Update()
    {
        if (pedidoAdivinar)
        {
            pedidoAdivinar = false;
            Adivinar();
        }
    }

    // Corrutina que busca un nuevo Pokémon y lo muestra en pantalla.
    private IEnumerator NuevoPokemon()
    {
        jugando = false;
        pistasDadas = 0;
        textoIngresado = "";
        pokemonActual = null;
        imagenPokemon.sprite = null;
        imagenPokemon.color = Color.black;

        int id = Random.Range(idMinimo, idMaximo + 1);

        using (UnityWebRequest request = UnityWebRequest.Get(BaseUrl + id))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al buscar el Pokémon: " + request.error + ". Reintentando...");
                yield return new WaitForSeconds(esperaSiguiente);
                StartCoroutine(NuevoPokemon());
                yield break;
            }

            pokemonActual = JsonUtility.FromJson<PokemonData>(request.downloadHandler.text);
        }

        yield return StartCoroutine(DescargarImagen(pokemonActual.sprites.front_default));

        Debug.Log("<b>¿Quién es ese Pokémon?</b> Escribí tu respuesta y apretá Enter.");
        DarPista();
        jugando = true;
    }

    // Espera que se descargue la imagen y la muestra en pantalla. Devuelve false si no se pudo descargar.
    private IEnumerator DescargarImagen(string urlImagen)
    {
        if (string.IsNullOrEmpty(urlImagen))
        {
            Debug.LogWarning("Este Pokémon no tiene imagen");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(urlImagen))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("No se pudo bajar la imagen: " + request.error);
                yield break;
            }

            Texture2D textura = DownloadHandlerTexture.GetContent(request);
            textura.filterMode = FilterMode.Point;

            imagenPokemon.sprite = Sprite.Create(
                textura,
                new Rect(0, 0, textura.width, textura.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }

    // Da la siguiente stat. Devuelve false si no quedan.
    private bool DarPista()
    {
        if (pistasDadas >= pokemonActual.stats.Count)
            return false;

        PokemonStatSlot s = pokemonActual.stats[pistasDadas];
        pistasDadas++;

        Debug.Log("Pista " + pistasDadas + "/" + pokemonActual.stats.Count + " → " + (s.stat.name) + ": " + s.base_stat);
        return true;
    }

    // Revisa si el texto ingresado es correcto, si lo es, muestra la imagen y espera para el siguiente Pokémon, si no, da otra pista.
    private void Adivinar()
    {
        if (!jugando)
            return;

        string intento = textoIngresado.Trim().ToLower();
        textoIngresado = "";

        if (intento == "")
            return;

        if (intento == pokemonActual.name)
        {
            jugando = false;
            imagenPokemon.color = Color.white;
            Debug.Log("<b>¡CORRECTO!</b> Era " + pokemonActual.name.ToUpper());
            MostrarPokemon(pokemonActual);
            StartCoroutine(SiguienteConEspera());
        }
        else
        {
            Debug.LogWarning("Incorrecto: '" + intento + "' no es.");

            if (!DarPista())
                Debug.Log("No quedan más pistas, seguí probando.");
        }
    }

    // Corrutina espera unos segundos y luego busca un nuevo Pokémon.
    private IEnumerator SiguienteConEspera()
    {
        Debug.Log("Siguiente Pokémon en " + esperaSiguiente + " segundos...");
        yield return new WaitForSeconds(esperaSiguiente);
        StartCoroutine(NuevoPokemon());
    }

    // Muestra en consola la información del Pokémon.
    private void MostrarPokemon(PokemonData p)
    {
        string tipos = "";
        foreach (PokemonTypeSlot t in p.types)
            tipos += t.type.name + " ";

        Debug.Log("#" + p.id + " " + p.name.ToUpper() +
                  "\nTipos: " + tipos +
                  "\nAltura: " + (p.height / 10f) + " m | Peso: " + (p.weight / 10f) + " kg" +
                  "\nExp base: " + p.base_experience);

        foreach (PokemonStatSlot s in p.stats)
        Debug.Log("  " + (s.stat.name) + ": " + s.base_stat);
    }

    // Caja de texto para ingresar la respuesta, y si se presiona Enter, se llama a Adivinar().
    private void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            pedidoAdivinar = true;
            e.Use();
        }

        GUI.skin.textField.fontSize = 22;

        GUI.SetNextControlName("CajaTexto");
        textoIngresado = GUI.TextField(new Rect(20, 20, 300, 36), textoIngresado, 30);
        GUI.FocusControl("CajaTexto");
    }
}