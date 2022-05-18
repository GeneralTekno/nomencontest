using System.Runtime.Serialization;
using System.Windows.Media;

namespace Forturna.Base.Models
{
    [DataContract]
    public class PlayerModel
    {
        [DataMember]
        public double Points { get; set; }
        [DataMember]
        public double TotalPoints { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Color Color { get; set; }
        [DataMember]
        private int _id;
        [IgnoreDataMember]
        public int ID { get { return _id; } }

        private static int _idGenerator = 1;
        public PlayerModel(string name, Color color)
        {
            Name = name;
            Points = 0;
            TotalPoints = 0;
            Color = color;
            _id = _idGenerator++;
        }
    }
     
}
