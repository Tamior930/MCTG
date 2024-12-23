namespace MCTG.BusinessLayer.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }

        public UserProfile(string name, string bio, string image)
        {
            Name = name;
            Bio = bio;
            Image = image;
        }
    }
} 