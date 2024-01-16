{ pkgs ? import <nixpkgs> { }, lib ? pkgs.lib, buildDotnetGlobalTool ? pkgs.buildDotnetGlobalTool }:

let
  docfx = buildDotnetGlobalTool {
    pname = "docfx";
    version = "2.75.1";
    nugetSha256 = "sha256-bbD4+yNxM4vmZXPczeFH+Hy5IohKVA2cIrb+88tLD8Y=";
    meta = with lib; {
      description = "Documentation Generation Tool";
      homepage = "https://dotnet.github.io/docfx/";
      license = licenses.mit;
      platforms = [ builtins.currentSystem ];
    };
  };
in
pkgs.mkShell {
  buildInputs = with pkgs; [
    dotnet-sdk_8
    docfx
  ];
}
