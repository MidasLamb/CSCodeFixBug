using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Analyzer2
{
   [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Analyzer2CodeFixProvider)), Shared]
   public class Analyzer2CodeFixProvider : CodeFixProvider
   {
      private const string title = "Make uppercase";

      public sealed override ImmutableArray<string> FixableDiagnosticIds
      {
         get { return ImmutableArray.Create(Analyzer2Analyzer.DiagnosticId); }
      }

      public async sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
      {
         var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

         // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
         var diagnostic = context.Diagnostics.First();
         var diagnosticSpan = diagnostic.Location.SourceSpan;

         var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();
         //var body = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<>


         // Register a code action that will invoke the fix.
         context.RegisterCodeFix(
            CodeAction.Create(
               title: "Implement function.",
                  createChangedSolution: c => Fix(context, root, methodDeclaration, c),
               equivalenceKey: "Implement function"),
            diagnostic);
      }

      public async Task<Solution> Fix(CodeFixContext context, SyntaxNode root, MethodDeclarationSyntax method, CancellationToken c)
      {
         var originalSolution = context.Document.Project.Solution;
         var parameters = method.ParameterList.Parameters;


         // Construct the body:
         var newMethod = method.WithBody(await GenerateBody(method));
         var newRoot = root.ReplaceNode(method, newMethod);

         var newSolution = originalSolution.WithDocumentSyntaxRoot(context.Document.Id, newRoot);
         return newSolution;
      }

      private async Task<BlockSyntax> GenerateBody(MethodDeclarationSyntax method)
      {
         bool hasReturnType = DoesMethodHaveReturnType(method);

         var methodInterface = SyntaxFactory.IdentifierName("ISomeInterface");
         TypeArgumentListSyntax typeArgumentList = null;
         if (hasReturnType)
         {
            // <ReturnType, IInterface>

         } else
         {
            // <IInterface>
            typeArgumentList = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(new List<TypeSyntax>() { methodInterface }));
         }

         var awaitCall = SyntaxFactory.AwaitExpression(
            SyntaxFactory.InvocationExpression(
               //Before the call, everything up to the '('
               SyntaxFactory.MemberAccessExpression(
                  SyntaxKind.SimpleMemberAccessExpression,
                  SyntaxFactory.IdentifierName("TestStaticClass"),
                  SyntaxFactory.GenericName(
                     SyntaxFactory.Identifier("TestFunction"),
                     typeArgumentList
                  )

               ),
               CreateArguments(method)
            )
            );
         if (hasReturnType)
         {
            return SyntaxFactory.Block(SyntaxFactory.ReturnStatement(awaitCall));
         } else
         {
            return SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(awaitCall));
         }

      }

      /// <summary>
      /// Create an argument list of the form (new object[] { argumentA, argumentB,... } )
      /// </summary>
      /// <param name="method"></param>
      /// <returns></returns>
      private ArgumentListSyntax CreateArguments(MethodDeclarationSyntax method)
      {
         SeparatedSyntaxList<ExpressionSyntax> arguments = SyntaxFactory.SeparatedList<ExpressionSyntax>();

         foreach(var p in method.ParameterList.Parameters)
         {
            arguments = arguments.Add(SyntaxFactory.IdentifierName(p.Identifier));
         }

         var array = SyntaxFactory.ArrayCreationExpression(
            SyntaxFactory.ArrayType(
               SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
               SyntaxFactory.List(new List<ArrayRankSpecifierSyntax> { SyntaxFactory.ArrayRankSpecifier() })
               ),
            SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, arguments)
         );

         var result =  SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
               SyntaxFactory.Argument(array)
            )
         );
         return result;
      }

      private bool DoesMethodHaveReturnType(MethodDeclarationSyntax method)
      {
         return false;
      }

   }
}
