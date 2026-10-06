using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;   

public class PokeApiRequest : MonoBehaviour
{
    private string pokemonABuscar = "blastoise";
    private float escalaImagen = 4f;
    private Vector3 posicionImagen = Vector3.zero;
    private const string BaseUrl = "https://pokeapi.co/api/v2/pokemon/";
    private SpriteRenderer imagenPokemon;

    private void Start()
    {
        GameObject go = new GameObject("ImagenPokemon");
        go.transform.position = posicionImagen;
        go.transform.localScale = Vector3.one * escalaImagen;
        imagenPokemon = go.AddComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(BuscarPokemon(pokemonABuscar));
        }
    }

    private IEnumerator BuscarPokemon(string nombre)
    {
        string url = BaseUrl + nombre.Trim().ToLower();
        Debug.Log("Buscando: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error de conexión: " + request.error);
                yield break;
            }

            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                if (request.responseCode == 404)
                    Debug.LogWarning("No existe ningún Pokémon llamado '" + nombre + "'");
                else
                    Debug.LogError("Error HTTP " + request.responseCode + ": " + request.error);
                yield break;
            }

            if (request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Error procesando los datos: " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;
            PokemonData pokemon = JsonUtility.FromJson<PokemonData>(json);
            MostrarPokemon(pokemon);
            yield return StartCoroutine(DescargarImagen(pokemon.sprites.front_default));
        }
    }

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

            Sprite sprite = Sprite.Create(
                textura,
                new Rect(0, 0, textura.width, textura.height),
                new Vector2(0.5f, 0.5f)   
            );

            imagenPokemon.sprite = sprite;
        }
    }

    private void MostrarPokemon(PokemonData p)
    {
        string tipos = "";
        foreach (PokemonTypeSlot t in p.types)
            tipos += t.type.name + " ";

        Debug.Log("#" + p.id + " " + p.name.ToUpper() +
                  "\nTipos: " + tipos +
                  "\nAltura: " + (p.height / 10f) + " m | Peso: " + (p.weight / 10f) + " kg" +
                  "\nExp base: " + p.base_experience +
                  "\nSprite: " + p.sprites.front_default);

        foreach (PokemonStatSlot s in p.stats)
            Debug.Log("  " + s.stat.name + ": " + s.base_stat);
    }
}