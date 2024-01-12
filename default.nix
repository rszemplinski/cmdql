{ pkgs ? import <nixpkgs> {} }:

let
  dotnetEnv = pkgs.mkShell {
    name = "qlsh";
    buildInputs = with pkgs; [
       (with dotnetCorePackages; combinePackages [
         sdk_8_0
       ])
    ];
  };
in
dotnetEnv
