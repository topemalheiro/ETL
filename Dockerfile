# Use the official .NET 8 SDK image with Ubuntu base for better browser support
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

# Set working directory
WORKDIR /app

# Install Chrome dependencies and Chrome browser
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    unzip \
    curl \
    xvfb \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list \
    && apt-get update \
    && apt-get install -y google-chrome-stable \
    && rm -rf /var/lib/apt/lists/*

# Copy solution and project files
COPY *.sln ./
COPY TestFramework/TestFramework.csproj ./TestFramework/
COPY UI.AutomationTests/UI.AutomationTests.csproj ./UI.AutomationTests/
COPY API.AutomationTests/API.AutomationTests.csproj ./API.AutomationTests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . ./

# Build the solution
RUN dotnet build --configuration Release --no-restore

# Create test execution stage
FROM build AS test
WORKDIR /app

# Set environment variables for CI execution
ENV CI=true
ENV BROWSER=Chrome
ENV DISPLAY=:99

# Create directories for test results and logs
RUN mkdir -p /app/TestResults /app/logs /app/Screenshots

# Create script for running tests
RUN echo '#!/bin/bash\n\
# Start virtual display for headless browser testing\n\
Xvfb :99 -screen 0 1920x1080x24 &\n\
\n\
# Wait for display to be ready\n\
sleep 2\n\
\n\
echo "=== Running Test Automation Framework ==="\n\
echo "Browser: $BROWSER"\n\
echo "Environment: CI"\n\
\n\
# Run API tests first (faster)\n\
echo "Running API Tests..."\n\
dotnet test API.AutomationTests/API.AutomationTests.csproj \\\n\
  --configuration Release \\\n\
  --logger "trx;LogFileName=api-test-results.trx" \\\n\
  --results-directory /app/TestResults \\\n\
  --verbosity normal\n\
\n\
API_EXIT_CODE=$?\n\
\n\
# Run UI tests\n\
echo "Running UI Tests..."\n\
dotnet test UI.AutomationTests/UI.AutomationTests.csproj \\\n\
  --configuration Release \\\n\
  --logger "trx;LogFileName=ui-test-results.trx" \\\n\
  --results-directory /app/TestResults \\\n\
  --verbosity normal\n\
\n\
UI_EXIT_CODE=$?\n\
\n\
# Generate summary\n\
echo "=== Test Execution Summary ==="\n\
echo "API Tests Exit Code: $API_EXIT_CODE"\n\
echo "UI Tests Exit Code: $UI_EXIT_CODE"\n\
\n\
# List generated artifacts\n\
echo "Generated test artifacts:"\n\
ls -la /app/TestResults/\n\
ls -la /app/logs/ 2>/dev/null || echo "No log files generated"\n\
ls -la /app/Screenshots/ 2>/dev/null || echo "No screenshots generated"\n\
\n\
# Return overall exit code\n\
if [ $API_EXIT_CODE -eq 0 ] && [ $UI_EXIT_CODE -eq 0 ]; then\n\
  echo "✓ All tests passed!"\n\
  exit 0\n\
else\n\
  echo "✗ Some tests failed!"\n\
  exit 1\n\
fi' > /app/run-tests.sh \
&& chmod +x /app/run-tests.sh

# Set the entrypoint to run tests
ENTRYPOINT ["/app/run-tests.sh"] 