# 🎮⛓️ Neo Unity SDK

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Cross--Platform-orange.svg)](https://docs.unity3d.com/Manual/system-requirements.html)
[![Neo Version](https://img.shields.io/badge/Neo-N3-brightgreen.svg)](https://neo.org)

**The Complete Neo N3 Blockchain SDK for Unity Game Developers**

Transform your Unity games with seamless Neo blockchain integration. Build next-generation Web3 games with NFTs, smart contracts, and decentralized economies.

## ✨ Features

### 🔐 **Comprehensive Wallet Management**
- NEP-6 wallet support with encryption
- BIP-39 mnemonic phrase generation
- Multi-signature account management
- Secure key storage and WIF import/export

### 📄 **Smart Contract Integration** 
- Easy smart contract deployment and interaction
- NEP-17 fungible token operations
- NEP-11 NFT support with metadata
- Built-in support for native Neo contracts

### 🎯 **Unity-Optimized Architecture**
- MonoBehaviour components for drag-and-drop integration
- ScriptableObject configuration system
- Coroutine-based async operations
- Inspector-friendly serialization

### ⚡ **High Performance**
- Optimized for Unity's main thread
- Memory-efficient operations
- Cross-platform compatibility (Mobile, Desktop, WebGL)
- Built-in connection pooling

## 🚀 Quick Start

### Installation

1. **Via Unity Package Manager:**
   ```
   Window → Package Manager → + → Add package from git URL
   https://github.com/neo-project/neo-unity-sdk.git
   ```

2. **Via Package Manager JSON:**
   ```json
   {
     "dependencies": {
       "com.neo.unity-sdk": "1.0.0"
     }
   }
   ```

### Basic Usage

```csharp
using Neo.Unity.SDK;
using UnityEngine;

public class BlockchainManager : MonoBehaviour
{
    [SerializeField] private NeoUnityConfig config;
    
    async void Start()
    {
        // Initialize Neo SDK
        await NeoUnity.Instance.Initialize(config);
        
        // Create a wallet
        var wallet = Wallet.Create();
        Debug.Log($"New Address: {wallet.GetDefaultAccount().Address}");
        
        // Check NEO balance
        var balance = await NeoUnity.Instance.GetNeoBalance(wallet.GetDefaultAccount().Address);
        Debug.Log($"NEO Balance: {balance}");
    }
}
```

## 📦 Sample Projects

### 🏪 **NFT Marketplace Game**
Complete marketplace with:
- NFT minting and trading
- In-game auction system  
- Player-to-player transactions
- Blockchain inventory management

### ⚔️ **Blockchain RPG**
RPG featuring:
- Blockchain-based character progression
- NFT weapons and armor
- Decentralized guild system
- Token-based economy

### 💰 **Wallet Integration Demo**
Demonstrates:
- Wallet creation and import
- Transaction signing and broadcasting
- Balance checking and transfers
- Smart contract interaction

## 🏗️ Architecture

```
Neo.Unity.SDK/
├── Core/           # Core blockchain functionality
├── Contracts/      # Smart contract interactions
├── Crypto/         # Cryptographic operations
├── Wallet/         # Wallet management
├── Utils/          # Utility classes
└── Components/     # Unity MonoBehaviour components
```

## 📚 Documentation

- **[API Reference](Documentation~/api-reference.md)** - Complete API documentation
- **[Unity Integration Guide](Documentation~/unity-integration.md)** - Unity-specific patterns
- **[Smart Contract Tutorial](Documentation~/smart-contracts.md)** - Contract interaction guide
- **[Performance Optimization](Documentation~/performance.md)** - Best practices

## 🛠️ Development

### Requirements
- Unity 2021.3 LTS or later
- .NET Standard 2.1
- Newtonsoft JSON package

### Building from Source
```bash
git clone https://github.com/neo-project/neo-unity-sdk.git
cd neo-unity-sdk
# Open in Unity 2021.3+
```

### Running Tests
```bash
# Run all tests via Unity Test Runner
Window → General → Test Runner → Run All
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🌐 Links

- **[Neo Official Website](https://neo.org)**
- **[Neo Developer Documentation](https://docs.neo.org)**
- **[Discord Community](https://discord.gg/neo)**
- **[GitHub Issues](https://github.com/neo-project/neo-unity-sdk/issues)**

---

**Ready to build the future of blockchain gaming? Let's get started! 🚀**