{ pkgs ? import
    (fetchTarball {
      name = "jpetrucciani-2024-01-09";
      url = "https://github.com/jpetrucciani/nix/archive/5257884e14fd174a3d55332330bc09575c3a06ed.tar.gz";
      sha256 = "05w91m5i7l767f5ihjh8bvpcdjrl59phx5rm4dg2z5778c0qgy4r";
    })
    { }
}:
let
  name = "qlsh";

  tools = with pkgs; {
    cli = [
      coreutils
      nixpkgs-fmt
    ];
    dotnet = [
      (with dotnetCorePackages; combinePackages [
        sdk_7_0
        sdk_8_0
      ])
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
})
