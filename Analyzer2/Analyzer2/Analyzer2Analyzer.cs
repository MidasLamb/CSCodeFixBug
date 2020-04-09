using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Analyzer2
{
   [DiagnosticAnalyzer(LanguageNames.CSharp)]
   public class Analyzer2Analyzer : DiagnosticAnalyzer
   {
      public const string DiagnosticId = "Analyzer2";

      public static readonly string Title = "TestTitle";
      public static readonly string MessageFormat = "TestMessage";
      private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
      private const string Category = "Naming";

      private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

      public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

      public override void Initialize(AnalysisContext context)
      {
         context.RegisterOperationBlockAction(AnalyzeOperationBlock);
      }

            private static void AnalyzeOperationBlock(OperationBlockAnalysisContext context)
      {
         if (context.OwningSymbol is IMethodSymbol)
         {
            var method = (IMethodSymbol)context.OwningSymbol;

            // The body should contain it:
            foreach(var operationBlock in context.OperationBlocks)
            {
               if (operationBlock.Children.Count() == 0)
               {
                  var diagnostic = Diagnostic.Create(Rule, operationBlock.Syntax.GetLocation());
                  context.ReportDiagnostic(diagnostic);
               }
               if (operationBlock.Children.Count() == 1)
               {
                  var child = operationBlock.Children.First();
                  if (child.Kind == OperationKind.Throw)
                  {
                     var throwOperation = (IThrowOperation)child;
                     var exception = throwOperation.Exception;
                     if (exception.Kind == OperationKind.Conversion)
                     {
                        var conversionException = (IConversionOperation)exception;
                        if (conversionException.Operand.Type.Name == nameof(NotImplementedException))
                        {
                           var diagnostic = Diagnostic.Create(Rule, operationBlock.Syntax.GetLocation());
                           context.ReportDiagnostic(diagnostic);
                        }

                     }
                  }
               }
            }
         }
      }

   }
}
