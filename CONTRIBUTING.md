# Contributing to Neo Unity SDK 🤝

We welcome contributions from the community! This guide will help you get started with contributing to the Neo Unity SDK.

## 🎯 How to Contribute

### 🐛 Reporting Bugs

1. **Search existing issues** to avoid duplicates
2. **Use the bug report template** when creating new issues
3. **Provide detailed information**:
   - Unity version and platform
   - Neo Unity SDK version
   - Steps to reproduce
   - Expected vs actual behavior
   - Console logs and error messages

### ✨ Suggesting Features

1. **Check the roadmap** and existing feature requests
2. **Use the feature request template**
3. **Explain the use case** and how it benefits game developers
4. **Consider the scope** - is it specific to Unity or general blockchain?

### 🔧 Contributing Code

1. **Fork the repository** and create a feature branch
2. **Follow the coding standards** outlined below
3. **Add tests** for new functionality
4. **Update documentation** as needed
5. **Submit a pull request** with a clear description

## 🏗️ Development Setup

### Prerequisites

- **Unity 2021.3 LTS** or later
- **.NET 6.0 SDK** or later
- **Git** for version control
- **Visual Studio** or **JetBrains Rider** (recommended)

### Setup Steps

```bash
# 1. Fork and clone the repository
git clone https://github.com/your-username/neo-unity-sdk.git
cd neo-unity-sdk

# 2. Open in Unity
# Open Unity Hub → Add → Select the neo-unity-sdk folder

# 3. Install dependencies
# Unity will automatically resolve package dependencies

# 4. Run tests
# Window → General → Test Runner → Run All Tests
```

## 📝 Coding Standards

### 🎨 **Code Style**

#### **C# Conventions**
- Use **PascalCase** for public members, **camelCase** for private
- Use **meaningful names** that clearly express intent
- **XML documentation** for all public APIs
- **Async methods** should end with `Async` suffix
- **Unity serialization** attributes where appropriate

```csharp
/// <summary>
/// Gets the balance of a NEP-17 token for the specified account.
/// </summary>
/// <param name="tokenHash">The token contract hash</param>
/// <param name="accountHash">The account script hash</param>
/// <returns>The token balance in fractions</returns>
public async Task<long> GetTokenBalanceAsync(Hash160 tokenHash, Hash160 accountHash)
{
    // Implementation
}
```

#### **Unity Integration**
- **MonoBehaviour** for game integration components
- **ScriptableObject** for configuration data
- **[SerializeField]** for Inspector-visible private fields
- **[ContextMenu]** for debugging utilities
- **Proper event handling** with Unity lifecycle

### 🧪 **Testing Standards**

#### **Test Structure**
- One test class per main class
- **[TestFixture]** attribute on test classes
- **[Test]** for synchronous tests, **[UnityTest]** for coroutines
- **Arrange-Act-Assert** pattern
- **Meaningful test names** describing the scenario

```csharp
[Test]
public async Task GetBalance_WithValidToken_ShouldReturnCorrectBalance()
{
    // Arrange
    var tokenHash = GasToken.SCRIPT_HASH;
    var account = await Account.Create();
    
    // Act
    var balance = await token.GetBalanceOf(account);
    
    // Assert
    Assert.GreaterOrEqual(balance, 0);
}
```

#### **Test Coverage Requirements**
- **100% coverage** for new public APIs
- **Edge cases** and error conditions
- **Integration tests** for blockchain operations
- **Performance tests** for critical paths
- **Unity-specific tests** for Inspector and serialization

### 🎮 **Unity Best Practices**

#### **Performance**
- **Avoid allocations** in Update loops
- **Use object pooling** for frequently created objects
- **Async operations** should be Unity main-thread safe
- **Cache expensive calculations** when possible

#### **Memory Management**
- **Implement IDisposable** for unmanaged resources
- **Null checks** before accessing Unity objects
- **Proper cleanup** in OnDestroy methods
- **Use using statements** for disposable objects

## 🔄 Development Workflow

### 🌿 **Branch Strategy**

