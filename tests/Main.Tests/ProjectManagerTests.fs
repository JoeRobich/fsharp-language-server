module Main.Tests.ProjectManagerTests

open Main
open Main.Tests.Common
open System
open System.IO
open NUnit.Framework

[<Test>]
let ``parse project file JSON`` () = 
    let json = """
    {
      "version": 3,
      "targets": {
        ".NETCoreApp,Version=v2.0": {
          "FSharp.Compiler.Service/16.0.2": {
            "type": "package",
            "compile": {
              "lib/netstandard1.6/FSharp.Compiler.Service.dll": {}
            }
          }
        }
      },
      "libraries": {
        "FSharp.Compiler.Service/16.0.2": {
          "path": "fsharp.compiler.service/16.0.2"
        }
      },
      "packageFolders": {
        "/Users/george/.nuget/packages/": {},
        "/usr/local/share/dotnet/sdk/NuGetFallbackFolder": {}
      }
    }"""
    let parsed = ProjectManagerUtils.parseAssetsJson json
    Assert.True(Map.containsKey ".NETCoreApp,Version=v2.0" parsed.targets)
    Assert.True(Map.containsKey "FSharp.Compiler.Service/16.0.2" parsed.libraries)
    Assert.That(parsed.packageFolders, Contains.Item "/Users/george/.nuget/packages/")

[<Test>]
let ``parse a project file`` () = 
    let file = FileInfo(Path.Combine [|projectRoot.FullName; "src"; "Main"; "Main.fsproj"|])
    let parsed = ProjectManagerUtils.parseBoth file
    let name (f: FileInfo): string = f.Name
    Assert.That(parsed.sources |> Seq.map name, Contains.Item "ProjectManager.fs")
    Assert.That(parsed.projectReferences |> Seq.map name, Contains.Item "LSP.fsproj")
    Assert.That(parsed.references |> Seq.map name, Contains.Item "FSharp.Compiler.Service.dll")

[<Test>]
let ``parse a project file recursively`` () = 
    let file = FileInfo(Path.Combine [|projectRoot.FullName; "src"; "Main"; "Main.fsproj"|])
    let parsed = ProjectManagerUtils.parseProjectOptions file
    let name (f: string) = 
      let parts = f.Split('/')
      parts.[parts.Length - 1]
    Assert.That(parsed.SourceFiles |> Seq.map name, Contains.Item "ProjectManager.fs")
    Assert.That(parsed.ReferencedProjects |> Seq.map (fun (x, _) -> x) |> Seq.map name, Contains.Item "LSP.fsproj")
    // Assert.That(parsed.references |> Seq.map name, Contains.Item "FSharp.Compiler.Service.dll")

[<Test>]
let ``find an fsproj in a parent dir`` () = 
    let projects = ProjectManager()
    let file = FileInfo(Path.Combine [|projectRoot.FullName; "src"; "Main"; "Program.fs"|])
    let parsed = projects.FindProjectOptions(Uri(file.FullName)) |> Option.get
    let name (f: string) = 
      let parts = f.Split('/')
      parts.[parts.Length - 1]
    Assert.That(parsed.SourceFiles |> Seq.map name, Contains.Item "ProjectManager.fs")
    Assert.That(parsed.ReferencedProjects |> Seq.map (fun (x, _) -> x) |> Seq.map name, Contains.Item "LSP.fsproj")
    // Assert.That(parsed.references |> Seq.map name, Contains.Item "FSharp.Compiler.Service.dll")