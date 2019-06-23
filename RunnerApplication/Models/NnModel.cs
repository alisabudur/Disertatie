using DataAccess.Attributes;

namespace SoftwareQualityPrediction.Models
{
    [Sheet("Foaie1")]
    public class NnModel
    {
        [Identifier]
        [Column("Path")]
        public string Path { get; set; }

        [Column("InputNodes")]
        public string InputNodes { get; set; }

        [Column("OutputNodes")]
        public string OutputNodes { get; set; }
    }
}
