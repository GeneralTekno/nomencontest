using System.Runtime.Serialization;

namespace Nomencontest.Base.Models
{
    [DataContract]
    public class CategoryModel
    {
        [IgnoreDataMember]
        private static uint _idGenerator = 1;
        [DataMember]
        public string[] Items { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        private uint _id;
        [DataMember]
        public uint ID { get { return _id; } set {} }
        [DataMember]
        public bool IsFinal { get; set; }

        public CategoryModel(string category, string[] items, bool isFinal)
        {
            Category = category.ToUpper();
            IsFinal = isFinal;
            Items = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                Items[i] = items[i].ToUpper();
            }
            _id = _idGenerator++;
        }
    }

}
