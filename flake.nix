{
  description = ".NET Development Environment";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, utils }:
    utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
        dotnet-sdk = with pkgs.dotnetCorePackages; combinePackages [
          sdk_10_0
        ];
      in
      {
        devShells.default = pkgs.mkShell {
          packages = [ 
            dotnet-sdk 
          ];

          shellHook = ''
            export DOTNET_ROOT="${dotnet-sdk}"
            echo ".NET environment loaded: $(dotnet --version)"
          '';
        };
      });
}