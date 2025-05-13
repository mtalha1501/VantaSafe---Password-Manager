# 🔒 Vanta Safe - Ultimate Password Protection  

**Your digital vault for bulletproof password management**  

[![Download Installer](https://img.shields.io/badge/Download-Win_x64_Installer-blue?style=for-the-badge&logo=windows)](https://drive.google.com/uc?export=download&id=1zH80TK4tHPl1lGcNU58NW5-hKX2IP78C)  

---

## 🌟 Why Choose Vanta Safe?  

✅ **Military-grade encryption** (AES-256 + BCrypt + PBKDF2)  
✅ **100% offline** - Your data never leaves your computer  
✅ **Brute force protection** - Auto-lock after 5 failed attempts  
✅ **Secure recovery** - Device-secret based account restoration  
✅ **Clipboard protection** - Auto-clears passwords after 15 seconds  

---



## 🚀 Getting Started  

1. **Download** the installer above  
2. **Install** with one click  
3. **Register** with a strong master password  
4. **Securely store** all your credentials  

---

## 🔐 Security Architecture  

### Encryption Workflow  
<pre lang="markdown"> ```mermaid graph TD A[Master Password] --> B(BCrypt Hashing) C[Device Secret] --> B B --> D[Secure Database Storage] A --> E(PBKDF2 Key Derivation) C --> E E --> F[AES-256 Encryption] F --> G[Encrypted Credential Storage] style A fill:#86C7F3,stroke:#333,stroke-width:2px style B fill:#B6D7A8,stroke:#333,stroke-width:2px style C fill:#86C7F3,stroke:#333,stroke-width:2px style D fill:#B6D7A8,stroke:#333,stroke-width:2px style E fill:#86C7F3,stroke:#333,stroke-width:2px style F fill:#B6D7A8,stroke:#333,stroke-width:2px style G fill:#86C7F3,stroke:#333,stroke-width:2px ``` </pre>

### DATA Workflow
``` mermaid
graph TD
    %% Entities
    User((User))
    Database[(Encrypted Database)]
    WindowsDPAPI[Windows DPAPI]

    %% Processes
    subgraph Vanta Safe
        A[Registration]
        B[Login]
        C[Credential Management]
        D[Encryption/Decryption]
    end

    %% Data Flows
    User -->|Master Password| A
    User -->|Master Password| B
    User -->|Add/Edit Credentials| C

    A -->|Device Secret| User
    A -->|Hashed Password + Encrypted Keys| Database

    B -->|Session Master Key| C
    B -->|Lockout Signal| User

    C -->|Encrypted Credentials| Database
    C -->|Decrypted Credentials| User

    D -->|AES-256 Keys| WindowsDPAPI
    WindowsDPAPI -->|Protected Keys| Database

    %% Legend
    legend[<u>Legend</u>:
    <br>○ = External Entity
    <br>□ = Process
    <br>⦿ = Data Store]
```
### Sequence Diagram
``` mermaid
sequenceDiagram
    participant User
    participant UI as Vanta UI
    participant Auth as AuthService
    participant Crypto as EncryptDecrypt
    participant DB as Encrypted DB
    participant DPAPI as Windows DPAPI

    Note over User,DPAPI: Registration Flow
    User->>UI: Enters master password
    UI->>Auth: Register(username, password)
    Auth->>Auth: Generate device_secret
    Auth->>Crypto: BCrypt(password + device_secret)
    Auth->>Crypto: DeriveKey(device_secret)
    Crypto->>Crypto: PBKDF2(device_secret, iterations=100k)
    Auth->>Crypto: EncryptMasterKey(password, aes_key)
    Crypto->>DB: Store hashed_pw + encrypted_master_key
    Auth->>User: Show device_secret (once)

    Note over User,DPAPI: Login Flow
    User->>UI: Enters password
    UI->>Auth: Login(username, password)
    Auth->>DB: Get hashed_pw + device_secret
    Auth->>Auth: BCryptVerify(password + device_secret)
    Auth->>Crypto: DeriveMasterKey(password, device_secret)
    Crypto->>Crypto: PBKDF2(password + device_secret)
    Auth->>UI: Session established (master_key in RAM)

    Note over User,DPAPI: Credential Access
    User->>UI: Requests "Add Credential"
    UI->>Crypto: EncryptField(site_password, master_key)
    Crypto->>Crypto: AES-256-CBC with random IV
    UI->>DB: Store encrypted_credential
    User->>UI: Requests "View Password"
    UI->>DB: Get encrypted_credential
    UI->>Crypto: DecryptField(ciphertext, master_key)
    Crypto->>UI: Plaintext (15s clipboard timeout)

    Note over User,DPAPI: Security Backbone
    DB->>DPAPI: Request DB encryption key
    DPAPI-->>DB: Unprotected key (in memory)
    UI->>UI: ZeroMemory(plaintext)
```


## Attack Prevention
🛡️ SQL Injection: Parameterized queries

💣 Brute Force: Account lockout + slow hashing

👀 Shoulder Surfing: Masked password fields

📋 Clipboard Risks: Auto-clear mechanism

## 🖥️ Key Features
Feature	Description
Zero-Knowledge	We never see/store your passwords
Secure Search	Find credentials without full decryption
Password Generator	Built-in strong password creator
Cross-Device Sync	(Future) Encrypted cloud sync
## 📦 Installation
Windows x64 (.NET 8.0 Required)
Click the Download Installer button above

Run VantaSafe_Setup.exe

Follow the simple wizard (takes < 1 minute)

Launch from your Start Menu

## ℹ️ System Requirements:

Windows 10/11 (64-bit)

.NET 8.0 Runtime

50MB disk space

## 🆘 Recovery Process
If locked out:

Click "Recover Account"

Enter your username and device secret

## Regain access securely

🔑 Your device secret was shown during registration - store it safely!

📜 Changelog (v2.1)
⬆️ Upgraded to .NET 8.0

🏷️ Added explicit x64 support

🚀 Optimized installer package

## 🛡️ Enhanced encryption routines

# 🤝 Contribute
Found an issue? Want to improve security?
Open an Issue

💎 Your Security Is Our Top Priority
"In a world of digital threats, Vanta Safe stands guard"

Download Now

Offline • Secure • Uncompromising

