# =============================================================================
# Health Data Export Tools - Makefile
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build release test clean restore publish docker docker-build docker-run
.PHONY: lint format docs examples format-check coverage

PROJECT_NAME := healthdata-export-tools
SOLUTION := healthdata-export-tools.sln
BUILD_DIR := ./healthdata-export-tools/bin
PUBLISH_DIR := ./publish
OUTPUT_DIR := ./output
EXPORTS_DIR := ./exports

# Default target
help:
	@echo "====================================================================="
	@echo "  Health Data Export Tools - Build Commands"
	@echo "====================================================================="
	@echo ""
	@echo "Build Targets:"
	@echo "  make build          - Build Debug configuration"
	@echo "  make release        - Build Release configuration"
	@echo "  make clean          - Clean build artifacts"
	@echo "  make restore        - Restore NuGet packages"
	@echo ""
	@echo "Testing & Quality:"
	@echo "  make test           - Run unit tests"
	@echo "  make coverage       - Generate code coverage report"
	@echo "  make lint           - Run code analysis"
	@echo "  make format         - Format code automatically"
	@echo "  make format-check   - Check code formatting"
	@echo ""
	@echo "Publishing:"
	@echo "  make publish        - Publish Release build"
	@echo "  make pack           - Create NuGet package"
	@echo ""
	@echo "Docker:"
	@echo "  make docker-build   - Build Docker image"
	@echo "  make docker-run     - Run Docker container"
	@echo "  make docker-compose - Run with docker-compose"
	@echo ""
	@echo "Development:"
	@echo "  make watch          - Build on file changes"
	@echo "  make examples       - Build and list examples"
	@echo "  make docs           - Generate documentation"
	@echo ""
	@echo "====================================================================="

# Restore dependencies
restore:
	@echo "📦 Restoring NuGet packages..."
	dotnet restore $(SOLUTION)
	@echo "✓ Restore complete"

# Debug build
build: restore
	@echo "🔨 Building Debug configuration..."
	dotnet build $(SOLUTION) -c Debug
	@echo "✓ Build complete"

# Release build
release: restore
	@echo "🔨 Building Release configuration..."
	dotnet build $(SOLUTION) -c Release
	@echo "✓ Release build complete"

# Clean build artifacts
clean:
	@echo "🧹 Cleaning build artifacts..."
	rm -rf $(BUILD_DIR)
	rm -rf $(PUBLISH_DIR)
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	@echo "✓ Cleanup complete"

# Run tests
test: build
	@echo "🧪 Running tests..."
	dotnet test $(SOLUTION) --configuration Debug --verbosity normal
	@echo "✓ Tests complete"

# Code coverage
coverage:
	@echo "📊 Generating code coverage report..."
	dotnet test $(SOLUTION) --configuration Release --collect:"XPlat Code Coverage" --verbosity normal
	@echo "✓ Coverage report generated"

# Code analysis & linting
lint:
	@echo "🔍 Running code analysis..."
	dotnet build $(SOLUTION) /p:EnforceCodeStyleInBuild=true
	@echo "✓ Code analysis complete"

# Format code
format:
	@echo "📝 Formatting code..."
	dotnet format $(SOLUTION)
	@echo "✓ Code formatted"

# Check code format
format-check:
	@echo "📝 Checking code format..."
	dotnet format $(SOLUTION) --verify-no-changes
	@echo "✓ Code format check complete"

# Publish Release build
publish: release
	@echo "📦 Publishing Release build..."
	dotnet publish ./$(PROJECT_NAME)/$(PROJECT_NAME).csproj -c Release -o $(PUBLISH_DIR)
	@echo "✓ Publish complete at $(PUBLISH_DIR)"

# Create NuGet package
pack: release
	@echo "📦 Creating NuGet package..."
	dotnet pack ./$(PROJECT_NAME)/$(PROJECT_NAME).csproj -c Release -o ./packages
	@echo "✓ NuGet package created in ./packages"

# Watch for changes and rebuild
watch:
	@echo "👀 Watching for changes (Ctrl+C to stop)..."
	dotnet watch --project ./$(PROJECT_NAME)/$(PROJECT_NAME).csproj

# Docker build
docker-build:
	@echo "🐳 Building Docker image..."
	docker build -t $(PROJECT_NAME):latest .
	docker build -t $(PROJECT_NAME):latest-alpine -f Dockerfile.alpine .
	@echo "✓ Docker image built"

# Docker run
docker-run: docker-build
	@echo "🐳 Running Docker container..."
	docker run --rm \
		-v $$(pwd)/$(EXPORTS_DIR):/data/exports:ro \
		-v $$(pwd)/$(OUTPUT_DIR):/data/output \
		$(PROJECT_NAME):latest
	@echo "✓ Docker container execution complete"

# Docker compose
docker-compose: docker-build
	@echo "🐳 Running with docker-compose..."
	docker-compose up
	@echo "✓ Docker compose running"

# Examples
examples:
	@echo "📚 Listing example applications..."
	@ls -1 ./examples/*.cs | xargs -I {} basename {}
	@echo ""
	@echo "To run an example:"
	@echo "  dotnet run --project healthdata-export-tools -- <ExampleName>"

# Documentation
docs:
	@echo "📖 Generating documentation..."
	dotnet build ./$(PROJECT_NAME)/$(PROJECT_NAME).csproj -c Release /p:GenerateDocumentationFile=true
	@echo "✓ Documentation generated"

# Git operations
git-status:
	@echo "📊 Git status:"
	@git status --short

git-log:
	@echo "📜 Recent commits:"
	@git log --oneline -10

# Information targets
info:
	@echo "====================================================================="
	@echo "  Project Information"
	@echo "====================================================================="
	@echo "Project:        $(PROJECT_NAME)"
	@echo "Solution:       $(SOLUTION)"
	@echo "Build Output:   $(BUILD_DIR)"
	@echo "Publish Output: $(PUBLISH_DIR)"
	@echo ""
	@dotnet --version

version:
	@grep -m 1 "<Version>" ./$(PROJECT_NAME)/$(PROJECT_NAME).csproj | \
		sed 's/.*<Version>//g' | sed 's/<\/Version>.*//g'

# Continuous Integration
ci: clean lint format-check test coverage
	@echo "✅ CI pipeline complete"

# Full build pipeline
all: clean restore lint test build pack docs
	@echo "✅ Full build pipeline complete"

# Development setup
dev-setup: restore
	@echo "🛠 Setting up development environment..."
	@echo "✓ Development environment ready"

# Default when no target specified
.DEFAULT_GOAL := help