- **main**: Stable, production-ready code
- **develop**: Integration branch for new features
- **feature/**: Individual feature development
- **bugfix/**: Bug fixes
- **release/**: Release preparation

### 📝 **Commit Standards**

Use **conventional commits** format:

```
type(scope): description

feat(wallet): add multi-signature account creation
fix(crypto): resolve key generation memory leak
docs(api): update smart contract examples
test(integration): add TestNet connectivity validation
```

### 🔍 **Pull Request Process**

1. **Create feature branch** from `develop`
2. **Implement changes** following coding standards
3. **Add comprehensive tests** with good coverage
4. **Update documentation** as needed
5. **Submit PR** with detailed description
6. **Address review feedback** promptly
7. **Squash commits** before merge

#### **PR Template**
```markdown
## Summary
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] Tests added and passing
```

## 🏷️ Release Process

### 📦 **Version Management**

Follow **semantic versioning**:
- **Major** (X.0.0): Breaking changes
- **Minor** (X.Y.0): New features, backward compatible
- **Patch** (X.Y.Z): Bug fixes, backward compatible

### 🚀 **Release Steps**

1. **Update CHANGELOG.md** with new version details
2. **Update package.json** version number
3. **Run full test suite** and validate all platforms
4. **Create release branch** and finalize documentation
5. **Submit PR to main** for release approval
6. **Tag release** and publish to GitHub
7. **Update Unity Asset Store** if applicable

## 🤝 Community Guidelines

### 💬 **Communication**

- **Be respectful** and constructive in all interactions
- **Use clear, descriptive language** in issues and PRs
- **Help others** by sharing knowledge and experience
- **Follow the code of conduct** in all community spaces

### 🎓 **Learning Resources**

- **[Neo Documentation](https://docs.neo.org/)** - Learn about Neo blockchain
- **[Unity Documentation](https://docs.unity3d.com/)** - Unity development guides
- **[C# Async Programming](https://docs.microsoft.com/en-us/dotnet/csharp/async)** - Async/await patterns
- **[Neo Unity SDK Examples](Samples~/)** - Practical implementation examples

## 🆘 Getting Help

### 📞 **Support Channels**

- **[GitHub Issues](https://github.com/neo-project/neo-unity-sdk/issues)** - Bug reports and feature requests
- **[Neo Discord](https://discord.gg/neo)** - Community chat and support
- **[Unity Forums](https://forum.unity.com/)** - Unity-specific questions
- **[Stack Overflow](https://stackoverflow.com/questions/tagged/neo-blockchain+unity)** - Technical Q&A

### 📚 **Documentation**

- **[Quick Start Guide](Documentation~/QuickStart.md)** - Get started in 10 minutes
- **[API Reference](Documentation~/api-reference.md)** - Complete API documentation
- **[Unity Integration Guide](Documentation~/unity-integration.md)** - Advanced patterns
- **[Examples](Samples~/)** - Production-ready sample applications

## 🏆 Recognition

### 🌟 **Contributors**

All contributors will be recognized in:
- **CONTRIBUTORS.md** file with detailed contributions
- **Release notes** for significant contributions
- **Special mentions** in community announcements
- **Swag and prizes** for major contributions (when available)

### 🎖️ **Contribution Types**

We value all types of contributions:
- **Code contributions** - New features, bug fixes, optimizations
- **Documentation** - Guides, examples, API documentation
- **Testing** - Test cases, platform validation, performance testing
- **Community support** - Helping other developers, answering questions
- **Advocacy** - Spreading awareness, writing tutorials, giving talks

---

## 📋 Quick Contribution Checklist

Before submitting your contribution:

- [ ] **Code compiles** without errors or warnings
- [ ] **Tests pass** on your local machine
- [ ] **Documentation updated** for any API changes
- [ ] **CHANGELOG.md** updated if appropriate
- [ ] **Code follows** established style guidelines
- [ ] **No sensitive information** (keys, passwords) in commits
- [ ] **PR description** clearly explains the changes
- [ ] **Linked to issue** if fixing a reported bug

---

**Thank you for contributing to the future of blockchain gaming! 🎮⛓️**

Together, we're building the tools that will power the next generation of Web3 games on the Neo blockchain.