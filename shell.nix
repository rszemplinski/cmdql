{ pkgs ? import <nixpkgs> { } }:

let
  name = "cmdql";
  
  docfx = pkgs.buildDotnetGlobalTool {
    pname = "docfx";
    version = "2.75.1";
    nugetSha256 = "sha256-bbD4+yNxM4vmZXPczeFH+Hy5IohKVA2cIrb+88tLD8Y=";
    meta = with pkgs.lib; {
      description = "Documentation Generation Tool";
      homepage = "https://dotnet.github.io/docfx/";
      license = licenses.mit;
      platforms = [ builtins.currentSystem ];
    };
  };

  tools = with pkgs; {
    cli = [
      coreutils
      nixpkgs-fmt
    ];
    dotnet = [
      pkgs.dotnet-sdk_8
      docfx
    ];
    scripts = [ ];
  };

  paths = pkgs.lib.flatten [ (builtins.attrValues tools) ];
  env = pkgs.buildEnv {
    inherit name paths;
    buildInputs = paths;
  };

in
env.overrideAttrs (_: {
  NIXUP = "0.0.5";
  DOTNET_ROOT = "${pkgs.dotnet-sdk_8}";
  NIX_LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath ([
    pkgs.stdenv.cc.cc
  ]);
  NIX_LD = "${pkgs.stdenv.cc.libc_bin}/bin/ld.so";
  nativeBuildInputs = [ ] ++ paths;
})
