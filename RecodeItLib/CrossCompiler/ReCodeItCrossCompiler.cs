﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReCodeIt.Models;
using ReCodeIt.ReMapper;
using ReCodeIt.Utils;

namespace ReCodeIt.CrossCompiler;

public class ReCodeItCrossCompiler
{
    public ReCodeItCrossCompiler()
    {
        Remapper = new(this);
    }

    private ReCodeItRemapper Remapper { get; }
    public CrossCompilerSettings Settings => DataProvider.Settings.CrossCompiler;

    public CrossCompilerProjectModel ActiveProject => ProjectManager.ActiveProject;

    /// <summary>
    /// Key: Remapped name, value: old name
    /// </summary>
    public Dictionary<string, string> ChangedTypes { get; set; } = [];

    public void StartRemap()
    {
        ChangedTypes.Clear();

        Remapper.InitializeRemap(
            ActiveProject.RemapModels,
            ActiveProject.OriginalAssemblyPath,
            ActiveProject.RemappedAssemblyPath,
            true);

        if (ActiveProject == null)
        {
            Logger.Log("ERROR: No Cross Compiler Project is loaded, create or load one first.", ConsoleColor.Red);
            return;
        }

        if (ActiveProject.ReCodeItProjectPath == string.Empty)
        {
            Logger.Log("ERROR: No ReCodeIt Project directory is set. (Project Creation Failed)", ConsoleColor.Red);
            return;
        }

        Logger.Log("-----------------------------------------------", ConsoleColor.Yellow);
        Logger.Log($"Cross patch remap result", ConsoleColor.Yellow);
        Logger.Log($"Changed {ChangedTypes.Count} types", ConsoleColor.Yellow);
        Logger.Log($"Original assembly path: {ActiveProject.OriginalAssemblyPath}", ConsoleColor.Yellow);
        Logger.Log($"Original assembly hash: {ActiveProject.OriginalAssemblyHash}", ConsoleColor.Yellow);
        Logger.Log($"Original patched assembly path: {ActiveProject.RemappedAssemblyPath}", ConsoleColor.Yellow);
        Logger.Log($"Original patched assembly hash: {ActiveProject.RemappedAssemblyHash}", ConsoleColor.Yellow);
        Logger.Log("-----------------------------------------------", ConsoleColor.Yellow);
    }

    public void StartCrossCompile()
    {
        AnalyzeSourceFiles();
    }

    private void AnalyzeSourceFiles()
    {
        foreach (var file in ProjectManager.AllProjectSourceFiles)
        {
            AnalyzeSourcefile(file);
        }
    }

    private void AnalyzeSourcefile(string file)
    {
        var source = LoadSourceFile(file);
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var root = syntaxTree.GetCompilationUnitRoot();

        var identifiers = root.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => ActiveProject.ChangedTypes.ContainsKey(id.Identifier.Text));

        if (!identifiers.Any()) { return; }

        Logger.Log($"found {identifiers.Count()} objects to change in file {Path.GetFileNameWithoutExtension(file)}");

        // Replace "RigClass" with "NewRigClass"
        var newRoot = root.ReplaceNodes(identifiers, (oldNode, newNode) =>
                SyntaxFactory.IdentifierName(ActiveProject.ChangedTypes[oldNode.Identifier.Text])
                    .WithLeadingTrivia(oldNode.GetLeadingTrivia())
                    .WithTrailingTrivia(oldNode.GetTrailingTrivia()));
    }

    /// <summary>
    /// Loads a source file from disk
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string LoadSourceFile(string path)
    {
        return File.ReadAllText(path);
    }
}