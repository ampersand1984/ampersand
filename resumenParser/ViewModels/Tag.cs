using ampersand.Core;

namespace ResumenParser.ViewModels
{
    public class Tag: BaseModel
    {
        public string Name { get; private set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        public override string ToString()
        {
            return Name;
        }

        public static Tag Get(string name)
        {
            var tag = new Tag()
            {
                Name = name
            };
            return tag;
        }
    }
}
