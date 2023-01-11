using System.ComponentModel.DataAnnotations.Schema;

namespace FileBaseContextCore.Example.Data.Entities
{
    public class Content : Base
    {
        public string Text { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual List<ContentEntry> Entries { get; set; }
    }
}
