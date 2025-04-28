# Vanta Safe :lock:

Secure Desktop Password Manager built with **C# WPF** and **SQLite**.

---

## :file_folder: Overview

Vanta Safe is a **desktop-based password manager** that allows users to:
- Register with a **master password**.
- Login securely using their credentials.
- Store multiple passwords for websites, apps, and services.
- **Encrypt** all sensitive data (site name, URL, username, password) using strong encryption.
- **Search** stored credentials quickly.
- **Prevent brute-force attacks**: Application auto-closes after 5 wrong login attempts.
- Smooth **UI with custom background**, hint boxes, and live search functionality.

---

## :hammer_and_wrench: Project Structure

| Folder/File            | Purpose |
| ---------------------- | ------- |
| `Ui/`                  | Contains WPF windows like Login, Register, Vault (password list) |
| `Services/`            | Authentication, Database connection services |
| `Models/`              | Credential and User data models |
| `EncryptDecrypt.cs`    | Encryption and decryption helpers |
| `App.xaml.cs`          | App-level properties like `MasterKey`, `CurrentUser` |

---

## :bar_chart: Application Flow Diagrams

### 1. Authentication Flow
```
Start
  ↓
Register → Save username + password hash + device secret
  ↓
Login → Verify master password
  ↓
Success → Derive master key → Load Vault
  ↓
Failure → Increase FailedAttempts
    ↓
    if (FailedAttempts == 5) → App closes
```

### 2. Credential Management Flow
```
Vault Window
  ↓
Add Credential → Encrypt fields → Save in database
  ↓
Search Credential → Decrypt fields → Filter matches
  ↓
Double Click → View full decrypted credentials
```

---

## :clipboard: Features in Detail

| Feature | Description |
| ------- | ----------- |
| **Register** | User registers a master password. Password is hashed + device secret generated. |
| **Login** | Master password is verified securely. On success, session master key is generated. |
| **Failed Login Handling** | After 5 incorrect attempts, application **force closes** to prevent brute force. |
| **Vault** | User can view all saved passwords with decrypted **site name and URL**. |
| **Encryption** | Site name, site URL, username, and password are encrypted separately using `EncryptField`/`DecryptField`. |
| **Search** | User can search credentials by **partial** match of site name, username, or site URL. |
| **Add New** | Add a new credential securely with encryption before saving. |
| **Refresh Button** | Refresh credential list without restarting app. |

---

## :lock: Security Details

- **SQLite database** is used to store encrypted fields.
- Encryption uses **symmetric key** derived from master password and device-specific secret.
- **ZeroMemory** is used to clean sensitive data from memory after use.
- User lockout on repeated login failures to resist brute force attacks.

---

## :man_technologist: Usage Guide

1. **Launch Vanta Safe**
2. **Register** if you are a new user.
3. **Login** with your username and master password.
4. **Vault opens**:
   - Add new passwords using the **Add (+)** button.
   - **Search** any saved password with site name, username, or URL.
   - **Refresh** password list using the **Refresh (🔄)** button.
5. **Double-click** any credential to view details (future expansion).

---

## :framed_picture: Screens Overview

| Screen | Purpose |
| ------ | ------- |
| **MainWindow** | Welcome screen, navigation to login/register |
| **RegisterWindow** | New user registration |
| **LoginWindow** | User login, validation, failed attempts handling |
| **VaultWindow** | Passwords dashboard (view, search, add) |

---

## :rocket: Future Improvements (Optional)

- Password strength checker.
- Auto logout after inactivity.
- Copy username/password to clipboard button.
- Categorization or folders for credentials.

---

# :white_check_mark: Project is COMPLETE and SECURE as per current goals.

---

