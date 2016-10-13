﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.IO;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)                    
        {
            int one, two;    
            string three;        
            
            Compilation test = CreateTestCompilation();

            foreach (SyntaxTree sourceTree in test.SyntaxTrees)
            {
                SemanticModel model = test.GetSemanticModel(sourceTree);

                InitializerRewriter rewriter = new InitializerRewriter(model);

                SyntaxNode newSource = rewriter.Visit(sourceTree.GetRoot());                

                if(!Directory.Exists(@"../new_src"))
                    Directory.CreateDirectory(@"../new_src");

                if (newSource != sourceTree.GetRoot())
                {                    
                    File.WriteAllText(Path.Combine(@"../new_src", Path.GetFileName(sourceTree.FilePath)), newSource.ToFullString());
                }
            }       
        }
        
        private static Compilation CreateTestCompilation()
        {    
            String programPath = @"Program.cs";
            String programText = File.ReadAllText(programPath);
            SyntaxTree programTree = CSharpSyntaxTree.ParseText(programText)
									.WithFilePath(programPath);

            String rewriterPath = @"InitializerRewriter.cs";
            String rewriterText = File.ReadAllText(rewriterPath);
            SyntaxTree rewriterTree = CSharpSyntaxTree.ParseText(rewriterText)
									 .WithFilePath(rewriterPath);

            SyntaxTree[] sourceTrees = { programTree, rewriterTree };

            MetadataReference mscorlib =	MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);
            MetadataReference codeAnalysis = MetadataReference.CreateFromFile(typeof(SyntaxTree).GetTypeInfo().Assembly.Location);
            MetadataReference csharpCodeAnalysis = MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).GetTypeInfo().Assembly.Location);                    

            MetadataReference[] references = { mscorlib, codeAnalysis, csharpCodeAnalysis };

            return CSharpCompilation.Create( "ConsoleApplication",
											 sourceTrees,
											 references,
											 new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
