# Contributing to healthdata-export-tools

First off, thank you for considering contributing to healthdata-export-tools! It's people like you that make the open source community such a great place to learn, inspire, and create.

## How to Contribute

### 1. Fork & Create a Branch
1. Fork the repository on GitHub.
2. Clone your fork locally: `git clone https://github.com/your-username/healthdata-export-tools.git`
3. Create a new branch for your feature or bugfix: `git checkout -b feature/your-feature-name` or `git checkout -b fix/your-bug-fix`

### 2. Development Requirements
- **.NET 10.0 SDK** or later is required to build and run the project.

### 3. Code Style & Conventions
- Follow the existing code style and naming conventions within the project.
- Provide XML documentation comments for all public APIs, classes, and methods.
- **Keep all author headers**: If a file already has an author header, do not remove or modify it. If you create a new file, you may add your own header following the existing format.

### 4. Running Tests
Before submitting a pull request, please ensure that all tests pass. Run the following command from the root of the project:
```bash
dotnet test
```
If you are adding new features or fixing bugs, please include corresponding unit tests.

### 5. Submit a Pull Request
1. Commit your changes with clear, descriptive commit messages.
2. Push your branch to your fork on GitHub.
3. Open a Pull Request against the `main` branch of the original repository.
4. Provide a detailed description of the changes in your PR, including any related issue numbers.

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
