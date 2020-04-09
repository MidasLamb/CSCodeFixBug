using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Analyzer2;

namespace Analyzer2.Test
{
   [TestClass]
   public class UnitTest : CodeFixVerifier
   {

      //No diagnostics expected to show up
      [TestMethod]
      public void TestOnEmpty()
      {
         var test = @"";

         VerifyCSharpDiagnostic(test);
      }

      //Diagnostic and CodeFix both triggered and checked for
      [TestMethod]
      public void TestFindError()
      {
         var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
   class TypeName
   {   
      public void Test(int a)
      {
      }
    
   }
}";
         var expected = new DiagnosticResult
         {
            Id = "Analyzer2",
            Message = Analyzer2Analyzer.MessageFormat,
            Severity = DiagnosticSeverity.Warning,
            Locations =
                 new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 7)
                     }
         };

         VerifyCSharpDiagnostic(test, expected);
      }

      [TestMethod]
      public void TestFix()
      {
         var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
   class TypeName
   {   
      public async Task Test(int a)
      {
      }
    
   }
}";
         var expected = new DiagnosticResult
         {
            Id = "Analyzer2",
            Message = Analyzer2Analyzer.MessageFormat,
            Severity = DiagnosticSeverity.Warning,
            Locations =
                 new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 7)
                     }
         };

         VerifyCSharpDiagnostic(test, expected);

         var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
   class TypeName
   {   
      public async Task Test(int a)
      {
         await TestStaticClass.TestMethod<ISomeInterface>(new object[] {a});
      }
    
   }
}";
         VerifyCSharpFix(test, fixtest);
      }

      [TestMethod]
      public void TestOutputOfCodeFix()
      {
         string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
   class TypeName
   {   
      public async Task Test(int a)
        {
            await TestStaticClass.TestFunction<ISomeInterface>(new object[] { a });
        }
    }
}";
         VerifyCSharpDiagnostic(test);
      }

      protected override CodeFixProvider GetCSharpCodeFixProvider()
      {
         return new Analyzer2CodeFixProvider();
      }

      protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
      {
         return new Analyzer2Analyzer();
      }
   }
}
