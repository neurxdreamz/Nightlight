namespace Nightlight
{
    public class ContractViewModel
    {
        public string Title { get; set; }
        public string Pre { get; set; }
        public string Post { get; set; }
        public string Effects { get; set; }
        public string ValidExample { get; set; }
        public string InvalidExample { get; set; }

        public ContractViewModel()
        {
            Title = "";
            Pre = "";
            Post = "";
            Effects = "";
            ValidExample = "";
            InvalidExample = "";
        }
    }
}