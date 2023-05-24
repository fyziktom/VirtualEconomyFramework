# VEFramework.BlockchainIndexerServer Linuxu installation and run script

# Download the .NET 7.0 SDK
echo "Downloading .NET 7.0 SDK..."
wget https://download.visualstudio.microsoft.com/download/pr/351400ef-f2e6-4ee7-9d1b-4c246231a065/9f7826270fb36ada1bdb9e14bc8b5123/dotnet-sdk-7.0.302-linux-x64.tar.gz
mkdir -p $HOME/dotnet && tar zxf dotnet-sdk-7.0.302-linux-x64.tar.gz -C $HOME/dotnet
export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet
dotnet --info

# Clone the repository
echo "Cloning the repository"
cd $HOME/vef
git clone https://github.com/fyziktom/VirtualEconomyFramework.git

# Checkout to blockchain indexer branch
git checkout 182-blockchain-indexer

# Go to the Blockchain Indexer Server folder
cd $HOME/vef/VirtualEconomyFramework/VirtualEconomyFramework/VEFramework.BlockchainIndexerServer/

# Optional Step: Edit appsettings.json if you use specific RPC Nebliod parameters

# Publish server
dotnet publish --configuration Release --output ./publish

# Run server
dotnet ./publish/VEFramework.BlockchainIndexerServer.dll
