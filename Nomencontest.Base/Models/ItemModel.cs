using System.Runtime.Serialization;

namespace Forturna.Base.Models
{
    [DataContract]
    public class ItemModel
    {
        private static int _idGenerator = 1;
        [DataMember]
        public string Answer { get; set; }
        [DataMember]
        public string Hint { get; set; }
        [DataMember]
        public double BaseValue { get; set; }
        private int _id;
        public int ID { get { return _id; } }

        public ItemModel(string answer, string hint = default(string), double basevalue = 0)
        {
            Answer = answer.ToUpper();
            Hint = hint != null ? hint.ToUpper() : string.Empty;
            BaseValue = BaseValue;
            _id = _idGenerator++;
        }
    }
}
