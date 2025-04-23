using KaffeMaskineProject.DomainModels.User;
using KaffeMaskineProject.DomainModels.Recipe;

namespace KaffeMaskineProject.DomainModels
{
    public class Statistics
    {
        public Recipe recipeName { get; set; } 
        public User userId { get; set; }
        public int numberOfUses { get; set; }

    }
}

