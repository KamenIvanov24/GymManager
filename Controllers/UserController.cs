using GymManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SQLite;

namespace GymManager.Controllers
{
    public class UserController : Controller
    {
        private readonly string _connectionString = "Data Source=Files/GymManager.db;Version=3;";

        // GET: /User/Index -> Display all users
        public IActionResult Index()
        {
            var users = new List<User>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID, Username, Password, Role FROM Users";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);  // Safely handle DBNull

                        users.Add(new User
                        {
                            ID = id,
                            Username = reader.GetString(1),
                            Password = reader.GetString(2),
                            Role = reader.GetString(3)
                        });
                    }
                }
            }

            return View(users);
        }

        // GET: /User/Add -> Display form to add a new user
        public IActionResult Add()
        {
            return View();
        }

        // POST: /User/Add -> Add a new user to the database
        [HttpPost]
        public IActionResult Add(User user)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // We do NOT need to insert the ID, since it's auto-incremented
                    string query = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameters for Username, Password, and Role
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Password", user.Password);
                        command.Parameters.AddWithValue("@Role", user.Role);

                        // Execute the command to insert the data
                        command.ExecuteNonQuery();
                    }
                }

                // After insertion, redirect to the list of users
                return RedirectToAction("Index");
            }

            // If model is not valid, return the same view to show validation errors
            return View(user);
        }
    }
}