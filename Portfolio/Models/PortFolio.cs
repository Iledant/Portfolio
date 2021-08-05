#nullable enable

namespace Portfolio.Models
{
    public class PortFolio
    {
        public readonly int ID;
        public readonly string Name;
        public readonly string? Comment;
        public readonly int? Lines;

        public PortFolio(int id = 0, string name = "", string? comment = null, int? lines = null)
        {
            ID = id;
            Name = name;
            Comment = comment;
            Lines = lines;
        }

        public PortFolio CopyWith(int? id = null, string? name = null, string? comment = null, int? lines = null)
        {
            return new(id ?? ID, name ?? Name, comment ?? Comment, lines ?? Lines);
        }
    }
}
