using CsvHelper.Configuration;
using FlowerAura.Models;

public class FlowerMap : ClassMap<Flowers>
{
    public FlowerMap()
    {
        Map(f => f.FlowerName).Name("FlowerName");
        Map(f => f.Image).Name("Image");
        Map(f => f.Category).Name("Category");
        Map(f => f.Amount).Name("Amount");
        Map(f => f.Description).Name("Description");
    }
}
