using GalaSoft.MvvmLight;

namespace Popcorn.Models.User
{
    public class Language : ObservableObject
    {
        private string _name;

        public string Culture { get; set; }

        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
