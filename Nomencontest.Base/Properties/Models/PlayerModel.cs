using System.Runtime.Serialization;
using System.Windows.Media;

namespace Nomencontest.Base.Models
{
    [DataContract]
    public class PlayerModel
    {
        [DataMember]
        public double Points { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        private uint _id;
        [IgnoreDataMember]
        public uint ID { get { return _id; } }

        private static uint _idGenerator = 1;
        public PlayerModel(string name)
        {
            Name = name;
            Points = 0;
            _id = _idGenerator++;
        }
    }
     
}
