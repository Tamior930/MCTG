namespace MCTG.Business.Models
{
    public class UserProfile
    {
        public string Bio { get; set; }
        public string Image { get; set; }

        public UserProfile(string bio, string image)
        {
            Bio = bio;
            Image = image;
        }
    }
}