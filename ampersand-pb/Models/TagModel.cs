using ampersand.Core;
using System.Collections.Generic;

namespace ampersand_pb.Models
{
    public class TagModel: BaseModel
    {
        private string _tag;
        public string Tag
        {
            get { return _tag; }
            set { _tag = value; OnPropertyChanged("Tag"); }
        }
        
        private bool _seleccionada;
        public bool Seleccionada
        {
            get { return _seleccionada; }
            set { _seleccionada = value; OnPropertyChanged("Seleccionada"); }
        }

        public override string ToString()
        {
            var str = Tag;
            return str;
        }
    }

    public static class TagModelExtension
    {
        public static IEnumerable<TagModel> GetTags(this IEnumerable<string> tagsStrings, bool seleccionadas = false)
        {
            var tags = new List<TagModel>();
            foreach (var strTag in tagsStrings)
                tags.Add(new TagModel() { Tag = strTag, Seleccionada = seleccionadas });

            return tags;
        }
    }
}
