using System;
using System.Collections.Generic;

[Serializable]
public class PokemonData
{
    public int id;
    public string name;
    public int height;          
    public int weight;          
    public int base_experience;
    public PokemonSprites sprites;
    public List<PokemonTypeSlot> types;
    public List<PokemonStatSlot> stats;
}

[Serializable]
public class PokemonSprites
{
    public string front_default;
    public string back_default;
}

[Serializable]
public class PokemonTypeSlot
{
    public int slot;
    public NamedResource type;
}

[Serializable]
public class PokemonStatSlot
{
    public int base_stat;
    public NamedResource stat;
}

[Serializable]
public class NamedResource
{
    public string name;
    public string url;
}