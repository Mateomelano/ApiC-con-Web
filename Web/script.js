document.addEventListener("DOMContentLoaded", () => {
  const apiBaseUrl = "http://localhost:5186/api"; // Reemplaza {puerto} con el puerto correcto

  const registerButton = document.getElementById("register-button");
  const loginButton = document.getElementById("login-button");
  const protectedButton = document.getElementById("protected-button");
  const updateButton = document.getElementById("update-button");

  let token = "";

  document.getElementById("register-button").addEventListener("click", () => {
    const username = document.getElementById("register-username").value;
    const password = document.getElementById("register-password").value;
    const email = document.getElementById("register-email").value;
    const role = document.getElementById("register-role").value;

    fetch(`${apiBaseUrl}/Auth/register`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ username, password, email, role }),
    })
    .then((response) => {
        if (!response.ok) {
            return response.json().then((errorData) => {
                throw new Error(errorData.message || "Registration failed");
            });
        }
        return response.json();
    })
    .then((data) => {
        document.getElementById("register-message").textContent = data.Message || "Registration successful";
    })
    .catch((error) => {
        document.getElementById("register-message").textContent = error.message;
    });
  });

  loginButton.addEventListener("click", () => {
    const username = document.getElementById("login-username").value;
    const password = document.getElementById("login-password").value;

    fetch(`${apiBaseUrl}/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ username, password }),
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error("Login failed");
        }
        return response.json();
      })
      .then((data) => {
        if (data.token) {
          token = data.token;
          document.getElementById("login-message").textContent =
            "Login successful";
        } else {
          throw new Error("Login failed");
        }
      })
      .catch((error) => {
        console.error("Error:", error);
        document.getElementById("login-message").textContent = "Login failed";
      });
  });

  const deleteButton = document.getElementById("delete-button");
  deleteButton.addEventListener("click", () => {
    const userId = document.getElementById("delete-id").value;

    fetch(`${apiBaseUrl}/auth/${userId}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error("Delete failed");
        }
        document.getElementById("delete-message").textContent =
          "User deleted successfully";
      })
      .catch((error) => {
        console.error("Error:", error);
        document.getElementById("delete-message").textContent = error.message;
      });
  });

  protectedButton.addEventListener("click", () => {
    console.log("Token:", token); // Verifica que el token se haya establecido correctamente

    fetch(`${apiBaseUrl}/values`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error("Access denied");
        }
        return response.json();
      })
      .then((data) => {
        console.log("Response data:", data); // Verifica la estructura de la respuesta

        if (Array.isArray(data)) {
          let tableHTML =
            "<table><tr><th>ID</th><th>Username</th><th>Email</th><th>Role</th></tr>";

          data.forEach((user) => {
            tableHTML += `<tr>
              <td>${user.id}</td>
              <td>${user.username}</td>
              <td>${user.email}</td>
              <td>${user.role}</td>
            </tr>`;
          });

          tableHTML += "</table>";

          document.getElementById("protected-message").innerHTML = tableHTML;
        } else {
          throw new Error("Unexpected response format");
        }
      })
      .catch((error) => {
        console.error("Error:", error);
        document.getElementById("protected-message").textContent = error.message;
      });
  });

  updateButton.addEventListener("click", () => {
    const userId = document.getElementById("update-id").value;
    const username = document.getElementById("update-username").value;
    const password = document.getElementById("update-password").value;
    const email = document.getElementById("update-email").value;
    const role = document.getElementById("update-role").value;

    fetch(`${apiBaseUrl}/auth/${userId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ username, password, email, role }),
    })
      .then((response) => {
        if (!response.ok) {
          return response.json().then((errorData) => {
            throw new Error(errorData.message || "Update failed");
          });
        }
        document.getElementById("update-message").textContent =
          "User updated successfully";
      })
      .catch((error) => {
        console.error("Error:", error);
        document.getElementById("update-message").textContent = error.message;
      });
  });
});