# Contributing to healthdata-export-tools

Thank you for considering contributing! This document covers everything you need to get started.

## Building Locally

**Prerequisites:** [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later.

```bash
# Clone the repository
git clone https://github.com/sarmkadan/healthdata-export-tools.git
cd healthdata-export-tools

# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Build the Docker image (optional)
docker build -t healthdata-export-tools .
```

## Running Tests

```bash
# Run all tests
dotnet test --configuration Release --verbosity normal

# Run tests with TRX output (as CI does)
dotnet test --configuration Release --logger "trx" --results-directory ./TestResults
```

If you are adding new features or fixing bugs, include corresponding unit tests under `tests/`.

## How to Contribute

### 1. Fork & Create a Branch
1. Fork the repository on GitHub.
2. Clone your fork: `git clone https://github.com/your-username/healthdata-export-tools.git`
3. Create a branch: `git checkout -b feature/your-feature-name` or `git checkout -b fix/your-bug-fix`

### 2. Code Style & Conventions
- The project uses `.editorconfig` for consistent formatting — most editors apply it automatically.
- Follow the existing naming conventions (PascalCase for types, interfaces prefixed with `I`).
- Provide XML documentation comments for all public APIs.
- Use `file`-scoped namespaces (`namespace Foo.Bar;`) consistent with the rest of the codebase.

### 3. Commit Messages
Use [Conventional Commits](https://www.conventionalcommits.org/) prefixes:

| Prefix | Purpose |
|--------|---------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `docs:` | Documentation only |
| `test:` | Adding or updating tests |
| `refactor:` | Code change that is neither a feature nor a fix |
| `ci:` | CI/CD changes |

### 4. Pull Request Guidelines
1. Ensure all tests pass locally before opening a PR.
2. Keep PRs focused — one logical change per PR.
3. Open a Pull Request against `main` with a clear description of what was changed and why.
4. Reference any related issues with `Fixes #<number>` or `Closes #<number>`.

## Reporting Issues

We use GitHub Issues to track bugs and feature requests. 

If you find a bug or have a suggestion, please check if an issue already exists. If not, open a new issue and provide as much detail as possible, including:
- A clear and descriptive title.
- Steps to reproduce the issue.
- Expected behavior vs. actual behavior.
- Relevant logs, error messages, or code snippets.
- Your environment details (OS, .NET SDK version, etc.).

## License

By contributing to healthdata-export-tools, you agree that your contributions will be licensed under its MIT License.
