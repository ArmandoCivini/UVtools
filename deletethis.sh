# 1. Fetch the real download link
# Visit the page above, right-click the “Linux x64 – Binaries (tar.gz)” link, and copy its URL

# 2. In your terminal, download using the copied link:
wget "<paste_copied_url_here>" -O aspnetcore-runtime-9.0.6-linux-x64.tar.gz

# 3. Extract it:
mkdir -p "$HOME/.dotnet"
tar -xzf aspnetcore-runtime-9.0.6-linux-x64.tar.gz -C "$HOME/.dotnet"

# 4. Temporarily set up environment:
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"

# 5. Check installation:
dotnet --info
# You should see something like:
# ".NET runtimes installed:
#  Microsoft.AspNetCore.App 9.0.6” — confirming success

# 6. Make it permanent:
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$DOTNET_ROOT:$PATH' >> ~/.bashrc
source ~/.bashrc

# 7. Restart VS Code from this terminal:
code .
