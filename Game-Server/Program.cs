namespace GameServer;

using System;
using System.Text.Json;
 
class Pet
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public double Age { get; set; }
}

public static class Program{
    public static void Main(string[] args){
        var pets = new List<Pet>
        {
            new Pet { Type = "Cat", Name = "MooMoo", Age = 3.4 },
            new Pet { Type = "Squirrel", Name = "Sandy", Age = 7 }
        };

        Console.WriteLine(JsonSerializer.Serialize(pets));
    }
}