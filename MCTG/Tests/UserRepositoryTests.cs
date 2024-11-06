using MCTG.BusinessLayer.Models;
using MCTG.Data.Repositories;

public class UserRepositoryTests
{
    public static void TestUserRetrieval()
    {
        // Setup
        var repo = new UserRepository();
        var testUser = new User("testuser", "password");
        
        // First save the user
        repo.AddUser(testUser);
        
        // Update their stats (simulate some game activity)
        testUser.UpdateELO(true);  // Win a game
        testUser.UpdateELO(true);  // Win another game
        
        // Now retrieve the user
        var retrievedUser = repo.GetUserByUsername("testuser");
        
        // Print results
        Console.WriteLine($"Original user - Wins: {testUser.Wins}, ELO: {testUser.ELO}");
        Console.WriteLine($"Retrieved user - Wins: {retrievedUser.Wins}, ELO: {retrievedUser.ELO}");
        
        // Cleanup
        repo.RemoveUserByUsername("testuser");
    }
} 