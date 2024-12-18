using GymManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
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
                string query = "SELECT ID,  Username, Password, Role FROM Users";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);  // Safely handle DBNull
                        string role = reader.IsDBNull(3) ? "User" : reader.GetString(3); // Default Role
                        users.Add(new User
                        {
                            ID = id,
                            Username = reader.GetString(1),
                            Password = reader.GetString(2),
                            Role = role
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
            if (ModelState.IsValid || true)
            {
                try
                {
                    using (var connection = new SQLiteConnection(_connectionString))
                    {
                        connection.Open();

                        string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Username", user.Username);
                            command.Parameters.AddWithValue("@Password", user.Password);

                            command.ExecuteNonQuery();
                        }
                    }

                    // Redirect to the Index page after a successful addition
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Log the exception and return the view with the error
                    Console.WriteLine("Error adding user: " + ex.Message);
                    ModelState.AddModelError("", "An error occurred while adding the user.");
                }
            }

            // If ModelState is invalid or an error occurs, return the view with the user data
            return View(user);
        }
        // GET: /User/Login -> Display the Login form
        public IActionResult Login()
        {
            return View();
        }

        // POST: /User/Login -> Validate and log the user in
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and Password are required.";
                return View();
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    var result = command.ExecuteScalar();
                    if (result != null && Convert.ToInt32(result) > 0)
                    {
                        // Login successful
                        TempData["Message"] = "Login successful!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Invalid credentials
                        ViewBag.Error = "Invalid Username or Password.";
                        return View();
                    }
                }
            }
        }

    }
}